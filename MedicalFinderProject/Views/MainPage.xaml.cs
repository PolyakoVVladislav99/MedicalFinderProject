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
    /// Логика взаимодействия для MainPage.xaml
    /// </summary>
    public partial class MainPage : Page
    {
        private List<DoctorViewModel> allDoctors;

        public MainPage()
        {
            InitializeComponent();
            LoadFilters();
            LoadDoctors();  // Убедитесь, что данные загружаются
        }

        private void FilterChanged(object sender, SelectionChangedEventArgs e)
        {
            ApplyFilters();
        }

        private void LoadFilters()
        {
            using (var context = new MedicalSpecialistServiceEntities3())
            {
                var clinics = context.Clinics.Select(c => c.Name).Distinct().ToList();
                foreach (var clinic in clinics)
                    ClinicFilter.Items.Add(clinic);

                var specializations = context.Specializations.Select(s => s.Name).Distinct().ToList();
                foreach (var spec in specializations)
                    SpecializationFilter.Items.Add(spec);

                ExperienceFilter.Items.Add("0-3 года");
                ExperienceFilter.Items.Add("4-7 лет");
                ExperienceFilter.Items.Add("8+ лет");

                RatingFilter.Items.Add("1+");
                RatingFilter.Items.Add("2+");
                RatingFilter.Items.Add("3+");
                RatingFilter.Items.Add("4+");
                RatingFilter.Items.Add("5");
            }

            ClinicFilter.SelectedIndex = 0;
            SpecializationFilter.SelectedIndex = 0;
            ExperienceFilter.SelectedIndex = 0;
            RatingFilter.SelectedIndex = 0;
        }

        private void LoadDoctors()
        {
            try
            {
                using (var context = new MedicalSpecialistServiceEntities3())
                {
                    allDoctors = (from d in context.Doctors
                                  join c in context.Clinics on d.ClinicID equals c.ClinicID
                                  join s in context.Specializations on d.SpecializationID equals s.SpecializationID
                                  select new DoctorViewModel
                                  {
                                      FullName = d.LastName + " " + d.FirstName + " " + d.MiddleName,
                                      ClinicName = c.Name,
                                      Specialization = s.Name,
                                      Experience = d.ExperienceYears + " лет",
                                      Rating = Math.Round(
                                        context.Reviews.Where(r => r.DoctorID == d.DoctorID).Any()
                                            ? context.Reviews.Where(r => r.DoctorID == d.DoctorID).Average(r => (double?)r.Rating) ?? 0
                                            : 0, 1)
                                  }).OrderBy(d => d.FullName).ToList();

                    DoctorsList.ItemsSource = allDoctors;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка при загрузке врачей: " + ex.Message);
            }
        }

        private void ApplyFilters()
        {
            if (allDoctors == null)
            {
                return; // Если allDoctors еще не инициализирован, не применяем фильтры.
            }

            var filtered = allDoctors.AsEnumerable();

            if (ClinicFilter.SelectedIndex > 0)
                filtered = filtered.Where(d => d.ClinicName == ClinicFilter.SelectedItem.ToString());

            if (SpecializationFilter.SelectedIndex > 0)
                filtered = filtered.Where(d => d.Specialization == SpecializationFilter.SelectedItem.ToString());

            if (ExperienceFilter.SelectedIndex > 0)
            {
                string exp = ExperienceFilter.SelectedItem.ToString();
                filtered = filtered.Where(d =>
                {
                    int years = int.Parse(d.Experience.Split(' ')[0]);
                    return MatchExperience(exp, years);
                });
            }

            if (RatingFilter.SelectedIndex > 0)
            {
                double minRating = double.Parse(RatingFilter.SelectedItem.ToString().Replace("+", ""));
                filtered = filtered.Where(d => d.Rating >= minRating);
            }

            DoctorsList.ItemsSource = filtered.ToList();
        }

        private bool MatchExperience(string exp, int years)
        {
            switch (exp)
            {
                case "0-3 года":
                    return years <= 3;
                case "4-7 лет":
                    return years >= 4 && years <= 7;
                case "8+ лет":
                    return years >= 8;
                default:
                    return true;
            }
        }
    }

    public class DoctorViewModel
    {
        public string FullName { get; set; }
        public string ClinicName { get; set; }
        public string Specialization { get; set; }
        public string Experience { get; set; }
        public double Rating { get; set; }
    }


}
