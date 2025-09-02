using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Oracle.ManagedDataAccess.Client;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace ECare_Revamp.Models
{
    public class ActivityLoggingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ActivityLoggingMiddleware> _logger;
        private readonly string _connectionString;
        private readonly string[] _allowedExtensions;
        private readonly string[] _blockedExtensions;
        private readonly string _logDirectory;

        public ActivityLoggingMiddleware(RequestDelegate next, ILogger<ActivityLoggingMiddleware> logger, IConfiguration config)
        {
            _next = next;
            _logger = logger;
            _connectionString = config.GetConnectionString("Ecare")
                ?? throw new ArgumentNullException(nameof(config), "Ecare connection string is missing.");

            _allowedExtensions = config.GetSection("DownloadLogging:AllowedExtensions").Get<string[]>() ?? Array.Empty<string>();
            _blockedExtensions = config.GetSection("DownloadLogging:BlockedExtensions").Get<string[]>() ?? Array.Empty<string>();

            _logDirectory = config.GetValue<string>("LoggingOptions:LogDirectory")
                ?? Path.Combine(Directory.GetCurrentDirectory(), "Logs");
            if (!Directory.Exists(_logDirectory))
                Directory.CreateDirectory(_logDirectory);
        }

        public async Task InvokeAsync(HttpContext context)
        {
            // Ensure request body can be re-read early
            if (context.Request.ContentLength > 0 && !context.Request.Body.CanSeek)
            {
                context.Request.EnableBuffering();
            }

            var timestamp = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss");
            var userName = context.User.Identity?.Name ?? "Anonymous";
            var ip = context.Connection.RemoteIpAddress?.ToString() ?? "Unknown";
            var path = context.Request.Path.Value ?? "Unknown";
            var method = context.Request.Method;

            try
            {
                // Handle POST requests (including login)

                //if (string.Equals(method, "POST", StringComparison.OrdinalIgnoreCase))
                //{
                //    await HandlePostLogging(context, timestamp, userName, ip, path);
                //}

                // Call the next middleware/page
                await _next(context);

                // Handle file downloads AFTER the response
                await HandleFileDownloadLogging(context, timestamp, userName, ip, path);
            }
            catch (Exception ex)
            {
                // Log any unhandled errors but don't break the app
                await WriteLogAsync($"[FATAL] {DateTime.UtcNow:yyyy-MM-dd HH:mm:ss} | Unexpected error in middleware: {ex.Message}");
                // Comment out throw to avoid "An unexpected error occurred"
                // throw;
            }
        }

        private async Task HandlePostLogging(HttpContext context, string timestamp, string userName, string ip, string path)
        {
            try
            {
                string formDetails = "[Non-form POST]";
                string postUserName = userName;

                // Only try reading form data if it's actually a form (not JSON or multipart)
                if (context.Request.HasFormContentType &&
                    context.Request.ContentType?.StartsWith("application/x-www-form-urlencoded", StringComparison.OrdinalIgnoreCase) == true)
                {
                    var form = await context.Request.ReadFormAsync();

                    // Mask sensitive fields
                    var safeForm = form.Select(f =>
                        $"{f.Key}={(f.Key.ToLower().Contains("password") ? "[HIDDEN]" : f.Value)}");
                    formDetails = string.Join(", ", safeForm);

                    // Try to extract username (flexible keys)
                    var possibleKeys = new[] { "username", "user", "login", "email", "admin" };
                    var key = possibleKeys.FirstOrDefault(k => form.ContainsKey(k));
                    if (key != null)
                        postUserName = form[key].ToString();

                    context.Request.Body.Position = 0; // Reset stream for next middleware
                }

                string logEntry = $"{timestamp} | User={postUserName ?? "Anonymous"} | Action=POST | Path={path} | IP={ip} | Data={formDetails}";
                await WriteLogAsync(logEntry);

                if (!string.IsNullOrEmpty(postUserName) && postUserName != "Anonymous")
                {
                    await LogToOracleSafeAsync(postUserName, "PageAction", "POST", $"Path: {path}, Data: {formDetails}, IP: {ip}");
                }
            }
            catch (Exception ex)
            {
                await WriteLogAsync($"[ERROR] {DateTime.UtcNow:yyyy-MM-dd HH:mm:ss} | Failed POST logging: {ex.Message}");
            }
        }

        private async Task HandleFileDownloadLogging(HttpContext context, string timestamp, string userName, string ip, string path)
        {
            try
            {
                bool isFileResponse = context.Response.Headers.ContainsKey("Content-Disposition") ||
                                      (context.Response.ContentType?.StartsWith("application/") ?? false) ||
                                      (context.Response.ContentType?.StartsWith("text/") ?? false);

                if (!isFileResponse) return;

                var disposition = context.Response.Headers["Content-Disposition"].ToString();
                var fileName = disposition.Split("filename=").LastOrDefault()?.Replace("\"", "") ?? "Unknown";

                if (fileName == "Unknown" && context.Request.Path.HasValue)
                    fileName = context.Request.Path.Value.Split('/').Last();

                var extension = Path.GetExtension(fileName)?.ToLowerInvariant() ?? string.Empty;

                if (_blockedExtensions.Contains(extension)) return;
                if (_allowedExtensions.Length > 0 && !_allowedExtensions.Contains(extension)) return;

                var contentType = context.Response.ContentType ?? "application/octet-stream";
                var success = context.Response.StatusCode == 200;

                long fileSize = 0;
                if (context.Request.Path.HasValue)
                {
                    var physicalPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", context.Request.Path.Value.TrimStart('/'));
                    if (File.Exists(physicalPath))
                        fileSize = new FileInfo(physicalPath).Length;
                }

                string downloadEntry = $"{timestamp} | User={userName} | Action=Download | File={fileName} | Path: {path} | Type={contentType} | Size={fileSize} | IP={ip} | Success={success}";
                await WriteLogAsync(downloadEntry);

                if (userName != "Anonymous")
                {
                    await LogToOracleSafeAsync(userName, "FileDownload", "Download",
                        $"File: {fileName}, Type: {contentType}, Size: {fileSize}, IP: {ip}, Success: {success}");
                }
            }
            catch (Exception ex)
            {
                await WriteLogAsync($"[ERROR] {DateTime.UtcNow:yyyy-MM-dd HH:mm:ss} | Failed Download logging: {ex.Message}");
            }
        }

        private async Task WriteLogAsync(string line)
        {
            var logFileName = Path.Combine(_logDirectory, $"activity-{DateTime.UtcNow:yyyyMMdd}.log");
            await File.AppendAllTextAsync(logFileName, line + Environment.NewLine);
            _logger.LogInformation(line);
        }

        private async Task LogToOracleSafeAsync(string userName, string source, string activityType, string details)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(userName))
                    userName = "Anonymous";

                await using var conn = new OracleConnection(_connectionString);
                await conn.OpenAsync();

                const string sql = @"INSERT INTO Ecare_Revamp_ActivityLogs 
                                     (UserName, Source, ActivityType, Details)
                                     VALUES (:userName, :source, :activityType, :details)";
                await using var cmd = new OracleCommand(sql, conn);
                cmd.Parameters.Add(new OracleParameter("userName", userName.Length > 20 ? userName.Substring(0, 20) : userName));
                cmd.Parameters.Add(new OracleParameter("source", source));
                cmd.Parameters.Add(new OracleParameter("activityType", activityType));
                cmd.Parameters.Add(new OracleParameter("details", details));

                await cmd.ExecuteNonQueryAsync();
            }
            catch (Exception ex)
            {
                // Always log failure to file, never crash
                await WriteLogAsync($"[ERROR] {DateTime.UtcNow:yyyy-MM-dd HH:mm:ss} | Failed DB log: {ex.Message}");
            }
        }
    }
}
