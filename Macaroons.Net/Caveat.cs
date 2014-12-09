using CuttingEdge.Conditions;
using System;
using System.Text;


namespace Macaroons.Net
{
  /// <summary>
  /// Represents a single caveat.
  /// </summary>
  public class Caveat
  {
    /// <summary>
    /// Caveat identifier.
    /// </summary>
    public Packet CId { get; protected set; }

    /// <summary>
    /// Verification identifier.
    /// </summary>
    public Packet VId { get; protected set; }

    /// <summary>
    /// Caveat location.
    /// </summary>
    public Packet Cl { get; protected set; }


    /// <summary>
    /// Initialize first party caveat from caveat identifer and location (both as strings).
    /// </summary>
    /// <param name="cid">Caveat identifer.</param>
    /// <param name="cl">Caveat location.</param>
    public Caveat(string cid, string cl = null)
    {
      Condition.Requires(cid).IsNotNull();
      CId = new Packet(cid);
      if (cl != null)
        Cl = new Packet(cl);
    }


    /// <summary>
    /// Initialize third party caveat from binary data.
    /// </summary>
    /// <param name="cid">Caveat identifier.</param>
    /// <param name="vid">Verification identifier.</param>
    /// <param name="cl">Caveat location.</param>
    public Caveat(Packet cid, Packet vid, Packet cl)
    {
      Condition.Requires(cid).IsNotNull();
      CId = cid;
      VId = vid;
      Cl = cl;
    }


    /// <summary>
    /// Initialize caveat with the values from another caveat.
    /// </summary>
    /// <param name="src"></param>
    public Caveat(Caveat src)
    {
      CId = new Packet(src.CId);
      if (src.VId != null)
        VId = new Packet(src.VId);
      if (src.Cl != null)
        Cl = new Packet(src.Cl);
    }


    public bool IsFirstPartyCaveat
    {
      get { return VId == null; }
    }


    public bool IsThirdPartyCaveat
    {
      get { return VId != null; }
    }


    /// <summary>
    /// Get long string representation of all caveat values.
    /// </summary>
    /// <returns></returns>
    public string Inspect()
    {
      return "CId = " + CId
       + (VId != null ? "\r\n  VId = " + VId : "")
       + (Cl != null ? "\r\n  Cl = " + Cl : "");
    }


    public override string ToString()
    {
      return CId.ToString();
    }
  }
}
