using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MedicalFinderProject
{
    public class DoctorViewModel
    {
        public int DoctorID { get; set; }
        public string FullName { get; set; }
        public string ClinicName { get; set; }
        public string Specialization { get; set; }
        public string Experience { get; set; }
        public double Rating { get; set; }
    }
}
