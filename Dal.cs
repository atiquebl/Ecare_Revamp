using Oracle.ManagedDataAccess.Client;
using System.Data;

namespace ECare_Revamp.Models
{
    public class Dal
    {
        private OracleConnection con;
        private OracleCommand com;
        public Dal()
        {
            con = new OracleConnection("user id=ebl_mk_chg;password=eastern1;data source=(DESCRIPTION=(ADDRESS=(PROTOCOL=tcp)(HOST=172.30.31.110)(PORT=1521))(CONNECT_DATA=(SERVICE_NAME=pcard)));");
            com = new OracleCommand(); // Initialize 'com' to avoid CS8618 error  
        }

        public string insertProcedure(string ASPxTextBox1)
        {
            string test = System.Configuration.ConfigurationManager.ConnectionStrings["ATM"].ConnectionString;
            var conn1 = new OracleConnection(test);

            conn1.Open();
            var cmd1 = conn1.CreateCommand();
            cmd1.CommandType = CommandType.Text;
            var tj1 = Convert.ToString(ASPxTextBox1);
            var para2 = new OracleParameter { DbType = DbType.String, Value = tj1 };
            cmd1.Parameters.Add(para2);
            cmd1.CommandText = "update NDC_WATCH_CONFIG set load_master_key_flag = '1' where terminal_atm_number= :1 ";
            try
            {
                int i = cmd1.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
                return ex.Message;
            }

            cmd1.Parameters.Clear();
            cmd1.Dispose();
            conn1.Close();
            conn1.Dispose();
            return "";
        }

        public string insertTerminal(string ASPxTextBox1, string ASPxTextBox2, string userid)
        {
            com = new OracleCommand();

            OracleParameter op1 = new OracleParameter();
            op1.DbType = DbType.String;
            op1.Direction = ParameterDirection.Input;
            op1.Value = ASPxTextBox1;
            op1.ParameterName = "p_terminal_atm_number";

            OracleParameter op2 = new OracleParameter();
            op2.DbType = DbType.String;
            op2.Direction = ParameterDirection.Input;
            op2.Value = ASPxTextBox2;
            op2.ParameterName = "p_reason";

            OracleParameter op3 = new OracleParameter();
            op3.DbType = DbType.String;
            op3.Direction = ParameterDirection.Input;
            op3.Value = userid;
            op3.ParameterName = "P_termid";

            com.Connection = con;
            com.CommandText = "Master_key_chang_log.insert_user_log";
            com.CommandType = CommandType.StoredProcedure;
            com.Parameters.Add(op1);
            com.Parameters.Add(op2);
            com.Parameters.Add(op3);

            try
            {
                con.Open();
                int i = com.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
            con.Close();

            return "";
        }
    }
}
