using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace MedicalFinderProject
{
    public class AppointmentViewModel
    {
        public int AppointmentID { get; set; }
        public string DoctorName { get; set; }
        public string Status { get; set; }
        public DateTime AppointmentDate { get; set; }
        public decimal Price { get; set; }
        public int? DocumentID { get; set; }
        public bool HasReceipt { get; set; }

        public Visibility ReceiptButtonVisibility => HasReceipt ? Visibility.Visible : Visibility.Collapsed;
        public Visibility CancelButtonVisibility => Status == "Ожидается" ? Visibility.Visible : Visibility.Collapsed;
    }
}
