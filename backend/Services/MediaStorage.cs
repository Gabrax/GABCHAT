namespace backend;

public sealed class MediaStorage
{
    private const long MaxFileSize = 8 * 1024 * 1024;
    private static readonly IReadOnlyDictionary<string, string> AllowedTypes =
        new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            ["image/jpeg"] = ".jpg",
            ["image/png"] = ".png",
            ["image/webp"] = ".webp",
            ["image/gif"] = ".gif"
        };

    private readonly string _uploadsRoot;

    public MediaStorage(IWebHostEnvironment environment)
    {
        _uploadsRoot = Path.Combine(environment.ContentRootPath, "uploads");
        Directory.CreateDirectory(_uploadsRoot);
    }

    public string UploadsRoot => _uploadsRoot;

    public async Task<string> SaveImageAsync(IFormFile file, string category, CancellationToken cancellationToken)
    {
        if (file.Length is <= 0 or > MaxFileSize)
            throw new InvalidOperationException("The image can be up to 8 MB.");
        if (!AllowedTypes.TryGetValue(file.ContentType, out var extension))
            throw new InvalidOperationException("Allowed image formats: JPG, PNG, WEBP, and GIF.");

        var directory = Path.Combine(_uploadsRoot, category);
        Directory.CreateDirectory(directory);
        var fileName = $"{Guid.NewGuid():N}{extension}";
        var destination = Path.Combine(directory, fileName);

        await using var stream = new FileStream(destination, FileMode.CreateNew, FileAccess.Write, FileShare.None);
        await file.CopyToAsync(stream, cancellationToken);
        return $"/uploads/{category}/{fileName}";
    }

    public void Delete(string? publicPath)
    {
        if (string.IsNullOrWhiteSpace(publicPath) || !publicPath.StartsWith("/uploads/", StringComparison.Ordinal))
            return;

        var relativePath = publicPath["/uploads/".Length..].Replace('/', Path.DirectorySeparatorChar);
        var fullPath = Path.GetFullPath(Path.Combine(_uploadsRoot, relativePath));
        var root = Path.GetFullPath(_uploadsRoot) + Path.DirectorySeparatorChar;
        if (fullPath.StartsWith(root, StringComparison.OrdinalIgnoreCase) && File.Exists(fullPath))
            File.Delete(fullPath);
    }
}
