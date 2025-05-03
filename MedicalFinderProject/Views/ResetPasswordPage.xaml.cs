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
                // Находим пользователя по токену
                var user = context.Users.FirstOrDefault(u => u.ResetPasswordToken == token);
                if (user == null)
                {
                    MessageBox.Show("Неверный токен.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                // Хэшируем новый пароль
                string newPasswordHash = HashPassword(newPassword);

                // Обновляем пароль в базе данных
                user.PasswordHash = newPasswordHash;
                user.ResetPasswordToken = null; // Очищаем токен после использования
                context.SaveChanges();

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
    }

}
