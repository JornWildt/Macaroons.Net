using System;
#if NET46
using System.Runtime.Remoting.Metadata.W3cXsd2001;
#endif


namespace Macaroons
{
  /// <summary>
  /// Represents an encoding for converting to and from hexadecimal strings.
  /// </summary>
  public class HexDataEncoding : DataEncoding
  {
    public override string GetString(byte[] d)
    {
#if NET46
      SoapHexBinary hb = new SoapHexBinary(d);
      return hb.ToString();
#else
      return BitConverter.ToString(d).Replace("-", "");
#endif
    }


    public override byte[] GetBytes(string s)
    {
      throw new NotImplementedException();
    }
  }
}
