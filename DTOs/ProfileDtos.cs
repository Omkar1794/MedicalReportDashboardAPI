using System.ComponentModel.DataAnnotations;

namespace MedicalReportDashboardAPI.DTOs
{
    public class UpdateProfileDto
    {
        [Required] public string FullName { get; set; }
        [Required, EmailAddress] public string Email { get; set; }
        public string Gender { get; set; }
        public string Phone { get; set; }
    }

    public class ProfileDto
    {
        public int Id { get; set; }
        public string FullName { get; set; }
        public string Email { get; set; }
        public string Gender { get; set; }
        public string Phone { get; set; }
        public string ProfileImageUrl { get; set; }
    }
}
