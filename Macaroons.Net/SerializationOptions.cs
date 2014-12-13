namespace Macaroons
{
  /// <summary>
  /// Represents a set of options for serializing macaroons.
  /// </summary>
  public class SerializationOptions
  {
    /// <summary>
    /// Text encoding for macaroon identifiers (default is UTF8).
    /// </summary>
    public DataEncoding MacaroonIdentifierEncoding { get; set; }

    /// <summary>
    /// Text encoding for caveat identifiers (default is UTF8).
    /// </summary>
    public DataEncoding CaveatIdentifierEncoding { get; set; }


    /// <summary>
    /// Default serialization options.
    /// </summary>
    public static readonly SerializationOptions Default = new SerializationOptions
    {
      MacaroonIdentifierEncoding = DataEncoding.UTF8,
      CaveatIdentifierEncoding = DataEncoding.UTF8
    };
  }
}
