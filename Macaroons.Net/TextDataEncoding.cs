using CuttingEdge.Conditions;
using System.Text;


namespace Macaroons
{
  /// <summary>
  /// Represents an encoding for converting to and from strings in UTF8, ASCII and other formats.
  /// </summary>
  public class TextDataEncoding : DataEncoding
  {
    protected Encoding Enc { get; set; }


    public TextDataEncoding(Encoding enc)
    {
      Condition.Requires(enc, "enc").IsNotNull();
      Enc = enc;
    }


    public override string GetString(byte[] d)
    {
      return Enc.GetString(d);
    }


    public override byte[] GetBytes(string s)
    {
      return Enc.GetBytes(s);
    }
  }
}
