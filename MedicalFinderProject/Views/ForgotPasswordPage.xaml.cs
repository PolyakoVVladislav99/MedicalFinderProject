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
                if (user == null)
                {
                    MessageBox.Show("Пользователь с таким email не найден.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                // Генерация токена сброса пароля
                string token = GenerateResetToken(user.UserID);

                // Сохраняем токен в базе данных
                user.ResetPasswordToken = token;
                context.SaveChanges();

                // Отображаем токен в TextBlock
                ResetTokenText.Text = $"{token}";

                // Автоматически копируем токен в буфер обмена
                Clipboard.SetText(token);

                // Показываем уведомление, что токен скопирован
                MessageBox.Show("Токен для сброса пароля скопирован в буфер обмена. Используйте его на странице сброса пароля.",
                                "Сброс пароля", MessageBoxButton.OK, MessageBoxImage.Information);
                NavigationService.Navigate(new ResetPasswordPage());
            }
        }

        private string GenerateResetToken(int userId)
        {
            // Генерация токена с использованием GUID
            return Guid.NewGuid().ToString();
        }
    }
}
