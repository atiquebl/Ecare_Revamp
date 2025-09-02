using DevExpress.DataAccess.DataFederation;
using Oracle.ManagedDataAccess.Client;
using Serilog;

namespace ECare_Revamp.Models
{
    public class GlobalActivityLogger
    {
        private readonly string _connectionString;

        public GlobalActivityLogger(IConfiguration config)
        {
            _connectionString = config.GetConnectionString("Ecare")
                ?? throw new InvalidOperationException("Connection string 'Ecare' not found.");
        }

        public async Task LogAsync(string userName, string activityType, string details)
        {
            string source = "APP";

            // Build message in the exact format (except timestamp, Serilog adds it)
            string message = $"{userName} | {source} | {activityType,-7} | {details}";

            // Log to file via Serilog (timestamp auto-prefixed by Serilog)
            // Tag log so Serilog routes it only to activity-.log
            Log.ForContext("LogType", "Activity")
               .Information(message);

            if (userName != "Anonymous")
            {
                // Log to Oracle DB
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
                await conn.DisposeAsync();
            }
        }
    }
}
