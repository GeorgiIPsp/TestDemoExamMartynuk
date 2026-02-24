using ExamenDem_First.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.IO;
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
    public partial class Equipment : Page
    {
        public Visibility IsStatusVisible { get; set; }
        string user_role;
        private readonly ConferencesDataBaseContext db = new ConferencesDataBaseContext();
        public static Equipment CurrentInstance { get; private set; }
        public Equipment()
        {
            InitializeComponent();
            Loaded += OnLoaded;
            FindRole();
            CurrentInstance = this;
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
        public static void RefreshList()
        {
            CurrentInstance?.outputInfo();
        }
        public void outputInfo()
        {
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
                FilterPodrazdelenia.Visibility = Visibility.Hidden;
                SortWeight.Visibility = Visibility.Hidden;
                Search.Visibility = Visibility.Hidden;
                AddEquipment.Visibility = Visibility.Hidden;
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

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            SortWeight.SelectedIndex = 0;

            var offices = db.Offices.ToList();
            var officesWithAll = new List<Office>();

            officesWithAll.Add(new Office { IdOffice = 0, Abbreviated = "Все подразделения" });
            officesWithAll.AddRange(offices);

            FilterPodrazdelenia.ItemsSource = officesWithAll;
            FilterPodrazdelenia.DisplayMemberPath = "Abbreviated";
            FilterPodrazdelenia.SelectedValuePath = "IdOffice";
            FilterPodrazdelenia.SelectedIndex = 0;
            FilterPodrazdelenia.SelectionChanged += FilterPodrazdelenia_SelectionChanged;
            SortWeight.SelectionChanged += SortWeight_SelectionChanged;
            Search.TextChanged += Search_TextChanged;

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

            outputInfo();
        }

        private void FilterPodrazdelenia_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ApplyFilters();
        }

        private void SortWeight_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ApplyFilters();
        }

        private void Search_TextChanged(object sender, TextChangedEventArgs e)
        {
            ApplyFilters();
        }

        private void ApplyFilters()
        {
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
                FilterPodrazdelenia.Visibility = Visibility.Hidden;
                SortWeight.Visibility = Visibility.Hidden;
                Search.Visibility = Visibility.Hidden;
                AddEquipment.Visibility = Visibility.Hidden;
                filteredEquipment = equipmentQuery.Distinct().ToList();
            }

            var filtered = filteredEquipment.AsEnumerable();

            if (FilterPodrazdelenia.SelectedValue != null && Convert.ToInt32(FilterPodrazdelenia.SelectedValue) != 0)
            {
                int selectedOfficeId = Convert.ToInt32(FilterPodrazdelenia.SelectedValue);
                filtered = filtered.Where(eq => eq.IdOffices == selectedOfficeId);
            }

            if (!string.IsNullOrEmpty(Search.Text))
            {
                filtered = filtered.Where(eq => eq.TitleEquipment != null &&
                    eq.TitleEquipment.ToLower().Contains(Search.Text.ToLower()));
            }

            if (SortWeight.SelectedIndex == 1)
            {
                filtered = filtered.OrderBy(eq => eq.WeightInKg);
            }
            else if (SortWeight.SelectedIndex == 2)
            {
                filtered = filtered.OrderByDescending(eq => eq.WeightInKg);
            }

            var resultList = filtered.ToList();

            var audiences = db.Audiences.ToList();
            var offices = db.Offices.ToList();
            var workers = db.Workers.ToList();

            foreach (var eq in resultList)
            {
                if (eq.Photo == null || !File.Exists(eq.Photo))
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
            ListEquipment.ItemsSource = resultList;
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

        private void AddEquipment_Click(object sender, RoutedEventArgs e)
        {
            EditOrAddWindow window = new EditOrAddWindow(-1 , user_role);
            window.ShowDialog();
        }

        private void ListEquipment_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (user_role != "Гость")
            {
                if (ListEquipment.SelectedItem != null)
                {
                    if (ListEquipment.SelectedItem is Models.Equipment selectedItem)
                    {
                        int idselected = selectedItem.IdEquipment;
                        EditOrAddWindow window = new EditOrAddWindow(idselected, user_role);
                        window.ShowDialog();
                    }
                }
            }
           
        }
    }
}