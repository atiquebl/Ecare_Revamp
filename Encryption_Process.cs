using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.OpenSsl;
using Org.BouncyCastle.Security;
using System.Security.Cryptography;
using System.Text;

namespace ECare_Revamp.Script
{
    public class Encryption_Process
    {
        private readonly string _encryptionKey;
        public Encryption_Process(IConfiguration configuration)
        {
            _encryptionKey = configuration.GetValue<string>("Encryption:Key")
                             ?? throw new InvalidOperationException("Encryption key not found in configuration.");
        }

        #region Decryption Methods
        public string DecryptString(string key, string cipherText)
        {
            byte[] fullCipher = Convert.FromBase64String(cipherText);
            using (Aes aes = Aes.Create())
            {
                var pdb = new Rfc2898DeriveBytes(key, new byte[] { 0x49, 0x76, 0x61, 0x6e, 0x20, 0x4d, 0x65, 0x64, 0x76, 0x65, 0x64, 0x65, 0x76 }, 10000, HashAlgorithmName.SHA256);
                aes.Key = pdb.GetBytes(32);

                byte[] iv = new byte[16];
                Array.Copy(fullCipher, 0, iv, 0, iv.Length);
                aes.IV = iv;

                int cipherTextStartIndex = iv.Length;
                int cipherTextLength = fullCipher.Length - cipherTextStartIndex;

                using (var ms = new MemoryStream(fullCipher, cipherTextStartIndex, cipherTextLength))
                using (var cs = new CryptoStream(ms, aes.CreateDecryptor(), CryptoStreamMode.Read))
                using (var sr = new StreamReader(cs, Encoding.UTF8))
                {
                    return sr.ReadToEnd();
                }
            }
        }
        #endregion

        #region Encryption Methods
        public string EncrytpString(string encryptString)
        {
            byte[] clearBytes = Encoding.UTF8.GetBytes(encryptString);
            using (Aes encryptor = Aes.Create())
            {
                var pdb = new Rfc2898DeriveBytes(_encryptionKey, new byte[] { 0x49, 0x76, 0x61, 0x6e, 0x20, 0x4d, 0x65, 0x64, 0x76, 0x65, 0x64, 0x65, 0x76 }, 10000, HashAlgorithmName.SHA256);
                encryptor.Key = pdb.GetBytes(32);
                encryptor.GenerateIV();
                byte[] iv = encryptor.IV;

                using (var ms = new MemoryStream())
                {
                    ms.Write(iv, 0, iv.Length); // Prepend IV
                    using (var cs = new CryptoStream(ms, encryptor.CreateEncryptor(), CryptoStreamMode.Write))
                    {
                        cs.Write(clearBytes, 0, clearBytes.Length);
                        cs.FlushFinalBlock();
                    }
                    return Convert.ToBase64String(ms.ToArray());
                }
            }
        }
        #endregion

    }
}