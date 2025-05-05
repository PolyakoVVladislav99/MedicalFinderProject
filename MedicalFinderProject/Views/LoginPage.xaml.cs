using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
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
using static MedicalFinderProject.Constants;

namespace MedicalFinderProject.Views
{
    /// <summary>
    /// Логика взаимодействия для LoginPage.xaml
    /// </summary>
    public partial class LoginPage : Page
    {
        MedicalSpecialistServiceEntities3 db = new MedicalSpecialistServiceEntities3();
        
        public LoginPage()
        {
            InitializeComponent();
        }
    
    private void LoginButton_Click(object sender, RoutedEventArgs e)
        {
            string email = EmailBox.Text.Trim();
            string password = PasswordBox.Password;
            

            if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
            {
                MessageBox.Show("Пожалуйста, заполните все поля.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                using (var context = new MedicalSpecialistServiceEntities3())
                {
                    var user = context.Users.FirstOrDefault(u => u.Email == email);

                    if (user == null)
                    {
                        LogUserAction(SYSTEM_USER_ID, "Неудачная попытка входа с неверным email");
                        MessageBox.Show("Пользователь не найден.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                        return;
                    }

                    string enteredHash = HashPassword(password);

                    if (user.PasswordHash != enteredHash)
                    {
                        LogUserAction(user.UserID, "Неудачная попытка входа с неверным паролем");
                        MessageBox.Show("Неверный пароль.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                        return;
                    }

                    LogUserAction(user.UserID, "Вход в систему (успешно)");
                    AddNotification(user.UserID, "Успешный вход");

                    if (user.RoleID == 1)
                    {
                        AdminPanelWindow adminWindow = new AdminPanelWindow();
                        adminWindow.Show();
                        Window mainWindow = Application.Current.MainWindow;
                        mainWindow.Hide();
                        return;
                    }

                    SessionManager.CurrentUser = user;
                    MainPage mainPage = new MainPage();
                    NavigationService.Navigate(mainPage);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка при авторизации: " + ex.Message, "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        private string HashPassword(string password)
        {
            using (SHA256 sha256 = SHA256.Create())
            {
                byte[] bytes = Encoding.UTF8.GetBytes(password.Trim());
                byte[] hashBytes = sha256.ComputeHash(bytes);
                return Convert.ToBase64String(hashBytes);
            }
        }
        private void RegisterLink_Click(object sender, RoutedEventArgs e)
        {   
            this.NavigationService?.Navigate(new RegisterPage());
        }
        private void ResetPassword_Click(object sender, RoutedEventArgs e)
        { 
            NavigationService?.Navigate(new ForgotPasswordPage());
        }
        
        private void LogUserAction(int userId, string action)
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
        public void AddNotification(int userId, string message)
        {
            using (var context = new MedicalSpecialistServiceEntities3())
            {
                var notification = new Notifications
                {
                    UserID = userId,
                    Message = message,
                    IsRead = false,
                    SentAt = DateTime.Now
                };

                context.Notifications.Add(notification);
                context.SaveChanges();
            }
        }
    }
}
