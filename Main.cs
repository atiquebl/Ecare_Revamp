using Oracle.ManagedDataAccess.Client;
using System.Data;

namespace ECare_Revamp.Models
{
    public class Main
    {
        private readonly IConfiguration _configuration;

        public Main(IConfiguration configuration)
        {
            _configuration = configuration;
        }
        public OracleConnection ECARE_USER()
        {
            string? oradb2 = _configuration.GetConnectionString("ECARE");
            if (string.IsNullOrEmpty(oradb2))
            {
                throw new InvalidOperationException("Connection string 'ECARE' is not configured.");
            }
            return new OracleConnection(oradb2);
        }
        public OracleConnection PWR_USER()
        {
            string? oradb2 = _configuration.GetConnectionString("PWRQRY");
            if (string.IsNullOrEmpty(oradb2))
            {
                throw new InvalidOperationException("Connection string 'PWRQRY' is not configured.");
            }
            return new OracleConnection(oradb2);
        }
        public OracleConnection ECARE_PrePROD()
        {
            string? oradb2 = _configuration.GetConnectionString("Ecare-PRE-PROD");
            if (string.IsNullOrEmpty(oradb2))
            {
                throw new InvalidOperationException("Connection string 'Ecare-PRE-PROD' is not configured.");
            }
            return new OracleConnection(oradb2);
        }
        public OracleConnection GIFFT_USER()
        {
            string? oradb2 = _configuration.GetConnectionString("GIFFT");
            if (string.IsNullOrEmpty(oradb2))
            {
                throw new InvalidOperationException("Connection string 'GIFFT' is not configured.");
            }
            return new OracleConnection(oradb2);
        }
        public OracleConnection SMS_USER()
        {
            string? oradb2 = _configuration.GetConnectionString("SMS");
            if (string.IsNullOrEmpty(oradb2))
            {
                throw new InvalidOperationException("Connection string 'SMS' is not configured.");
            }
            return new OracleConnection(oradb2);
        }
        public OracleConnection DEBIT_CARD_USER()
        {
            string? oradb2 = _configuration.GetConnectionString("DEBIT-CARD");
            if (string.IsNullOrEmpty(oradb2))
            {
                throw new InvalidOperationException("Connection string 'DEBIT-CARD' is not configured.");
            }
            return new OracleConnection(oradb2);
        }
        public OracleConnection OnlinePrepaid()
        {
            string? oradb2 = _configuration.GetConnectionString("ONLINEPREPAID");
            if (string.IsNullOrEmpty(oradb2))
            {
                throw new InvalidOperationException("Connection string 'ONLINEPREPAID' is not configured.");
            }
            return new OracleConnection(oradb2);
        }
        public void Prepiad_Execute(string SQL)
        {
            try
            {
                OracleConnection con = OnlinePrepaid();
                if (con.State == ConnectionState.Closed)
                {
                    con.Open();
                }
                DataTable consultanttable = new DataTable();
                OracleDataAdapter Consultantdataadapter = new OracleDataAdapter(SQL, con);
                Consultantdataadapter.Fill(consultanttable);
                con.Close();
                con.Dispose();
            }
            catch (InvalidCastException) // Removed unused variable 'e'  
            {
                throw; // Preserve original stack trace by using `throw` without specifying the exception  
            }
        }
        public void Execute(string SQL)
        {
            try
            {
                OracleConnection con = ECARE_USER();
                if (con.State == ConnectionState.Closed)
                {
                    con.Open();
                }
                DataTable consultanttable = new DataTable();
                OracleDataAdapter Consultantdataadapter = new OracleDataAdapter(SQL, con);
                Consultantdataadapter.Fill(consultanttable);
                con.Close();
                con.Dispose();
            }
            catch (InvalidCastException) // Removed unused variable 'e'  
            {
                throw; // Preserve original stack trace by using `throw` without specifying the exception  
            }
        }
        public void Ecare_PreProdExecute(string SQL)
        {
            try
            {
                OracleConnection con = ECARE_PrePROD();
                if (con.State == ConnectionState.Closed)
                {
                    con.Open();
                }
                DataTable consultanttable = new DataTable();
                OracleDataAdapter Consultantdataadapter = new OracleDataAdapter(SQL, con);
                Consultantdataadapter.Fill(consultanttable);
                con.Close();
                con.Dispose();
            }
            catch (InvalidCastException) // Removed unused variable 'e'  
            {
                throw; // Preserve original stack trace by using `throw` without specifying the exception  
            }
        }
        public void Gifft_Execute(string SQL)
        {
            try
            {
                OracleConnection con = GIFFT_USER();
                if (con.State == ConnectionState.Closed)
                {
                    con.Open();
                }
                DataTable consultanttable = new DataTable();
                OracleDataAdapter Consultantdataadapter = new OracleDataAdapter(SQL, con);
                Consultantdataadapter.Fill(consultanttable);
                con.Close();
                con.Dispose();
            }
            catch (InvalidCastException) // Removed unused variable 'e'  
            {
                throw; // Preserve original stack trace by using `throw` without specifying the exception  
            }
        }
        public void SMS_Execute(string SQL)
        {
            try
            {
                OracleConnection con = SMS_USER();
                if (con.State == ConnectionState.Closed)
                {
                    con.Open();
                }
                DataTable consultanttable = new DataTable();
                OracleDataAdapter Consultantdataadapter = new OracleDataAdapter(SQL, con);
                Consultantdataadapter.Fill(consultanttable);
                con.Close();
                con.Dispose();
            }
            catch (InvalidCastException) // Removed unused variable 'e'  
            {
                throw; // Preserve original stack trace by using `throw` without specifying the exception  
            }
        }
        public void Debit_Card_Execute(string SQL)
        {
            try
            {
                OracleConnection con = DEBIT_CARD_USER();
                if (con.State == ConnectionState.Closed)
                {
                    con.Open();
                }
                DataTable consultanttable = new DataTable();
                OracleDataAdapter Consultantdataadapter = new OracleDataAdapter(SQL, con);
                Consultantdataadapter.Fill(consultanttable);
                con.Close();
                con.Dispose();
            }
            catch (InvalidCastException) // Removed unused variable 'e'  
            {
                throw; // Preserve original stack trace by using `throw` without specifying the exception  
            }
        }
        public void Pwr_Execute(string SQL)
        {
            try
            {
                OracleConnection con = PWR_USER();
                if (con.State == ConnectionState.Closed)
                {
                    con.Open();
                }
                DataTable consultanttable = new DataTable();
                OracleDataAdapter Consultantdataadapter = new OracleDataAdapter(SQL, con);
                Consultantdataadapter.Fill(consultanttable);
                con.Close();
                con.Dispose();
            }
            catch (InvalidCastException) // Removed unused variable 'e'  
            {
                throw; // Preserve original stack trace by using `throw` without specifying the exception  
            }
        }
        public string Converter_Description(string SQL)
        {
            string StringConsultant = "";
            try
            {
                OracleConnection con = ECARE_USER();
                if (con.State == ConnectionState.Closed)
                {
                    con.Open();
                }
                DataTable consultanttable = new DataTable();

                OracleDataAdapter Consultantdataadapter = new OracleDataAdapter(SQL, con);
                Consultantdataadapter.Fill(consultanttable);
                foreach (DataRow myrow in consultanttable.Rows)
                {
                    StringConsultant = myrow[0]?.ToString() ?? string.Empty;
                }
                con.Close();
                con.Dispose();
            }
            catch (InvalidCastException) // Removed unused variable 'e'  
            {
                throw; // Preserve original stack trace by using `throw` without specifying the exception  
            }
            return StringConsultant;
        }
        public string Gifft_Converter_Description(string SQL)
        {
            string StringConsultant = "";
            try
            {
                OracleConnection con = GIFFT_USER();
                if (con.State == ConnectionState.Closed)
                {
                    con.Open();
                }
                DataTable consultanttable = new DataTable();

                OracleDataAdapter Consultantdataadapter = new OracleDataAdapter(SQL, con);
                Consultantdataadapter.Fill(consultanttable);
                foreach (DataRow myrow in consultanttable.Rows)
                {
                    StringConsultant = myrow[0]?.ToString() ?? string.Empty;
                }
                con.Close();
                con.Dispose();
            }
            catch (InvalidCastException) // Removed unused variable 'e'  
            {
                throw; // Preserve original stack trace by using `throw` without specifying the exception  
            }
            return StringConsultant;
        }
        public string Pwr_Converter_Description(string SQL)
        {
            string StringConsultant = "";
            try
            {
                OracleConnection con = PWR_USER();
                if (con.State == ConnectionState.Closed)
                {
                    con.Open();
                }
                DataTable consultanttable = new DataTable();

                OracleDataAdapter Consultantdataadapter = new OracleDataAdapter(SQL, con);
                Consultantdataadapter.Fill(consultanttable);
                foreach (DataRow myrow in consultanttable.Rows)
                {
                    StringConsultant = myrow[0]?.ToString() ?? string.Empty;
                }
                con.Close();
                con.Dispose();
            }
            catch (InvalidCastException) // Removed unused variable 'e'  
            {
                throw; // Preserve original stack trace by using `throw` without specifying the exception  
            }
            return StringConsultant;
        }
        public string Online_Prepaid_Converter_Description(string SQL)
        {
            string StringConsultant = "";
            try
            {
                OracleConnection con = OnlinePrepaid();
                if (con.State == ConnectionState.Closed)
                {
                    con.Open();
                }
                DataTable consultanttable = new DataTable();

                OracleDataAdapter Consultantdataadapter = new OracleDataAdapter(SQL, con);
                Consultantdataadapter.Fill(consultanttable);
                foreach (DataRow myrow in consultanttable.Rows)
                {
                    StringConsultant = myrow[0]?.ToString() ?? string.Empty;
                }
                con.Close();
                con.Dispose();
            }
            catch (InvalidCastException) // Removed unused variable 'e'  
            {
                throw; // Preserve original stack trace by using `throw` without specifying the exception  
            }
            return StringConsultant;
        }
        public string Debit_Card_Debit_Converter_Description(string SQL)
        {
            string StringConsultant = "";
            try
            {
                OracleConnection con = DEBIT_CARD_USER();
                if (con.State == ConnectionState.Closed)
                {
                    con.Open();
                }
                DataTable consultanttable = new DataTable();

                OracleDataAdapter Consultantdataadapter = new OracleDataAdapter(SQL, con);
                Consultantdataadapter.Fill(consultanttable);
                foreach (DataRow myrow in consultanttable.Rows)
                {
                    StringConsultant = myrow[0]?.ToString() ?? string.Empty;
                }
                con.Close();
                con.Dispose();
            }
            catch (InvalidCastException) // Removed unused variable 'e'  
            {
                throw; // Preserve original stack trace by using `throw` without specifying the exception  
            }
            return StringConsultant;
        }
    }
}
