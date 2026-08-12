using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace backend.Controllers;

[ApiController]
[Route("api/users")]
public class UsersController : ControllerBase
{
    private readonly AppDbContext _db;
    private readonly PresenceService _presence;
    private readonly MediaStorage _media;

    public UsersController(AppDbContext db, PresenceService presence, MediaStorage media)
    {
        _db = db;
        _presence = presence;
        _media = media;
    }

    [HttpGet("me")]
    public async Task<IActionResult> Me()
    {
        var userId = HttpContext.GetCurrentUserId();
        var user = await _db.Users.AsNoTracking().FirstAsync(u => u.Id == userId);
        return Ok(ToDto(user, false));
    }

    [HttpPut("me")]
    public async Task<IActionResult> UpdateProfile(UpdateProfileRequest request)
    {
        var userId = HttpContext.GetCurrentUserId();
        if (string.IsNullOrWhiteSpace(request.Username))
            return BadRequest(new { message = "The username cannot be empty." });
        var username = request.Username.Trim();
        if (username.Length is < 2 or > 50)
            return BadRequest(new { message = "The username must be between 2 and 50 characters long." });
        if (await _db.Users.AnyAsync(u => u.Id != userId && u.Username == username))
            return Conflict(new { message = "This username is already taken." });

        var user = await _db.Users.FirstAsync(u => u.Id == userId);
        user.Username = username;
        user.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();
        return Ok(ToDto(user, false));
    }

    [HttpPost("me/avatar")]
    [Consumes("multipart/form-data")]
    public async Task<IActionResult> UploadAvatar([FromForm] IFormFile? image, CancellationToken cancellationToken)
    {
        if (image is null)
            return BadRequest(new { message = "Select a profile picture." });

        string path;
        try
        {
            path = await _media.SaveImageAsync(image, "avatars", cancellationToken);
        }
        catch (InvalidOperationException exception)
        {
            return BadRequest(new { message = exception.Message });
        }

        var userId = HttpContext.GetCurrentUserId();
        var user = await _db.Users.FirstAsync(u => u.Id == userId, cancellationToken);
        var oldAvatar = user.AvatarPath;
        user.AvatarPath = path;
        user.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync(cancellationToken);
        _media.Delete(oldAvatar);
        return Ok(ToDto(user, false));
    }

    [HttpGet("search")]
    public async Task<IActionResult> Search([FromQuery] string? query = null)
    {
        var userId = HttpContext.GetCurrentUserId();
        var normalized = query?.Trim() ?? string.Empty;
        if (normalized.Length < 2)
            return Ok(Array.Empty<UserDto>());

        var contactIds = await _db.Contacts
            .Where(x => x.OwnerUserId == userId)
            .Select(x => x.ContactUserId)
            .ToListAsync();

        var users = await _db.Users.AsNoTracking()
            .Where(u => u.Id != userId && u.IsActive &&
                        (u.Username.Contains(normalized) || u.Email.Contains(normalized)))
            .OrderBy(u => u.Username)
            .Take(30)
            .ToListAsync();

        var contacts = contactIds.ToHashSet();
        return Ok(users.Select(u => ToDto(u, contacts.Contains(u.Id))));
    }

    [HttpGet("contacts")]
    public async Task<IActionResult> Contacts()
    {
        var userId = HttpContext.GetCurrentUserId();
        var users = await _db.Contacts.AsNoTracking()
            .Where(x => x.OwnerUserId == userId && x.Contact.IsActive)
            .Select(x => x.Contact)
            .OrderBy(x => x.Username)
            .ToListAsync();

        return Ok(users
            .OrderByDescending(x => _presence.IsOnline(x.Id))
            .ThenBy(x => x.Username)
            .Select(u => ToDto(u, true)));
    }

    [HttpPost("contacts/{contactUserId:int}")]
    public async Task<IActionResult> AddContact(int contactUserId)
    {
        var userId = HttpContext.GetCurrentUserId();
        if (contactUserId == userId)
            return BadRequest(new { message = "You cannot add yourself to your contacts." });
        if (!await _db.Users.AnyAsync(u => u.Id == contactUserId && u.IsActive))
            return NotFound(new { message = "User not found." });

        await AddContactPairAsync(userId, contactUserId);
        await AddContactPairAsync(contactUserId, userId);
        await _db.SaveChangesAsync();

        var user = await _db.Users.AsNoTracking().FirstAsync(u => u.Id == contactUserId);
        return Ok(ToDto(user, true));
    }

    [HttpDelete("contacts/{contactUserId:int}")]
    public async Task<IActionResult> RemoveContact(int contactUserId)
    {
        var userId = HttpContext.GetCurrentUserId();
        var entries = await _db.Contacts.Where(x =>
                (x.OwnerUserId == userId && x.ContactUserId == contactUserId) ||
                (x.OwnerUserId == contactUserId && x.ContactUserId == userId))
            .ToListAsync();
        if (entries.Count > 0)
        {
            _db.Contacts.RemoveRange(entries);
            await _db.SaveChangesAsync();
        }
        return NoContent();
    }

    private async Task AddContactPairAsync(int ownerId, int contactId)
    {
        if (!await _db.Contacts.AnyAsync(x => x.OwnerUserId == ownerId && x.ContactUserId == contactId))
        {
            _db.Contacts.Add(new UserContact
            {
                OwnerUserId = ownerId,
                ContactUserId = contactId,
                CreatedAt = DateTime.UtcNow
            });
        }
    }

    private UserDto ToDto(User user, bool isContact) =>
        UserDto.From(user, _presence.IsOnline(user.Id), isContact);
}
