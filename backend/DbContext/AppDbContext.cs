using Microsoft.EntityFrameworkCore;

namespace backend;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<User> Users => Set<User>();
    public DbSet<UserContact> Contacts => Set<UserContact>();
    public DbSet<ChatMessage> Messages => Set<ChatMessage>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        var user = modelBuilder.Entity<User>();
        user.ToTable("users");
        user.HasKey(x => x.Id);
        user.Property(x => x.Id).HasColumnName("id");
        user.Property(x => x.Username).HasColumnName("username").HasMaxLength(50);
        user.Property(x => x.Email).HasColumnName("email").HasMaxLength(255);
        user.Property(x => x.PasswordHash).HasColumnName("password_hash").HasMaxLength(255);
        user.Property(x => x.AvatarPath).HasColumnName("avatar_path").HasMaxLength(500);
        user.Property(x => x.IsActive).HasColumnName("is_active");
        user.Property(x => x.LastActiveAt).HasColumnName("last_active_at");
        user.Property(x => x.CreatedAt).HasColumnName("created_at");
        user.Property(x => x.UpdatedAt).HasColumnName("updated_at");
        user.HasIndex(x => x.Username).IsUnique();
        user.HasIndex(x => x.Email).IsUnique();

        var contact = modelBuilder.Entity<UserContact>();
        contact.ToTable("user_contacts");
        contact.HasKey(x => new { x.OwnerUserId, x.ContactUserId });
        contact.Property(x => x.OwnerUserId).HasColumnName("owner_user_id");
        contact.Property(x => x.ContactUserId).HasColumnName("contact_user_id");
        contact.Property(x => x.CreatedAt).HasColumnName("created_at");
        contact.HasOne(x => x.Owner).WithMany().HasForeignKey(x => x.OwnerUserId).OnDelete(DeleteBehavior.Cascade);
        contact.HasOne(x => x.Contact).WithMany().HasForeignKey(x => x.ContactUserId).OnDelete(DeleteBehavior.Cascade);

        var message = modelBuilder.Entity<ChatMessage>();
        message.ToTable("messages");
        message.HasKey(x => x.Id);
        message.Property(x => x.Id).HasColumnName("id");
        message.Property(x => x.SenderId).HasColumnName("sender_id");
        message.Property(x => x.RecipientId).HasColumnName("recipient_id");
        message.Property(x => x.TextContent).HasColumnName("text_content");
        message.Property(x => x.ImagePath).HasColumnName("image_path").HasMaxLength(500);
        message.Property(x => x.SentAt).HasColumnName("sent_at");
        message.Property(x => x.ReadAt).HasColumnName("read_at");
        message.HasOne(x => x.Sender).WithMany().HasForeignKey(x => x.SenderId).OnDelete(DeleteBehavior.Cascade);
        message.HasOne(x => x.Recipient).WithMany().HasForeignKey(x => x.RecipientId).OnDelete(DeleteBehavior.Cascade);
        message.HasIndex(x => new { x.SenderId, x.RecipientId, x.Id });
    }
}
