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
    /// Логика взаимодействия для DoctorServicesPage.xaml
    /// </summary>
    public partial class DoctorServicesPage : Page
    {
        private DoctorViewModel doctor;

        public ObservableCollection<ServiceViewModel> DoctorServices { get; set; }
        public ObservableCollection<ServiceViewModel> Cart { get; set; }  // Для хранения добавленных в корзину услуг

        public DoctorServicesPage(DoctorViewModel doctor)
        {
            InitializeComponent();
            this.doctor = doctor;
            Cart = new ObservableCollection<ServiceViewModel>();
            LoadDoctorServices();
            this.DataContext = this;  // Устанавливаем DataContext для биндинга
        }
        private void LoadDoctorServices()
        {
            
            using (var context = new MedicalSpecialistServiceEntities3())
            {
                var services = context.Services
                    .Where(s => s.DoctorID == doctor.DoctorID)
                    .Select(s => new ServiceViewModel
                    {
                        Name = s.Name,
                        Description = s.Description,
                        Price = s.Price
                    }).ToList();

                if (services.Count == 0)
                {
                    NoServicesText.Visibility = Visibility.Visible;
                    ServicesList.Visibility = Visibility.Collapsed;
                }
                else
                {
                    NoServicesText.Visibility = Visibility.Collapsed;
                    ServicesList.Visibility = Visibility.Visible;
                    ServicesList.ItemsSource = services;
                }
            }
        }
        public ICommand AddToCartCommand => new RelayCommand<ServiceViewModel>(AddToCart);

        private void AddToCart(ServiceViewModel service)
        {
            if (service != null && !Cart.Contains(service))
            {
                Cart.Add(service);
                MessageBox.Show($"Услуга {service.Name} добавлена в корзину.");
            }
        }

        private void GoToCartButton_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new CartPage(Cart));  // Переход на страницу корзины
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new MainPage());
        }
    }
}
