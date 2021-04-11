using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ClinicLog.Web.Models
{
    public class PatientRequest
    {
        [Key]
        public int Id { get; set; }

        public string Subscject { get; set; }

        public string Description { get; set; }

        public string DoctorID { get; set; }

        public string PatientID { get; set; }

        [ForeignKey(nameof(DoctorID))]
        [InverseProperty(nameof(ApplicationUser.PatientRequests))]
        public ApplicationUser Doctor { get; set; }

        [ForeignKey(nameof(PatientID))]
        [InverseProperty(nameof(ApplicationUser.Requests))]
        public ApplicationUser Patient { get; set; }
    }
}
