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
            UpdateAppointmentStatuses();
            LoadAppointments();
        }

        private List<AppointmentViewModel> allAppointments; 

        private void LoadAppointments()
        {
            using (var context = new MedicalSpecialistServiceEntities3())
            {
                int userId = SessionManager.CurrentUser.UserID;

                
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

                
                AppointmentsListView.ItemsSource = allAppointments;
                FullNameTextBlock.Text = $"Пациент: {SessionManager.CurrentUser.FullName}";
            }
        }

        private void FilterChanged(object sender, EventArgs e)
        {
            if (allAppointments == null || MinPriceTextBox == null || StatusComboBox == null || AppointmentsListView == null)
                return;
            
            var filtered = allAppointments.AsEnumerable();

            
            if (StartDatePicker.SelectedDate is DateTime startDate)
                filtered = filtered.Where(a => a.AppointmentDate >= startDate);
            if (EndDatePicker.SelectedDate is DateTime endDate)
                filtered = filtered.Where(a => a.AppointmentDate <= endDate);

            
            if (StatusComboBox.SelectedItem is ComboBoxItem selectedItem)
            {
                string status = selectedItem.Content.ToString();
                if (status != "Все")
                    filtered = filtered.Where(a => a.Status == status);
            }

            
            if (decimal.TryParse(MinPriceTextBox.Text, out decimal minPrice))
                filtered = filtered.Where(a => a.Price >= minPrice);

            
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
            NavigationService.GoBack();
        }
        private void UpdateAppointmentStatuses()
        {
            int userId = SessionManager.CurrentUser.UserID;

            using (var context = new MedicalSpecialistServiceEntities3())
            {
                var now = DateTime.Now;

                var appointmentsToFinish = context.Appointments
                    .Where(a => a.UserID == userId && a.Status == "Ожидается" && DbFunctions.TruncateTime(a.AppointmentDate) < now.Date)
                    .ToList();

                foreach (var appointment in appointmentsToFinish)
                {
                    appointment.Status = "Завершено";
                }

                context.SaveChanges();
            }
        }
        private void CancelAppointment_Click(object sender, RoutedEventArgs e)
        {
            if ((sender as Button)?.DataContext is AppointmentViewModel model)
            {
                var result = MessageBox.Show("Вы уверены, что хотите отменить запись?",
                                             "Подтверждение", MessageBoxButton.YesNo, MessageBoxImage.Question);

                if (result == MessageBoxResult.Yes)
                {
                    using (var context = new MedicalSpecialistServiceEntities3())
                    {
                        var appointment = context.Appointments.FirstOrDefault(a => a.AppointmentID == model.AppointmentID);
                        if (appointment != null && appointment.Status == "Ожидается")
                        {
                            appointment.Status = "Отменено";
                            context.SaveChanges();

                            LogUserAction(SessionManager.CurrentUser.UserID, $"Отмена записи (ID: {appointment.AppointmentID})");
                            MessageBox.Show("Запись успешно отменена.", "Готово", MessageBoxButton.OK, MessageBoxImage.Information);
                        }
                    }

                    LoadAppointments(); // метод, который ты уже используешь для загрузки данных
                }
            }
        }
        public static void LogUserAction(int userId, string action)
        {
            using (var context = new MedicalSpecialistServiceEntities3())
            {
                var log = new ActivityLogs
                {
                    UserID = userId,
                    Action = action,
                    LogDate = DateTime.Now
                };
                context.ActivityLogs.Add(log);
                context.SaveChanges();
            }
        }
    }
}
