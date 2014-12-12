using System;
using System.Text;


namespace Macaroons.Tests
{
  public class TestBase
  {
    // The three values below are from the C-implementation at GitHub https://github.com/rescrv/libmacaroons

    // The secret key used to create most test macaroons
    public const string Secret = "this is our super secret key; only we should know it";

    // The public identifier used to create most test macaroons
    public const string Identifier = "we used our secret key";

    // The location used to create most test macaroons
    public const string Location = "http://mybank/";
    
    
    // A simplistic timestamp verifier. Verifies that current time is before the stated time.
    protected bool TimeVerifier(Packet cid)
    {
      string caveat = cid.ToString();
      if (!caveat.StartsWith("time < "))
        return false;
      DateTime t = DateTime.Parse(caveat.Substring(7));
      return DateTime.Now < t;
    }
  }
}
