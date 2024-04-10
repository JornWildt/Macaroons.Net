using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;


namespace Macaroons.Tests
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
      Assert.That(ok1, Is.True);
      Assert.That(ok2, Is.False);
      Assert.That(ok3, Is.False);
      Assert.That(ok4, Is.True);
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
      Assert.That(h1a, Is.Not.EqualTo(0));
      Assert.That(h1b, Is.EqualTo(h1a));
      Assert.That(h2, Is.Not.EqualTo(h1a));
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
      Assert.That(q1.Data, Is.EqualTo(p1.Data));
      Assert.That(q1.Encoding, Is.EqualTo(p1.Encoding));
      Assert.That(q2.Data, Is.EqualTo(p2.Data));
      Assert.That(q2.Encoding, Is.EqualTo(p2.Encoding));
    }
  }
}
