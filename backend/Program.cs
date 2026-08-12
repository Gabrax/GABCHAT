using backend;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.FileProviders;

var builder = WebApplication.CreateBuilder(args);

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
if (string.IsNullOrWhiteSpace(connectionString))
    throw new InvalidOperationException("DefaultConnection is missing from appsettings.json.");

builder.Services.AddDbContext<AppDbContext>(options => options.UseMySQL(connectionString));
builder.Services.AddSingleton<SessionStore>();
builder.Services.AddSingleton<PresenceService>();
builder.Services.AddSingleton<MediaStorage>();
builder.Services.Configure<FormOptions>(options => options.MultipartBodyLengthLimit = 9 * 1024 * 1024);
builder.Services.AddCors(options => options.AddDefaultPolicy(policy =>
    policy.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod()));
builder.Services.AddControllers();

var app = builder.Build();

await using (var scope = app.Services.CreateAsyncScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    await DatabaseInitializer.InitializeAsync(db);
}

if (!app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}

app.UseCors();

var mediaStorage = app.Services.GetRequiredService<MediaStorage>();
app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new PhysicalFileProvider(mediaStorage.UploadsRoot),
    RequestPath = "/uploads"
});

app.Use(async (context, next) =>
{
    var path = context.Request.Path;
    var isPublicEndpoint = path.Equals("/api/auth/login", StringComparison.OrdinalIgnoreCase) ||
                           path.Equals("/api/auth/register", StringComparison.OrdinalIgnoreCase);

    if (path.StartsWithSegments("/api") && !isPublicEndpoint)
    {
        var token = context.GetBearerToken();
        var sessions = context.RequestServices.GetRequiredService<SessionStore>();
        if (token is null || !sessions.TryGetUserId(token, out var userId))
        {
            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
            await context.Response.WriteAsJsonAsync(new { message = "Your session has expired. Sign in again." });
            return;
        }

        context.Items["CurrentUserId"] = userId;
        context.RequestServices.GetRequiredService<PresenceService>().MarkOnline(userId);
    }

    await next();
});

app.MapGet("/health", () => Results.Ok(new { status = "ok" }));
app.MapControllers();
app.Run();
