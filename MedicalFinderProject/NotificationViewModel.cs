using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MedicalFinderProject
{
    public class NotificationViewModel
    {
        public string Message { get; set; }
        public bool IsRead { get; set; }
        public DateTime SentAt { get; set; }
    }

}
