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
    /// Логика взаимодействия для ResetPasswordPage.xaml
    /// </summary>
    public partial class ResetPasswordPage : Page
    {
        public ResetPasswordPage()
        {
            InitializeComponent();
        }
        private void ResetPassword_Click(object sender, RoutedEventArgs e)
        {
            string token = TokenBox.Text.Trim();
            string newPassword = NewPasswordBox.Password.Trim();

            if (string.IsNullOrEmpty(token) || string.IsNullOrEmpty(newPassword))
            {
                MessageBox.Show("Заполните все поля.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            using (var context = new MedicalSpecialistServiceEntities3())
            {
               
                var user = context.Users.FirstOrDefault(u => u.ResetPasswordToken == token);
                if (user == null)
                {
                    LogUserAction(SYSTEM_USER_ID, $"Попытка сброса пароля с неверным токеном: {token}");
                    MessageBox.Show("Неверный токен.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }
                string newPasswordHash = HashPassword(newPassword);
                user.PasswordHash = newPasswordHash;
                user.ResetPasswordToken = null;
                context.SaveChanges();

                LogUserAction(user.UserID, "Сброс пароля");
                AddNotification(user.UserID, "Пароль изменен");
                MessageBox.Show("Пароль успешно сброшен!", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                NavigationService.Navigate(new LoginPage());
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
