using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace backend.Controllers;

[ApiController]
[Route("api/presence")]
public class PresenceController : ControllerBase
{
    private readonly AppDbContext _db;
    private readonly PresenceService _presence;

    public PresenceController(AppDbContext db, PresenceService presence)
    {
        _db = db;
        _presence = presence;
    }

    [HttpPost("heartbeat")]
    public async Task<IActionResult> Heartbeat()
    {
        var userId = HttpContext.GetCurrentUserId();
        _presence.MarkOnline(userId);
        var user = await _db.Users.FirstAsync(u => u.Id == userId);
        user.LastActiveAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();
        return NoContent();
    }
}
