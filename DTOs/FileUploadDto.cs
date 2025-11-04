using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace MedicalReportDashboardAPI.DTOs
{
    public class FileUploadDto
    {
        [Required] public string FileType { get; set; }
        [Required] public string FileName { get; set; }
        [Required] public IFormFile File { get; set; }
    }
}