using System.Security.Cryptography;

namespace QuickMarket.Api.Utils
{
    public static class SecurityUtils
    {
        public static string HashPassword(string password) =>
            BCrypt.Net.BCrypt.HashPassword(password, workFactor: 11);

        public static bool VerifyPassword(string password, string hash) =>
            BCrypt.Net.BCrypt.Verify(password, hash);

        // Genera un código de 5 dígitos "00000..99999"
        public static string Generate5DigitCode()
        {
            Span<byte> bytes = stackalloc byte[4];
            RandomNumberGenerator.Fill(bytes);
            int code = (int)(BitConverter.ToUInt32(bytes) % 100000);
            return code.ToString("D5");
        }
    }
}
