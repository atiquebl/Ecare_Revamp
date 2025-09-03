using DevExpress.CodeParser;
using ECare_Revamp.Script;

namespace ECare_Revamp.Models
{
    public class Globals_IP
    {
        public string Switch_IP { get; }
        public string Switch_User { get; }
        public string Switch_Password { get; }

        public string App_IP { get; }
        public string App_User { get; }
        public string App_Password { get; }

        public string App_IP_1 { get; }
        public string App_User_1 { get; }
        public string App_Password_1 { get; }

        public string Db_IP { get; }
        public string Db_User { get; }
        public string Db_Password { get; }

        public string Core_Db_IP { get; }
        public string Core_Db_User { get; }
        public string Core_Db_Password { get; }

        private readonly IConfiguration _config;
        private readonly Encryption_Process _encryption;
        public Globals_IP(IConfiguration config)
        {
            _config = config ?? throw new ArgumentNullException(nameof(config));
            _encryption = new Encryption_Process(config);

            var encryptionProcess = new Encryption_Process(_config);
            Switch_IP = _config.GetValue<string>("Switch_Details:Switch_IP") ?? string.Empty;
            Switch_User = DecryptValue("Switch_Details:Switch_User");
            Switch_Password = DecryptValue("Switch_Details:Switch_Password");

            App_IP = _config.GetValue<string>("Application_Details:App_1_IP") ?? string.Empty;
            App_User = DecryptValue("Application_Details:App_1_User");
            App_Password = DecryptValue("Application_Details:App_1_Password");

            App_IP_1 = _config.GetValue<string>("Application_Details:App_2_IP") ?? string.Empty;
            App_User_1 = DecryptValue("Application_Details:App_2_User");
            App_Password_1 = DecryptValue("Application_Details:App_2_Password");

            Db_IP = _config.GetValue<string>("DB_Details:Db_IP") ?? string.Empty;
            Db_User = DecryptValue("DB_Details:Db_User");
            Db_Password = DecryptValue("DB_Details:Db_Password");

            Core_Db_IP = _config.GetValue<string>("DB_Details:Core_Db_IP") ?? string.Empty;
            Core_Db_User = DecryptValue("DB_Details:Core_Db_User");
            Core_Db_Password = DecryptValue("DB_Details:Core_Db_Password");
        }
        private string GetEncryptionKey()
        {
            // Assuming the encryption key is stored in the configuration
            return _config.GetValue<string>("Encryption:Key") ?? throw new InvalidOperationException("Encryption key is not configured.");
        }
        private string DecryptValue(string key)
        {
            var raw = _config.GetValue<string>(key);
            return string.IsNullOrEmpty(raw)
                ? string.Empty
                : _encryption.DecryptString(GetEncryptionKey(), raw);
        }
        //public static readonly String Switch_IP = "192.168.2.72";
        //public static readonly String Switch_User = "pwrcard";
        //public static readonly String Switch_Password = "pwrcard";

        //public static readonly String App_IP = "172.30.42.11";
        //public static readonly String App_User = "hpsweb";
        //public static readonly String App_Password = "hpsweb";

        //public static readonly String App_IP_1 = "172.30.42.12";
        //public static readonly String App_User_1 = "hpsweb";
        //public static readonly String App_Password_1 = "hpsweb";

        //public static readonly String Db_IP = "10.172.252.132";
        //public static readonly String Db_User = "oracle";
        //public static readonly String Db_Password = "Ebl2022it";

        //public static readonly String Core_Db_IP = "172.25.50.226";
        //public static readonly String Core_Db_User = "oracle";
        //public static readonly String Core_Db_Password = "Oracle12345";


    }
}
