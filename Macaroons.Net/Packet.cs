using CuttingEdge.Conditions;
using System.Text;


namespace Macaroons
{
  /// <summary>
  /// Represents a blob of bytes and associated encoding for textual representation of it.
  /// </summary>
  public class Packet
  {
    /// <summary>
    /// The data in this packet.
    /// </summary>
    public byte[] Data { get; protected set; }


    /// <summary>
    /// The length of the data array (number of bytes).
    /// </summary>
    public int Length { get { return Data.Length; } }


    /// <summary>
    /// Encoding for representing data as text.
    /// </summary>
    public DataEncoding Encoding { get; protected set; }


    /// <summary>
    /// Indexer for accessing the bytes in the array
    /// </summary>
    /// <param name="i">Array index.</param>
    /// <returns></returns>
    public byte this[int i]
    {
      get { return Data[i]; }
      set { Data[i] = value; }
    }


    /// <summary>
    /// Initialize packet from raw data bytes and associated textual encoding.
    /// </summary>
    /// <param name="data">Data bytes.</param>
    /// <param name="enc">Text encoding.</param>
    public Packet(byte[] data, DataEncoding enc)
    {
      Condition.Requires(enc, "enc").IsNotNull();

      Data = data;
      Encoding = enc;
    }


    /// <summary>
    /// Initialize packet from a string using the associated textual encoding for conversion to byte array.
    /// </summary>
    /// <param name="s">String representation of data.</param>
    /// <param name="enc">Text encoding (default is UTF8).</param>
    public Packet(string s, DataEncoding enc = null)
    {
      if (enc == null)
        enc = DataEncoding.UTF8;

      if (s != null)
        Data = enc.GetBytes(s);
      Encoding = enc;
    }


    /// <summary>
    /// Initialize packet with the values from another packet.
    /// </summary>
    /// <param name="src">Source packet.</param>
    public Packet(Packet src)
    {
      Condition.Requires(src, "src").IsNotNull();

      if (src.Data != null)
        Data = Utility.CopyByteArray(src.Data);
      Encoding = src.Encoding;
    }


    public override string ToString()
    {
      if (Data == null)
        return null;
      return Encoding.GetString(Data);
    }


    public override bool Equals(object obj)
    {
      Packet p = obj as Packet;
      if (p == null)
        return false;
      // Encoding is not considered part of equality.
      return Utility.ByteArrayEquals(Data, p.Data);
    }


    public override int GetHashCode()
    {
      unchecked
      {
        int hash = 17;

        // Cycle through each element in the array.
        foreach (byte b in Data)
        {
            // Update the hash.
            hash = hash * 23 + b.GetHashCode();
        }

        return hash;
      }
    }


    public static bool operator ==(Packet p1, Packet p2)
    {
      if ((object)p1 == null && (object)p2 == null)
        return true;
      if ((object)p1 == null || (object)p2 == null)
        return false;
      return p1.Equals(p2);
    }


    public static bool operator !=(Packet p1, Packet p2)
    {
      return !(p1 == p2);
    }
  }
}
