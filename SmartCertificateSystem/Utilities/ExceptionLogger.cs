using Microsoft.AspNetCore.Hosting;

namespace SmartCertificateSystem.Utilities;

public class ExceptionLogger(IWebHostEnvironment environment)
{
    private readonly string _logPath = Path.Combine(environment.ContentRootPath, "App_Data", "exceptions.log");

    public async Task LogAsync(Exception exception)
    {
        Directory.CreateDirectory(Path.GetDirectoryName(_logPath)!);
        var entry = $"[{DateTime.UtcNow:u}] {exception.GetType().Name}: {exception.Message}{Environment.NewLine}{exception.StackTrace}{Environment.NewLine}";
        await File.AppendAllTextAsync(_logPath, entry);
    }
}
