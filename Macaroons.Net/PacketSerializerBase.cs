using System.Text;


namespace Macaroons
{
  /// <summary>
  /// Represents a base for packet reader and writer.
  /// </summary>
  public class PacketSerializerBase
  {
    /// <summary>
    /// Number of bytes in packet prefix.
    /// </summary>
    public const int PACKET_PREFIX = 4;

    /// <summary>
    /// Location identifier represented as ASCII bytes.
    /// </summary>

    /// <summary>
    /// "Location" identifier represented as ASCII bytes.
    /// </summary>
    public static readonly byte[] LocationID = Encoding.ASCII.GetBytes("location");

    /// <summary>
    /// "Identifier" identifier represented as ASCII bytes.
    /// </summary>
    public static readonly byte[] IdentifierID = Encoding.ASCII.GetBytes("identifier");

    /// <summary>
    /// "Signature" identifier represented as ASCII bytes.
    /// </summary>
    public static readonly byte[] SignatureID = Encoding.ASCII.GetBytes("signature");

    /// <summary>
    /// "CId" identifier represented as ASCII bytes.
    /// </summary>
    public static readonly byte[] CIdID = Encoding.ASCII.GetBytes("cid");

    /// <summary>
    /// "VId" identifier represented as ASCII bytes.
    /// </summary>
    public static readonly byte[] VIdID = Encoding.ASCII.GetBytes("vid");

    /// <summary>
    /// "Cl" identifier represented as ASCII bytes.
    /// </summary>
    public static readonly byte[] ClID = Encoding.ASCII.GetBytes("cl");

    internal const string Hex = "0123456789abcdef";
  }
}
