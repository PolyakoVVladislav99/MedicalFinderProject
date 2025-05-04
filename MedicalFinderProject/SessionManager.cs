using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MedicalFinderProject.dbModel;

namespace MedicalFinderProject
{
     public static class SessionManager
    {
        public static Users CurrentUser { get; set; }
    }
}
