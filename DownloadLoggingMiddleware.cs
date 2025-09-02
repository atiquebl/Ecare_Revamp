using Oracle.ManagedDataAccess.Client;

namespace ECare_Revamp.Models
{
    public class DownloadLoggingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<DownloadLoggingMiddleware> _logger;
        private readonly string _connectionString;
        private readonly string[] _allowedExtensions;
        private readonly string[] _blockedExtensions;
        private readonly string _logDirectory;
        public DownloadLoggingMiddleware(RequestDelegate next, ILogger<DownloadLoggingMiddleware> logger, IConfiguration config)
        {
            _next = next;
            _logger = logger;

            // Fix for CS8601: Ensure the connection string is not null by providing a fallback value
            _connectionString = config.GetConnectionString("Ecare") ?? throw new ArgumentNullException(nameof(config), "Ecare string is missing in configuration.");

            _allowedExtensions = config.GetSection("DownloadLogging:AllowedExtensions").Get<string[]>() ?? Array.Empty<string>();
            _blockedExtensions = config.GetSection("DownloadLogging:BlockedExtensions").Get<string[]>() ?? Array.Empty<string>();

            // Get or set default log directory
            _logDirectory = config.GetValue<string>("LoggingOptions:LogDirectory") ?? Path.Combine(Directory.GetCurrentDirectory(), "Logs");

            // Ensure directory exists
            if (!Directory.Exists(_logDirectory))
                Directory.CreateDirectory(_logDirectory);
        }

        public async Task InvokeAsync(HttpContext context)
        {
            await _next(context);

            bool isFileResponse = context.Response.Headers.ContainsKey("Content-Disposition") ||
                                  (context.Response.ContentType?.StartsWith("application/") ?? false) ||
                                  (context.Response.ContentType?.StartsWith("text/") ?? false);

            if (!isFileResponse)
                return;

            var disposition = context.Response.Headers["Content-Disposition"].ToString();
            var fileName = disposition.Split("filename=").LastOrDefault()?.Replace("\"", "") ?? "Unknown";

            if (fileName == "Unknown" && context.Request.Path.HasValue)
            {
                fileName = context.Request.Path.Value.Split('/').Last();
            }

            var extension = Path.GetExtension(fileName)?.ToLowerInvariant() ?? string.Empty;

            if (_blockedExtensions.Contains(extension))
                return;

            if (_allowedExtensions.Length > 0 && !_allowedExtensions.Contains(extension))
                return;

            var contentType = context.Response.ContentType ?? "application/octet-stream";
            var userName = context.User.Identity?.Name ?? "Anonymous";
            var ip = context.Connection.RemoteIpAddress?.ToString() ?? "Unknown";
            var success = context.Response.StatusCode == 200;

            long fileSize = 0;
            if (context.Request.Path.HasValue)
            {
                var physicalPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", context.Request.Path.Value.TrimStart('/'));
                if (File.Exists(physicalPath))
                {
                    fileSize = new FileInfo(physicalPath).Length;
                }
            }

            var timestamp = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss");
            var logLine = $"{timestamp} | User={userName} | File={fileName} | Type={contentType} | Size={fileSize} | IP={ip} | Success={success}";

            // Log to console
            _logger.LogInformation(logLine);

            // Append to local log file
            var logFileName = Path.Combine(_logDirectory, $"activity-{DateTime.UtcNow:yyyyMMdd}.log");
            await File.AppendAllTextAsync(logFileName, logLine + Environment.NewLine);

            // Log to Oracle DB (skip anonymous)
            if (userName != "Anonymous")
            {
                string source = "FileDownload";
                string activityType = "Download";
                string details = $"File: {fileName}, Type: {contentType}, Size: {fileSize}, IP: {ip}, Success: {success}";

                await using var conn = new OracleConnection(_connectionString);
                await conn.OpenAsync();

                const string sql = @"INSERT INTO Ecare_Revamp_ActivityLogs (UserName, Source, ActivityType, Details)
                                 VALUES (:userName, :source, :activityType, :details)";
                await using var cmd = new OracleCommand(sql, conn);
                cmd.Parameters.Add(new OracleParameter("userName", userName));
                cmd.Parameters.Add(new OracleParameter("source", source));
                cmd.Parameters.Add(new OracleParameter("activityType", activityType));
                cmd.Parameters.Add(new OracleParameter("details", details));

                await cmd.ExecuteNonQueryAsync();
                await conn.CloseAsync();
            }
        }
    }
}
