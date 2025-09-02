using System;
using System.Security.Cryptography;
using System.Text;

namespace ECare_Revamp.Models
{
    public class Generate_Random_Number
    {
        private readonly IConfiguration _configuration;
        private readonly Random _random = new Random();
        public Generate_Random_Number(IConfiguration configuration)
        {
            _configuration = configuration;
        }
        public string GenerateSecureRandomAlphanumeric(int length)
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
            var result = new char[length];

            for (int i = 0; i < length; i++)
            {
                int index = RandomNumberGenerator.GetInt32(chars.Length);
                result[i] = chars[index];
            }

            return new string(result);
        }
        public int Random_Number(int min, int max)
        {
            return _random.Next(min, max);
        }
        public string RandomString(int size, bool lowerCase = false)
        {
            var FinalRefBuilder = new StringBuilder();
            var builder = new StringBuilder(size);
            char offset = lowerCase ? 'a' : 'A';
            const int lettersOffset = 26;

            builder.Append(Random_Number(3, 999));
            for (var i = 0; i < 5; i++)
            {
                var @char = (char)_random.Next(offset, offset + lettersOffset);
                builder.Append(@char);

            }
            builder.Append(Random_Number(1, 999));
            for (var j = 0; j < 4; j++)
            {
                var @char = (char)_random.Next(offset, offset + lettersOffset);
                builder.Append(@char);

            }
            return lowerCase ? builder.ToString().ToLower() : builder.ToString();
        }
    }
}
