namespace Macaroons
{
  /// <summary>
  /// Represents a crypto algorithm for encrypting and decrypting data.
  /// </summary>
  public abstract class CryptoAlgorithm
  {
    public abstract byte[] Encrypt(byte[] key, byte[] plainText);

    public abstract byte[] Decrypt(byte[] key, byte[] cipherText);
  }
}
