using System.Text;

namespace System
{
    public static class StringExtensions
    {
        public static string ToMD5Hash(this string input)
        {
            using (Security.Cryptography.MD5 md5 = Security.Cryptography.MD5.Create())
            {
                var inputBytes = Encoding.UTF8.GetBytes(input);
                var hashBytes = md5.ComputeHash(inputBytes);

                StringBuilder sb = new StringBuilder();
                for (int i = 0; i < hashBytes.Length; i++)
                {
                    sb.Append(hashBytes[i].ToString("X2"));
                }
                return sb.ToString();
            }
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
