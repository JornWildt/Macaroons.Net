using Macaroons;
using System;
using System.Collections.Generic;
using System.Diagnostics;


namespace PerformanceTests
{
  // These are just some simple experiments to get an idea of where time is spend in the code. Nothing usefull, really ...

  class NoEncryption : CryptoAlgorithm
  {
    public override byte[] Encrypt(byte[] key, byte[] plainText)
    {
      return plainText;
    }

    public override byte[] Decrypt(byte[] key, byte[] cipherText)
    {
      return cipherText;
    }
  }


  class Program
  {
    const string Secret2 = "this is a different super-secret key; never use the same secret twice";
    const string Identifier2 = "we used our other secret key";
    const string Location2 = "http://mybank/";

    static readonly Packet SecretBytes = new Packet(Secret2);
    static readonly Packet IdentifierBytes = new Packet(Identifier2);
    static readonly Packet LocationBytes = new Packet(Location2);

    static void Main(string[] args)
    {
      TestBase64UrlSafe();
      //TestSerialization();
      //TestCreateAndDecrypt();
    }

    static void TestSerialization()
    {
      Macaroon m = new Macaroon(LocationBytes, SecretBytes, IdentifierBytes);
      m.AddFirstPartyCaveat("account = 3735928559");

      string caveat_key = "4; guaranteed random by a fair toss of the dice";
      string identifier = "this was how we remind auth of key/pred";
      m.AddThirdPartyCaveat("http://auth.mybank/", caveat_key, identifier);

      Stopwatch w1 = new Stopwatch();
      w1.Start();

      for (int i = 0; i < 10000; ++i)
      {
        string s = m.Serialize();
        Macaroon n = Macaroon.Deserialize(s);
      }

      w1.Stop();

      Console.WriteLine("Total: " + w1.Elapsed);
    }

    static void TestBase64UrlSafe()
    {
      byte[] data = new byte[255];
      for (int i = 0; i < 255; ++i)
        data[i] = (byte)i;

      Stopwatch w1 = new Stopwatch();
      w1.Start();

      for (int i = 0; i < 200000; ++i)
      {
        string s = Utility.ToBase64UrlSafe(data);
        byte[] result = Utility.FromBase64UrlSafe(s);
      }

      w1.Stop();

      Console.WriteLine("Total: " + w1.Elapsed);
    }

    static void TestCreateAndDecrypt()
    {
      //Macaroon.Crypto = new NoEncryption();

      Stopwatch w1 = new Stopwatch();
      Stopwatch w2 = new Stopwatch();
      Stopwatch w3 = new Stopwatch();
      Stopwatch w4 = new Stopwatch();

      w1.Start();

      for (int i = 0; i < 2000; ++i)
      {
        w2.Start();
        //Macaroon m = new Macaroon(Location2, Secret2, Identifier2);
        Macaroon m = new Macaroon(LocationBytes, SecretBytes, IdentifierBytes);
        m.AddFirstPartyCaveat("account = 3735928559");

        string caveat_key = "4; guaranteed random by a fair toss of the dice";
        string identifier = "this was how we remind auth of key/pred";
        m.AddThirdPartyCaveat("http://auth.mybank/", caveat_key, identifier);
        //m.AddFirstPartyCaveat("account = 3735928559");
        w2.Stop();

        Macaroon d = new Macaroon("http://auth.mybank/", caveat_key, identifier);
        //d.AddFirstPartyCaveat("time < 2015-01-01T00:00");

        w3.Start();
        Macaroon dp = m.PrepareForRequest(d);
        w3.Stop();

        w4.Start();
        Verifier v = new Verifier();
        v.SatisfyExact("account = 3735928559");
        //v.SatisfyGeneral(TimeVerifier);

        VerificationResult result = m.Verify(v, Secret2, new List<Macaroon> { dp });
        w4.Stop();

        if (!result.Success)
          throw new InvalidOperationException();
      }
      //Console.WriteLine(result.Success);

      w1.Stop();
      Console.WriteLine("Total: " + w1.Elapsed);
      Console.WriteLine("Create: " + w2.Elapsed);
      Console.WriteLine("Prepare: " + w3.Elapsed);
      Console.WriteLine("Verify: " + w4.Elapsed);
    }
  }
}
