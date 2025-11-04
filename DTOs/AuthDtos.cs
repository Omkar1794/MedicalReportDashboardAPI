using System.ComponentModel.DataAnnotations;

namespace MedicalReportDashboardAPI.DTOs
{
    public class SignUpDto
    {
        [Required] public string FullName { get; set; }
        [Required, EmailAddress] public string Email { get; set; }
        public string Gender { get; set; }
        public string Phone { get; set; }
        [Required] public string Password { get; set; }
    }

    public class LoginDto
    {
        [Required, EmailAddress] public string Email { get; set; }
        [Required] public string Password { get; set; }
    }

    public class AuthResponseDto
    {
        public string Token { get; set; }
        public int UserId { get; set; }
    }
}
