using NUnit.Framework;


namespace Macaroons.Tests
{
  [TestFixture]
  public class BasicMacaroonTests : TestBase
  {
    [Test]
    public void CanCopyMacaroon()
    {
      // Arrange
      Macaroon m1 = new Macaroon(Location, Secret, Identifier);
      m1.AddFirstPartyCaveat("account = 3735928559");

      // Act
      Macaroon m2 = new Macaroon(m1);

      // Assert
      Assert.AreEqual(m1.Location, m2.Location);
      Assert.AreEqual(m1.Identifier, m2.Identifier);
      Assert.AreEqual(m1.Signature, m2.Signature);

      // - Change m2 and check that m1 stays the same
      m2.AddFirstPartyCaveat("a = 10");
      Assert.AreEqual(2, m2.Caveats.Count);
      Assert.AreEqual("account = 3735928559", m2.Caveats[0].CId.ToString());
      Assert.AreEqual(1, m1.Caveats.Count);
    }


    [Test]
    public void CanPrintMacaroon()
    {
      // Arrange
      Macaroon m = new Macaroon(Location, Secret, Identifier);

      // Act
      string s = m.ToString();

      // Assert
      Assert.AreEqual(Location, s);
    }
  }
}
