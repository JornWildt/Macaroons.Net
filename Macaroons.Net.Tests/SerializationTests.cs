using NUnit.Framework;
using System.Text;
using System.IO;

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
      Assert.That(s, Is.EqualTo("MDAxY2xvY2F0aW9uIGh0dHA6Ly9teWJhbmsvCjAwMjZpZGVudGlmaWVyIHdlIHVzZWQgb3VyIHNlY3JldCBrZXkKMDAyZnNpZ25hdHVyZSDj2eApCFJsTAA5rhURQRXZf91ovyujebNCqvD2F9BVLwo"));
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
      Assert.That(s, Is.EqualTo("MDAxY2xvY2F0aW9uIGh0dHA6Ly9teWJhbmsvCjAwMjZpZGVudGlmaWVyIHdlIHVzZWQgb3VyIHNlY3JldCBrZXkKMDAxZGNpZCBhY2NvdW50ID0gMzczNTkyODU1OQowMDIwY2lkIHRpbWUgPCAyMDE1LTAxLTAxVDAwOjAwCjAwMjJjaWQgZW1haWwgPSBhbGljZUBleGFtcGxlLm9yZwowMDJmc2lnbmF0dXJlIIgubVlJbtUkXtt6tbiDns1j5dUE5Ug5gE8WQHDY7tlSCg"));
    }


    [Test]
    public void CanDeserializeEmptyMacaroon()
    {
      // Arrange (this is a Macaroon from the tutorial (https://github.com/rescrv/libmacaroons) containing an invalid signature - but that should not be checked here)
      string serialized = "MDAxY2xvY2F0aW9uIGh0dHA6Ly9teWJhbmsvCjAwMjZpZGVudGlmaWVyIHdlIHVzZWQgb3VyIHNlY3JldCBrZXkKMDAyZnNpZ25hdHVyZSDj2eApCFJsTAA5rhURQRXZf91ovyujebNCqvD2F9BVLwo";
      
      // Act
      Macaroon m = Macaroon.Deserialize(serialized);

      // Assert
      Assert.That(m.Location.ToString(), Is.EqualTo(Location));
      Assert.That(m.Identifier.ToString(), Is.EqualTo(Identifier));
      Assert.That(m.Caveats.Count, Is.EqualTo(0));
      Assert.That(m.Verify(new Verifier(), Secret).Success, Is.True);
    }


    [Test]
    public void CanDeserializeMultipleFirstPartyCaveats()
    {
      // Arrange
      string serialized = "MDAxY2xvY2F0aW9uIGh0dHA6Ly9teWJhbmsvCjAwMjZpZGVudGlmaWVyIHdlIHVzZWQgb3VyIHNlY3JldCBrZXkKMDAxZGNpZCBhY2NvdW50ID0gMzczNTkyODU1OQowMDIwY2lkIHRpbWUgPCAyMDE1LTAxLTAxVDAwOjAwCjAwMjJjaWQgZW1haWwgPSBhbGljZUBleGFtcGxlLm9yZwowMDJmc2lnbmF0dXJlID8f19FL+bkC9p/aoMmIecC7GxdOcLVyUnrv6lJMM7NSCg==";

      // Act
      Macaroon m = Macaroon.Deserialize(serialized);

      // Assert
      Assert.That(m.Location.ToString(), Is.EqualTo(Location));
      Assert.That(m.Identifier.ToString(), Is.EqualTo(Identifier));
      Assert.That(m.Caveats.Count, Is.EqualTo(3));
      Assert.That(m.Caveats[0].CId.ToString(), Is.EqualTo("account = 3735928559"));
      Assert.That(m.Caveats[1].CId.ToString(), Is.EqualTo("time < 2015-01-01T00:00"));
      Assert.That(m.Caveats[2].CId.ToString(), Is.EqualTo("email = alice@example.org"));
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
      Macaroon m2 = Macaroon.Deserialize(s);

      // Assert
      Assert.That(m2.Location, Is.EqualTo(m1.Location));
      Assert.That(m2.Identifier, Is.EqualTo(m1.Identifier));
      Assert.That(m2.Signature, Is.EqualTo(m1.Signature));
      Assert.That(m2.Caveats.Count, Is.EqualTo(m1.Caveats.Count));
      Assert.That(m2.Caveats[0].Cl, Is.EqualTo(m1.Caveats[0].Cl));
      Assert.That(m2.Caveats[0].CId, Is.EqualTo(m1.Caveats[0].CId));
      Assert.That(m2.Caveats[0].VId, Is.EqualTo(m1.Caveats[0].VId));
      Assert.That(m2.Caveats[1].Cl, Is.EqualTo(m1.Caveats[1].Cl));
      Assert.That(m2.Caveats[1].CId, Is.EqualTo(m1.Caveats[1].CId));
      Assert.That(m2.Caveats[1].VId, Is.EqualTo(m1.Caveats[1].VId));
    }


    [Test]
    public void WhenDeserializingBadPacketItThrowsInvalidDataException()
    {
      // Arrange
      // - This data would make the deserializer run around in circles in earlier versions.
      string s = "MDAyNWxvY2F0aW9uIGNTZWFyY2g6ZG9jdW1lbnQ6MTQ5MzY0CjAwMjJpZGVudGlmaWVyIGRvY3VtZW50SWQ6IDE0OTM2NAowMDFiY2lkIGRvY3VtZW50SWQ6IDE0OTM2NAowMDIzY2lkIHRpbWUgPCAyMDE2LTAxLTA0VDEyOjQzOjU2CjAwMmZzaWduyXR1cmUgQbpcMXKEUSc4AE1xANE2V4b1BbKAGSbrEO2oAOqZYhkK";

      // Act
      try
      {
        Macaroon m = Macaroon.Deserialize(s);

        Assert.Fail("Did not throw exception as expected.");
      }
      catch (InvalidDataException)
      {
        // Success
      }
    }
  }
}
