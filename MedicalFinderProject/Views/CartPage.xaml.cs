using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
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
using iTextSharp.text.pdf;
using iTextSharp.text;
using MedicalFinderProject.dbModel;
using Path = System.IO.Path;
using ZXing;
using System.Drawing.Imaging;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Data.Entity;
using System.Xml.Linq;
using ZXing.Aztec.Internal;


namespace MedicalFinderProject.Views
{
    /// <summary>
    /// Логика взаимодействия для CartPage.xaml
    /// </summary>
    public partial class CartPage : Page
    {
        public ObservableCollection<ServiceViewModel> Cart { get; set; }  

        public decimal TotalAmount => Cart.Sum(service => service.Price); 
        

        public CartPage(ObservableCollection<ServiceViewModel> cart)
        {
            InitializeComponent();
            Cart = cart;
            Cart.CollectionChanged += Cart_CollectionChanged;
            UpdatePayButtonVisibility();
            this.DataContext = this;  
            TotalTextBlock.Text = $" Итоговая сумма: { Cart.Sum(service => service.Price) }";
        }
        private void ClearCart_Click(object sender, RoutedEventArgs e)
        {
            Cart.Clear();
            TotalTextBlock.Text = "Итого: 0 руб.";
        }

        private void BackToServices_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.GoBack(); 
        }
        private void PayButton_Click(object sender, RoutedEventArgs e)
        {
            int userId = SessionManager.CurrentUser.UserID;
            List<int> newAppointmentIds = new List<int>();

            using (var context = new MedicalSpecialistServiceEntities3())
            {
                foreach (var service in Cart)
                {
                    var appointment = new Appointments
                    {
                        UserID = userId,
                        DoctorID = service.DoctorID,
                        ServiceID = service.ServiceID,
                        AppointmentDate = service.SelectedDate ?? DateTime.Now,
                        Status = "Ожидается"
                    };

                    context.Appointments.Add(appointment);
                    context.SaveChanges();

                    newAppointmentIds.Add(appointment.AppointmentID);

                    var medicalCard = context.MedicalCards.FirstOrDefault(mc => mc.UserID == userId);

                   
                    if (medicalCard == null)
                    {
                        medicalCard = new MedicalCards
                        {
                            UserID = userId,
                            CreatedAt = DateTime.Now
                        };
                        context.MedicalCards.Add(medicalCard);
                        context.SaveChanges();  
                    }

                    
                    var medicalRecord = new MedicalRecords
                    {
                        CardID = medicalCard.CardID,  
                        DoctorID = service.DoctorID,  
                        RecordDate = appointment.AppointmentDate,  
                        Diagnosis = "",  
                        Recommendations = ""  
                    };

                    context.MedicalRecords.Add(medicalRecord);
                    context.SaveChanges();
                }
                var savedAppointments = context.Appointments
                    .Where(a => newAppointmentIds.Contains(a.AppointmentID))
                    .Include(a => a.Doctors)
                    .Include(a => a.Services)
                    .Include(a => a.Users)
                    .ToList();

                
                string pdfPath = GeneratePdfReceipt(Cart, SessionManager.CurrentUser);

                
                byte[] fileBytes = File.ReadAllBytes(pdfPath);

                foreach (var appointment in savedAppointments)
                {
                    var document = new Documents
                    {
                        UserID = userId,
                        UploadDate = DateTime.Now,
                        Description = $"Чек за {appointment.AppointmentDate:dd.MM.yyyy} (AppointmentID: {appointment.AppointmentID})",
                        FileName = Path.GetFileName(pdfPath),
                        FileData = fileBytes
                    };

                    context.Documents.Add(document);
                    context.SaveChanges();
                    appointment.DocumentID = document.DocumentID;
                }
                

                context.SaveChanges(); 

                
            }

            Cart.Clear();
            LogUserAction(userId, "Успешная оплата");
            MessageBox.Show("Оплата прошла успешно!");

            NavigationService.Navigate(new MyAppointmentsPage());

        }
        private string GeneratePdfReceipt(IEnumerable<ServiceViewModel> services, Users user)
        {
            string fileName = $"Чек_{user.FullName}_{DateTime.Now:yyyyMMddHHmmss}.pdf";
            string path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), fileName);

            
            string fontPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Fonts), "arial.ttf");
            BaseFont baseFont = BaseFont.CreateFont(fontPath, BaseFont.IDENTITY_H, BaseFont.EMBEDDED);
            var font = new iTextSharp.text.Font(baseFont, 12);

            using (FileStream fs = new FileStream(path, FileMode.Create))
            {
                var doc = new Document(PageSize.A4);
                PdfWriter writer = PdfWriter.GetInstance(doc, fs);
                doc.Open();

                
                doc.Add(new iTextSharp.text.Paragraph("Медицинский чек", new iTextSharp.text.Font(baseFont, 16, Font.BOLD)));
                doc.Add(new iTextSharp.text.Paragraph($"Пациент: {user.FullName}", font));
                doc.Add(new iTextSharp.text.Paragraph($"Дата: {DateTime.Now:dd.MM.yyyy HH:mm}", font));
                doc.Add(new iTextSharp.text.Paragraph(" ", font));

                
                PdfPTable table = new PdfPTable(3) { WidthPercentage = 100 };
                table.AddCell(new PdfPCell(new Phrase("Название", font)));
                table.AddCell(new PdfPCell(new Phrase("Дата проведения", font)));
                table.AddCell(new PdfPCell(new Phrase("Цена", font)));

                foreach (var s in services)
                {
                    table.AddCell(new PdfPCell(new Phrase(s.Name, font)));
                    table.AddCell(new PdfPCell(new Phrase(s.SelectedDate?.ToString("dd.MM.yyyy") ?? "", font)));
                    table.AddCell(new PdfPCell(new Phrase($"{s.Price} руб.", font)));
                }

                doc.Add(table);

                doc.Add(new iTextSharp.text.Paragraph(" ", font));
                doc.Add(new iTextSharp.text.Paragraph($"Итого: {services.Sum(x => x.Price)} руб.", font));
                doc.Add(new iTextSharp.text.Paragraph(" ", font));

                
                string qrContent = "https://youtu.be/dQw4w9WgXcQ"; 

                var qrWriter = new ZXing.BarcodeWriter
                {
                    Format = ZXing.BarcodeFormat.QR_CODE,
                    Options = new ZXing.Common.EncodingOptions
                    {
                        Height = 150,
                        Width = 150,
                        Margin = 1
                    }
                };

                using (var bitmap = qrWriter.Write(qrContent))
                using (var ms = new MemoryStream())
                {
                    bitmap.Save(ms, System.Drawing.Imaging.ImageFormat.Png);
                    iTextSharp.text.Image qrImage = iTextSharp.text.Image.GetInstance(ms.ToArray());
                    qrImage.Alignment = Element.ALIGN_RIGHT;
                    doc.Add(qrImage);
                }

                doc.Add(new iTextSharp.text.Paragraph("Сканируйте QR-код — получайте бонусы!", font));
                doc.Close();
            }

            return path;
        }

        
        private void Cart_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            UpdatePayButtonVisibility();
        }
        private void UpdatePayButtonVisibility()
        {
            PayButton.Visibility = Cart.Any() ? Visibility.Visible : Visibility.Collapsed;
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
