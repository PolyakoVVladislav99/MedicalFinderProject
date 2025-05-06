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
    /// Логика взаимодействия для MedicalCardPage.xaml
    /// </summary>
    public partial class MedicalCardPage : Page
    {
        public MedicalCardPage()
        {
            InitializeComponent();
            LoadMedicalCardData();
        }
        private void LoadMedicalCardData()
        {
            
            var user = SessionManager.CurrentUser;

            if (user == null)
            {
                MessageBox.Show("Не удалось найти информацию о пользователе.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            
            using (var context = new MedicalSpecialistServiceEntities4())
            {
                var medicalCard = context.MedicalCards
                                        .FirstOrDefault(m => m.UserID == user.UserID);

                if (medicalCard != null)
                {
                    
                    CreatedAtTextBlock.Text = $"Дата создания карты: {medicalCard.CreatedAt:dd.MM.yyyy}";

                    
                    var medicalRecords = context.MedicalRecords
                                               .Where(mr => mr.CardID == medicalCard.CardID)
                                               .Join(context.Doctors,
                                                     mr => mr.DoctorID,
                                                     doc => doc.DoctorID,
                                                     (mr, doc) => new
                                                     {
                                                         mr.RecordDate,
                                                         mr.Diagnosis,
                                                         mr.Recommendations,
                                                         DoctorName = doc.LastName + " " + doc.FirstName
                                                     })
                                               .ToList();
                    MedicalRecordsDataGrid.ItemsSource = medicalRecords;
                }
                
            }
        }
        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            NavigationService?.GoBack();
        }
    }
}
