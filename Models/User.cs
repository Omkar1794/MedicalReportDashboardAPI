using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace MedicalReportDashboardAPI.Models
{
    public class User
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string FullName { get; set; }

        [Required, EmailAddress]
        public string Email { get; set; }

        public string? Gender { get; set; } // "Male" or "Female"
        public string? Phone { get; set; }

        [Required]
        public string PasswordHash { get; set; }

        public string? ProfileImagePath { get; set; } // optional

        public ICollection<MedicalFile>? MedicalFiles { get; set; }
    }
}