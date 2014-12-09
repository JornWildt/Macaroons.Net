using CuttingEdge.Conditions;
using System;
using System.Collections.Generic;
using System.Text;


namespace Macaroons
{
  /// <summary>
  /// Represents a verifier mechanism for verifying macaroons against a list of predicates.
  /// </summary>
  public class Verifier
  {
    protected List<Packet> Predicates { get; set; }

    protected List<Func<Packet, bool>> VerifierCallbacks { get; set; }


    /// <summary>
    /// Initialize verifier with an empty list of predicates.
    /// </summary>
    public Verifier()
    {
      Predicates = new List<Packet>();
      VerifierCallbacks = new List<Func<Packet, bool>>();
    }


    /// <summary>
    /// Add string representation of a predicate.
    /// </summary>
    /// <param name="predicate"></param>
    public void SatisfyExact(string predicate)
    {
      Condition.Requires(predicate, "predicate").IsNotNull();
      SatisfyExact(new Packet(predicate));
    }


    public void SatisfyExact(Packet predicate)
    {
      Condition.Requires(predicate, "predicate");
      Predicates.Add(predicate);
    }


    public void SatisfyGeneral(Func<Packet, bool> verifier)
    {
      Condition.Requires(verifier, "verifier").IsNotNull();
      VerifierCallbacks.Add(verifier);
    }

    
    public bool IsValidFirstPartyCaveat(Packet cid)
    {
      foreach (Packet p in Predicates)
        if (p == cid)
          return true;

      foreach (Func<Packet, bool> verifier in VerifierCallbacks)
        if (verifier(cid))
          return true;

      return false;
    }
  }
}
