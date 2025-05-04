using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MedicalFinderProject
{
    public class ServiceViewModel
    {
        public int DoctorID { get; set; }
        public int ServiceID { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public decimal Price { get; set; }


        public DateTime? SelectedDate { get; set; }
    }
}
