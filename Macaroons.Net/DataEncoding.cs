using System.Text;


namespace Macaroons
{
  /// <summary>
  /// Represents an encoding scheme for conversion between bytes and strings. Primarily for getting string representations of caveat and macaroon locations but also used for debugging other values.
  /// </summary>
  public abstract class DataEncoding
  {
    /// <summary>
    /// Decode all bytes and return string representation.
    /// </summary>
    /// <param name="d"></param>
    /// <returns></returns>
    public abstract string GetString(byte[] d);


    /// <summary>
    /// Encode all bytes needed to represent string.
    /// </summary>
    /// <param name="s"></param>
    /// <returns></returns>
    public abstract byte[] GetBytes(string s);


    public readonly static DataEncoding UTF8 = new TextDataEncoding(Encoding.UTF8);

    public readonly static DataEncoding ASCII = new TextDataEncoding(Encoding.ASCII);

    public readonly static DataEncoding Hex = new HexDataEncoding();

    public readonly static DataEncoding Base64UrlSafe = new Base64UrlSafeDataEncoding();
  }
}
