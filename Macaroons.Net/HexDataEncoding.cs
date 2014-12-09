using System;
using System.Runtime.Remoting.Metadata.W3cXsd2001;


namespace Macaroons.Net
{
  /// <summary>
  /// Represents an encoding for converting to and from hexadecimal strings.
  /// </summary>
  public class HexDataEncoding : DataEncoding
  {
    public override string GetString(byte[] d)
    {
      SoapHexBinary hb = new SoapHexBinary(d);
      return hb.ToString();
    }


    public override byte[] GetBytes(string s)
    {
      throw new NotImplementedException();
    }
  }
}
