using System.Data.Common;
using Microsoft.EntityFrameworkCore;

namespace backend;

public static class DatabaseInitializer
{
    public static async Task InitializeAsync(AppDbContext db, CancellationToken cancellationToken = default)
    {
        await db.Database.OpenConnectionAsync(cancellationToken);
        try
        {
            await db.Database.ExecuteSqlRawAsync("""
                CREATE TABLE IF NOT EXISTS users (
                    id int unsigned NOT NULL AUTO_INCREMENT,
                    username varchar(50) NOT NULL,
                    email varchar(255) NOT NULL,
                    password_hash varchar(255) NOT NULL,
                    avatar_path varchar(500) NULL,
                    is_active tinyint(1) NOT NULL DEFAULT 1,
                    last_active_at datetime(6) NULL,
                    created_at datetime(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6),
                    updated_at datetime(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6) ON UPDATE CURRENT_TIMESTAMP(6),
                    PRIMARY KEY (id),
                    UNIQUE KEY ux_users_username (username),
                    UNIQUE KEY ux_users_email (email)
                ) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;
                """, cancellationToken);

            await EnsureUserColumnAsync(db, "avatar_path", "varchar(500) NULL", cancellationToken);
            await EnsureUserColumnAsync(db, "last_active_at", "datetime(6) NULL", cancellationToken);

            await db.Database.ExecuteSqlRawAsync("""
                CREATE TABLE IF NOT EXISTS user_contacts (
                    owner_user_id int unsigned NOT NULL,
                    contact_user_id int unsigned NOT NULL,
                    created_at datetime(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6),
                    PRIMARY KEY (owner_user_id, contact_user_id),
                    CONSTRAINT fk_contacts_owner FOREIGN KEY (owner_user_id) REFERENCES users(id) ON DELETE CASCADE,
                    CONSTRAINT fk_contacts_contact FOREIGN KEY (contact_user_id) REFERENCES users(id) ON DELETE CASCADE
                ) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;
                """, cancellationToken);

            await db.Database.ExecuteSqlRawAsync("""
                CREATE TABLE IF NOT EXISTS messages (
                    id bigint unsigned NOT NULL AUTO_INCREMENT,
                    sender_id int unsigned NOT NULL,
                    recipient_id int unsigned NOT NULL,
                    text_content text NULL,
                    image_path varchar(500) NULL,
                    sent_at datetime(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6),
                    read_at datetime(6) NULL,
                    PRIMARY KEY (id),
                    KEY ix_messages_conversation (sender_id, recipient_id, id),
                    KEY ix_messages_recipient (recipient_id, sender_id, id),
                    CONSTRAINT fk_messages_sender FOREIGN KEY (sender_id) REFERENCES users(id) ON DELETE CASCADE,
                    CONSTRAINT fk_messages_recipient FOREIGN KEY (recipient_id) REFERENCES users(id) ON DELETE CASCADE
                ) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;
                """, cancellationToken);
        }
        finally
        {
            await db.Database.CloseConnectionAsync();
        }
    }

    private static async Task EnsureUserColumnAsync(
        AppDbContext db,
        string columnName,
        string definition,
        CancellationToken cancellationToken)
    {
        var connection = db.Database.GetDbConnection();
        await using var command = connection.CreateCommand();
        command.CommandText = """
            SELECT COUNT(*)
            FROM information_schema.columns
            WHERE table_schema = DATABASE() AND table_name = 'users' AND column_name = @columnName
            """;
        var parameter = command.CreateParameter();
        parameter.ParameterName = "@columnName";
        parameter.Value = columnName;
        command.Parameters.Add(parameter);
        var exists = Convert.ToInt32(await command.ExecuteScalarAsync(cancellationToken)) > 0;
        if (exists)
            return;

        await using var alter = connection.CreateCommand();
        alter.CommandText = $"ALTER TABLE users ADD COLUMN `{columnName}` {definition}";
        await alter.ExecuteNonQueryAsync(cancellationToken);
    }
}
