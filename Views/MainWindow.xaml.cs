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
        private DAL.DAO.ProductGroupDao productGroupDao = null!;
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
                productGroupDao = new DAL.DAO.ProductGroupDao(connection);  // создаём объект DAO для ProductGroup
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
            connection?.Close();  // закрываем соединение с БД
        }


        private void LoadGroups()
        {
            try
            {
                foreach (var group in productGroupDao.GetAll())  // получаем список продуктов из БД и добавляем в коллекцию
                {
                    ProductGroups.Add(group);
                }
            }
            catch (Exception ex) { MessageBox.Show(ex.Message, "Query error", MessageBoxButton.OK, MessageBoxImage.Error); }
        }

        private bool SaveProductGroup(DAL.Entity.ProductGroup group)
        {
            try
            {
                productGroupDao.Update(group);  // обновляем данные о группе
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
            try
            {
                productGroupDao.Delete(group);  // удаляем группу
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
            try
            {
                productGroupDao.CreateTable();
                MessageBox.Show("Table created!");
            }
            catch (Exception ex) { MessageBox.Show(ex.Message, "Create error", MessageBoxButton.OK, MessageBoxImage.Error); }
        }

        private void InsertGroup_Click(object sender, RoutedEventArgs e)
        {
            // добавляем несколько товаров
            try
            {
                DAL.Entity.ProductGroup newGroup = new DAL.Entity.ProductGroup { Id = Guid.NewGuid(), Name = "Інструменти", Description = "Ручний інструмент для побутового використання", Picture = "tools.png" };
                productGroupDao.Add(newGroup);
                ProductGroups.Add(newGroup);

                newGroup = new DAL.Entity.ProductGroup { Id = Guid.NewGuid(), Name = "Офісні товари", Description = "Декоративні товари для офісного облаштування", Picture = "office.jpg" };
                productGroupDao.Add(newGroup);
                ProductGroups.Add(newGroup);

                MessageBox.Show("Insert data in table!");
            }
            catch (Exception ex) { MessageBox.Show(ex.Message, "Insert error", MessageBoxButton.OK, MessageBoxImage.Error); }
        }

        private void GroupCount_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                MessageBox.Show($"Table has {productGroupDao.GetAllCount()} rows!");
            }
            catch (Exception ex) { MessageBox.Show(ex.Message, "Query error", MessageBoxButton.OK, MessageBoxImage.Error); }
        }

        private void ListViewItem_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (sender is ListViewItem item)  // паттерн matching
            {
                if (item.Content is DAL.Entity.ProductGroup group)
                {
                    CrudGroupsWindow window = new(group with { });
                    bool? dialogResult = window.ShowDialog();
                    if (dialogResult == false)
                    {
                        if (window.ProductGroup == null)  // удаление
                        {
                            if (MessageBox.Show("Подтверждаете удаление?", "Удаление", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
                            {
                                if (DeleteProductGroup(group))
                                {
                                    ProductGroups.Remove(group);
                                    MessageBox.Show("Данные удалены!");
                                }
                                else
                                    MessageBox.Show("Проблемы с БД. Попробуйте действие позже.");
                            }
                        }
                        else
                            MessageBox.Show("Действие отменено!");  // отмена
                    }
                    else if (dialogResult == true)  // сохранение
                    {
                        if (SaveProductGroup(window.ProductGroup!))
                        {
                            int index = ProductGroups.IndexOf(group);
                            ProductGroups.Remove(group);
                            ProductGroups.Insert(index, window.ProductGroup!);

                            MessageBox.Show("Данные сохранены!");
                        }
                        else
                            MessageBox.Show("Проблемы с БД. Попробуйте действие позже.");
                    }
                }
            }
        }

        private void AddGroupBtn_Click(object sender, RoutedEventArgs e)
        {
            DAL.Entity.ProductGroup newGroup = new() { Id = Guid.NewGuid() };
            CrudGroupsWindow window = new(newGroup);
            bool? result = window.ShowDialog();
            if (result ?? false)  // null-coalescence
            {
                try
                {
                    productGroupDao.Add(newGroup);
                    ProductGroups.Add(newGroup);
                    MessageBox.Show("Данные сохранены!");
                }
                catch (Exception) { MessageBox.Show("Проблемы с БД. Попробуйте действие позже."); }
            }
        }
    }
}
