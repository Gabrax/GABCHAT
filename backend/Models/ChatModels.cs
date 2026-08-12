namespace backend;

public class User
{
    public int Id { get; set; }
    public string Username { get; set; } = null!;
    public string Email { get; set; } = null!;
    public string PasswordHash { get; set; } = null!;
    public string? AvatarPath { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime? LastActiveAt { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}

public class UserContact
{
    public int OwnerUserId { get; set; }
    public int ContactUserId { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public User Owner { get; set; } = null!;
    public User Contact { get; set; } = null!;
}

public class ChatMessage
{
    public long Id { get; set; }
    public int SenderId { get; set; }
    public int RecipientId { get; set; }
    public string? TextContent { get; set; }
    public string? ImagePath { get; set; }
    public DateTime SentAt { get; set; } = DateTime.UtcNow;
    public DateTime? ReadAt { get; set; }
    public User Sender { get; set; } = null!;
    public User Recipient { get; set; } = null!;
}

public record RegisterRequest(string Username, string Email, string Password);
public record LoginRequest(string Email, string Password);
public record UpdateProfileRequest(string Username);
public record LoginResponse(string Token, UserDto User);

public record UserDto(
    int Id,
    string Username,
    string Email,
    string? AvatarUrl,
    bool IsOnline,
    DateTime? LastSeenAt,
    bool IsContact)
{
    public static UserDto From(User user, bool isOnline, bool isContact) =>
        new(user.Id, user.Username, user.Email, user.AvatarPath, isOnline, user.LastActiveAt, isContact);
}

public record MessageDto(
    long Id,
    int SenderId,
    int RecipientId,
    string? Text,
    string? ImageUrl,
    DateTime SentAt,
    DateTime? ReadAt)
{
    public static MessageDto From(ChatMessage message) =>
        new(message.Id, message.SenderId, message.RecipientId, message.TextContent,
            message.ImagePath, message.SentAt, message.ReadAt);
}
