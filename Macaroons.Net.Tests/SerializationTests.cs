using NUnit.Framework;
using System.Text;


namespace Macaroons.Tests
{
  [TestFixture]
  public class SerializationTests : TestBase
  {
    [Test]
    public void CanSerializeEmptyMacaroon()
    {
      // Arrange
      Macaroon m = new Macaroon(Location, Secret, Identifier);

      // Act
      string s = m.Serialize();

      // Assert
      Assert.AreEqual("MDAxY2xvY2F0aW9uIGh0dHA6Ly9teWJhbmsvCjAwMjZpZGVudGlmaWVyIHdlIHVzZWQgb3VyIHNlY3JldCBrZXkKMDAyZnNpZ25hdHVyZSDj2eApCFJsTAA5rhURQRXZf91ovyujebNCqvD2F9BVLwo", s);
    }


    [Test]
    public void CanSerializeMultipleFirstPartyCaveats()
    {
      // Arrange
      Macaroon m = new Macaroon(Location, Secret, Identifier);
      m.AddFirstPartyCaveat("account = 3735928559");
      m.AddFirstPartyCaveat("time < 2015-01-01T00:00");
      m.AddFirstPartyCaveat("email = alice@example.org");

      // Act
      string s = m.Serialize();

      // Assert (the expected value here is just calculated - I havent seen any correct value on the web)
      Assert.AreEqual("MDAxY2xvY2F0aW9uIGh0dHA6Ly9teWJhbmsvCjAwMjZpZGVudGlmaWVyIHdlIHVzZWQgb3VyIHNlY3JldCBrZXkKMDAxZGNpZCBhY2NvdW50ID0gMzczNTkyODU1OQowMDIwY2lkIHRpbWUgPCAyMDE1LTAxLTAxVDAwOjAwCjAwMjJjaWQgZW1haWwgPSBhbGljZUBleGFtcGxlLm9yZwowMDJmc2lnbmF0dXJlIIgubVlJbtUkXtt6tbiDns1j5dUE5Ug5gE8WQHDY7tlSCg", s);
    }


    [Test]
    public void CanDeserializeEmptyMacaroon()
    {
      // Arrange (this is a Macaroon from the tutorial (https://github.com/rescrv/libmacaroons) containing an invalid signature - but that should not be checked here)
      string serialized = "MDAxY2xvY2F0aW9uIGh0dHA6Ly9teWJhbmsvCjAwMjZpZGVudGlmaWVyIHdlIHVzZWQgb3VyIHNlY3JldCBrZXkKMDAyZnNpZ25hdHVyZSDj2eApCFJsTAA5rhURQRXZf91ovyujebNCqvD2F9BVLwo";
      
      // Act
      Macaroon m = Macaroon.Deserialize(serialized, DebugSerializationOptions);

      // Assert
      Assert.AreEqual(Location, m.Location.ToString());
      Assert.AreEqual(Identifier, m.Identifier.ToString());
      Assert.AreEqual(0, m.Caveats.Count);
      Assert.IsTrue(m.Verify(new Verifier(), Secret).Success);
    }


    [Test]
    public void CanDeserializeMultipleFirstPartyCaveats()
    {
      // Arrange
      string serialized = "MDAxY2xvY2F0aW9uIGh0dHA6Ly9teWJhbmsvCjAwMjZpZGVudGlmaWVyIHdlIHVzZWQgb3VyIHNlY3JldCBrZXkKMDAxZGNpZCBhY2NvdW50ID0gMzczNTkyODU1OQowMDIwY2lkIHRpbWUgPCAyMDE1LTAxLTAxVDAwOjAwCjAwMjJjaWQgZW1haWwgPSBhbGljZUBleGFtcGxlLm9yZwowMDJmc2lnbmF0dXJlID8f19FL+bkC9p/aoMmIecC7GxdOcLVyUnrv6lJMM7NSCg==";

      // Act
      Macaroon m = Macaroon.Deserialize(serialized, DebugSerializationOptions);

      // Assert
      Assert.AreEqual(Location, m.Location.ToString());
      Assert.AreEqual(Identifier, m.Identifier.ToString());
      Assert.AreEqual(3, m.Caveats.Count);
      Assert.AreEqual("account = 3735928559", m.Caveats[0].CId.ToString());
      Assert.AreEqual("time < 2015-01-01T00:00", m.Caveats[1].CId.ToString());
      Assert.AreEqual("email = alice@example.org", m.Caveats[2].CId.ToString());
    }


    [Test]
    public void CanSerializeAndDeserializeThirdPartyCaveats()
    {
      // Arrange
      Macaroon m1 = new Macaroon(Location, Secret, Identifier);
      m1.AddFirstPartyCaveat("account = 3735928559");

      string caveat_key = "4; guaranteed random by a fair toss of the dice";
      string identifier = "this was how we remind auth of key/pred";
      m1.AddThirdPartyCaveat("http://auth.mybank/", caveat_key, identifier);

      // Act
      string s = m1.Serialize();
      Macaroon m2 = Macaroon.Deserialize(s, DebugSerializationOptions);

      // Assert
      Assert.AreEqual(m1.Location, m2.Location);
      Assert.AreEqual(m1.Identifier, m2.Identifier);
      Assert.AreEqual(m1.Signature, m2.Signature);
      Assert.AreEqual(m1.Caveats.Count, m2.Caveats.Count);
      Assert.AreEqual(m1.Caveats[0].Cl, m2.Caveats[0].Cl);
      Assert.AreEqual(m1.Caveats[0].CId, m2.Caveats[0].CId);
      Assert.AreEqual(m1.Caveats[0].VId, m2.Caveats[0].VId);
      Assert.AreEqual(m1.Caveats[1].Cl, m2.Caveats[1].Cl);
      Assert.AreEqual(m1.Caveats[1].CId, m2.Caveats[1].CId);
      Assert.AreEqual(m1.Caveats[1].VId, m2.Caveats[1].VId);
    }
  }
}
