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
using MedicalFinderProject.dbModel;
using Microsoft.Win32;
using System.IO;
using System.Collections.ObjectModel;


namespace MedicalFinderProject.Views
{
    /// <summary>
    /// Логика взаимодействия для Profile.xaml
    /// </summary>
    public partial class Profile : Page
    {
        public ObservableCollection<NotificationViewModel> Notifications { get; set; }
        public Profile()
        {
            InitializeComponent();
            NameTextBlock.Text = $"{SessionManager.CurrentUser.FullName}";
            int userId = SessionManager.CurrentUser.UserID;
            LoadNotifications();
        }

        private void ForgotPassButton_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new ForgotPasswordPage());
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            NavigationService?.Navigate(new MyAppointmentsPage());
        }
        private void UploadPhotoButton_Click(object sender, RoutedEventArgs e)
        {
            
            Microsoft.Win32.OpenFileDialog openFileDialog = new Microsoft.Win32.OpenFileDialog();
            openFileDialog.Filter = "Image Files|*.jpg;*.jpeg;*.png;*.bmp;*.gif";

            if (openFileDialog.ShowDialog() == true)
            {
                string filePath = openFileDialog.FileName;

                
                byte[] imageBytes = File.ReadAllBytes(filePath);

                
                using (var context = new MedicalSpecialistServiceEntities4())
                {
                    var user = context.Users.FirstOrDefault(u => u.UserID == SessionManager.CurrentUser.UserID);
                    if (user != null)
                    {
                        user.ProfilePicture = imageBytes;
                        context.SaveChanges();
                        LogUserAction(user.UserID, "Загружено фото профиля");
                    }
                }

                DisplayProfilePicture(imageBytes);
            }
        }
        private void DisplayProfilePicture(byte[] imageBytes)
        {
            
            using (var ms = new MemoryStream(imageBytes))
            {
                var image = new BitmapImage();
                image.BeginInit();
                image.StreamSource = ms;
                image.CacheOption = BitmapCacheOption.OnLoad;
                image.EndInit();

                
                ProfileImage.Source = image;
            }
        }
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            
            using (var context = new MedicalSpecialistServiceEntities4())
            {
                var user = context.Users.FirstOrDefault(u => u.UserID == SessionManager.CurrentUser.UserID);
                if (user != null)
                {
                    
                    NameTextBlock.Text = user.FullName;

                    
                    if (user.ProfilePicture != null)
                    {
                        DisplayProfilePicture(user.ProfilePicture);
                    }
                }
            }
        }
        private void ViewDoctorScheduleButton_Click(object sender, RoutedEventArgs e)
        {
            
            DoctorSchedulePage schedulePage = new DoctorSchedulePage();
            NavigationService.Navigate(schedulePage);
        }
        public static void LogUserAction(int userId, string action)
        {
            using (var context = new MedicalSpecialistServiceEntities4())
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

        private void MedicalCardButton_Click(object sender, RoutedEventArgs e)
        {
            
                MedicalCardPage medicalCardPage = new MedicalCardPage();
                NavigationService.Navigate(medicalCardPage);
            
        }
        private void LoadNotifications()
        {
            using (var context = new MedicalSpecialistServiceEntities4())
            {   
                int userId = SessionManager.CurrentUser.UserID;
                var notifications = context.Notifications
                    .Where(n => n.UserID == userId)
                    .OrderByDescending(n => n.SentAt)
                    .Select(n => new NotificationViewModel
                    {
                        Message = n.Message,
                        SentAt = n.SentAt ?? DateTime.MinValue,
                        IsRead = n.IsRead ?? false
                    })
                    .ToList();

                if (notifications.Count == 0)
                {
                    NotificationsListView.ItemsSource = null;
                }
                else
                {
                    NotificationsListView.ItemsSource = notifications;
                }
            }
        }
        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.GoBack();
        }
    }
}
