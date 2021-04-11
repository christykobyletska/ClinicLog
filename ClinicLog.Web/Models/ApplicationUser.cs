using Microsoft.AspNetCore.Identity;
using System.Collections.Generic;

namespace ClinicLog.Web.Models
{
    public class ApplicationUser : IdentityUser
    {
        public string FullName { get; set; }

        public virtual ICollection<PatientRequest> PatientRequests { get; set; } = new List<PatientRequest>();

        public virtual ICollection<PatientRequest> Requests { get; set; } = new List<PatientRequest>();
    }
}
