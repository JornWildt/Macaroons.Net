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
    /// <param name="predicate">The predicate.</param>
    public void SatisfyExact(string predicate)
    {
      Condition.Requires(predicate, "predicate").IsNotNull();
      SatisfyExact(new Packet(predicate));
    }


    /// <summary>
    /// Add binary representation of predicate.
    /// </summary>
    /// <param name="predicate">The predicate.</param>
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


    /// <summary>
    /// Verify macaroon with respect to the verifier's set of valid predicates and a set of discharge macaroons.
    /// </summary>
    /// <param name="m"></param>
    /// <param name="key"></param>
    /// <param name="ms"></param>
    /// <returns></returns>
    public VerificationResult Verify(Macaroon m, string key, List<Macaroon> ms = null)
    {
      Condition.Requires(m, "m").IsNotNull();
      Condition.Requires(key, "key").IsNotNull();

      return m.Verify(this, key, ms);
    }


    /// <summary>
    /// Verify macaroon with respect to the verifier's set of valid predicates and a set of discharge macaroons.
    /// </summary>
    public VerificationResult Verify(Macaroon m, Packet key, List<Macaroon> ms = null)
    {
      Condition.Requires(m, "m").IsNotNull();
      Condition.Requires(key, "key").IsNotNull();

      return m.Verify(this, key, ms);
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
