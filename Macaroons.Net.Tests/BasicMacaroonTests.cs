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
      Assert.That(m2.Location, Is.EqualTo(m1.Location));
      Assert.That(m2.Identifier, Is.EqualTo(m1.Identifier));
      Assert.That(m2.Signature, Is.EqualTo(m1.Signature));

      // - Change m2 and check that m1 stays the same
      m2.AddFirstPartyCaveat("a = 10");
      Assert.That(m2.Caveats.Count, Is.EqualTo(2));
      Assert.That(m2.Caveats[0].CId.ToString(), Is.EqualTo("account = 3735928559"));
      Assert.That(m1.Caveats.Count, Is.EqualTo(1));
    }


    [Test]
    public void CanPrintMacaroon()
    {
      // Arrange
      Macaroon m = new Macaroon(Location, Secret, Identifier);

      // Act
      string s = m.ToString();

      // Assert
      Assert.That(s, Is.EqualTo(Location));
    }
  }
}
