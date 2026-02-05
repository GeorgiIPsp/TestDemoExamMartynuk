using ExamenDem_First.Models;
using Microsoft.EntityFrameworkCore;
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
    /// Логика взаимодействия для Equipment.xaml
    /// </summary>
    public partial class Equipment : Page
    {
        public Visibility IsStatusVisible { get; set; }
        string user_role;
        private readonly ConferencesDataBaseContext db = new ConferencesDataBaseContext();
        public Equipment()
        {
            InitializeComponent();
            Loaded += OnLoaded;
            FindRole();
        }

        private void FindRole()
        {
            // Проверка на отсутствие авторизованного пользователя
            if (UserRole.CurrentUser == null || UserRole.CurrentUser.Id == 0)
            {
                user_role = "Гость";
                return;
            }

            // Флаг, чтобы прервать поиск после нахождения
            bool found = false;

            // Перебираем всех работников
            foreach (var work in db.Workers.ToList())
            {
                // Проверяем, совпадает ли IdWorker с Id текущего пользователя
                if (work.IdWorker == UserRole.CurrentUser.Id)
                {
                    // Теперь ищем должность этого работника
                    foreach (var post in db.Posts.ToList())
                    {
                        if (work.IdPost == post.IdPost)
                        {
                            user_role = post.TitlePost;
                            found = true;
                            break; // Выходим из цикла по должностям
                        }
                    }

                    if (found) break; // Выходим из цикла по работникам
                }
            }

            // Если роль не найдена — устанавливаем значение по умолчанию
            if (!found)
            {
                user_role = "без должности";
            }
        }



        public void outputInfo()
        {
            string number_room = "";

            // 1. Фильтруем оборудование в зависимости от роли
            List<Models.Equipment> filteredEquipment = new List<Models.Equipment>();

            if (user_role == "администратор бд" || user_role == "инженер")
            {
                // Администратор видит всё
                filteredEquipment = db.Equipment.ToList();
            }
            else if (user_role == "заведующий" || user_role == "техник" || user_role == "лаборант")
            {
                // Заведующий видит только своё оборудование в своём офисе
                var currentWorker = db.Workers
                    .FirstOrDefault(w => w.IdWorker == UserRole.CurrentUser.Id);

                if (currentWorker == null)
                {
                    // ...
                }
                else
                {
                    filteredEquipment = db.Equipment
                        .Where(eq => eq.IdWorker == UserRole.CurrentUser.Id ||
                                   eq.IdOffices == currentWorker.IdOffices)
                        .ToList();
                }
            }
            else if (user_role == "Гость")
            {
                // Сохраняем ваши циклы, но исправляем логику
                foreach (var eq in db.Equipment.ToList())
                {
                    foreach (var au in db.Audiences.ToList())
                    {
                        
                        if ((eq.IdAudience == null || eq.IdAudience == au.IdAudience) &&
                            au.NumberAudience == "склад" &&
                            eq.IdWorker == null &&
                            eq.IdOffices == null)
                        {
                            // Чтобы избежать дублирования, проверяем, не добавили ли уже эту запись
                            if (!filteredEquipment.Contains(eq))
                            {
                                filteredEquipment.Add(eq);
                            }
                        }
                    }

                    // Отдельно обрабатываем случай, когда у оборудования нет аудитории (IdAudience == null)
                    // и при этом IdWorker и IdOffices равны null
                    if (eq.IdAudience == null && eq.IdWorker == null && eq.IdOffices == null)
                    {
                        if (!filteredEquipment.Contains(eq))
                        {
                            filteredEquipment.Add(eq);
                        }
                    }
                }
            }
            else if (user_role == "без должности")
            {
                MessageBox.Show("Без");
            }

            // 2. Обрабатываем отфильтрованные записи (добавляем фото, аудитории и т. д.)
            foreach (var eq in filteredEquipment)
            {
                if (eq.Photo == null)
                {
                    eq.Photo = "../media/stub.jpg";
                }

                foreach (var ad in db.Audiences.ToList())
                {
                    if (eq.IdAudience == ad.IdAudience)
                    {
                        eq.NumberAudience = ad.NumberAudience;
                    }
                    else if (eq.IdAudience == null)
                    {
                        eq.NumberAudience = "-";
                    }
                }

                bool found = false;
                foreach (var off in db.Offices.ToList())
                {
                    if (eq.IdOffices != null)
                    {
                        if (eq.IdOffices == off.IdOffice)
                        {
                            eq.OfficesString = off.Abbreviated;
                            found = true;
                        }
                    }
                    else 
                    {
                        foreach(var work in db.Workers.ToList())
                        {
                            if(eq.IdWorker == work.IdWorker)
                            {
                                if(work.IdOffices == off.IdOffice)
                                {
                                    eq.OfficesString = off.Abbreviated;
                                    found = true;
                                }
                            }
                        }
                    }
                    if(found == false)
                    {
                        eq.OfficesString = "-";
                    }
                    
                }
                

                // 3. Устанавливаем видимость StatusTextBlock для каждой строки
                eq.StatusVisibility = (user_role == "администратор бд" || user_role == "заведующий" || user_role ==  "инженер")
                    ? Visibility.Visible
                    : Visibility.Collapsed;
                if (user_role == "администратор бд" || user_role == "заведующий" || user_role == "инженер")
                {
                    if ((eq.DateTransferToCompanyBalance.Year + eq.StandardServiceLife) > DateTime.Now.Year)
                    {
                        eq.Status = $"Статус: годен до {eq.DateTransferToCompanyBalance.Year + eq.StandardServiceLife}";

                    }
                    else if ((eq.DateTransferToCompanyBalance.Year + eq.StandardServiceLife) == DateTime.Now.Year)
                    {
                        eq.Status = $"Статус: истекает в текущем году";
                        eq.BushStatus = "#FFA500";
                    }
                    else if ((eq.DateTransferToCompanyBalance.Year + eq.StandardServiceLife) < DateTime.Now.Year)
                    {
                        eq.Status = $"Статус: истек";
                        eq.BushStatus = "#E32636";
                    }
                }
            }

            ListEquipment.ItemsSource = filteredEquipment;
        }


        private async void OnLoaded(object sender, RoutedEventArgs e)
        {
           
                outputInfo();
           
            foreach (var work in db.Workers.ToList())
            {
                string fio = work.LastName + " " + work.Name.ToCharArray()[0] + "." + work.Patronymic.ToCharArray()[0] + ".";
                if (UserRole.CurrentUser == null)
                {
                    fioWorker.Text = "Гость";
                    return;
                }
                if (work.IdWorker == UserRole.CurrentUser.Id)
                {
                    fioWorker.Text = fio;


                }
            }

        }
        
        private void ExitToLogin_Click(object sender, RoutedEventArgs e)
        {
            UserRole.CurrentUser = null;
            var mainwindow = Application.Current.MainWindow as MainWindow;
            if (mainwindow != null)
            {
                mainwindow.MainFrame.Navigate(new Login());
            }
        }
    }
}
