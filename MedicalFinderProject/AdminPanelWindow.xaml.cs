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
using MedicalFinderProject.Views;
using Org.BouncyCastle.Utilities;

namespace MedicalFinderProject
{
    /// <summary>
    /// Логика взаимодействия для AdminPanelWindow.xaml
    /// </summary>
    public partial class AdminPanelWindow : Window
    {
        private MedicalSpecialistServiceEntities3 db = new MedicalSpecialistServiceEntities3();
        public AdminPanelWindow()
        {
            InitializeComponent();
            LoadComboBoxes();
            LoadUserList();
            LoadActivityLogs();
        }
        private void LoadUserList()
        {
            using (var context = new MedicalSpecialistServiceEntities3())
            {
                var users = context.Users
                    .Where(u => u.RoleID == 2)
                    .Select(u => new { u.UserID, u.FullName })
                    .ToList();

                UserListView.ItemsSource = users;
            }
        }
        private void LoadComboBoxes()
        {
            using (var context = new MedicalSpecialistServiceEntities3())
            {
                SpecializationComboBox.ItemsSource = context.Specializations.ToList();
                SpecializationComboBox.DisplayMemberPath = "Name";
                SpecializationComboBox.SelectedValuePath = "SpecializationID";

                ClinicComboBox.ItemsSource = context.Clinics.ToList();
                ClinicComboBox.DisplayMemberPath = "Name";
                ClinicComboBox.SelectedValuePath = "ClinicID";
            }
        }

        private void AddDoctor_Click(object sender, RoutedEventArgs e)
        {
            string lastName = DoctorLastNameTextBox.Text.Trim();
            string firstName = DoctorFirstNameTextBox.Text.Trim();
            string middleName = DoctorMiddleNameTextBox.Text.Trim();
            var specialization = SpecializationComboBox.SelectedValue;
            var clinic = ClinicComboBox.SelectedValue;
            string bio = DoctorBiographyTextBox.Text.Trim();

            if (string.IsNullOrWhiteSpace(lastName) || string.IsNullOrWhiteSpace(firstName) ||
                specialization == null || clinic == null || string.IsNullOrWhiteSpace(bio))
            {
                MessageBox.Show("Пожалуйста, заполните все поля.");
                return;
            }

            using (var context = new MedicalSpecialistServiceEntities3())
            {
                var doctor = new Doctors
                {
                    LastName = lastName,
                    FirstName = firstName,
                    MiddleName = middleName,
                    SpecializationID = (int)specialization,
                    ClinicID = (int)clinic,
                    Bio = bio
                };

                context.Doctors.Add(doctor);
                context.SaveChanges();
            }

            MessageBox.Show("Врач добавлен.");
        }

        private void AddClinic_Click(object sender, RoutedEventArgs e)
        {
            string name = ClinicNameTextBox.Text.Trim();
            string address = ClinicAddressTextBox.Text.Trim();
            string city = ClinicCityTextBox.Text.Trim();
            string phone = ClinicPhoneTextBox.Text.Trim();

            if (string.IsNullOrWhiteSpace(name) || string.IsNullOrWhiteSpace(address) || string.IsNullOrWhiteSpace(city) || string.IsNullOrWhiteSpace(phone)
)
            {
                MessageBox.Show("Пожалуйста, заполните все поля.");
                return;
            }

            using (var context = new MedicalSpecialistServiceEntities3())
            {
                var clinic = new Clinics
                {
                    Name = name,
                    Address = address,
                    City = city,
                    Phone = phone
                };

                context.Clinics.Add(clinic);
                context.SaveChanges();
            }

            MessageBox.Show("Клиника добавлена.");
            LoadComboBoxes(); 
        }

        private void AddUser_Click(object sender, RoutedEventArgs e)
        {
            string fullName = UserFullNameTextBox.Text.Trim();
            string email = UserEmailTextBox.Text.Trim();
            string phone = UserPhoneTextBox.Text.Trim();
            string password = UserPasswordBox.Password;
            int roleId = IsAdminCheckBox.IsChecked == true ? 1 : 2;

            if (string.IsNullOrWhiteSpace(fullName) || string.IsNullOrWhiteSpace(email) ||
                string.IsNullOrWhiteSpace(phone) || string.IsNullOrWhiteSpace(password))
            {
                MessageBox.Show("Пожалуйста, заполните все поля.");
                return;
            }

            using (var context = new MedicalSpecialistServiceEntities3())
            {
                if (context.Users.Any(u => u.Email == email))
                {
                    MessageBox.Show("Пользователь с таким email уже существует.");
                    return;
                }

                string hashedPassword = HashPassword(password);

                var user = new Users
                {
                    FullName = fullName,
                    Email = email,
                    Phone = phone,
                    PasswordHash = hashedPassword,
                    RoleID = roleId
                };

                context.Users.Add(user);
                context.SaveChanges();
            }

            MessageBox.Show("Пользователь добавлен.");
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
        private void ViewAppointments_Click(object sender, RoutedEventArgs e)
        {
            Button btn = sender as Button;
            int userId = (int)btn.Tag;

            using (var context = new MedicalSpecialistServiceEntities3())
            {
                var user = context.Users.FirstOrDefault(u => u.UserID == userId);
                if (user != null)
                    SelectedUserNameText.Text = $"Назначения пользователя: {user.FullName}";

                var appointments = context.Appointments
                    .Where(a => a.UserID == userId)
                    .Select(a => new
                    {
                        a.AppointmentDate,
                        DoctorName = a.Doctors.LastName + " " + a.Doctors.FirstName,
                        ServiceName = a.Services.Name,
                        a.Status
                    })
                    .ToList();

                AppointmentListView.ItemsSource = appointments;
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            SessionManager.CurrentUser = null;

            
            App.MainAppWindow.Show();

            
            App.MainAppWindow.MainFrame.Navigate(new LoginPage());

            
            this.Close();

        }
        private void LoadActivityLogs()
        {
            using (var context = new MedicalSpecialistServiceEntities3())
            {
                
                var users = context.Users.Select(u => new { u.UserID, u.FullName }).ToList();
                UserComboBox.ItemsSource = users;
                UserComboBox.DisplayMemberPath = "FullName";
                UserComboBox.SelectedValuePath = "UserID";

                
                var logs = context.ActivityLogs
                                  .Select(log => new
                                  {
                                      UserID = log.UserID,
                                      Action = log.Action,
                                      LogDate = log.LogDate
                                  }).ToList();

                ActivityLogDataGrid.ItemsSource = logs;
            }
        }

        private void UserComboBox_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            if (UserComboBox.SelectedValue != null)
            {
                int selectedUserId = (int)UserComboBox.SelectedValue;

                using (var context = new MedicalSpecialistServiceEntities3())
                {
                    var filteredLogs = context.ActivityLogs
                                              .Where(log => log.UserID == selectedUserId)
                                              .Select(log => new
                                              {
                                                  UserID = log.UserID,
                                                  Action = log.Action,
                                                  LogDate = log.LogDate
                                              }).ToList();

                    ActivityLogDataGrid.ItemsSource = filteredLogs;
                }
            }
        }
    }
}
