namespace Macaroons
{
  /// <summary>
  /// Represents an encoding for converting to and from BASE64 URL safe strings.
  /// </summary>
  public class Base64UrlSafeDataEncoding : DataEncoding
  {
    public override string GetString(byte[] d)
    {
      return Utility.ToBase64UrlSafe(d);
    }


    public override byte[] GetBytes(string s)
    {
      return Utility.FromBase64UrlSafe(s);
    }
  }
}
