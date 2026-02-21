using ExamenDem_First.Models;
using System;
using System.Collections.Generic;
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

namespace ExamenDem_First
{
    /// <summary>
    /// Логика взаимодействия для Login.xaml
    /// </summary>
    public partial class Login : Page
    {
        private readonly ConferencesDataBaseContext db = new ConferencesDataBaseContext();

        public Login()
        {
            InitializeComponent();
        }
        private void LoginButton_Click(object sender, RoutedEventArgs e)
        {
            string login = LoginBox.Text;
            string passwordd = PasswordBox.Password;

            if (string.IsNullOrEmpty(login) || string.IsNullOrEmpty(passwordd))
            {
                MessageBox.Show("Логин или пароль не может быть пустыми!");
                return;
            }

            var user = db.Users.FirstOrDefault(u => u.Login == login);

            if (user == null)
            {
                MessageBox.Show("Логин не верный!");
                return;
            }

            if (user.Password != passwordd)
            {
                MessageBox.Show("Пароль неверный!");
                return;
            }

            var worker = db.Workers.FirstOrDefault(w => w.IdWorker == user.IdWorker);

            if (worker != null)
            {
                UserRole.SetCurrentUser(worker.IdWorker);

                var mainwindow = Application.Current.MainWindow as MainWindow;
                if (mainwindow != null)
                {
                    mainwindow.MainFrame.Navigate(new Equipment());
                }
            }
        }



        private void LoginButtonGuest_Click(object sender, RoutedEventArgs e)
        {
            UserRole.CurrentUser = null;
            var mainwindow = Application.Current.MainWindow as MainWindow;
            if(mainwindow != null)
            {
                mainwindow.MainFrame.Navigate(new Equipment());
            }
        }
    }
}

