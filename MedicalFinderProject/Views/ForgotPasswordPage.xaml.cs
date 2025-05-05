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
using static MedicalFinderProject.Constants;

namespace MedicalFinderProject.Views
{
    /// <summary>
    /// Логика взаимодействия для ForgotPasswordPage.xaml
    /// </summary>
    public partial class ForgotPasswordPage : Page
    {
        public ForgotPasswordPage()
        {
            InitializeComponent();
        }
        private void SendResetLink_Click(object sender, RoutedEventArgs e)
        {
            string email = EmailBox.Text.Trim();

            using (var context = new MedicalSpecialistServiceEntities3())   
            {
                var user = context.Users.FirstOrDefault(u => u.Email == email);
                if (string.IsNullOrWhiteSpace(email))
                {
                    LogUserAction(SYSTEM_USER_ID, $"Попытка сброса пароля c пустыми полями");
                    MessageBox.Show("Пожалуйста, заполните все поля.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }
                if (user == null)
                {
                    LogUserAction(Constants.SYSTEM_USER_ID, $"Попытка сброса пароля с неверным email: {email}");
                    MessageBox.Show("Такой email не найден.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }
                

                
                string token = GenerateResetToken(user.UserID);

                
                user.ResetPasswordToken = token;
                context.SaveChanges();
                ResetTokenText.Text = $"{token}";
                Clipboard.SetText(token);
                MessageBox.Show("Токен для сброса пароля скопирован в буфер обмена. Используйте его на странице сброса пароля.",
                                "Сброс пароля", MessageBoxButton.OK, MessageBoxImage.Information);
                NavigationService.Navigate(new ResetPasswordPage());
            }
        }
        private string GenerateResetToken(int userId)
        {
            return Guid.NewGuid().ToString();
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
