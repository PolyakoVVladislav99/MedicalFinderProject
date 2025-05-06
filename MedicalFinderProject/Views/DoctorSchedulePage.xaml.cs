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
    /// Логика взаимодействия для DoctorSchedulePage.xaml
    /// </summary>
    public partial class DoctorSchedulePage : Page
    {
        public DoctorSchedulePage()
        {
            InitializeComponent();
            LoadDoctorSchedules();
        }
        private void LoadDoctorSchedules()
        {
            using (var context = new MedicalSpecialistServiceEntities4())
            {
                var schedules = context.DoctorSchedules
                    .Join(context.Doctors, schedule => schedule.DoctorID, doctor => doctor.DoctorID,
                        (schedule, doctor) => new
                        {
                            FullName = doctor.LastName + " " + doctor.FirstName + " " + doctor.MiddleName,
                            schedule.Weekday,
                            schedule.StartTime,
                            schedule.EndTime
                        })
                    .ToList();

                
                DoctorScheduleDataGrid.ItemsSource = schedules;
            }
        }

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.GoBack();
        }
    }
}
