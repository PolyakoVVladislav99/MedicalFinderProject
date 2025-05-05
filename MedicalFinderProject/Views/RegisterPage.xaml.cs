using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
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
    /// Логика взаимодействия для RegisterPage.xaml
    /// </summary>
    public partial class RegisterPage : Page
    {
        MedicalSpecialistServiceEntities3 db = new MedicalSpecialistServiceEntities3();
        public RegisterPage()
        {
            InitializeComponent(); 
        }
        private void RegisterButton_Click(object sender, RoutedEventArgs e)
        {
            string fullName = FullNameBox.Text.Trim();
            string email = EmailBox.Text.Trim().ToLower();
            string password = PasswordBox.Password;
            string phone = PhoneBox.Text.Trim();
            
            if (string.IsNullOrEmpty(fullName) || string.IsNullOrEmpty(email) ||
                string.IsNullOrEmpty(password) || string.IsNullOrEmpty(phone))
            {
                MessageBox.Show("Пожалуйста, заполните все поля.");
                return;
            }
           
            if (!Regex.IsMatch(email, @"^[^@\s]+@[^@\s]+\.[^@\s]+$"))
            {
                MessageBox.Show("Некорректный формат email.");
                return;
            }
        
            if (!Regex.IsMatch(phone, @"^\+?\d[\d\s\-\(\)]{9,14}$"))
            {
                MessageBox.Show("Некорректный формат номера телефона.");
                return;
            }

            
            if (db.Users.Any(u => u.Email == email))
            {
                MessageBox.Show("Пользователь с таким email уже существует.");
                return;
            }
            
            string hashedPassword = HashPassword(password);
    
            Users newUser = new Users
            {
                FullName = fullName,
                Email = email,
                PasswordHash = hashedPassword,
                Phone = phone,
                RoleID = 2
            };

            db.Users.Add(newUser);
            db.SaveChanges();
            LogUserAction(newUser.UserID, "Регистрация нового пользователя");
            MessageBox.Show("Регистрация успешна!");
            
            NavigationService.Navigate(new LoginPage());
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
        private void LoginLink_Click(object sender, RoutedEventArgs e)
        {
            this.NavigationService?.Navigate(new LoginPage());
        }
        private void EmailTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (!Regex.IsMatch(EmailBox.Text, @"^[^@\s]+@[^@\s]+\.[^@\s]+$"))
                EmailBox.Tag = "Invalid";
            else
                EmailBox.Tag = null;
        }
        private void PhoneTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (!Regex.IsMatch(PhoneBox.Text, @"^\+?\d[\d\s\-\(\)]{9,14}$"))
                PhoneBox.Tag = "Invalid";
            else
                PhoneBox.Tag = null;
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
