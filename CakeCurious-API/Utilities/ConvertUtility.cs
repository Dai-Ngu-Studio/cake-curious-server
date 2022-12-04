using System.Security.Cryptography;
using System.Text;

namespace CakeCurious_API.Utilities
{
    public static class ConvertUtility
    {
        public static Guid ToGuid(string value)
        {
            var md5Hasher = MD5.Create();
            byte[] data = md5Hasher.ComputeHash(Encoding.Default.GetBytes(value));
            return new Guid(data);
        }
    }
}
