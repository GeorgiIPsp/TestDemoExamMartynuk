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
                return; // Прерываем выполнение
            }

            bool loginFound = false;
            bool passwordCorrect = false;

            foreach (var users in db.Users.ToList())
            {
                if (login == users.Login)
                {
                    loginFound = true;

                    if (passwordd == users.Password)
                    {
                        passwordCorrect = true;

                        foreach (var workers in db.Workers.ToList())
                        {
                            if (users.IdWorker == workers.IdWorker)
                            {
                                UserRole.SetCurrentUser(workers.IdWorker);

                                var mainwindow = Application.Current.MainWindow as MainWindow;
                                if (mainwindow != null)
                                {
                                    mainwindow.MainFrame.Navigate(new Equipment());
                                }
                                break; // Выходим из цикла по работникам
                            }
                        }
                        break; // Выходим из цикла по пользователям
                    }
                    else
                    {
                        MessageBox.Show("Пароль неверный!");
                        break; // Некорректный пароль — прерываем
                    }
                }
            }

            // Если логин не найден ни в одной записи
            if (!loginFound)
            {
                MessageBox.Show("Логин не верный!");
            }
        }


        private void LoginButtonNotLogin_Click(object sender, RoutedEventArgs e)
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

