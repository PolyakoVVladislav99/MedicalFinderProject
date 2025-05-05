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
        public ObservableCollection<ServiceViewModel> Cart { get; set; }  

        public DoctorServicesPage(DoctorViewModel doctor)
        {
            InitializeComponent();
            this.doctor = doctor;
            Cart = new ObservableCollection<ServiceViewModel>();
            LoadDoctorServices();
            this.DataContext = this;  
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
                        Price = s.Price,
                        ServiceID = s.ServiceID,
                        DoctorID = s.DoctorID,
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
            var datePicker = new DatePicker
            {
                SelectedDate = DateTime.Today,
                DisplayDateStart = DateTime.Today,
                Margin = new Thickness(10)
            };

            var dialogPanel = new StackPanel();
            dialogPanel.Children.Add(new TextBlock
            {
                Text = $"Выберите дату для услуги: {service.Name}",
                Margin = new Thickness(10)
            });
            dialogPanel.Children.Add(datePicker);

            var dialogWindow = new Window
            {
                Title = "Выбор даты",
                Content = dialogPanel,
                Width = 300,
                Height = 150,
                WindowStartupLocation = WindowStartupLocation.CenterScreen,
                ResizeMode = ResizeMode.NoResize,
                WindowStyle = WindowStyle.ToolWindow,
                Owner = Application.Current.MainWindow,
                ShowInTaskbar = false
            };

            var okButton = new Button
            {
                Content = "ОК",
                Width = 60,
                Margin = new Thickness(10),
                HorizontalAlignment = HorizontalAlignment.Center,
                
            };
            okButton.Click += (s, e) => dialogWindow.DialogResult = true;
            dialogPanel.Children.Add(okButton);

            if (dialogWindow.ShowDialog() == true && datePicker.SelectedDate.HasValue)
            {
                service.SelectedDate = datePicker.SelectedDate.Value; 
                Cart.Add(service);
                MessageBox.Show($"Услуга '{service.Name}' добавлена в корзину на дату {service.SelectedDate:dd.MM.yyyy}.");
            }
            if (service != null && !Cart.Contains(service))
            {
                Cart.Add(service);
                MessageBox.Show($"Услуга {service.Name} добавлена в корзину.");
            }
        }

        private void GoToCartButton_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new CartPage(Cart));  
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new MainPage());
        }
    }
}
