using LanguageSchool.EF;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
using System.Windows.Shapes;

namespace LanguageSchool.Windows
{
    /// <summary>
    /// Логика взаимодействия для ClientServiceListWindow.xaml
    /// </summary>
    public partial class ClientServiceListWindow : Window
    {

        private LanguageContext context = new LanguageContext();
        public ObservableCollection<ClientService> ClientServices { get; }

        public ClientServiceListWindow(int clientId)
        {

            var client = context.Client.Find(clientId);
            ClientServices = new ObservableCollection<ClientService>(
                client.ClientService);


            InitializeComponent();

            if (ClientServices.Count == 0)
            {
                lvClientServices.Visibility = Visibility.Hidden;
            }
        }

        private void btnBackClick(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            context.Dispose();
        }
    }
}
