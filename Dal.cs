using Oracle.ManagedDataAccess.Client;
using System.Data;

namespace ECare_Revamp.Models
{
    public class Dal
    {
        private OracleConnection con;
        private OracleCommand com;
        private readonly string _connectionString;
        public Dal(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("ATM")
                ?? throw new InvalidOperationException("Connection string 'ATM' not found.");
        }

        public string InsertProcedure(string textDetails)
        {
            try
            {
                using (var conn = new OracleConnection(_connectionString))
                using (var cmd = conn.CreateCommand())
                {
                    conn.OpenAsync();

                    cmd.CommandText = "update NDC_WATCH_CONFIG set load_master_key_flag = '1' where terminal_atm_number = :terminal";
                    cmd.CommandType = CommandType.Text;

                    cmd.Parameters.Add(new OracleParameter("terminal", OracleDbType.Varchar2, textDetails, ParameterDirection.Input));

                    cmd.ExecuteNonQueryAsync();
                    conn.CloseAsync();
                    conn.DisposeAsync();
                }
                return ""; // success
            }
            catch (Exception ex)
            {
                // TODO: log the exception
                return ex.Message;
            }
        }
        public string InsertTerminal(string terminalAtmNumber, string reason, string userId)
        {
            try
            {
                using (var conn = new OracleConnection(_connectionString))
                using (var cmd = conn.CreateCommand())
                {
                    conn.OpenAsync();

                    cmd.CommandText = "Master_key_chang_log.insert_user_log";
                    cmd.CommandType = CommandType.StoredProcedure;

                    cmd.Parameters.Add(new OracleParameter("p_terminal_atm_number", OracleDbType.Varchar2, terminalAtmNumber, ParameterDirection.Input));
                    cmd.Parameters.Add(new OracleParameter("p_reason", OracleDbType.Varchar2, reason, ParameterDirection.Input));
                    cmd.Parameters.Add(new OracleParameter("P_termid", OracleDbType.Varchar2, userId, ParameterDirection.Input));

                    cmd.ExecuteNonQueryAsync();
                    conn.CloseAsync();
                    conn.DisposeAsync();
                }
                return ""; // success
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
        }
    }
}
