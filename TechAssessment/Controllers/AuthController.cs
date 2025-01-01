using Application.Commons;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Persistence.Data;
using System.Security.Cryptography;
using System.Text;

namespace TechAssessment.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly AplicationDbContext _context;
        private readonly JwtHelper _jwtHelper;

        public AuthController(AplicationDbContext context, JwtHelper jwtHelper)
        {
            _context = context;
            _jwtHelper = jwtHelper;
        }

        [Authorize(Roles = "Admin")]
        [HttpPost("admin-endpoint")]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            var user = await _context.users
                .FirstOrDefaultAsync(u => u.Username == request.Username);

            if (user == null || user.PasswordHash != HashPassword(request.Password))
                return Unauthorized(new { message = "Invalid credentials" });

            var token = _jwtHelper.GenerateToken(user.Username, user.Role);
            return Ok(new { Token = token, Role = user.Role });
        }

        private string HashPassword(string password)
        {
            using var sha256 = SHA256.Create();
            return Convert.ToBase64String(sha256.ComputeHash(Encoding.UTF8.GetBytes(password)));
        }


    }

    public class LoginRequest
    {
        public string Username { get; set; }
        public string Password { get; set; }
    }
}
