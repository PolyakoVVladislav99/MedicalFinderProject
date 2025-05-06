using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
    /// Логика взаимодействия для FavoritesPage.xaml
    /// </summary>
    public partial class FavoritesPage : Page
    {
        public ObservableCollection<ServiceViewModel> FavoriteServices { get; set; }

        public FavoritesPage()
        {
            InitializeComponent();
            this.DataContext = this;
            FavoriteServices = new ObservableCollection<ServiceViewModel>();
            LoadFavoriteServices();
        }

        private void LoadFavoriteServices()
        {
            using (var context = new MedicalSpecialistServiceEntities4())
            {
                int currentUserId = SessionManager.CurrentUser.UserID;

                var favoriteServices = context.Favorites
                    .Where(f => f.UserID == currentUserId)
                    .Join(context.Services, f => f.ServiceID, s => s.ServiceID, (f, s) => new ServiceViewModel
                    {
                        Name = s.Name,
                        Description = s.Description,
                        Price = s.Price,
                        ServiceID = s.ServiceID
                    }).ToList();

                foreach (var service in favoriteServices)
                {
                    FavoriteServices.Add(service);
                }

                FavoritesList.ItemsSource = FavoriteServices;
            }
        }

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.GoBack();
        }
    }
}
