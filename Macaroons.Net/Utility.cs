using CuttingEdge.Conditions;
using System;
using System.Runtime.InteropServices;


namespace Macaroons
{
  public static class Utility
  {
    public static string ToBase64UrlSafe(byte[] data)
    {
      return Convert.ToBase64String(data).Replace("=", String.Empty).Replace('+', '-').Replace('/', '_');
    }


    public static byte[] FromBase64UrlSafe(string s)
    {
      s = s.PadRight(s.Length + (4 - s.Length % 4) % 4, '=');
      s = s.Replace('-', '+').Replace('_', '/');
      return Convert.FromBase64String(s);
    }


    public static byte[] CopyByteArray(byte[] src)
    {
      if (src == null)
        return null;

      byte[] dst = new byte[src.Length];
      Buffer.BlockCopy(src, 0, dst, 0, src.Length);

      return dst;
    }


    [DllImport("msvcrt.dll", CallingConvention = CallingConvention.Cdecl)]
    static extern int memcmp(byte[] b1, byte[] b2, long count);


    internal static bool ByteArrayEquals(byte[] b1, byte[] b2)
    {
      // Validate buffers are the same length.
      // This also ensures that the count does not exceed the length of either buffer.  
      return b1.Length == b2.Length && memcmp(b1, b2, b1.Length) == 0;
    }


    public static void DataSizeLessThanOrEqual(this ConditionValidator<Packet> validator, int length)
    {
      if (validator.Value.Length > length)
        throw new ArgumentException(string.Format("{0} data length too big (got {1} bytes, max is {2})", validator.ArgumentName, validator.Value.Length, length));
    }


    public static void DataSizeEquals(this ConditionValidator<Packet> validator, int length)
    {
      if (validator.Value.Length != length)
        throw new ArgumentException(string.Format("{0} data length not expected size (got {1} bytes, max is {2})", validator.ArgumentName, validator.Value.Length, length));
    }
  }
}
