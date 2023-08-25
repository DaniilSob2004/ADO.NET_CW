using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media.Imaging;

namespace first.Views
{
    public partial class CrudGroupsWindow : Window
    {
        public DAL.Entity.ProductGroup? ProductGroup { get; set; }  // ссылка на редактируемую группу
        private string name;
        private string description;
        private string picture;

        public CrudGroupsWindow(DAL.Entity.ProductGroup productGroup)
        {
            InitializeComponent();

            ProductGroup = productGroup;
            name = ProductGroup.Name;  // запоминаем начальные данные продукта
            description = ProductGroup.Description;
            picture = ProductGroup.Picture;

            DataContext = ProductGroup;
        }


        private bool DataValidation()
        {
            if (ProductGroup != null)
            {  // проверка валидности
                return !String.IsNullOrEmpty(ProductGroup.Name) &&
                       !String.IsNullOrEmpty(ProductGroup.Description) &&
                       (ProductGroup.Picture.EndsWith(".jpg") || ProductGroup.Picture.EndsWith(".png"));
            }
            return false;
        }

        private bool CheckChangedData()
        {
            if (ProductGroup != null)  // изменились ли данные
                return ProductGroup.Name != name || ProductGroup.Description != description || ProductGroup.Picture != picture;
            return false;
        }

        private void CancelChangeData()
        {
            if (ProductGroup != null)
            {
                textBoxName.Text = name;
                textBoxDescription.Text = description;
                textBoxPicture.Text = picture;
            }
        }


        private void BtnClose_Click(object sender, RoutedEventArgs e)
        {
            CancelChangeData();  // сбрасываем значение полей, которые были изменены
            DialogResult = false;
        }

        private void BtnSave_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
        }

        private void BtnDelete_Click(object sender, RoutedEventArgs e)
        {
            ProductGroup = null;
            Close();
        }

        private void TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (sender is TextBox)
            {
                if (DataValidation() && CheckChangedData())  // если данные корректны и изменены
                {
                    if (!btnSave.IsEnabled) btnSave.IsEnabled = true;
                }
                else
                {
                    if (btnSave.IsEnabled) btnSave.IsEnabled = false;
                }
            }
        }
    }
}
