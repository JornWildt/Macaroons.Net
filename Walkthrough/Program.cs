using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Macaroons;
using System.Security.Cryptography;


namespace Walkthrough
{
  public class Program
  {
    static void Main(string[] args)
    {
      string secret = "this is our super secret key; only we should know it";
      string pubid = "we used our secret key";
      string location = "http://mybank/";
      Macaroon m = new Macaroon(location, secret, pubid);

      Console.WriteLine(m.Identifier);
      
      Console.WriteLine(m.Location);
      Console.WriteLine(m.Signature);

      Console.WriteLine(m.Serialize());

      Console.WriteLine(m.Inspect());

      m.AddFirstPartyCaveat("account = 3735928559");
      
      Console.WriteLine(m.Inspect());

      m.AddFirstPartyCaveat("time < 2015-01-01T00:00");
      Console.WriteLine(m.Signature);
      m.AddFirstPartyCaveat("email = alice@example.org");
      Console.WriteLine(m.Signature);
      Console.WriteLine(m.Inspect());

      string msg = m.Serialize();

      // Send to bank
      // Receive again

      m = Macaroon.Deserialize(msg);
      Console.WriteLine(m.Inspect());

      Verifier v = new Verifier();
      var result = v.Verify(m, secret);
      Console.WriteLine("Success: {0}", result.Success);

      v.SatisfyExact("account = 3735928559");
      v.SatisfyExact("email = alice@example.org");

      v.SatisfyExact("IP = 127.0.0.1");
      v.SatisfyExact("browser = Chrome");
      v.SatisfyExact("action = deposit");

      Console.WriteLine(CheckTime(new Packet("time < 2015-01-01T00:00")));
      Console.WriteLine(CheckTime(new Packet("time < 2014-01-01T00:00")));
      Console.WriteLine(CheckTime(new Packet("account = 3735928559")));

      v.SatisfyGeneral(CheckTime);
      
      result = v.Verify(m, secret);
      Console.WriteLine("Success: {0}", result.Success);

      Macaroon n = new Macaroon(m).AddFirstPartyCaveat("action = deposit");
      result = v.Verify(n, secret);
      Console.WriteLine("Success: {0}", result.Success);

      n = new Macaroon(m).AddFirstPartyCaveat("OS = Windows XP");
      result = v.Verify(n, secret);
      Console.WriteLine("Success: {0}", result.Success);

      n = new Macaroon(m).AddFirstPartyCaveat("time < 2014-01-01T00:00");
      result = v.Verify(n, secret);
      Console.WriteLine("Success: {0}", result.Success);

      result = v.Verify(m, "this is not the secret we were looking for");
      Console.WriteLine("Success: {0}", result.Success);

      n = Macaroon.Deserialize("MDAxY2xvY2F0aW9uIGh0dHA6Ly9teWJhbmsvCjAwMjZpZGVudGlmaWVyIHdlIHVzZWQgb3VyIHNlY3JldCBrZXkKMDAxZGNpZCBhY2NvdW50ID0gMzczNTkyODU1OQowMDIwY2lkIHRpbWUgPCAyMDE1LTAxLTAxVDAwOjAwCjAwMjJjaWQgZW1haWwgPSBhbGljZUBleGFtcGxlLm9yZwowMDJmc2lnbmF0dXJlID8f19FL+bkC9p/aoMmIecC7GxdOcLVyUnrv6lJMM7NSCg==");
      Console.WriteLine(n.Inspect());
      Console.WriteLine("n.Signature == m.Signature: {0}", m.Signature == n.Signature);
      result = v.Verify(n, secret);
      Console.WriteLine("Success: {0}", result.Success);

      string location2 = "http://mybank/";
      string secret2 = "this is a different super-secret key; never use the same secret twice";
      string pubid2 = "we used our other secret key";
      m = new Macaroon(location2, secret2, pubid2);
      m.AddFirstPartyCaveat("account = 3735928559");
      Console.WriteLine(m.Inspect());

      string caveat_key = "4; guaranteed random by a fair toss of the dice";
      // string predicate = "user = Alice";
      // send_to_auth(caveat_key, predicate)
      // identifier = recv_from_auth()
      string identifier = "this was how we remind auth of key/pred";
      m.AddThirdPartyCaveat("http://auth.mybank/", caveat_key, identifier);
      Console.WriteLine(m.Inspect());

      var caveats = m.ThirdPartyCaveats;

      Macaroon d = new Macaroon("http://auth.mybank/", caveat_key, identifier);
      d.AddFirstPartyCaveat("time < 2015-01-01T00:00");
      Console.WriteLine(d.Inspect());

      Macaroon dp = m.PrepareForRequest(d);
      Console.WriteLine(d.Signature);
      Console.WriteLine(dp.Signature);

      result = v.Verify(m, secret2, new List<Macaroon> { dp });
      Console.WriteLine("Success: {0}", result.Success);

      result = v.Verify(m, secret2, new List<Macaroon> { d });
      Console.WriteLine("Success: {0}", result.Success);

      Console.WriteLine(Macaroon.MACAROON_SUGGESTED_SECRET_LENGTH);

      byte[] randomSecret = new byte[Macaroon.MACAROON_SUGGESTED_SECRET_LENGTH];
      using (RNGCryptoServiceProvider rng = new RNGCryptoServiceProvider())
        rng.GetBytes(randomSecret);

      Packet key = new Packet(randomSecret, DataEncoding.Hex);
      Console.WriteLine(key);

      m = new Macaroon(new Packet(location), key, new Packet(pubid));
      Console.WriteLine(m.Inspect());
    }


    static bool CheckTime(Packet cid)
    {
      string caveat = cid.ToString();
      if (!caveat.StartsWith("time < "))
        return false;
      DateTime t = DateTime.Parse(caveat.Substring(7));
      return DateTime.Now < t;
    }
  }
}
