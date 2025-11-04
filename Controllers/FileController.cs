using MedicalReportDashboardAPI.Data;
using MedicalReportDashboardAPI.DTOs;
using MedicalReportDashboardAPI.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace MedicalReportDashboardAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class FileController : ControllerBase
    {
        private readonly AppDbContext _db;
        private readonly IWebHostEnvironment _env;
        private readonly ILogger<FileController> _logger;

        private readonly string[] _allowedExtensions = new[] { ".pdf", ".png", ".jpg", ".jpeg" };

        public FileController(AppDbContext db, IWebHostEnvironment env, ILogger<FileController> logger)
        {
            _db = db;
            _env = env;
            _logger = logger;
        }

        private int GetUserIdFromClaims()
        {
            var uid = User.FindFirst("uid")?.Value;
            return uid != null ? int.Parse(uid) : 0;
        }

        [Authorize]
        [HttpPost("upload")]
        [RequestSizeLimit(20_000_000)] // ~20 MB
        public async Task<IActionResult> Upload([FromForm] FileUploadDto model)
        {
            var userId = GetUserIdFromClaims();
            if (userId == 0)
                return Unauthorized(new { message = "Invalid or missing user token." });

            var file = model.File;
            if (file == null || file.Length == 0)
                return BadRequest(new { message = "No file uploaded." });

            var ext = Path.GetExtension(file.FileName).ToLower();
            if (!_allowedExtensions.Contains(ext))
                return BadRequest(new { message = "Unsupported file type." });

            var uploads = Path.Combine(Directory.GetCurrentDirectory(), "Uploads");
            if (!Directory.Exists(uploads)) Directory.CreateDirectory(uploads);

            var storedFileName = $"{Guid.NewGuid()}{ext}";
            var filePath = Path.Combine(uploads, storedFileName);

            using (var fs = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(fs);
            }

            var medicalFile = new MedicalFile
            {
                FileType = model.FileType,
                FileName = model.FileName,
                StoredFileName = storedFileName,
                FilePath = filePath,
                ContentType = file.ContentType,
                UserId = userId
            };

            _db.MedicalFiles.Add(medicalFile);
            await _db.SaveChangesAsync();

            return Ok(new { message = "Uploaded", fileId = medicalFile.Id });
        }

        [Authorize]
        [HttpGet("list")]
        public async Task<IActionResult> ListMyFiles()
        {
            var userId = GetUserIdFromClaims();
            if (userId == 0) return Unauthorized();

            var files = await _db.MedicalFiles
                .Where(f => f.UserId == userId)
                .OrderByDescending(f => f.UploadedAt)
                .Select(f => new
                {
                    f.Id,
                    f.FileName,
                    f.FileType,
                    Url = Url.Action("GetFile", "File", new { id = f.Id }, Request.Scheme)
                }).ToListAsync();

            return Ok(files);
        }

        [Authorize]
        [HttpGet("{id}")]
        public async Task<IActionResult> GetFile(int id)
        {
            var userId = GetUserIdFromClaims();
            if (userId == 0) return Unauthorized();

            var file = await _db.MedicalFiles.FirstOrDefaultAsync(f => f.Id == id && f.UserId == userId);
            if (file == null) return NotFound();

            var fs = System.IO.File.OpenRead(file.FilePath);
            return File(fs, file.ContentType, file.StoredFileName);
        }

        [Authorize]
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteFile(int id)
        {
            var userId = GetUserIdFromClaims();
            if (userId == 0) return Unauthorized();

            var file = await _db.MedicalFiles.FirstOrDefaultAsync(f => f.Id == id && f.UserId == userId);
            if (file == null) return NotFound();

            try
            {
                if (System.IO.File.Exists(file.FilePath))
                    System.IO.File.Delete(file.FilePath);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Deleting file from disk failed.");
            }

            _db.MedicalFiles.Remove(file);
            await _db.SaveChangesAsync();

            return Ok(new { message = "Deleted" });
        }
    }
}