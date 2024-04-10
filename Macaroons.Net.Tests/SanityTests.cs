using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;


namespace Macaroons.Tests
{
  // These tests go through the examples from the tutorial on
  // the original libmacaroons GitHub page at https://github.com/rescrv/libmacaroons

  [TestFixture]
  public class SanityTests : TestBase
  {
    const string Secret2 = "this is a different super-secret key; never use the same secret twice";
    const string Identifier2 = "we used our other secret key";
    const string Location2 = "http://mybank/";


    [OneTimeSetUpAttribute]
    public void SetupFixture()
    {
      Macaroon.Crypto = new SecretBoxCryptoAlgorithm(false);
    }


    [Test]
    public void CanCreateEmptyMacaroonWithSignature()
    {
      // Act
      Macaroon m = new Macaroon(Location, Secret, Identifier);

      // Assert
      Assert.That(m.Identifier.ToString(), Is.EqualTo(Identifier));
      Assert.That(m.Location.ToString(), Is.EqualTo(Location));
      Assert.That(m.Signature.ToString(), Is.EqualTo("E3D9E02908526C4C0039AE15114115D97FDD68BF2BA379B342AAF0F617D0552F"));
      Assert.That(m.Caveats.Count, Is.EqualTo(0));
    }


    [Test]
    public void CanAddOneFirstPartyCaveat()
    {
      // Arrange
      Macaroon m = new Macaroon(Location, Secret, Identifier);

      // Act
      m.AddFirstPartyCaveat("account = 3735928559");

      // Assert
      Assert.That(m.Caveats.Count, Is.EqualTo(1));
      Assert.That(m.Caveats[0].Inspect(), Is.EqualTo("CId = account = 3735928559"));
      Assert.That(m.Signature.ToString(), Is.EqualTo("1EFE4763F290DBCE0C1D08477367E11F4EEE456A64933CF662D79772DBB82128"));
    }


    [Test]
    public void CanAddMultipleFirstPartyCaveats()
    {
      // Arrange
      Macaroon m = new Macaroon(Location, Secret, Identifier);

      // Act
      m.AddFirstPartyCaveat("account = 3735928559");
      m.AddFirstPartyCaveat("time < 2015-01-01T00:00");
      m.AddFirstPartyCaveat("email = alice@example.org");

      // Assert
      Assert.That(m.Caveats.Count, Is.EqualTo(3));
      Assert.That(m.Caveats[0].Inspect(), Is.EqualTo("CId = account = 3735928559"));
      Assert.That(m.Caveats[1].Inspect(), Is.EqualTo("CId = time < 2015-01-01T00:00"));
      Assert.That(m.Caveats[2].Inspect(), Is.EqualTo("CId = email = alice@example.org"));
      Assert.That(m.Signature.ToString(), Is.EqualTo("882E6D59496ED5245EDB7AB5B8839ECD63E5D504E54839804F164070D8EED952"));

      string expectedStringRepresentation = string.Join(Environment.NewLine, new[] {
        "Location = http://mybank/",
        "Identifier = we used our secret key",
        "CId = account = 3735928559",
        "CId = time < 2015-01-01T00:00",
        "CId = email = alice@example.org",
        "Signature = 882E6D59496ED5245EDB7AB5B8839ECD63E5D504E54839804F164070D8EED952",
        ""
      });

      Assert.That(m.Inspect(), Is.EqualTo(expectedStringRepresentation));
    }


    [Test]
    public void CanAddThirdPartyCaveat()
    {
      // Arrange
      Macaroon m = new Macaroon(Location2, Secret2, Identifier2);
      m.AddFirstPartyCaveat("account = 3735928559");

      // - just checking (this should although be covered in other tests) ...
      Assert.That(m.Signature.ToString(), Is.EqualTo("1434E674AD84FDFDC9BC1AA00785325C8B6D57341FC7CE200BA4680C80786DDA"));

      // Act
      string caveat_key = "4; guaranteed random by a fair toss of the dice";
      // string predicate = "user = Alice";
      // # send_to_auth(caveat_key, predicate)
      // # identifier = recv_from_auth()
      string identifier = "this was how we remind auth of key/pred";

      m.AddThirdPartyCaveat("http://auth.mybank/", caveat_key, identifier);

      // Assert
      Assert.That(m.Signature.ToString(), Is.EqualTo("D27DB2FD1F22760E4C3DAE8137E2D8FC1DF6C0741C18AED4B97256BF78D1F55C"));

      string expectedStringRepresentation = string.Join(Environment.NewLine, new[] {
        "Location = http://mybank/",
        "Identifier = we used our other secret key",
        "CId = account = 3735928559",
        "CId = this was how we remind auth of key/pred",
        "  VId = AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA027FAuBYhtHwJ58FX6UlVNFtFsGxQHS7uD_w_dedwv4Jjw7UorCREw5rXbRqIKhr",
        "  Cl = http://auth.mybank/",
        "Signature = D27DB2FD1F22760E4C3DAE8137E2D8FC1DF6C0741C18AED4B97256BF78D1F55C",
        ""
      });

      Assert.That(m.Inspect(), Is.EqualTo(expectedStringRepresentation));

      List<Caveat> thirdPartyCaveats = m.ThirdPartyCaveats.ToList();
      Assert.That(thirdPartyCaveats.Count, Is.EqualTo(1));
      Assert.That(thirdPartyCaveats[0].Cl.ToString(), Is.EqualTo("http://auth.mybank/"));
      Assert.That(thirdPartyCaveats[0].CId.ToString(), Is.EqualTo("this was how we remind auth of key/pred"));
    }


    [Test]
    public void CanPrepareForRequest()
    {
      // Arrange
      Macaroon m = new Macaroon(Location2, Secret2, Identifier2);
      m.AddFirstPartyCaveat("account = 3735928559");

      string caveat_key = "4; guaranteed random by a fair toss of the dice";
      string identifier = "this was how we remind auth of key/pred";
      m.AddThirdPartyCaveat("http://auth.mybank/", caveat_key, identifier);

      Macaroon d = new Macaroon("http://auth.mybank/", caveat_key, identifier);
      d.AddFirstPartyCaveat("time < 2015-01-01T00:00");

      // Act
      Macaroon dp = m.PrepareForRequest(d);

      // Assert
      Assert.That(d.Signature.ToString(), Is.EqualTo("82A80681F9F32D419AF12F6A71787A1BAC3AB199DF934ED950DDF20C25AC8C65"));
      Assert.That(dp.Signature.ToString(), Is.EqualTo("2EB01D0DD2B4475330739140188648CF25DDA0425EA9F661F1574CA0A9EAC54E"));
    }


    [Test]
    public void CanVerifyWithDischargeMacaroon()
    {
      // Arrange
      Macaroon m = new Macaroon(Location2, Secret2, Identifier2);
      m.AddFirstPartyCaveat("account = 3735928559");

      string caveat_key = "4; guaranteed random by a fair toss of the dice";
      string identifier = "this was how we remind auth of key/pred";
      m.AddThirdPartyCaveat("http://auth.mybank/", caveat_key, identifier);

      Macaroon d = new Macaroon("http://auth.mybank/", caveat_key, identifier);
      d.AddFirstPartyCaveat("time < 2115-01-01T00:00");

      Macaroon dp = m.PrepareForRequest(d);

      Verifier v = new Verifier();
      v.SatisfyExact("account = 3735928559");
      v.SatisfyGeneral(TimeVerifier);

      // Act
      VerificationResult result = m.Verify(v, Secret2, new List<Macaroon> { dp });

      // Assert
      Assert.That(result.Success, Is.True);
    }
  }
}
