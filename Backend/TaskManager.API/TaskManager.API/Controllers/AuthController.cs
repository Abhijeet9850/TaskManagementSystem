using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TaskManager.API.Data;
using TaskManager.API.DTOs;
using TaskManager.API.Models;
using TaskManager.API.Services;

namespace TaskManager.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly AppDbContext _db;
        private readonly ITokenService _tokenService;

        public AuthController(AppDbContext db, ITokenService tokenService)
        {
            _db = db;
            _tokenService = tokenService;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register(RegisterDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var emailExists = await _db.Users.AnyAsync(u => u.Email.ToLower() == dto.Email.ToLower());
            if (emailExists)
                return Conflict(new { message = "A user with this email already exists." });

            var user = new User
            {
                FullName = dto.FullName,
                Email = dto.Email,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password),
                Role = dto.Role
            };

            _db.Users.Add(user);
            await _db.SaveChangesAsync();

            var (token, expiresAt) = _tokenService.GenerateToken(user, rememberMe: false);

            return Ok(new AuthResponseDto
            {
                Token = token,
                ExpiresAt = expiresAt,
                UserId = user.Id,
                FullName = user.FullName,
                Email = user.Email,
                Role = user.Role.ToString()
            });
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login(LoginDto dto)
        {
            var user = await _db.Users.FirstOrDefaultAsync(u => u.Email.ToLower() == dto.Email.ToLower());
            if (user == null || !BCrypt.Net.BCrypt.Verify(dto.Password, user.PasswordHash))
                return Unauthorized(new { message = "Invalid email or password." });

            var (token, expiresAt) = _tokenService.GenerateToken(user, dto.RememberMe);

            return Ok(new AuthResponseDto
            {
                Token = token,
                ExpiresAt = expiresAt,
                UserId = user.Id,
                FullName = user.FullName,
                Email = user.Email,
                Role = user.Role.ToString()
            });
        }

        // Client-side logout: the client discards the JWT. This endpoint exists for
        // consistency with the required API surface and can be extended with a
        // server-side token blocklist if needed.
        [HttpPost("logout")]
        public IActionResult Logout()
        {
            return Ok(new { message = "Logged out successfully." });
        }
    }
}
