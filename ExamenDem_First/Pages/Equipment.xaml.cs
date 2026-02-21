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
            if (UserRole.CurrentUser == null || UserRole.CurrentUser.Id == 0)
            {
                user_role = "Гость";
                return;
            }

            var worker = db.Workers.FirstOrDefault(w => w.IdWorker == UserRole.CurrentUser.Id);

            if (worker == null)
            {
                user_role = "без должности";
                return;
            }

            var post = db.Posts.FirstOrDefault(p => p.IdPost == worker.IdPost);

            user_role = post != null ? post.TitlePost : "без должности";
        }




        public void outputInfo()
        {
            string number_room = "";
            List<Models.Equipment> filteredEquipment = new List<Models.Equipment>();

            if (user_role == "администратор бд" || user_role == "инженер")
            {
                filteredEquipment = db.Equipment.ToList();
            }
            else if (user_role == "заведующий" || user_role == "техник" || user_role == "лаборант")
            {
                var currentWorker = db.Workers.FirstOrDefault(w => w.IdWorker == UserRole.CurrentUser.Id);
                if (currentWorker != null)
                {
                    filteredEquipment = db.Equipment
                        .Where(eq => eq.IdWorker == UserRole.CurrentUser.Id || eq.IdOffices == currentWorker.IdOffices)
                        .ToList();
                }
            }
            else if (user_role == "Гость")
            {
                var aud = db.Audiences.FirstOrDefault(a => a.NumberAudience == "склад");
                int? audId = aud != null ? (int?)aud.IdAudience : null;

                var equipmentQuery = db.Equipment.Where(eq =>
                    ((eq.IdAudience == null || eq.IdAudience == audId) &&
                     eq.IdWorker == null && eq.IdOffices == null) ||
                    (eq.IdAudience == null && eq.IdWorker == null && eq.IdOffices == null));

                filteredEquipment = equipmentQuery.Distinct().ToList();
            }
            

            var audiences = db.Audiences.ToList();
            var offices = db.Offices.ToList();
            var workers = db.Workers.ToList();

            foreach (var eq in filteredEquipment)
            {
                if (eq.Photo == null)
                {
                    eq.Photo = "../media/stub.jpg";
                }

                eq.NumberAudience = audiences.Any(a => a.IdAudience == eq.IdAudience)
                    ? audiences.First(a => a.IdAudience == eq.IdAudience).NumberAudience
                    : "-";

                var office = offices.FirstOrDefault(o => o.IdOffice == eq.IdOffices);
                if (office != null)
                {
                    eq.OfficesString = office.Abbreviated;
                }
                else
                {
                    var worker = workers.FirstOrDefault(w => w.IdWorker == eq.IdWorker);
                    if (worker != null)
                    {
                        var workerOffice = offices.FirstOrDefault(o => o.IdOffice == worker.IdOffices);
                        eq.OfficesString = workerOffice?.Abbreviated ?? "-";
                    }
                    else
                    {
                        eq.OfficesString = "-";
                    }
                }

                eq.StatusVisibility = (user_role == "администратор бд" || user_role == "заведующий" || user_role == "инженер")
                    ? Visibility.Visible
                    : Visibility.Collapsed;

                if (user_role == "администратор бд" || user_role == "заведующий" || user_role == "инженер")
                {
                    int endYear = eq.DateTransferToCompanyBalance.Year + eq.StandardServiceLife;
                    if (endYear > DateTime.Now.Year)
                    {
                        eq.Status = $"Статус: годен до {endYear}";
                    }
                    else if (endYear == DateTime.Now.Year)
                    {
                        eq.Status = "Статус: истекает в текущем году";
                        eq.BushStatus = "#FFA500";
                    }
                    else
                    {
                        eq.Status = "Статус: истек";
                        eq.BushStatus = "#E32636";
                    }
                }
            }

            ListEquipment.ItemsSource = filteredEquipment;
        }



        private async void OnLoaded(object sender, RoutedEventArgs e)
        {
            SortWeight.SelectedIndex = 0;
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

        private void SortWeight_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if(SortWeight.SelectedValue != null)
            {
                var currentItems = ListEquipment.Items
                    .Cast<Models.Equipment>()
                    .ToList();
                var curreentItemsGlobal = ListEquipment.Items.Cast<Models.Equipment>().ToList();
                List<Models.Equipment> equipment = new List<Models.Equipment>();
                
                if (SortWeight.SelectedIndex.ToString() == "1")
                {

                    equipment = currentItems.OrderBy(e => e.WeightInKg).ToList();
                    ListEquipment.ItemsSource = equipment;

                }
                else if (SortWeight.SelectedIndex.ToString() == "2")
                {
                    equipment = currentItems.OrderByDescending(e => e.WeightInKg).ToList();
                    ListEquipment.ItemsSource = equipment;
                }
                else if(SortWeight.SelectedIndex.ToString() == "0")
                {
                    outputInfo();
                }
            }
        }

        private void Search_TextChanged(object sender, TextChangedEventArgs e)
        {
            if(Search.Text != null)
            {
                List<Models.Equipment> equipment = new List<Models.Equipment>();

                equipment = db.Equipment.ToList();
                var filtered = equipment.Where(e => e.TitleEquipment != null && e.TitleEquipment.ToLower().Contains(Search.Text.ToLower()));
                ListEquipment.ItemsSource = filtered;
            }
            else if (string.IsNullOrEmpty(Search.Text))
            {
                outputInfo();
            }
        }
    }
}
