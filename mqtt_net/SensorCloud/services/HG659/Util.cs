using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace SensorCloud.services.HG659
{
    public static class Util
    {
        public static string Base64Decode(string rawData)
        {
            return Encoding.UTF8.GetString(Convert.FromBase64String(rawData));
        }
        public static string Base64Encode(string rawData)
        {
            return Convert.ToBase64String(Encoding.UTF8.GetBytes(rawData));
        }

        public static string Sha256(string rawData)
        {
            // Create a SHA256   
            using (SHA256 sha256Hash = SHA256.Create())
            {
                // ComputeHash - returns byte array  
                byte[] bytes = sha256Hash.ComputeHash(Encoding.UTF8.GetBytes(rawData));

                // Convert byte array to a string   
                StringBuilder builder = new StringBuilder();
                for (int i = 0; i < bytes.Length; i++)
                {
                    builder.Append(bytes[i].ToString("x2"));
                }
                return builder.ToString();
            }
        }
    }
}
