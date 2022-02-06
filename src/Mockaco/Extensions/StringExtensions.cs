using System.Text;

namespace System
{
    public static class StringExtensions
    {
        public static string ToSHA1Hash(this string input)
        {
            using var sha1 = Security.Cryptography.SHA1.Create();

            var inputBytes = Encoding.UTF8.GetBytes(input);
            var hashBytes = sha1.ComputeHash(inputBytes);

            var stringBuilder = new StringBuilder();

            for (int i = 0; i < hashBytes.Length; i++)
            {
                stringBuilder.Append(hashBytes[i].ToString("X2"));
            }

            return stringBuilder.ToString();
        }
        
        public static bool IsRemoteAbsolutePath(this string input)
        {
            if (Uri.TryCreate(input, UriKind.Absolute, out var uri))
            {
                return !uri.IsFile;
            }

            return false;
        }
    }
}
