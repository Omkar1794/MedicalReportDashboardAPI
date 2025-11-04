using System.Security.Claims;
using MedicalReportDashboardAPI.Data;
using MedicalReportDashboardAPI.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace MedicalReportDashboardAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ProfileController : ControllerBase
    {
        private readonly AppDbContext _db;
        private readonly IWebHostEnvironment _env;

        public ProfileController(AppDbContext db, IWebHostEnvironment env)
        {
            _db = db;
            _env = env;
        }

        private int GetUserIdFromClaims()
        {
            var uid = User.FindFirst("uid")?.Value;
            return uid != null ? int.Parse(uid) : 0;
        }

        [Authorize]
        [HttpGet("me")]
        public async Task<IActionResult> GetMyProfile()
        {
            var userId = GetUserIdFromClaims();
            if (userId == 0) return Unauthorized();

            var user = await _db.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Id == userId);
            if (user == null) return NotFound();

            var dto = new ProfileDto
            {
                Id = user.Id,
                FullName = user.FullName,
                Email = user.Email,
                Gender = user.Gender,
                Phone = user.Phone,
                ProfileImageUrl = string.IsNullOrEmpty(user.ProfileImagePath)
                    ? null
                    : Url.Content($"~/Uploads/{Path.GetFileName(user.ProfileImagePath)}")
            };

            return Ok(dto);
        }

        [Authorize]
        [HttpPut("me")]
        public async Task<IActionResult> UpdateProfile([FromBody] UpdateProfileDto dto)
        {
            var userId = GetUserIdFromClaims();
            if (userId == 0) return Unauthorized();

            var user = await _db.Users.FirstOrDefaultAsync(u => u.Id == userId);
            if (user == null) return NotFound();

            // Check for duplicate email
            if (!string.Equals(user.Email, dto.Email, StringComparison.OrdinalIgnoreCase))
            {
                var exists = await _db.Users.AnyAsync(u => u.Email.ToLower() == dto.Email.ToLower() && u.Id != userId);
                if (exists) return BadRequest(new { message = "Email already in use." });
            }

            user.FullName = dto.FullName;
            user.Email = dto.Email;
            user.Gender = dto.Gender;
            user.Phone = dto.Phone;

            await _db.SaveChangesAsync();

            return Ok(new { message = "Profile updated." });
        }

        [Authorize]
        [HttpPost("me/profile-image")]
        public async Task<IActionResult> UploadProfileImage([FromForm] ProfileImageUploadDto model)
        {
            var userId = GetUserIdFromClaims();
            if (userId == 0) return Unauthorized();

            var image = model.Image;
            if (image == null || image.Length == 0)
                return BadRequest(new { message = "No image uploaded." });

            var allowed = new[] { ".png", ".jpg", ".jpeg" };
            var ext = Path.GetExtension(image.FileName).ToLower();
            if (!allowed.Contains(ext))
                return BadRequest(new { message = "Only jpg/png allowed." });

            var uploads = Path.Combine(Directory.GetCurrentDirectory(), "Uploads");
            if (!Directory.Exists(uploads)) Directory.CreateDirectory(uploads);

            var unique = $"profile_{userId}_{Guid.NewGuid()}{ext}";
            var path = Path.Combine(uploads, unique);

            using (var fs = new FileStream(path, FileMode.Create))
                await image.CopyToAsync(fs);

            var user = await _db.Users.FirstOrDefaultAsync(u => u.Id == userId);
            if (user == null) return NotFound();

            user.ProfileImagePath = path;
            await _db.SaveChangesAsync();

            return Ok(new
            {
                message = "Profile image uploaded.",
                url = Url.Content($"~/Uploads/{unique}")
            });
        }
    }
}