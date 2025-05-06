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
using iTextSharp.text.pdf.parser;
using MedicalFinderProject.dbModel;


namespace MedicalFinderProject.Views
{
    /// <summary>
    /// Логика взаимодействия для MainPage.xaml
    /// </summary>
    public partial class MainPage : Page
    {
        
        private List<string> cities = new List<string>();
        private List<string> specializations = new List<string>();

        public MainPage()
        {
            InitializeComponent();
            LoadCities();
            LoadSpecializations();
        }

        private void LoadCities()
        {
            using (var context = new MedicalSpecialistServiceEntities4())
            {
                cities = context.Clinics.Select(c => c.City).Distinct().ToList();
                CityComboBox.ItemsSource = cities;
            }
        }

        private void LoadSpecializations()
        {
            using (var context = new MedicalSpecialistServiceEntities4())
            {
                specializations = context.Specializations.Select(s => s.Name).Distinct().ToList();
                SpecializationComboBox.ItemsSource = specializations;
            }
        }

        private void CityComboBox_KeyUp(object sender, KeyEventArgs e)
        {
            string query = CityComboBox.Text.ToLower();
            var filteredCities = cities.Where(c => c.ToLower().Contains(query)).ToList();
            CityComboBox.ItemsSource = filteredCities;
            CityComboBox.IsDropDownOpen = true; 
        }

        private void SpecializationComboBox_KeyUp(object sender, KeyEventArgs e)
        {
            string query = SpecializationComboBox.Text.ToLower();
            var filteredSpecializations = specializations.Where(s => s.ToLower().Contains(query)).ToList();
            SpecializationComboBox.ItemsSource = filteredSpecializations;
            SpecializationComboBox.IsDropDownOpen = true; 
        }

        private void FindSpecialistButton_Click(object sender, RoutedEventArgs e)
        {
            string selectedCity = CityComboBox.Text;
            string selectedSpecialization = SpecializationComboBox.Text;

            using (var context = new MedicalSpecialistServiceEntities4())
            {
                var filteredDoctors = (from d in context.Doctors
                                       join c in context.Clinics on d.ClinicID equals c.ClinicID
                                       join s in context.Specializations on d.SpecializationID equals s.SpecializationID
                                       where c.City == selectedCity && s.Name == selectedSpecialization
                                       select new DoctorViewModel
                                       {
                                           DoctorID = d.DoctorID,
                                           FullName = d.LastName + " " + d.FirstName + " " + d.MiddleName,
                                           ClinicName = c.Name,
                                           Specialization = s.Name,
                                           Experience = d.ExperienceYears + " лет",
                                           Rating = Math.Round(
                                               context.Reviews.Where(r => r.DoctorID == d.DoctorID).Any()
                                               ? context.Reviews.Where(r => r.DoctorID == d.DoctorID).Average(r => (double?)r.Rating) ?? 0
                                               : 0,
                                               1)
                                       }).ToList();

                if (filteredDoctors.Count == 0)
                {
                    NoDoctorsFoundMessage.Visibility = Visibility.Visible; 
                    DoctorsList.Visibility = Visibility.Collapsed; 
                    ShowAllButton.Visibility = Visibility.Visible; 
                }
                else
                {
                    NoDoctorsFoundMessage.Visibility = Visibility.Collapsed;
                    DoctorsList.Visibility = Visibility.Visible; 
                    DoctorsList.ItemsSource = filteredDoctors; 
                }
            }
        }
        private void ShowAllButton_Click(object sender, RoutedEventArgs e)
        {
            
            NoDoctorsFoundMessage.Visibility = Visibility.Collapsed;
            DoctorsList.Visibility = Visibility.Visible;
            ShowAllButton.Visibility = Visibility.Collapsed;

            using (var context = new MedicalSpecialistServiceEntities4())
            {
                var allDoctors = (from d in context.Doctors
                                  join c in context.Clinics on d.ClinicID equals c.ClinicID
                                  join s in context.Specializations on d.SpecializationID equals s.SpecializationID
                                  
                                  select new DoctorViewModel
                                  {
                                      DoctorID = d.DoctorID,
                                      FullName = d.LastName + " " + d.FirstName + " " + d.MiddleName,
                                      ClinicName = c.Name,
                                      Specialization = s.Name,
                                      Experience = d.ExperienceYears + " лет",
                                      Rating = Math.Round(
                                          context.Reviews.Where(r => r.DoctorID == d.DoctorID).Any()
                                          ? context.Reviews.Where(r => r.DoctorID == d.DoctorID).Average(r => (double?)r.Rating) ?? 0
                                          : 0,
                                          1)
                                  }).ToList();

                DoctorsList.ItemsSource = allDoctors;
            }
        }
        private void DoctorsList_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (e.OriginalSource is FrameworkElement fe && fe.DataContext is DoctorViewModel doctor)
            {
                DoctorsList.SelectedItem = doctor;
                NavigationService?.Navigate(new DoctorServicesPage(doctor));
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            NavigationService?.Navigate(new Profile());
        }
    }


}
