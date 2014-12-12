using NUnit.Framework;
using System.Collections.Generic;
using System.Linq;


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


    [TestFixtureSetUp]
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
      Assert.AreEqual(Identifier, m.Identifier.ToString());
      Assert.AreEqual(Location, m.Location.ToString());
      Assert.AreEqual("E3D9E02908526C4C0039AE15114115D97FDD68BF2BA379B342AAF0F617D0552F", m.Signature.ToString());
      Assert.AreEqual(0, m.Caveats.Count);
    }


    [Test]
    public void CanAddOneFirstPartyCaveat()
    {
      // Arrange
      Macaroon m = new Macaroon(Location, Secret, Identifier);

      // Act
      m.AddFirstPartyCaveat("account = 3735928559");

      // Assert
      Assert.AreEqual(1, m.Caveats.Count);
      Assert.AreEqual("CId = account = 3735928559", m.Caveats[0].Inspect());
      Assert.AreEqual("1EFE4763F290DBCE0C1D08477367E11F4EEE456A64933CF662D79772DBB82128", m.Signature.ToString());
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
      Assert.AreEqual(3, m.Caveats.Count);
      Assert.AreEqual("CId = account = 3735928559", m.Caveats[0].Inspect());
      Assert.AreEqual("CId = time < 2015-01-01T00:00", m.Caveats[1].Inspect());
      Assert.AreEqual("CId = email = alice@example.org", m.Caveats[2].Inspect());
      Assert.AreEqual("882E6D59496ED5245EDB7AB5B8839ECD63E5D504E54839804F164070D8EED952", m.Signature.ToString());

      string expectedStringRepresentation = @"Location = http://mybank/
Identifier = we used our secret key
CId = account = 3735928559
CId = time < 2015-01-01T00:00
CId = email = alice@example.org
Signature = 882E6D59496ED5245EDB7AB5B8839ECD63E5D504E54839804F164070D8EED952
";

      Assert.AreEqual(expectedStringRepresentation, m.Inspect());
    }


    [Test]
    public void CanAddThirdPartyCaveat()
    {
      // Arrange
      Macaroon m = new Macaroon(Location2, Secret2, Identifier2);
      m.AddFirstPartyCaveat("account = 3735928559");

      // - just checking (this should although be covered in other tests) ...
      Assert.AreEqual("1434E674AD84FDFDC9BC1AA00785325C8B6D57341FC7CE200BA4680C80786DDA", m.Signature.ToString());

      // Act
      string caveat_key = "4; guaranteed random by a fair toss of the dice";
      // string predicate = "user = Alice";
      // # send_to_auth(caveat_key, predicate)
      // # identifier = recv_from_auth()
      string identifier = "this was how we remind auth of key/pred";

      m.AddThirdPartyCaveat("http://auth.mybank/", caveat_key, identifier);

      // Assert
      Assert.AreEqual("D27DB2FD1F22760E4C3DAE8137E2D8FC1DF6C0741C18AED4B97256BF78D1F55C", m.Signature.ToString());
     
      string expectedStringRepresentation = @"Location = http://mybank/
Identifier = we used our other secret key
CId = account = 3735928559
CId = this was how we remind auth of key/pred
  VId = AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA027FAuBYhtHwJ58FX6UlVNFtFsGxQHS7uD_w_dedwv4Jjw7UorCREw5rXbRqIKhr
  Cl = http://auth.mybank/
Signature = D27DB2FD1F22760E4C3DAE8137E2D8FC1DF6C0741C18AED4B97256BF78D1F55C
";

      Assert.AreEqual(expectedStringRepresentation, m.Inspect());

      List<Caveat> thirdPartyCaveats = m.ThirdPartyCaveats.ToList();
      Assert.AreEqual(1, thirdPartyCaveats.Count);
      Assert.AreEqual("http://auth.mybank/", thirdPartyCaveats[0].Cl.ToString());
      Assert.AreEqual("this was how we remind auth of key/pred", thirdPartyCaveats[0].CId.ToString());
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
      Assert.AreEqual("82A80681F9F32D419AF12F6A71787A1BAC3AB199DF934ED950DDF20C25AC8C65", d.Signature.ToString());
      Assert.AreEqual("2EB01D0DD2B4475330739140188648CF25DDA0425EA9F661F1574CA0A9EAC54E", dp.Signature.ToString());
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
      d.AddFirstPartyCaveat("time < 2015-01-01T00:00");

      Macaroon dp = m.PrepareForRequest(d);

      Verifier v = new Verifier();
      v.SatisfyExact("account = 3735928559");
      v.SatisfyGeneral(TimeVerifier);

      // Act
      VerificationResult result = m.Verify(v, Secret2, new List<Macaroon> { dp });

      // Assert
      Assert.IsTrue(result.Success);
    }
  }
}
