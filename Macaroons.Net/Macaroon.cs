using CuttingEdge.Conditions;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;


namespace Macaroons
{
  public class Macaroon
  {
    /// <summary>
    /// Specifies which crypto algorithm to use for third party caveats. Can be set to other algorithms if required.
    /// </summary>
    public static CryptoAlgorithm Crypto = new SecretBoxCryptoAlgorithm(true);


    #region Constants

    /// <summary>
    /// Number of bytes in generated hash values.
    /// </summary>
    public const int MACAROON_HASH_BYTES = 32;

    /// <summary>
    /// Max length of "strings" used for identifiers and locations. A bit misleading when using UTF8 encodings.
    /// </summary>
    public const int MACAROON_MAX_STRLEN = 32768;

    /// <summary>
    /// Max allowed number of caveats.
    /// </summary>
    public const int MACAROON_MAX_CAVEATS = 65536;

    /// <summary>
    /// Recommended secret length (should match hash length, otherwise the "derived key" will have the wrong size).
    /// </summary>
    public const int MACAROON_SUGGESTED_SECRET_LENGTH = 32;

    #endregion


    #region Public properties

    /// <summary>
    /// A hint to the macaroon's target location. Usually a human readable string.
    /// </summary>
    public Packet Location { get; protected set; }


    /// <summary>
    /// Macaroon identifier. Sometimes a human readable string - but can for instance also be binary cipher text.
    /// </summary>
    public Packet Identifier { get; protected set; }

    
    /// <summary>
    /// Macaroon signature over the identifier and all contained caveats.
    /// </summary>
    public Packet Signature { get; protected set; }


    /// <summary>
    /// Read only list of all caveats.
    /// </summary>
    public IList<Caveat> Caveats { get { return CaveatsList.AsReadOnly(); } }


    /// <summary>
    /// Selection of all third party caveats.
    /// </summary>
    public IEnumerable<Caveat> ThirdPartyCaveats { get { return Caveats.Where(c => c.IsThirdPartyCaveat); } }

    #endregion


    #region Internal properties

    protected List<Caveat> CaveatsList { get; set; }

    #endregion


    #region Creation

    protected Macaroon()
    {
    }


    /// <summary>
    /// Initializes a macaroon from string representations of location, secret key and identifier.
    /// </summary>
    /// <param name="location">Macaroon location hint.</param>
    /// <param name="key">Secret key.</param>
    /// <param name="identifier">Macaroon identifier</param>
    public Macaroon(string location, string key, string identifier)
    {
      Condition.Requires(key, "key").IsNotNull();
      Condition.Requires(identifier, "identifier").IsNotNull();

      Initialize(location != null ? new Packet(location) : null, new Packet(key), new Packet(identifier));
    }


    /// <summary>
    /// Initializes a macaroon from binary representations of location, secret key and identifier.
    /// </summary>
    /// <param name="location">Macaroon location hint.</param>
    /// <param name="key">Secret key.</param>
    /// <param name="identifier">Macaroon identifier</param>
    public Macaroon(Packet location, Packet key, Packet identifier)
    {
      Initialize(location, key, identifier);
    }


    /// <summary>
    /// Initializes a macaroon with a copy of another macaroon.
    /// </summary>
    /// <param name="src"></param>
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


    /// <summary>
    /// Adds a first party caveat predicate to the macaroon.
    /// </summary>
    /// <param name="predicate">Caveat predicate represented as a string.</param>
    /// <returns>Returns this macaroon (not a copy).</returns>
    public Macaroon AddFirstPartyCaveat(string predicate)
    {
      Condition.Requires(predicate, "predicate").IsNotNull();

      AddFirstPartyCaveat(new Packet(predicate));

      return this;
    }


    /// <summary>
    /// Adds a first party caveat predicate to the macaroon.
    /// </summary>
    /// <param name="predicate">Caveat predicate represented as binary data.</param>
    /// <returns>Returns this macaroon (not a copy).</returns>
    public Macaroon AddFirstPartyCaveat(Packet predicate)
    {
      Condition.Requires(Signature != null  &&  Signature.Length > PacketSerializerBase.PACKET_PREFIX).IsTrue();
      Condition.Requires(CaveatsList.Count + 1, "Number of caveats").IsLessThan(MACAROON_MAX_CAVEATS);

      Packet hash = CalculateHash1(Signature, predicate);

      CaveatsList.Add(new Caveat(predicate, null, null));

      Signature = hash;

      return this;
    }


    /// <summary>
    /// Adds a third party caveat predicate to the macaroon.
    /// </summary>
    /// <param name="location">Hint to the discharge location.</param>
    /// <param name="key">Caveat secret key.</param>
    /// <param name="identifier">Identifier for looking up secret at discharging service.</param>
    /// <returns>Returns this macaroon (not a copy).</returns>
    public Macaroon AddThirdPartyCaveat(string location, string key, string identifier)
    {
      Condition.Requires(identifier, "identifier").IsNotNull();
      Condition.Requires(key, "key").IsNotNull();

      AddThirdPartyCaveat(location != null ? new Packet(location) : null, new Packet(key), new Packet(identifier));

      return this;
    }


    /// <summary>
    /// Adds a third party caveat predicate to the macaroon.
    /// </summary>
    /// <param name="location">Hint to the discharge location.</param>
    /// <param name="key">Caveat secret key.</param>
    /// <param name="identifier">Identifier for looking up secret at discharging service.</param>
    /// <returns>Returns this macaroon (not a copy).</returns>
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

      // Encrypt the secret root key using the current signature as encryption key.
      byte[] vid = Crypto.Encrypt(Signature.Data, key.Data);

      Packet newSig = CalculateHash2(Signature, new Packet(vid, DataEncoding.Base64UrlSafe), identifier);

      CaveatsList.Add(new Caveat(identifier, new Packet(vid, DataEncoding.Base64UrlSafe), location));

      Signature = newSig;
    }


    /// <summary>
    /// Prepare this macaroon for request by binding it to the authorizing macaroon.
    /// </summary>
    /// <param name="d">Authorizing macaroon</param>
    /// <returns>A new bound discharge macaroon ready for sending along with the authorizing macaroon.</returns>
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
    /// <param name="key">Authorizing macaroon root key.</param>
    /// <param name="ms">List of discharging macaroons.</param>
    /// <returns>Result of verification.</returns>
    public VerificationResult Verify(Verifier v, string key, List<Macaroon> ms = null)
    {
      Condition.Requires(v, "v").IsNotNull();
      Condition.Requires(key, "key").IsNotNull();

      return Verify(v, new Packet(key), ms);
    }


    /// <summary>
    /// Verify this macaroon with respect to a set of valid predicates and a set of discharge macaroons.
    /// </summary>
    /// <param name="v">Verifier containing all valid first party caveat predicates.</param>
    /// <param name="key">Authorizing macaroon root key.</param>
    /// <param name="ms">List of discharging macaroons.</param>
    /// <returns>Result of verification.</returns>
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

      try
      {
        // Decrypt root key for discharge macaroon
        byte[] keyData = Crypto.Decrypt(csig.Data, c.VId.Data);
        Packet key = new Packet(keyData, DataEncoding.Hex);

        // Keep track of visited discharge macaroons
        treePath.Push(discharge);

        // Use the root key to verify discharge macaroon recursively
        VerificationResult result = discharge.VerifyInner(TM, v, key, ms, treePath);

        treePath.Pop();

        return result;
      }
      catch (CryptographicException ex)
      {
        return new VerificationResult(ex.Message);
      }
    }

    #endregion


    #region IO

    public override string ToString()
    {
      return Location.ToString();
    }


    /// <summary>
    /// Return string representation of all content in this macaroon.
    /// </summary>
    /// <returns></returns>
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


    /// <summary>
    /// Serialize this macaroon to a string. The string is safe for use in URLs as it is based on the BASE64 URL Safe encoding.
    /// </summary>
    /// <returns>Result of serialization.</returns>
    public string Serialize()
    {
      return Utility.ToBase64UrlSafe(SerializeToBytes());
    }


    /// <summary>
    /// Serialize this macaroon to a byte array.
    /// </summary>
    /// <returns>Result of serialization.</returns>
    public byte[] SerializeToBytes()
    {
      using (MemoryStream s = new MemoryStream())
      {
        Serialize(s);
        return s.ToArray();
      }
    }


    /// <summary>
    /// Serialize this macaroon to a stream.
    /// </summary>
    /// <param name="s">Output stream.</param>
    public void Serialize(Stream s)
    {
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
      }
    }


    /// <summary>
    /// Deserialize a string into a macaroon.
    /// </summary>
    /// <param name="s">Input string.</param>
    /// <param name="options">Serialization options.</param>
    /// <returns>Deserialized macaroon.</returns>
    public static Macaroon Deserialize(string s, SerializationOptions options = null)
    {
      Condition.Requires(s, "s").IsNotNullOrEmpty();
      byte[] data = Utility.FromBase64UrlSafe(s);
      return Deserialize(data);
    }


    /// <summary>
    /// Deserialize a byte array into a macaroon.
    /// </summary>
    /// <param name="data">Input bytes.</param>
    /// <param name="options">Serialization options.</param>
    /// <returns>Deserialized macaroon.</returns>
    public static Macaroon Deserialize(byte[] data, SerializationOptions options = null)
    {
      using (MemoryStream m = new MemoryStream(data))
        return Deserialize(m, options);
    }


    /// <summary>
    /// Deserialize macaroon from a stream.
    /// </summary>
    /// <param name="s">Input stream.</param>
    /// <param name="options">Serialization options.</param>
    /// <returns>Deserialized macaroon.</returns>
    public static Macaroon Deserialize(Stream s, SerializationOptions options = null)
    {
      if (options == null)
        options = SerializationOptions.Default;

      using (PacketReader r = new PacketReader(s, options))
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


    #region Internal utility methods

    private static byte[] MacaroonsKeyGeneratorSecret = Encoding.ASCII.GetBytes("macaroons-key-generator");


    protected Packet GenerateDerivedKey(Packet key)
    {
      Condition.Requires(key, "key").IsNotNull();

      // Generate derived key as a hash of a hard coded "secret" value and the original key.
      byte[] genkey = new byte[MACAROON_HASH_BYTES];
      byte[] mkeygen = MacaroonsKeyGeneratorSecret;
      Buffer.BlockCopy(mkeygen, 0, genkey, 0, mkeygen.Length);

      return CalculateHash1(genkey, key.Data);
    }


    protected Packet CalculateHash1(Packet key, Packet p)
    {
      return CalculateHash1(key.Data, p.Data);
    }


    protected Packet CalculateHash1(byte[] key, byte[] data)
    {
      using (HMACSHA256 alg = new HMACSHA256(key))
      {
        byte[] hash = alg.ComputeHash(data);
        return new Packet(hash, DataEncoding.Hex);
      }
    }


    protected Packet CalculateHash2(Packet key, Packet data1, Packet data2)
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
