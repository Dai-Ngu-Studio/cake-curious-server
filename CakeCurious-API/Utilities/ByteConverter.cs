using System.Text;

namespace CakeCurious_API.Utilities
{
    public static class ByteConvertUtility
    {
        public static string ToVarbinary(byte[] bytes)
        {
            var sb = new StringBuilder((bytes.Length * 2) + 2);
            sb.Append("0x");
            for (int i = 0; i < bytes.Length; i++)
            {
                sb.Append(bytes[i].ToString("X2"));
            }
            return sb.ToString();
        }
    }
}
