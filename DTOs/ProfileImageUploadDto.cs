using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace MedicalReportDashboardAPI.DTOs
{
    public class ProfileImageUploadDto
    {
        [Required]
        public IFormFile Image { get; set; }
    }
}