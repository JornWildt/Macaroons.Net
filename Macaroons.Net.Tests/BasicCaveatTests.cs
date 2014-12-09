using NUnit.Framework;


namespace Macaroons.Tests
{
  [TestFixture]
  public class BasicCaveatTests
  {
    [Test]
    public void CanCopyCaveat()
    {
      // Arrange
      byte[] cid = new byte[] { 1, 2, 3, 4 };
      byte[] vid = new byte[] { 4, 3, 2, 1 };
      byte[] cl = new byte[] { 1, 1, 2, 2 };
      Caveat c1 = new Caveat(new Packet(cid, DataEncoding.Hex), new Packet(vid, DataEncoding.Hex), new Packet(cl, DataEncoding.Hex));

      // Act
      Caveat c2 = new Caveat(c1);

      // Assert
      Assert.AreEqual(c1.CId, c2.CId);

      // Change original values and verify the new values doesn't change
      c1.CId[0] = 9;
      c1.VId[0] = 8;
      c1.CId[0] = 7;

      Assert.AreNotEqual(c1.CId, c2.CId);

      Assert.AreEqual(1, c2.CId[0]);
      Assert.AreEqual(4, c2.VId[0]);
      Assert.AreEqual(1, c2.Cl[0]);
    }


    [Test]
    public void CanPrintCaveat()
    {
      // Arrange
      Caveat c = new Caveat("Caveat 1");

      // Act
      string s = c.ToString();

      // Assert
      Assert.AreEqual("Caveat 1", s);
    }
  }
}
