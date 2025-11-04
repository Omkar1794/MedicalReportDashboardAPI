using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MedicalReportDashboardAPI.Models
{
    public class MedicalFile
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string FileType { get; set; } // e.g., Lab Report, Prescription, X-Ray, etc.

        [Required]
        public string FileName { get; set; } // user-specified title

        [Required]
        public string StoredFileName { get; set; } // unique name on disk

        [Required]
        public string FilePath { get; set; } // relative or absolute path

        [Required]
        public string ContentType { get; set; } // MIME type

        [Required]
        public int UserId { get; set; }

        [ForeignKey(nameof(UserId))]
        public User? User { get; set; }

        public DateTime UploadedAt { get; set; } = DateTime.UtcNow;
    }
}