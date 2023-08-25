using System;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using first.Views;
using Microsoft.Data.SqlClient;

namespace first
{
    public partial class MainWindow : Window
    {
        private SqlConnection connection;
        public ObservableCollection<DAL.Entity.ProductGroup> ProductGroups { get; set; } = new();

        public MainWindow()
        {
            InitializeComponent();
            connection = null!;  // ! - для того чтобы не было предупреждения
            DataContext = this;
        }


        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                connection = new SqlConnection(App.ConnectionString);
                connection.Open();  // подключаемся к БД
                LoadGroups();  // загружаем данные
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                Close();
            }
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            connection?.Close();  // закрываем соединение с БД!
        }


        private void LoadGroups()
        {
            using SqlCommand command = new() { Connection = connection };
            command.CommandText = @"SELECT pg.* FROM ProductGroups AS pg WHERE DeleteDt IS NULL";
            try
            {
                using SqlDataReader reader = command.ExecuteReader();
                while (reader.Read())  // get result`s one row
                {
                    ProductGroups.Add(new()
                    {
                        Id = reader.GetGuid(0),
                        Name = reader.GetString(1),
                        Description = reader.GetString(2),
                        Picture = reader.GetString(3),
                    });
                }
            }
            catch (Exception ex) { MessageBox.Show(ex.Message, "Query error", MessageBoxButton.OK, MessageBoxImage.Error); }
        }

        private bool SaveProductGroup(DAL.Entity.ProductGroup group)
        {
            using SqlCommand command = new() { Connection = connection };
            command.CommandText = @$"UPDATE ProductGroups
                                     SET Name = N'{group.Name}', Description = N'{group.Description}', Picture = N'{group.Picture}' WHERE Id = '{group.Id}'";
            try
            {
                command.ExecuteNonQuery();
                return true;
            }
            catch (Exception ex)
            {
                Title = ex.Message;
                return false;
            }
        }

        private bool DeleteProductGroup(DAL.Entity.ProductGroup group)
        {
            using SqlCommand command = new() { Connection = connection };
            command.CommandText = @$"UPDATE ProductGroups
                                     SET DeleteDt = CURRENT_TIMESTAMP WHERE Id = '{group.Id}'";
            try
            {
                command.ExecuteNonQuery();
                return true;
            }
            catch (Exception ex)
            {
                Title = ex.Message;
                return false;
            }
        }


        private void CreateGroup_Click(object sender, RoutedEventArgs e)
        {
            using SqlCommand command = new SqlCommand() { Connection = connection };
            command.CommandText =
                @"CREATE TABLE ProductGroups (
                    Id          UNIQUEIDENTIFIER NOT NULL PRIMARY KEY,
                    Name        NVARCHAR(50)     NOT NULL,
                    Description NTEXT            NOT NULL,
                    Picture     NVARCHAR(50)     NULL
                )";
            try
            {
                command.ExecuteNonQuery();
                MessageBox.Show("Table created!");
            }
            catch (Exception ex) { MessageBox.Show(ex.Message, "Create error", MessageBoxButton.OK, MessageBoxImage.Error); }
        }

        private void InsertGroup_Click(object sender, RoutedEventArgs e)
        {
            using SqlCommand command = new SqlCommand() { Connection = connection };
            command.CommandText =
                @"INSERT INTO ProductGroups (Id, Name, Description, Picture)
                VALUES
                    ('089015F4-31B5-4F2B-BA05-A813B5419285', N'Інструменти',     N'Ручний інструмент для побутового використання', N'tools.png' ),
                    ('A6D7858F-6B75-4C75-8A3D-C0B373828558', N'Офісні товари',   N'Декоративні товари для офісного облаштування', N'office.jpg' ),
                    ('DEF24080-00AA-440A-9690-3C9267243C43', N'Вироби зі скла',  N'Творчі вироби зі скла', N'glass.jpg' ),
                    ('2F9A22BC-43F4-4F73-BAB1-9801052D85A9', N'Вироби з дерева', N'Композиції та декоративні твори з деревини', N'wood.jpg' ),
                    ('D6D9783F-2182-469A-BD08-A24068BC2A23', N'Вироби з каменя', N'Корисні та декоративні вироби з натурального каменю', N'stone.jpg'
                )";
            try
            {
                command.ExecuteNonQuery();
                MessageBox.Show("Insert data in table!");
            }
            catch (Exception ex) { MessageBox.Show(ex.Message, "Insert error", MessageBoxButton.OK, MessageBoxImage.Error); }
        }

        private void GroupCount_Click(object sender, RoutedEventArgs e)
        {
            using SqlCommand command = new SqlCommand() { Connection = connection };
            command.CommandText = "SELECT COUNT(*) FROM ProductGroups WHERE Deletedt IS NULL";
            try
            {
                int count = Convert.ToInt32(command.ExecuteScalar());
                MessageBox.Show($"Table has {count} rows!");
            }
            catch (Exception ex) { MessageBox.Show(ex.Message, "Query error", MessageBoxButton.OK, MessageBoxImage.Error); }
        }

        private void ListViewItem_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (sender is ListViewItem item)  // паттерн matching
            {
                if (item.Content is DAL.Entity.ProductGroup group)
                {
                    CrudGroupsWindow window = new(group);
                    bool? dialogResult = window.ShowDialog();
                    if (dialogResult == false)
                    {
                        if (window.ProductGroup == null)  // удаление
                        {
                            if (DeleteProductGroup(group))
                            {
                                ProductGroups.Remove(group);
                                MessageBox.Show("Данные удалены!");
                            }
                            else MessageBox.Show("Проблемы с БД. Попробуйте действие позже.");
                        }
                        else MessageBox.Show("Действие отменено!");  // отмена
                    }
                    else if (dialogResult == true)  // сохранение
                    {
                        if (SaveProductGroup(group)) MessageBox.Show("Данные сохранены!");
                        else MessageBox.Show("Проблемы с БД. Попробуйте действие позже.");
                    }
                }
            }
        }
    }
}
