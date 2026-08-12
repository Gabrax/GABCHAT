using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace backend.Controllers;

[ApiController]
[Route("api/messages")]
public class MessagesController : ControllerBase
{
    private readonly AppDbContext _db;
    private readonly MediaStorage _media;

    public MessagesController(AppDbContext db, MediaStorage media)
    {
        _db = db;
        _media = media;
    }

    [HttpGet("{otherUserId:int}")]
    public async Task<IActionResult> Conversation(
        int otherUserId,
        [FromQuery] long afterId = 0,
        CancellationToken cancellationToken = default)
    {
        var userId = HttpContext.GetCurrentUserId();
        if (!await _db.Users.AnyAsync(u => u.Id == otherUserId && u.IsActive, cancellationToken))
            return NotFound(new { message = "User not found." });

        var query = _db.Messages.AsNoTracking().Where(m =>
            m.Id > afterId &&
            ((m.SenderId == userId && m.RecipientId == otherUserId) ||
             (m.SenderId == otherUserId && m.RecipientId == userId)));

        List<ChatMessage> messages;
        if (afterId == 0)
        {
            messages = await query.OrderByDescending(m => m.Id).Take(100).ToListAsync(cancellationToken);
            messages.Reverse();
        }
        else
        {
            messages = await query.OrderBy(m => m.Id).Take(100).ToListAsync(cancellationToken);
        }

        var unread = await _db.Messages.Where(m =>
                m.SenderId == otherUserId && m.RecipientId == userId && m.ReadAt == null)
            .ToListAsync(cancellationToken);
        if (unread.Count > 0)
        {
            var now = DateTime.UtcNow;
            foreach (var message in unread)
                message.ReadAt = now;
            await _db.SaveChangesAsync(cancellationToken);
        }

        return Ok(messages.Select(MessageDto.From));
    }

    [HttpPost("{recipientId:int}")]
    [Consumes("multipart/form-data")]
    [RequestSizeLimit(9 * 1024 * 1024)]
    public async Task<IActionResult> Send(
        int recipientId,
        [FromForm] string? text,
        [FromForm] IFormFile? image,
        CancellationToken cancellationToken)
    {
        var userId = HttpContext.GetCurrentUserId();
        if (recipientId == userId)
            return BadRequest(new { message = "You cannot send a message to yourself." });
        if (!await _db.Users.AnyAsync(u => u.Id == recipientId && u.IsActive, cancellationToken))
            return NotFound(new { message = "Recipient not found." });

        var trimmedText = text?.Trim();
        if (string.IsNullOrEmpty(trimmedText) && image is null)
            return BadRequest(new { message = "Enter a message or select an image." });
        if (trimmedText?.Length > 4000)
            return BadRequest(new { message = "A message can contain up to 4,000 characters." });

        string? imagePath = null;
        try
        {
            if (image is not null)
                imagePath = await _media.SaveImageAsync(image, "messages", cancellationToken);

            var message = new ChatMessage
            {
                SenderId = userId,
                RecipientId = recipientId,
                TextContent = string.IsNullOrEmpty(trimmedText) ? null : trimmedText,
                ImagePath = imagePath,
                SentAt = DateTime.UtcNow
            };
            _db.Messages.Add(message);

            if (!await _db.Contacts.AnyAsync(x => x.OwnerUserId == userId && x.ContactUserId == recipientId, cancellationToken))
            {
                _db.Contacts.Add(new UserContact { OwnerUserId = userId, ContactUserId = recipientId });
                _db.Contacts.Add(new UserContact { OwnerUserId = recipientId, ContactUserId = userId });
            }

            await _db.SaveChangesAsync(cancellationToken);
            return Ok(MessageDto.From(message));
        }
        catch (InvalidOperationException exception)
        {
            return BadRequest(new { message = exception.Message });
        }
        catch
        {
            _media.Delete(imagePath);
            throw;
        }
    }
}
