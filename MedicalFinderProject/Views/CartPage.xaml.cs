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

namespace MedicalFinderProject.Views
{
    /// <summary>
    /// Логика взаимодействия для CartPage.xaml
    /// </summary>
    public partial class CartPage : Page
    {
        public ObservableCollection<ServiceViewModel> Cart { get; set; }  // Получаем корзину от предыдущей страницы

        public decimal TotalAmount => Cart.Sum(service => service.Price);  // Общая сумма товаров в корзине

        public CartPage(ObservableCollection<ServiceViewModel> cart)
        {
            InitializeComponent();
            Cart = cart;
            this.DataContext = this;  // Устанавливаем DataContext для биндинга
        }
        private void ClearCart_Click(object sender, RoutedEventArgs e)
        {
            Cart.Clear();
            CartListView.ItemsSource = null;
            TotalTextBlock.Text = "Итого: 0 руб.";
        }

        private void BackToServices_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.GoBack(); // или NavigationService.Navigate(new DoctorServicesPage(...));
        }
        
    }
}
