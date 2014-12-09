using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;


namespace Macaroons.Net.Tests
{
  [TestFixture]
  public class BasicPackageTests
  {
    [Test]
    public void CanComparePackages()
    {
      // Arrange
      Packet p1a = new Packet(new byte[] { 1, 2, 3 }, DataEncoding.Hex);
      Packet p1b = new Packet(new byte[] { 1, 2, 3 }, DataEncoding.Hex);
      Packet p2 = new Packet(new byte[] { 3, 4, 5 }, DataEncoding.Hex);

      // Act
      bool ok1 = p1a == p1b;
      bool ok2 = p1a == p2;
      bool ok3 = p1a != p1b;
      bool ok4 = p1a != p2;

      // Assert
      Assert.IsTrue(ok1);
      Assert.IsFalse(ok2);
      Assert.IsFalse(ok3);
      Assert.IsTrue(ok4);
    }


    [Test]
    public void CanCalculateHasCodes()
    {
      // Arrange
      Packet p1a = new Packet(new byte[] { 1, 2, 3 }, DataEncoding.Hex);
      Packet p1b = new Packet(new byte[] { 1, 2, 3 }, DataEncoding.Hex);
      Packet p2 = new Packet(new byte[] { 3, 4, 5 }, DataEncoding.Hex);

      // Act
      int h1a = p1a.GetHashCode();
      int h1b = p1b.GetHashCode();
      int h2 = p2.GetHashCode();

      // Assert
      Assert.AreNotEqual(0, h1a);
      Assert.AreEqual(h1a, h1b);
      Assert.AreNotEqual(h1a, h2);
    }


    [Test]
    public void CanCopyPackage()
    {
      // Arrange
      Packet p1 = new Packet(new byte[] { 1, 2 }, DataEncoding.Hex);
      Packet p2 = new Packet("abc");

      // Act
      Packet q1 = new Packet(p1);
      Packet q2 = new Packet(p2);

      // Assert
      Assert.AreEqual(p1.Data, q1.Data);
      Assert.AreEqual(p1.Encoding, p1.Encoding);
      Assert.AreEqual(p2.Data, q2.Data);
      Assert.AreEqual(p2.Encoding, p2.Encoding);
    }
  }
}
