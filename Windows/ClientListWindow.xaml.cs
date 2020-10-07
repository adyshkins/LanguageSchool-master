using LanguageSchool.EF;
using LanguageSchool.Windows;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace LanguageSchool
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class ClientListWindow : Window, INotifyPropertyChanged
    {

        /// <summary>
        /// Клиенты на текущей странице
        /// </summary>
        public ObservableCollection<vw_ClientDetails> ClientsPage { get; }
                = new ObservableCollection<vw_ClientDetails>();

        public event PropertyChangedEventHandler PropertyChanged;


        private int currentPage = 1;

        /// <summary>
        /// Номер текущей страницы
        /// </summary>
        public int CurrentPage
        {
            get => currentPage;
            set
            {
                currentPage = value;
                PropertyChanged?.Invoke(this,
                    new PropertyChangedEventArgs("CurrentPage"));
            }
        }

        private int clientAmountOnPage = -1;

        /// <summary>
        /// Количество клиентов на страницу 
        /// Если значеник = -1, то значит выводятья все доступные клиенты
        /// </summary>
        public int ClientAmountOnPage
        {
            get => clientAmountOnPage;
            set
            {
                clientAmountOnPage = value;
                PropertyChanged?.Invoke(this,
                    new PropertyChangedEventArgs("ClientAmountOnPage"));
            }
        }

        private int allClientCount;
        public int AllClientCount
        {
            get => allClientCount;
            set
            {
                allClientCount = value;
                PropertyChanged?.Invoke(this,
                    new PropertyChangedEventArgs("AllClientCount"));
            }
        }


        private int clientCount;
        public int ClientCount
        {
            get => clientCount;
            set
            {
                clientCount = value;
                PropertyChanged?.Invoke(this,
                    new PropertyChangedEventArgs("ClientCount"));
            }
        }

        public ClientListWindow()
        {

   

            InitializeComponent();

        
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {

            using (var context = new LanguageContext())
            {
                cmbSorts.ItemsSource = new List<string>()
                {
                    "ФИО",
                    "Количество посещений",
                    "Дата поселднего посещения"
                };
                cmbSorts.SelectedIndex = 0;


                cmbAmounts.ItemsSource = new List<string>()
                 {
                "Все",
                "10",
                "50",
                "200"
                };
                cmbAmounts.SelectedIndex = 0;


                var genders = context.Gender
                    .Select(g => g.Name)
                    .ToList();
                genders.Insert(0, "Все");
                cmbGenders.ItemsSource = genders;
                cmbGenders.SelectedIndex = 0;
            }

            UpdatePage();
        }


        private IEnumerable<vw_ClientDetails> FilterClients(IEnumerable<vw_ClientDetails> query)
        {
            query = query
                .Where(c => c.Email.Contains(tbxEmail.Text))
                .Where(c => c.ФИО.Contains(tbxFIO.Text))
                .Where(c => c.Телефон.Contains(tbxPhone.Text));

            if(cmbGenders.SelectedIndex > 0)
            {
                var selectedGender = cmbGenders.SelectedItem as string;
                query = query.Where(c => c.Пол == selectedGender);
            }

            if(chBirthday.IsChecked == true)
            {
                var currentMonth = DateTime.Now.Month;
                query = query.Where(c => c.ДатаРождения.Month == currentMonth);
            }

            return query;
        }

        private IEnumerable<vw_ClientDetails> SortClients(LanguageContext context)
        {
            switch(cmbSorts.SelectedIndex)
            {
                case 0: return context.vw_ClientDetails
                        .OrderBy(c => c.ФИО);
                case 1: return context.vw_ClientDetails
                        .OrderByDescending(c => c.КоличествоПосещений);
                case 2: return context.vw_ClientDetails
                        .OrderByDescending(c => c.ДатаПоследнегоПосещения);
                default: return null;
            }  
        }

        /// <summary>
        /// Применить сортировку, фильтрацию и вывести на стрницу
        /// </summary>
        private void UpdatePage()
        {
            using(var context = new LanguageContext())
            {
                AllClientCount = context.vw_ClientDetails.Count();

                var query = SortClients(context);
                query = FilterClients(query);


                //Если показать всех клиентов
                if(cmbAmounts.SelectedIndex == 0)
                {
                    ClientAmountOnPage = AllClientCount;
                }

                int start = (CurrentPage - 1) * ClientAmountOnPage;

                if(start > query.Count())
                {
                    start = 0;
                    CurrentPage = 1;
                }


                ClientsPage.Clear();

                foreach(var client in query
                                    .Skip(start)
                                    .Take(ClientAmountOnPage))
                {
                    ClientsPage.Add(client);
                }

                ClientCount = ClientsPage.Count;
            }
        }

        private void btnPreviousPageClick(object sender, RoutedEventArgs e)
        {
            if(CurrentPage > 1)
            {
                CurrentPage--;
                UpdatePage();
            }
        }

        private void btnNextPageClick(object sender, RoutedEventArgs e)
        {
            if (CurrentPage * clientAmountOnPage < AllClientCount)
            {
                CurrentPage++;
                UpdatePage();
            }
        }

        private void CmbAmounts_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if(cmbAmounts.SelectedIndex == 0)
            {
                ClientAmountOnPage = -1;
            }
            else
            {
                ClientAmountOnPage = Int32.Parse(
                    cmbAmounts.SelectedItem.ToString());
            }

            UpdatePage();
        }

   


        private void FilterChanged(object sender, EventArgs args)
        {
            UpdatePage();
        }

        private void LvClients_KeyUp(object sender, KeyEventArgs e)
        {
            if(e.Key != Key.Delete)
            {
                return;
            }

            var clientDetails = lvClients.SelectedItem as vw_ClientDetails;

            if(clientDetails == null)
            {
                MessageBox.Show("Выберите клиента");
                return;
            }



            using(var context = new LanguageContext())
            {
                var client = context.Client.Find(clientDetails.Идентификатор);

                if(client.ClientService.Count > 0)
                {
                    MessageBox.Show("У клиента есть записи о посещении\n" +
                        "Удаление запрещено");
                    return;
                }

                var result = MessageBox.Show(
                    "Вы хотите удалить клиента?",
                    "Удаление клиента",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Warning);

                if(result != MessageBoxResult.Yes)
                {
                    return;
                }

                context.Client.Remove(client);
                try
                {
                    context.SaveChanges();
                    UpdatePage();
                    MessageBox.Show("Клиента удален");
                }
                catch(Exception ex)
                {
                    MessageBox.Show("Ошибка: " + ex.Message);
                }
         
            }
        }

        private void btnAddClientClick(object sender, RoutedEventArgs e)
        {
            new ClientDetailsWindow().ShowDialog();

            UpdatePage();
        }

        private void btnEditClientClick(object sender, RoutedEventArgs e)
        {
            var clientView = lvClients.SelectedItem as vw_ClientDetails;

            if(clientView  == null)
            {
                MessageBox.Show("Выберите клиента");
                return;
            }


            new ClientDetailsWindow(clientView.Идентификатор)
                .ShowDialog();

            UpdatePage();
        }

        private void btnClientServiceListClick(object sender, RoutedEventArgs e)
        {
            var clientView = lvClients.SelectedItem as vw_ClientDetails;

            if(clientView == null)
            {
                MessageBox.Show("Выберите клиента");
                return;
            }

            new ClientServiceListWindow(clientView.Идентификатор)
                .ShowDialog();
        }
    }
}
