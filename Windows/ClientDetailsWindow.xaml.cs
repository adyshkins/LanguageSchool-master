using LanguageSchool.EF;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace LanguageSchool.Windows
{
    /// <summary>
    /// Логика взаимодействия для ClientDetailsWindow.xaml
    /// </summary>
    public partial class ClientDetailsWindow : Window, INotifyPropertyChanged
    {
        private bool isEdit;
        private LanguageContext context = new LanguageContext();

        public event PropertyChangedEventHandler PropertyChanged;


        public ObservableCollection<Tag> AddedTags { get; }
             = new ObservableCollection<Tag>();




        private byte[] photo;


        /// <summary>
        /// Фото клиента для привязки к интерфейсу
        /// </summary>
        public byte[] Photo
        {
            get => photo;
            set
            {
                photo = value;

                //Обновить привязанный ImageView
                PropertyChanged?.Invoke(this,
                    new PropertyChangedEventArgs("Photo"));
            }
        }
        public Client Client { get; }
        public ClientDetailsWindow(int clientId = -1)
        {
            isEdit = clientId != -1;

            if(isEdit)
            {
                Client = context.Client.Find(clientId);
            }
            else
            {
                Client = new Client();
            }

            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            cmbGenders.ItemsSource = context.Gender.ToList();
     

            cmbTags.ItemsSource = context.Tag.ToList();
            cmbTags.SelectedIndex = 0;

            if(isEdit)
            {
                tbxId.Text = Client.ClientId.ToString();
                Photo = Client.Photo;
                foreach (var tag in Client.Tag)
                    AddedTags.Add(tag);
            }
            else
            {
                cmbGenders.SelectedIndex = 0;
                tbId.Visibility = Visibility.Collapsed;
                tbxId.Visibility = Visibility.Collapsed;
                dpBirthday.SelectedDate = DateTime.Now;
            }
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            context.Dispose();

        }

        private void btnCancelClick(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }

        private void btnSaveClick(object sender, RoutedEventArgs e)
        {

            //Проверить и собрать все возможные ошибки

            string errors = String.Empty;

            string error = CheckFio(Client.Name, "Имя");
            if(error != null)
            {
                errors += error;
            }

            error = CheckFio(Client.LastName, "Фамилия");
            if (error != null)
            {
                errors += "\n" + error;
            }

            error = CheckFio(Client.Patronymic, "Отчество");
            if (error != null)
            {
                errors += "\n" + error;
            }
            Client.Email = Client.Email?.Trim();
            error = CheckEmail(Client.Email);
            if (error != null)
            {
                errors += "\n" + error;
            }

            error = CheckPhone(Client.Phone?.Trim());
            if (error != null)
            {
                errors += "\n" + error;
            }


            //Если есть ошибки показать их пользователю
            if(errors != String.Empty)
            {
                MessageBox.Show(errors);
                return;
            }


            if(isEdit)
            {
                context.Entry(Client).State
                       = System.Data.Entity.EntityState.Modified;
            }
            else
            {
                Client.RegistrationDate = DateTime.Now;
                context.Client.Add(Client);
            }

            try
            {
                context.SaveChanges();
                MessageBox.Show("Клиент сохранен");
                DialogResult = true;
            }
            catch(Exception ex)
            {
                MessageBox.Show("Ошибка: " + ex.Message);
            }

        }


        private string CheckFio(string str, string part)
        {
            if(String.IsNullOrWhiteSpace(str))
            {
                return part + " - поле обязательное";
            }

            if(str.Length > 50)
            {
                return part + " не может быть длинее 50 символов";
            }

            var allowedChars = new List<char>()
            {
                '-',
                ' '
            };


            if(str.Any(c => !Char.IsLetter(c) &&
                        !allowedChars.Contains(c)))
            {
                return part + " может содержать содержать " +
                    "только буквы, пробел и дефис";
            }
            else
            {
                return null;
            }
        }

        private string CheckPhone(string str)
        {

            if (String.IsNullOrWhiteSpace(str))
            {
                return "Телефон - поле обязательное";
            }


            var allowedChars = new List<char>()
            {
                '-',
                '+',
                ' ',
                '(',
                ')'
            };


            if (str.Any(c => !Char.IsDigit(c) &&
                         !allowedChars.Contains(c)))
            {
                return "Телефон может содержать " +
                    "только цифры, пробел, плюс, минус," +
                    " (, ) и пробел";
            }
            else
            {
                return null;
            }
        }

        public string CheckEmail(string str)
        {

            if (String.IsNullOrWhiteSpace(str))
            {
                return "Email - поле обязательное";
            }

            Regex regex = new Regex(@"\w+@\w+\.\w+");

            if(regex.IsMatch(str))
            {
                return null;
            }
            else
            {
                return "Неправильный формат Email";
            }
        }

        private void btnChoosePhotoClick(object sender, RoutedEventArgs e)
        {
            var dialog = new OpenFileDialog();

            dialog.Filter = "Image Files | *jpg; *jpeg; *png; *bmp;";

            if(dialog.ShowDialog() == true)
            {
                Photo = File.ReadAllBytes(dialog.FileName);

                if (Photo.Length > 1024 * 1024 * 2)
                {
                    MessageBox.Show("Максимальный размер " +
                        "фотографии не должен привышать 2 мб");
                    Photo = Client.Photo;
                }
                else
                { 
                    Client.Photo = Photo;
                }
            }
        }

        private void btnAddTagClick(object sender, RoutedEventArgs e)
        {
              var tag = cmbTags.SelectedItem as Tag;

            if(!AddedTags.Contains(tag))
            {
                AddedTags.Add(tag);
                Client.Tag.Add(tag);
            }
        }

        private void btnRemoveTagClick(object sender, RoutedEventArgs e)
        {
            var tag = (sender as Button).DataContext as Tag;

            AddedTags.Remove(tag);
            Client.Tag.Remove(tag);
        }
    }
}
