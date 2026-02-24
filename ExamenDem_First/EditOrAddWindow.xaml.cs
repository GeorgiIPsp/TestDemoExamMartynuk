using ExamenDem_First.Models;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;

namespace ExamenDem_First
{
    public partial class EditOrAddWindow : Window
    {
        private readonly ConferencesDataBaseContext db = new ConferencesDataBaseContext();
        private static bool isWindowOpen = false;
        int idEq = 0;
        string role_user = null;
        Models.Equipment currentEquipment = null;
        string selectedPhotoPath = null;

        public EditOrAddWindow(int idEquipment, string user_role)
        {
            if (isWindowOpen)
            {
                MessageBox.Show("Окно редактирования уже открыто!", "Предупреждение", MessageBoxButton.OK, MessageBoxImage.Warning);
                this.Close();
                return;
            }

            InitializeComponent();
            Loaded += OnLoaded;
            idEq = idEquipment;
            role_user = user_role;
            isWindowOpen = true;
            this.Closed += (s, e) => isWindowOpen = false;
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            if (idEq == -1)
            {
                this.Title = "Добавление оборудования";
                AddOrEditButton.Content = "Добавить";
                DeleteButton.Visibility = Visibility.Hidden;

                if (role_user == "администратор бд")
                {
                    var offices = db.Offices.ToList();
                    var officesWithAll = new List<Office>();
                    officesWithAll.Add(new Office { IdOffice = 0, Abbreviated = "" });
                    officesWithAll.AddRange(offices);
                    ComboBoxOffice.ItemsSource = officesWithAll;
                    ComboBoxOffice.DisplayMemberPath = "Abbreviated";
                    ComboBoxOffice.SelectedValuePath = "IdOffice";
                    ComboBoxOffice.SelectedIndex = 0;

                    var audiences = db.Audiences.ToList();
                    var audi = new List<Audience>();
                    audi.Add(new Audience { IdAudience = 0, NumberAudience = "", IdFloor = 1 });
                    audi.AddRange(audiences);
                    Audiences.ItemsSource = audi;
                    Audiences.DisplayMemberPath = "NumberAudience";
                    Audiences.SelectedValuePath = "IdAudience";
                    Audiences.SelectedIndex = 0;
                }
                else if (role_user == "заведующий")
                {
                    var currentWorker = db.Workers.FirstOrDefault(w => w.IdWorker == UserRole.CurrentUser.Id);
                    if (currentWorker != null)
                    {
                        var offices = db.Offices.Where(o => o.IdOffice == currentWorker.IdOffices).ToList();
                        var officesWithAll = new List<Office>();
                        officesWithAll.Add(new Office { IdOffice = 0, Abbreviated = "" });
                        officesWithAll.AddRange(offices);
                        ComboBoxOffice.ItemsSource = officesWithAll;
                        ComboBoxOffice.DisplayMemberPath = "Abbreviated";
                        ComboBoxOffice.SelectedValuePath = "IdOffice";
                        ComboBoxOffice.SelectedValue = currentWorker.IdOffices;
                        ComboBoxOffice.IsEnabled = false;

                        var officeAudienceIds = db.OfficesAudiences
                            .Where(oa => oa.IdOffice == currentWorker.IdOffices)
                            .Select(oa => oa.IdAudience)
                            .ToList();

                        var officeAudiences = db.Audiences
                            .Where(a => officeAudienceIds.Contains(a.IdAudience))
                            .ToList();

                        var audi = new List<Audience>();
                        audi.Add(new Audience { IdAudience = 0, NumberAudience = "", IdFloor = 1 });
                        audi.AddRange(officeAudiences);
                        Audiences.ItemsSource = audi;
                        Audiences.DisplayMemberPath = "NumberAudience";
                        Audiences.SelectedValuePath = "IdAudience";
                        Audiences.SelectedIndex = 0;
                    }
                }
            }
            else
            {
                this.Title = "Редактирование оборудования";
                AddOrEditButton.Content = "Изменить";

                currentEquipment = db.Equipment.FirstOrDefault(e => e.IdEquipment == idEq);

                if (currentEquipment != null)
                {
                    TitleEquipment.Text = currentEquipment.TitleEquipment;
                    InventNumber.Text = currentEquipment.InventoryNumber;
                    WidhtEquipment.Text = currentEquipment.WeightInKg.ToString();
                    DateRegist.Text = currentEquipment.DateTransferToCompanyBalance.ToString();
                    YearWork.Text = currentEquipment.StandardServiceLife.ToString();
                    Description.Text = currentEquipment.Description;

                    if (!string.IsNullOrEmpty(currentEquipment.Photo) && File.Exists(currentEquipment.Photo))
                    {
                        try
                        {
                            EquipmentImage.Source = new System.Windows.Media.Imaging.BitmapImage(new Uri(currentEquipment.Photo));
                            selectedPhotoPath = currentEquipment.Photo;
                        }
                        catch { }
                    }

                    if (role_user == "администратор бд")
                    {
                        var offices = db.Offices.ToList();
                        var officesWithAll = new List<Office>();
                        officesWithAll.Add(new Office { IdOffice = 0, Abbreviated = "" });
                        officesWithAll.AddRange(offices);
                        ComboBoxOffice.ItemsSource = officesWithAll;
                        ComboBoxOffice.DisplayMemberPath = "Abbreviated";
                        ComboBoxOffice.SelectedValuePath = "IdOffice";

                        if (currentEquipment.IdOffices.HasValue)
                        {
                            ComboBoxOffice.SelectedValue = currentEquipment.IdOffices;
                        }
                        else
                        {
                            ComboBoxOffice.SelectedIndex = 0;
                        }

                        var audiences = db.Audiences.ToList();
                        var audi = new List<Audience>();
                        audi.Add(new Audience { IdAudience = 0, NumberAudience = "", IdFloor = 1 });
                        audi.AddRange(audiences);
                        Audiences.ItemsSource = audi;
                        Audiences.DisplayMemberPath = "NumberAudience";
                        Audiences.SelectedValuePath = "IdAudience";

                        if (currentEquipment.IdAudience.HasValue)
                        {
                            Audiences.SelectedValue = currentEquipment.IdAudience;
                        }
                        else
                        {
                            Audiences.SelectedIndex = 0;
                        }

                        
                        if (currentEquipment != null)
                        {
                            var audience = db.Audiences.FirstOrDefault(a => a.IdAudience == currentEquipment.IdAudience);
                            bool isOnStock = audience != null && audience.NumberAudience != null && audience.NumberAudience.ToLower() == "склад";

                            int endYear = currentEquipment.DateTransferToCompanyBalance.Year + currentEquipment.StandardServiceLife;
                            bool isExpired = endYear < DateTime.Now.Year;

                            if (isOnStock && isExpired)
                            {
                                DeleteButton.Visibility = Visibility.Visible;
                                DeleteButton.IsEnabled = true;
                            }
                            else
                            {
                                DeleteButton.Visibility = Visibility.Collapsed;
                            }
                        }
                        else
                        {
                            DeleteButton.Visibility = Visibility.Collapsed;
                        }
                    }
                    else if (role_user == "заведующий")
                    {
                        DeleteButton.Visibility = Visibility.Collapsed;

                        var currentWorker = db.Workers.FirstOrDefault(w => w.IdWorker == UserRole.CurrentUser.Id);
                        if (currentWorker != null)
                        {
                            var offices = db.Offices.Where(o => o.IdOffice == currentWorker.IdOffices).ToList();
                            var officesWithAll = new List<Office>();
                            officesWithAll.Add(new Office { IdOffice = 0, Abbreviated = "" });
                            officesWithAll.AddRange(offices);
                            ComboBoxOffice.ItemsSource = officesWithAll;
                            ComboBoxOffice.DisplayMemberPath = "Abbreviated";
                            ComboBoxOffice.SelectedValuePath = "IdOffice";
                            ComboBoxOffice.SelectedValue = currentWorker.IdOffices;
                            ComboBoxOffice.IsEnabled = false;

                            var officeAudienceIds = db.OfficesAudiences
                                .Where(oa => oa.IdOffice == currentWorker.IdOffices)
                                .Select(oa => oa.IdAudience)
                                .ToList();

                            var officeAudiences = db.Audiences
                                .Where(a => officeAudienceIds.Contains(a.IdAudience))
                                .ToList();

                            var audi = new List<Audience>();
                            audi.Add(new Audience { IdAudience = 0, NumberAudience = "", IdFloor = 1 });
                            audi.AddRange(officeAudiences);
                            Audiences.ItemsSource = audi;
                            Audiences.DisplayMemberPath = "NumberAudience";
                            Audiences.SelectedValuePath = "IdAudience";

                            if (currentEquipment.IdAudience.HasValue)
                            {
                                Audiences.SelectedValue = currentEquipment.IdAudience;
                            }
                            else
                            {
                                Audiences.SelectedIndex = 0;
                            }
                        }
                    }
                }

                if (role_user == "техник" || role_user == "инженер")
                {
                    TitleEquipment.IsReadOnly = true;
                    InventNumber.IsReadOnly = true;
                    WidhtEquipment.IsReadOnly = true;
                    DateRegist.IsEnabled = false;
                    YearWork.IsReadOnly = true;
                    Description.IsReadOnly = true;
                    ComboBoxOffice.IsEnabled = false;
                    Audiences.IsEnabled = false;
                    SelectPhotoButton.IsEnabled = false;
                    AddOrEditButton.Visibility = System.Windows.Visibility.Collapsed;
                    DeleteButton.Visibility = Visibility.Collapsed;
                }

                if (role_user == "администратор бд" || role_user == "заведующий")
                {
                    InventNumber.IsReadOnly = true;
                    DateRegist.IsEnabled = false;
                }
            }

            ComboBoxOffice.SelectionChanged += ComboBoxOffice_SelectionChanged;
        }
        private void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (role_user != "администратор бд")
                {
                    MessageBox.Show("Только администратор может удалять оборудование!", "Доступ запрещен", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                if (currentEquipment == null)
                {
                    MessageBox.Show("Оборудование не найдено!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                var audience = db.Audiences.FirstOrDefault(a => a.IdAudience == currentEquipment.IdAudience);
                bool isOnStock = audience != null && audience.NumberAudience.ToLower() == "склад";

                if (!isOnStock)
                {
                    MessageBox.Show("Оборудование можно удалять только со склада!", "Удаление невозможно", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                int endYear = currentEquipment.DateTransferToCompanyBalance.Year + currentEquipment.StandardServiceLife;
                bool isExpired = endYear < DateTime.Now.Year;

                if (!isExpired)
                {
                    MessageBox.Show("Можно удалять только оборудование с истекшим сроком использования!", "Удаление невозможно", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                var result = MessageBox.Show($"Вы уверены, что хотите удалить оборудование '{currentEquipment.TitleEquipment}'?\nЭто действие нельзя отменить!",
                    "Подтверждение удаления", MessageBoxButton.YesNo, MessageBoxImage.Question);

                if (result == MessageBoxResult.Yes)
                {
                    using (var transaction = db.Database.BeginTransaction())
                    {
                        try
                        {
                            var writeOffRecords = db.Set<EquipmentWriteOff>().Where(wo => wo.IdEquipment == currentEquipment.IdEquipment).ToList();
                            if (writeOffRecords.Any())
                            {
                                db.Set<EquipmentWriteOff>().RemoveRange(writeOffRecords);
                                db.SaveChanges();
                            }

                            currentEquipment.IdAudience = null;
                            currentEquipment.IdOffices = null;
                            db.SaveChanges();

                            if (!string.IsNullOrEmpty(currentEquipment.Photo) && File.Exists(currentEquipment.Photo))
                            {
                                try
                                {
                                    File.Delete(currentEquipment.Photo);
                                }
                                catch { }
                            }

                            db.Equipment.Remove(currentEquipment);
                            db.SaveChanges();

                            transaction.Commit();

                            foreach (Window window in Application.Current.Windows)
                            {
                                if (window.Title == "MainWindow" || window is MainWindow)
                                {
                                    var frame = window.FindName("MainFrame") as System.Windows.Controls.Frame;
                                    if (frame != null)
                                    {
                                        frame.Navigate(new Equipment());
                                        break;
                                    }
                                }
                            }

                            MessageBox.Show("Оборудование успешно удалено!", "Информация", MessageBoxButton.OK, MessageBoxImage.Information);
                            this.Close();
                        }
                        catch (Exception ex)
                        {
                            transaction.Rollback();

                            if (ex.InnerException != null)
                            {
                                MessageBox.Show($"Ошибка при удалении: {ex.InnerException.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                            }
                            else
                            {
                                MessageBox.Show($"Ошибка при удалении: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                if (ex.InnerException != null)
                {
                    MessageBox.Show($"Ошибка при удалении: {ex.InnerException.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
                else
                {
                    MessageBox.Show($"Ошибка при удалении: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void ComboBoxOffice_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            if (role_user == "администратор бд")
            {
                if (ComboBoxOffice.SelectedValue != null && Convert.ToInt32(ComboBoxOffice.SelectedValue) != 0)
                {
                    int officeId = Convert.ToInt32(ComboBoxOffice.SelectedValue);

                    var officeAudienceIds = db.OfficesAudiences
                        .Where(oa => oa.IdOffice == officeId)
                        .Select(oa => oa.IdAudience)
                        .ToList();

                    var officeAudiences = db.Audiences
                        .Where(a => officeAudienceIds.Contains(a.IdAudience))
                        .ToList();

                    var audi = new List<Audience>();
                    audi.Add(new Audience { IdAudience = 0, NumberAudience = "", IdFloor = 1 });
                    audi.AddRange(officeAudiences);

                    Audiences.ItemsSource = audi;
                    Audiences.DisplayMemberPath = "NumberAudience";
                    Audiences.SelectedValuePath = "IdAudience";
                    Audiences.SelectedIndex = 0;
                }
                else
                {
                    var audiences = db.Audiences.ToList();
                    var audi = new List<Audience>();
                    audi.Add(new Audience { IdAudience = 0, NumberAudience = "", IdFloor = 1 });
                    audi.AddRange(audiences);

                    Audiences.ItemsSource = audi;
                    Audiences.DisplayMemberPath = "NumberAudience";
                    Audiences.SelectedValuePath = "IdAudience";
                    Audiences.SelectedIndex = 0;
                }
            }
        }

        private void SelectPhotoButton_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Title = "Выберите фото оборудования";
            openFileDialog.Filter = "Изображения (*.jpg;*.jpeg;*.png)|*.jpg;*.jpeg;*.png";

            if (openFileDialog.ShowDialog() == true)
            {
                try
                {
                    using (FileStream originalStream = new FileStream(openFileDialog.FileName, FileMode.Open, FileAccess.Read))
                    {
                        var bitmap = new System.Windows.Media.Imaging.BitmapImage();
                        bitmap.BeginInit();
                        bitmap.StreamSource = originalStream;
                        bitmap.CacheOption = System.Windows.Media.Imaging.BitmapCacheOption.OnLoad;
                        bitmap.EndInit();

                        if (bitmap.PixelWidth > 300 || bitmap.PixelHeight > 200)
                        {
                            MessageBox.Show($"Размер изображения {bitmap.PixelWidth}x{bitmap.PixelHeight} превышает допустимый 300x200 пикселей!\nПожалуйста, выберите изображение с размерами не более 300x200 пикселей.",
                                "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                            return;
                        }
                    }

                    string fileName = System.IO.Path.GetFileName(openFileDialog.FileName);
                    string photoDirectory = System.IO.Path.Combine(Environment.CurrentDirectory, "media");

                    if (!Directory.Exists(photoDirectory))
                    {
                        Directory.CreateDirectory(photoDirectory);
                    }

                    string destinationPath = System.IO.Path.Combine(photoDirectory, fileName);

                    int counter = 1;
                    string fileNameWithoutExt = System.IO.Path.GetFileNameWithoutExtension(fileName);
                    string extension = System.IO.Path.GetExtension(fileName);

                    while (File.Exists(destinationPath))
                    {
                        string newFileName = $"{fileNameWithoutExt}_{counter}{extension}";
                        destinationPath = System.IO.Path.Combine(photoDirectory, newFileName);
                        counter++;
                    }

                    string oldPhotoPath = selectedPhotoPath;

                    using (FileStream originalStream = new FileStream(openFileDialog.FileName, FileMode.Open, FileAccess.Read))
                    {
                        var bitmap = new System.Windows.Media.Imaging.BitmapImage();
                        bitmap.BeginInit();
                        bitmap.StreamSource = originalStream;
                        bitmap.DecodePixelWidth = 300;
                        bitmap.DecodePixelHeight = 200;
                        bitmap.CacheOption = System.Windows.Media.Imaging.BitmapCacheOption.OnLoad;
                        bitmap.EndInit();
                        bitmap.Freeze();

                        var encoder = new System.Windows.Media.Imaging.JpegBitmapEncoder();
                        encoder.Frames.Add(System.Windows.Media.Imaging.BitmapFrame.Create(bitmap));

                        using (FileStream fileStream = new FileStream(destinationPath, FileMode.Create))
                        {
                            encoder.Save(fileStream);
                        }
                    }

                    if (!string.IsNullOrEmpty(oldPhotoPath) && File.Exists(oldPhotoPath))
                    {
                        try
                        {
                            File.Delete(oldPhotoPath);
                        }
                        catch { }
                    }

                    var displayBitmap = new System.Windows.Media.Imaging.BitmapImage();
                    displayBitmap.BeginInit();
                    displayBitmap.UriSource = new Uri(destinationPath);
                    displayBitmap.CacheOption = System.Windows.Media.Imaging.BitmapCacheOption.OnLoad;
                    displayBitmap.EndInit();

                    EquipmentImage.Source = displayBitmap;
                    selectedPhotoPath = destinationPath;

                    MessageBox.Show("Фото успешно загружено!", "Информация", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка при загрузке фото: {ex.Message}\n\nПопробуйте выбрать другое изображение.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }
        private void ExitToLogin_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void AddOrEditButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (string.IsNullOrEmpty(TitleEquipment.Text) || string.IsNullOrEmpty(InventNumber.Text) ||
                    string.IsNullOrEmpty(WidhtEquipment.Text) || string.IsNullOrEmpty(DateRegist.Text) ||
                    string.IsNullOrEmpty(YearWork.Text) || string.IsNullOrEmpty(Description.Text))
                {
                    MessageBox.Show("Все поля должны быть заполнены!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                if (Audiences.SelectedIndex == 0 && ComboBoxOffice.SelectedIndex == 0)
                {
                    MessageBox.Show("Выберите подразделение или аудиторию!", "Предупреждение", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                if (!double.TryParse(WidhtEquipment.Text, out double weight) || weight <= 0)
                {
                    MessageBox.Show("Вес должен быть положительным числом!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                if (!int.TryParse(YearWork.Text, out int serviceLife) || serviceLife <= 0)
                {
                    MessageBox.Show("Нормативный срок службы должен быть положительным целым числом!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                if (!DateOnly.TryParse(DateRegist.Text, out DateOnly regDate))
                {
                    MessageBox.Show("Дата постановки на учет должна быть корректной датой (ДД.ММ.ГГГГ)!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                string photoPathForDb = null;
                if (!string.IsNullOrEmpty(selectedPhotoPath))
                {
                    if (File.Exists(selectedPhotoPath))
                    {
                        photoPathForDb = selectedPhotoPath;
                    }
                }

                if (idEq == -1)
                {
                    int idEquipment = db.Equipment.Any() ? db.Equipment.Max(e => e.IdEquipment) + 1 : 1;

                    Models.Equipment equipment = new Models.Equipment
                    {
                        IdEquipment = idEquipment,
                        TitleEquipment = TitleEquipment.Text,
                        InventoryNumber = InventNumber.Text,
                        WeightInKg = weight,
                        DateTransferToCompanyBalance = regDate,
                        StandardServiceLife = serviceLife,
                        Description = Description.Text,
                        Photo = photoPathForDb
                    };

                    if (ComboBoxOffice.SelectedIndex != 0 && ComboBoxOffice.SelectedValue != null)
                    {
                        equipment.IdOffices = Convert.ToInt32(ComboBoxOffice.SelectedValue);
                        equipment.IdAudience = null;
                    }
                    else if (Audiences.SelectedIndex != 0 && Audiences.SelectedValue != null)
                    {
                        equipment.IdAudience = Convert.ToInt32(Audiences.SelectedValue);
                        equipment.IdOffices = null;
                    }

                    try
                    {
                        db.Equipment.Add(equipment);
                        db.SaveChanges();
                        foreach (Window window in Application.Current.Windows)
                        {
                            if (window.Title == "MainWindow" || window is MainWindow)
                            {
                                var frame = window.FindName("MainFrame") as System.Windows.Controls.Frame;
                                if (frame != null)
                                {
                                    frame.Navigate(new Equipment());
                                    break;
                                }
                            }
                        }
                        MessageBox.Show("Оборудование успешно добавлено!", "Информация", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                    catch (Exception ex)
                    {
                        if (ex.InnerException != null)
                        {
                            MessageBox.Show($"Ошибка при сохранении: {ex.InnerException.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                        }
                        else
                        {
                            MessageBox.Show($"Ошибка при сохранении: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                        }
                        return;
                    }
                }
                else
                {
                    currentEquipment = db.Equipment.FirstOrDefault(e => e.IdEquipment == idEq);
                    if (currentEquipment != null)
                    {
                        string oldPhoto = currentEquipment.Photo;

                        currentEquipment.TitleEquipment = TitleEquipment.Text;
                        currentEquipment.WeightInKg = weight;
                        currentEquipment.StandardServiceLife = serviceLife;
                        currentEquipment.Description = Description.Text;
                        currentEquipment.Photo = photoPathForDb;

                        if (ComboBoxOffice.SelectedIndex != 0 && ComboBoxOffice.SelectedValue != null)
                        {
                            currentEquipment.IdOffices = Convert.ToInt32(ComboBoxOffice.SelectedValue);
                        }
                        else
                        {
                            currentEquipment.IdOffices = null;
                        }

                        if (Audiences.SelectedIndex != 0 && Audiences.SelectedValue != null)
                        {
                            currentEquipment.IdAudience = Convert.ToInt32(Audiences.SelectedValue);
                        }
                        else
                        {
                            currentEquipment.IdAudience = null;
                        }

                        try
                        {
                            db.SaveChanges();

                            if (!string.IsNullOrEmpty(selectedPhotoPath) && selectedPhotoPath != oldPhoto)
                            {
                                if (!string.IsNullOrEmpty(oldPhoto) && File.Exists(oldPhoto) && oldPhoto != selectedPhotoPath)
                                {
                                    try { File.Delete(oldPhoto); } catch { }
                                }
                            }

                            foreach (Window window in Application.Current.Windows)
                            {
                                if (window.Title == "MainWindow" || window is MainWindow)
                                {
                                    var frame = window.FindName("MainFrame") as System.Windows.Controls.Frame;
                                    if (frame != null)
                                    {
                                        frame.Navigate(new Equipment());
                                        break;
                                    }
                                }
                            }

                            MessageBox.Show("Оборудование успешно обновлено!", "Информация", MessageBoxButton.OK, MessageBoxImage.Information);
                        }
                        catch (Exception ex)
                        {
                            if (ex.InnerException != null)
                            {
                                MessageBox.Show($"Ошибка при сохранении: {ex.InnerException.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                            }
                            else
                            {
                                MessageBox.Show($"Ошибка при сохранении: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                            }
                            return;
                        }
                    }
                }

                this.Close();
            }
            catch (Exception ex)
            {
                if (ex.InnerException != null)
                {
                    MessageBox.Show($"Произошла ошибка: {ex.InnerException.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
                else
                {
                    MessageBox.Show($"Произошла ошибка: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }
    }
}