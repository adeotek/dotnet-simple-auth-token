using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace SimpleAuthToken
{
    public static class TokenProvider
    {
        public static (string, DateTime) GenerateToken(string publicKey, string secretKey, string issuer = null, int validity = 10)
        {
            if (string.IsNullOrEmpty(publicKey))
            {
                throw new Exception("Invalid public key for authorization TOKEN generation");
            }

            if (string.IsNullOrEmpty(secretKey))
            {
                throw new Exception("Invalid secret key for authorization TOKEN generation");
            }
            
            var token = ComputeSha256($"{issuer ?? string.Empty}-{secretKey}").ToUpper();
            var expiresAt = DateTime.UtcNow.AddMinutes(validity);
            var expiresAtHash = BitConverter.ToString(Encoding.ASCII.GetBytes(expiresAt.ToString("yyyy-MM-ddTHH:mm:ss"))).Replace("-", "");
            return ($"{publicKey}{token}{expiresAtHash}", expiresAt);
        }

        public static bool ValidateToken(string token, string secretKey, string issuer, out string publicKey)
        {
            if (string.IsNullOrEmpty(token) || token.Length < 102)
            {
                publicKey = null;
                return false;
            }

            if (string.IsNullOrEmpty(secretKey))
            {
                throw new Exception("Invalid secret key for authorization TOKEN");
            }
            
            var hash = ComputeSha256($"{issuer ?? string.Empty}-{secretKey}")?.ToUpper() ?? string.Empty;
            publicKey = token.Substring(0, token.Length - 102);
            var pToken = token.Substring(token.Length - 102, 64);
            if (!hash.Equals(pToken, StringComparison.InvariantCultureIgnoreCase))
            {
                return false;
            }
            
            var expiresAtString = token.Substring(token.Length - 38);
            var expiresAtArray = Enumerable.Range(0, expiresAtString.Length)
                .Where(x => x % 2 == 0)
                .Select(x => Convert.ToByte(expiresAtString.Substring(x, 2), 16))
                .ToArray();
            
            if (!DateTime.TryParse(Encoding.ASCII.GetString(expiresAtArray), out var expiresAt))
            {
                return false;
            }
            
            return expiresAt > DateTime.UtcNow;
        }

        private static string ComputeSha256(string data)
        {
            if (string.IsNullOrEmpty(data))
            {
                return data;
            }

            var hash = SHA256.Create().ComputeHash(Encoding.UTF8.GetBytes(data));
            return BinaryHashToString(hash);
        }

        private static string BinaryHashToString(IEnumerable<byte> hashData)
            => hashData?.Aggregate(string.Empty, (current, b) => current + $"{b:x2}");
    }
}