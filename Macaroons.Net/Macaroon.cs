﻿using CuttingEdge.Conditions;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Remoting.Metadata.W3cXsd2001;
using System.Security.Cryptography;
using System.Text;


namespace Macaroons
{
  public class Macaroon
  {
    #region Constants

    public const int MACAROON_HASH_BYTES = 32;
    public const int MACAROON_SECRET_KEY_BYTES = 32;
    public const int MACAROON_SECRET_NONCE_BYTES = 24;

    /*
     * The number of zero bytes required by crypto_secretbox
     * before the plaintext.
     */
    public const int MACAROON_SECRET_TEXT_ZERO_BYTES = 32;

    /*
     * The number of zero bytes placed by crypto_secretbox
     * before the ciphertext
     */
    public const int MACAROON_SECRET_BOX_ZERO_BYTES = 16;

    public const int SECRET_BOX_OVERHEAD = MACAROON_SECRET_TEXT_ZERO_BYTES - MACAROON_SECRET_BOX_ZERO_BYTES;
    public const int VID_NONCE_KEY_SZ = MACAROON_SECRET_NONCE_BYTES + MACAROON_HASH_BYTES + SECRET_BOX_OVERHEAD;


    public const int MACAROON_MAX_STRLEN = 32768;

    /* Place a sane limit on the number of caveats */
    public const int MACAROON_MAX_CAVEATS = 65536;

    /* Recommended secret length */
    public const int MACAROON_SUGGESTED_SECRET_LENGTH = 32;

    #endregion


    #region Public properties

    public Packet Location { get; protected set; }

    public Packet Identifier { get; protected set; }

    public Packet Signature { get; protected set; }

    public IList<Caveat> Caveats { get { return CaveatsList.AsReadOnly(); } }

    public IEnumerable<Caveat> ThirdPartyCaveats { get { return Caveats.Where(c => c.IsThirdPartyCaveat); } }

    #endregion


    #region Internal properties

    protected List<Caveat> CaveatsList { get; set; }

    #endregion


    #region Creation

    protected Macaroon()
    {
    }


    public Macaroon(Macaroon src)
    {
      Condition.Requires(src, "src").IsNotNull();

      if (src.Location != null)
        Location = new Packet(src.Location);
      Identifier = new Packet(src.Identifier);
      Signature = new Packet(src.Signature);

      CaveatsList = new List<Caveat>(src.CaveatsList.Count);
      foreach (Caveat c in src.CaveatsList)
      {
        CaveatsList.Add(new Caveat(c));
      }
    }


    public Macaroon(string location, string key, string identifier)
    {
      Condition.Requires(key, "key").IsNotNull();
      Condition.Requires(identifier, "identifier").IsNotNull();

      Initialize(location != null ? new Packet(location) : null, new Packet(key), new Packet(identifier));
    }


    public Macaroon(Packet location, Packet key, Packet identifier)
    {
      Initialize(location, key, identifier);
    }


    protected void Initialize(Packet location, Packet key, Packet identifier)
    {
      Condition.Requires(key, "key").IsNotNull();

      Packet derievedKey = GenerateDerivedKey(key);

      InitializeRaw(location, derievedKey, identifier);
    }


    protected void InitializeRaw(Packet location, Packet key, Packet identifier)
    {
      if (location != null)
        Condition.Requires(location, "location").DataSizeLessThanOrEqual(MACAROON_MAX_STRLEN);

      Condition.Requires(key, "key").IsNotNull();
      Condition.Requires(identifier, "identifier").IsNotNull().DataSizeLessThanOrEqual(MACAROON_MAX_STRLEN);

      Location = location;
      Identifier = identifier;
      Signature = CalculateHash1(key, Identifier);
      CaveatsList = new List<Caveat>();
    }


    public Macaroon AddFirstPartyCaveat(string predicate)
    {
      Condition.Requires(predicate, "predicate").IsNotNull();

      AddFirstPartyCaveat(new Packet(predicate));

      return this;
    }


    public Macaroon AddFirstPartyCaveat(Packet predicate)
    {
      Condition.Requires(Signature != null  &&  Signature.Length > PacketSerializerBase.PACKET_PREFIX).IsTrue();
      Condition.Requires(CaveatsList.Count + 1, "Number of caveats").IsLessThan(MACAROON_MAX_CAVEATS);

      Packet hash = CalculateHash1(Signature, predicate);

      CaveatsList.Add(new Caveat(predicate, null, null));

      Signature = hash;

      return this;
    }


    public Macaroon AddThirdPartyCaveat(string location, string key, string identifier)
    {
      Condition.Requires(identifier, "identifier").IsNotNull();
      Condition.Requires(key, "key").IsNotNull();

      AddThirdPartyCaveat(location != null ? new Packet(location) : null, new Packet(key), new Packet(identifier));

      return this;
    }


    public Macaroon AddThirdPartyCaveat(Packet location, Packet key, Packet identifier)
    {
      Condition.Requires(key, "key").IsNotNull();

      Packet derievedKey = GenerateDerivedKey(key);
      AddThirdPartyCaveatRaw(location, derievedKey, identifier);

      return this;
    }


    protected void AddThirdPartyCaveatRaw(Packet location, Packet key, Packet identifier)
    {
      if (location != null)
        Condition.Requires(location, "location").IsNotNull().DataSizeLessThanOrEqual(MACAROON_MAX_STRLEN);
      Condition.Requires(identifier, "identifier").IsNotNull().DataSizeLessThanOrEqual(MACAROON_MAX_STRLEN);
      Condition.Requires(key, "key").IsNotNull().DataSizeEquals(MACAROON_SUGGESTED_SECRET_LENGTH);
      Condition.Requires(CaveatsList.Count + 1, "Number of caveats").IsLessThan(MACAROON_MAX_CAVEATS);

      byte[] enc_nonce = new byte[MACAROON_SECRET_NONCE_BYTES];

      // Encrypt the secret key using the current signature as encryption key. The nonce is a parameter for SecretBox encryption.
      byte[] cipherText = SecretBoxCreate(Signature.Data, enc_nonce, key.Data);

      // Create "vid" as the concatenation of the nonce and the cipher text (without the prefixed zero bytes from the encryption algorithm)
      byte[] vid = new byte[MACAROON_SECRET_NONCE_BYTES + cipherText.Length - MACAROON_SECRET_BOX_ZERO_BYTES];
      Buffer.BlockCopy(enc_nonce, 0, vid, 0, MACAROON_SECRET_NONCE_BYTES);
      Buffer.BlockCopy(cipherText, MACAROON_SECRET_BOX_ZERO_BYTES, vid, MACAROON_SECRET_NONCE_BYTES, cipherText.Length - MACAROON_SECRET_BOX_ZERO_BYTES);

      Packet newSig = CalculateHash2(Signature, new Packet(vid, DataEncoding.Base64UrlSafe), identifier);

      CaveatsList.Add(new Caveat(identifier, new Packet(vid, DataEncoding.Base64UrlSafe), location));

      Signature = newSig;
    }


    // FIXME: move else where (make configurable?)

    protected byte[] SecretBoxCreate(byte[] key, byte[] nonce, byte[] plainText)
    {
      byte[] cipherText = Sodium.SecretBox.Create(plainText, nonce, key);
      return cipherText;
    }


    protected Packet SecretBoxOpen(byte[] key, byte[] nonce, byte[] cipherText)
    {
      byte[] plainText = Sodium.SecretBox.Open(cipherText, nonce, key);
      return new Packet(plainText, DataEncoding.Hex);
    }


    public Macaroon PrepareForRequest(Macaroon d)
    {
      Packet boundSignature = Bind(Signature, d.Signature);
      Macaroon bound = new Macaroon(d) { Signature = boundSignature };
      return bound;
    }


    protected Packet Bind(Packet sig1, Packet sig2)
    {
      byte[] zeros = new byte[MACAROON_HASH_BYTES];
      Packet boundSignature = CalculateHash2(new Packet(zeros, DataEncoding.Hex), sig1, sig2);
      return boundSignature;
    }


    #endregion


    #region Verification

    /// <summary>
    /// Verify this macaroon with respect to a set of valid predicates and a set of discharge macaroons.
    /// </summary>
    /// <param name="v">Verifier containing all valid first party caveat predicates.</param>
    /// <param name="key">Authorizating macaroon root key.</param>
    /// <param name="ms">List of all discharging macaroons.</param>
    /// <returns></returns>
    public VerificationResult Verify(Verifier v, string key, List<Macaroon> ms = null)
    {
      Condition.Requires(key, "key").IsNotNull();

      return Verify(v, new Packet(key), ms);
    }

    
    public VerificationResult Verify(Verifier v, Packet key, List<Macaroon> ms = null)
    {
      if (ms == null)
        ms = new List<Macaroon>();

      Packet derivedKey = GenerateDerivedKey(key);

      return VerifyRaw(v, derivedKey, ms);
    }


    protected VerificationResult VerifyRaw(Verifier v, Packet key, List<Macaroon> ms)
    {
      Condition.Requires(v, "v").IsNotNull();
      Condition.Requires(key, "key").IsNotNull().DataSizeEquals(Macaroon.MACAROON_SUGGESTED_SECRET_LENGTH);
      Condition.Requires(ms, "ms").IsNotNull();

      return VerifyInner(this, v, key, ms, new Stack<Macaroon>());
    }


    /// <summary>
    /// Recursive verification of both 1st and 3rd party caveats in this macaroon.
    /// </summary>
    /// <param name="TM">Primary authorizing macaroon.</param>
    /// <param name="v">Verifier containing all valid first party caveat predicates.</param>
    /// <param name="key">Secret key for calculating signatures.</param>
    /// <param name="ms">List of all discharging macaroons.</param>
    /// <param name="treePath">Stack containing the macaroons already visitied while trying to verify third party macaroons.</param>
    /// <returns>Result of verification.</returns>
    protected VerificationResult VerifyInner(Macaroon TM, Verifier v, Packet key, List<Macaroon> ms, Stack<Macaroon> treePath)
    {
      VerificationResult result = new VerificationResult();

      Packet csig = CalculateHash1(key, Identifier);

      foreach (Caveat c in Caveats)
      {
        if (c.IsFirstPartyCaveat)
        {
          if (!VerifyInner1st(c,v))
            result.AddFailure(string.Format("Caveat '{0}' failed", c));
          csig = CalculateHash1(csig, c.CId);
        }
        else
        {
          VerificationResult res = VerifyInner3rd(TM, c, v, ms, csig, treePath);
          result.MergeFailures(res);
          csig = CalculateHash2(csig, c.VId, c.CId);
        }
      }

      // Is this a discharge macaroon? Then bind signature to primary authorizing macaroon
      if (treePath.Count > 0)
      {
        csig = Bind(TM.Signature, csig);
      }

      bool isValidSignature = (Signature == csig);
      if (!isValidSignature)
        result.AddFailure("Signature mismatch");

      return result;
    }


    protected bool VerifyInner1st(Caveat c, Verifier v)
    {
      return v.IsValidFirstPartyCaveat(c.CId);
    }


    protected VerificationResult VerifyInner3rd(Macaroon TM, Caveat c, Verifier v, List<Macaroon> ms, Packet csig, Stack<Macaroon> treePath)
    {
      // Find discharge macaroon
      Macaroon discharge = ms.Where(m => m.Identifier == c.CId).FirstOrDefault();

      // No discharge found? Thats an error.
      if (discharge == null)
        return new VerificationResult(string.Format("No discharge macaroon found for caveat '{0}'", c));

      // Have we used this before? That would mean a circular reference was found
      if (treePath.Contains(discharge))
          return new VerificationResult(string.Format("A circular discharge macaroon reference was found for caveat '{0}'", c));

      // Extract nonce and encrypted root key cipher text from VId. Remember to add SecretBox zero padding
      byte[] nonce = new byte[MACAROON_SECRET_NONCE_BYTES];
      Buffer.BlockCopy(c.VId.Data, 0, nonce, 0, MACAROON_SECRET_NONCE_BYTES);
      byte[] cipherText = new byte[c.VId.Length - MACAROON_SECRET_NONCE_BYTES + MACAROON_SECRET_BOX_ZERO_BYTES];
      Buffer.BlockCopy(c.VId.Data, MACAROON_SECRET_NONCE_BYTES, cipherText, MACAROON_SECRET_BOX_ZERO_BYTES, c.VId.Length - MACAROON_SECRET_NONCE_BYTES);

      // Decrypt root key for discharge macaroon
      Packet key = SecretBoxOpen(csig.Data, nonce, cipherText);

      // Keep track of visited discharge macaroons
      treePath.Push(discharge);

      // Use the root key to verify discharge macaroon recursively
      VerificationResult result = discharge.VerifyInner(TM, v, key, ms, treePath);

      treePath.Pop();

      return result;
    }

    #endregion


    #region IO

    public override string ToString()
    {
      return Location.ToString();
    }


    public string Inspect()
    {
      using (StringWriter w = new StringWriter())
      {
        w.WriteLine("Location = {0}", Location);
        w.WriteLine("Identifier = {0}", Identifier);
        foreach (Caveat c in CaveatsList)
          w.WriteLine(c.Inspect());
        w.WriteLine("Signature = {0}", Signature);
        return w.ToString();
      }
    }


    public string Serialize()
    {
      using (MemoryStream s = new MemoryStream())
      using (PacketWriter w = new PacketWriter(s))
      {
        w.WriteLocationPacket(Location);
        w.WriteIdentifierPacket(Identifier);

        foreach (Caveat c in Caveats)
        {
          w.WriteCIdPacket(c.CId);
          if (c.VId != null)
            w.WriteVIdPacket(c.VId);
          if (c.Cl != null)
            w.WriteClPacket(c.Cl);
        }

        w.WriteSignaturePacket(Signature);

        return Utility.ToBase64UrlSafe(s.ToArray());
      }
    }


    public static Macaroon Deserialize(string s, SerializationOptions options = null)
    {
      if (options == null)
        options = SerializationOptions.Default;

      byte[] data = Utility.FromBase64UrlSafe(s);
      using (MemoryStream m = new MemoryStream(data))
      using (PacketReader r = new PacketReader(m,options))
      {
        Packet location = r.ReadLocationPacket();
        Packet identifier = r.ReadIdentifierPacket();
        Packet signature = null;

        // Start by reading the first packet
        KeyValuePair<byte[], byte[]> packet = r.ReadKVPacket();

        List<Caveat> caveats = new List<Caveat>();
        while (true)
        {
          if (Utility.ByteArrayEquals(PacketSerializerBase.CIdID, packet.Key))
          {
            Packet cid = new Packet(packet.Value, options.CaveatIdentifierEncoding);
            Packet vid = null;
            Packet cl = null;

            // Done with this package, now read next one
            packet = r.ReadKVPacket();

            if (Utility.ByteArrayEquals(PacketSerializerBase.VIdID, packet.Key))
            {
              vid = new Packet(packet.Value, DataEncoding.Base64UrlSafe);

              // Done with this package, now read next one
              packet = r.ReadKVPacket();
            }

            if (Utility.ByteArrayEquals(PacketSerializerBase.ClID, packet.Key))
            {
              cl = new Packet(packet.Value, DataEncoding.UTF8);

              // Done with this package, now read next one
              packet = r.ReadKVPacket();
            }

            Caveat c = new Caveat(cid, vid, cl);
            caveats.Add(c);
          }
          
          if (Utility.ByteArrayEquals(PacketSerializerBase.SignatureID, packet.Key))
          {
            signature = new Packet(packet.Value, DataEncoding.Hex);

            // Done with this package - don't read more packages since signature should be the last one
            break;
          }
        }

        if (signature == null)
          throw new InvalidDataException("Missing signature packet");

        return new Macaroon()
        {
          Location = location,
          Identifier = identifier,
          Signature = signature,
          CaveatsList = caveats
        };
      }
    }

    #endregion


    #region Internal

    internal Packet GenerateDerivedKey(Packet key)
    {
      Condition.Requires(key, "key").IsNotNull();

      // Generate derived key as a hash of a hard coded "secret" value and the original key.
      byte[] genkey = new byte[MACAROON_HASH_BYTES];
      byte[] mkeygen = Encoding.ASCII.GetBytes("macaroons-key-generator");
      Buffer.BlockCopy(mkeygen, 0, genkey, 0, mkeygen.Length);

      return CalculateHash1(genkey, key.Data);
    }


    internal Packet CalculateHash1(Packet key, Packet p)
    {
      return CalculateHash1(key.Data, p.Data);
    }


    internal Packet CalculateHash1(byte[] key, byte[] data)
    {
      using (HMACSHA256 alg = new HMACSHA256(key))
      {
        byte[] hash = alg.ComputeHash(data);
        return new Packet(hash, DataEncoding.Hex);
      }
    }


    internal Packet CalculateHash2(Packet key, Packet data1, Packet data2)
    {
      using (HMACSHA256 alg = new HMACSHA256(key.Data))
      {
        byte[] tmp1 = alg.ComputeHash(data1.Data);
        byte[] tmp2 = alg.ComputeHash(data2.Data);
        byte[] tmp = new byte[MACAROON_HASH_BYTES * 2];
        Buffer.BlockCopy(tmp1, 0, tmp, 0, MACAROON_HASH_BYTES);
        Buffer.BlockCopy(tmp2, 0, tmp, MACAROON_HASH_BYTES, MACAROON_HASH_BYTES);

        byte[] hash = alg.ComputeHash(tmp);

        return new Packet(hash, DataEncoding.Hex);
      }
    }

    #endregion
  }
}
