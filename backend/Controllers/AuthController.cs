using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace backend.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly AppDbContext _db;
    private readonly SessionStore _sessions;
    private readonly PresenceService _presence;

    public AuthController(AppDbContext db, SessionStore sessions, PresenceService presence)
    {
        _db = db;
        _sessions = sessions;
        _presence = presence;
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register(RegisterRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Username) ||
            string.IsNullOrWhiteSpace(request.Email) ||
            string.IsNullOrEmpty(request.Password))
            return BadRequest(new { message = "Complete all fields." });

        var username = request.Username.Trim();
        var email = request.Email.Trim().ToLowerInvariant();

        if (username.Length is < 2 or > 50)
            return BadRequest(new { message = "The username must be between 2 and 50 characters long." });
        if (!email.Contains('@') || email.Length > 255)
            return BadRequest(new { message = "Enter a valid email address." });
        if (request.Password.Length < 6)
            return BadRequest(new { message = "The password must be at least 6 characters long." });

        if (await _db.Users.AnyAsync(u => u.Email == email || u.Username == username))
            return Conflict(new { message = "A user with this username or email address already exists." });

        var user = new User
        {
            Username = username,
            Email = email,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            LastActiveAt = DateTime.UtcNow
        };

        _db.Users.Add(user);
        await _db.SaveChangesAsync();
        return Ok(CreateLoginResponse(user));
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login(LoginRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrEmpty(request.Password))
            return BadRequest(new { message = "Enter your email address and password." });

        var email = request.Email.Trim().ToLowerInvariant();
        var user = await _db.Users.FirstOrDefaultAsync(u => u.Email == email && u.IsActive);

        if (user is null || !BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
            return Unauthorized(new { message = "Invalid email address or password." });

        user.LastActiveAt = DateTime.UtcNow;
        user.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();
        return Ok(CreateLoginResponse(user));
    }

    [HttpPost("logout")]
    public async Task<IActionResult> Logout()
    {
        var userId = HttpContext.GetCurrentUserId();
        var token = HttpContext.GetBearerToken();
        if (token is not null)
            _sessions.Remove(token);

        _presence.MarkOffline(userId);
        var user = await _db.Users.FindAsync(userId);
        if (user is not null)
        {
            user.LastActiveAt = DateTime.UtcNow;
            await _db.SaveChangesAsync();
        }

        return NoContent();
    }

    private LoginResponse CreateLoginResponse(User user)
    {
        var token = _sessions.Create(user.Id);
        _presence.MarkOnline(user.Id);
        return new LoginResponse(token, UserDto.From(user, true, false));
    }
}
