using Microsoft.AspNetCore.Mvc;
using Org.BouncyCastle.Crypto.Generators;
using System;
using Microsoft.EntityFrameworkCore;

public class User
{
    public int id { get; set; }

    public string username { get; set; } = null!;
    public string email { get; set; } = null!;

    public string password_hash { get; set; } = null!;

    public bool is_active { get; set; } = true;

    public DateTime created_at { get; set; } = DateTime.UtcNow;
    public DateTime updated_at { get; set; } = DateTime.UtcNow;
}

public record RegisterRequest(
    string Username,
    string Email,
    string Password
);

public record LoginRequest(
    string Email,
    string Password
);

namespace backend
{
    [ApiController]
    [Route("api/auth")]
    public class AuthController : ControllerBase
    {
        private readonly AppDbContext _db;

        public AuthController(AppDbContext db)
        {
            _db = db;
        }

        // REGISTER
        [HttpPost("register")]
        public async Task<IActionResult> Register(RegisterRequest request)
        {
            if (await _db.users.AnyAsync(u =>
                u.email == request.Email || u.username == request.Username))
            {
                return BadRequest("User already exists");
            }

            var user = new User
            {
                username = request.Username,
                email = request.Email,
                password_hash = BCrypt.Net.BCrypt.HashPassword(request.Password)
            };

            _db.users.Add(user);
            await _db.SaveChangesAsync();

            return Ok(new { message = "User registered" });
        }

        // LOGIN
        [HttpPost("login")]
        public async Task<IActionResult> Login(LoginRequest request)
        {
            var user = await _db.users
                .FirstOrDefaultAsync(u => u.email == request.Email);

            if (user == null)
                return Unauthorized("Invalid credentials");

            bool passwordOk = BCrypt.Net.BCrypt.Verify(
                request.Password,
                user.password_hash
            );

            if (!passwordOk) return Unauthorized("Invalid credentials");

            return Ok(new
            {
                user.id,
                user.username,
                user.email
            });
        }
    }
}
