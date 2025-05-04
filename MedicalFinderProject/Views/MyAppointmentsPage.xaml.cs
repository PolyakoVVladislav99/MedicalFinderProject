using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Diagnostics;
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
using iTextSharp.text.pdf.parser;
using MedicalFinderProject.dbModel;

namespace MedicalFinderProject.Views
{
    /// <summary>
    /// Логика взаимодействия для MyAppointmentsPage.xaml
    /// </summary>
    public partial class MyAppointmentsPage : Page
    {
        private List<AppointmentViewModel> _appointments;

        public MyAppointmentsPage()
        {
            InitializeComponent();
            FilterChanged(null, null);
            LoadAppointments();
        }

        private List<AppointmentViewModel> allAppointments; // Переменная для хранения всех записей

        private void LoadAppointments()
        {
            using (var context = new MedicalSpecialistServiceEntities3())
            {
                int userId = SessionManager.CurrentUser.UserID;

                // Получаем все записи из базы
                allAppointments = context.Appointments
    .Where(a => a.UserID == userId)
    .Include(a => a.Doctors)
    .Include(a => a.Services)
    .ToList()
    .Select(a => new AppointmentViewModel
    {
        AppointmentID = a.AppointmentID,
        DoctorName = a.Doctors.LastName + " " + a.Doctors.FirstName,
        Status = a.Status,
        AppointmentDate = a.AppointmentDate,
        Price = a.Services.Price,
        HasReceipt = a.DocumentID != null,
        DocumentID = a.DocumentID
    })
    .ToList();

                // Привязываем все записи к ListView
                AppointmentsListView.ItemsSource = allAppointments;
                FullNameTextBlock.Text = $"Пациент: {SessionManager.CurrentUser.FullName}";
            }
        }

        private void FilterChanged(object sender, EventArgs e)
        {
            if (allAppointments == null || MinPriceTextBox == null || StatusComboBox == null || AppointmentsListView == null)
                return;
            // Фильтрация по дате, статусу и цене
            var filtered = allAppointments.AsEnumerable();

            // Дата
            if (StartDatePicker.SelectedDate is DateTime startDate)
                filtered = filtered.Where(a => a.AppointmentDate >= startDate);
            if (EndDatePicker.SelectedDate is DateTime endDate)
                filtered = filtered.Where(a => a.AppointmentDate <= endDate);

            // Статус
            if (StatusComboBox.SelectedItem is ComboBoxItem selectedItem)
            {
                string status = selectedItem.Content.ToString();
                if (status != "Все")
                    filtered = filtered.Where(a => a.Status == status);
            }

            // Цена
            if (decimal.TryParse(MinPriceTextBox.Text, out decimal minPrice))
                filtered = filtered.Where(a => a.Price >= minPrice);

            // Обновляем ItemsSource с отфильтрованными данными
            AppointmentsListView.ItemsSource = filtered.ToList();
        }
        private void ShowReceipt_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.DataContext is AppointmentViewModel appointment && appointment.DocumentID.HasValue)
            {
                using (var context = new MedicalSpecialistServiceEntities3())
                {
                    var doc = context.Documents.Find(appointment.DocumentID.Value);
                    if (doc != null && doc.FileData != null)
                    {
                        string tempPath = System.IO.Path.Combine(System.IO.Path.GetTempPath(), doc.FileName);
                        File.WriteAllBytes(tempPath, doc.FileData);
                        Process.Start(new ProcessStartInfo(tempPath) { UseShellExecute = true });
                    }
                    else
                    {
                        MessageBox.Show("Чек не найден в базе данных.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {

        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            NavigationService?.Navigate(new MainPage());
        }
    }
}
