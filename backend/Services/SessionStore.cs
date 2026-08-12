using System.Collections.Concurrent;
using System.Security.Cryptography;

namespace backend;

public sealed class SessionStore
{
    private readonly ConcurrentDictionary<string, SessionInfo> _sessions = new();
    private static readonly TimeSpan Lifetime = TimeSpan.FromDays(14);

    public string Create(int userId)
    {
        RemoveUserSessions(userId);
        var token = Convert.ToBase64String(RandomNumberGenerator.GetBytes(48))
            .Replace('+', '-').Replace('/', '_').TrimEnd('=');
        _sessions[token] = new SessionInfo(userId, DateTime.UtcNow.Add(Lifetime));
        return token;
    }

    public bool TryGetUserId(string token, out int userId)
    {
        userId = 0;
        if (!_sessions.TryGetValue(token, out var session))
            return false;
        if (session.ExpiresAt <= DateTime.UtcNow)
        {
            _sessions.TryRemove(token, out _);
            return false;
        }

        userId = session.UserId;
        return true;
    }

    public void Remove(string token) => _sessions.TryRemove(token, out _);

    private void RemoveUserSessions(int userId)
    {
        foreach (var session in _sessions.Where(x => x.Value.UserId == userId))
            _sessions.TryRemove(session.Key, out _);
    }

    private sealed record SessionInfo(int UserId, DateTime ExpiresAt);
}

public sealed class PresenceService
{
    private readonly ConcurrentDictionary<int, DateTime> _lastHeartbeat = new();
    private static readonly TimeSpan OnlineWindow = TimeSpan.FromSeconds(45);

    public void MarkOnline(int userId) => _lastHeartbeat[userId] = DateTime.UtcNow;
    public void MarkOffline(int userId) => _lastHeartbeat.TryRemove(userId, out _);

    public bool IsOnline(int userId) =>
        _lastHeartbeat.TryGetValue(userId, out var lastSeen) &&
        DateTime.UtcNow - lastSeen <= OnlineWindow;
}

public static class HttpContextExtensions
{
    public static int GetCurrentUserId(this HttpContext context) =>
        context.Items.TryGetValue("CurrentUserId", out var value) && value is int id
            ? id
            : throw new UnauthorizedAccessException("No active session.");

    public static string? GetBearerToken(this HttpContext context)
    {
        var header = context.Request.Headers.Authorization.ToString();
        return header.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase)
            ? header[7..].Trim()
            : null;
    }
}
