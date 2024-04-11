﻿using NaCl;
using System;
using System.Security.Cryptography;


namespace Macaroons
{
  /// <summary>
  /// Representing algorithms for encrypting and decrypting data using the Secret Box algorithm.
  /// </summary>
  /// <remarks>Secret Box as used in Macaroons is identical to the XSalsa20-Poly1305 algorithm.</remarks>
  public class SecretBoxCryptoAlgorithm : CryptoAlgorithm
  {
    /// <summary>
    /// Initialize crypto algorithm configured to use a random nonce.
    /// </summary>
    public SecretBoxCryptoAlgorithm()
    {
    }


    /// <summary>
    /// Initialize crypto algorithm with nonce usage as specified.
    /// </summary>
    /// <param name="useRandomNonce">Use random nonce for encryption (true) or all zeros nonce (false).</param>
    public SecretBoxCryptoAlgorithm(bool useRandomNonce)
    {
      UseRandomNonce = useRandomNonce;
    }


    protected bool UseRandomNonce = false;

    
    /// <summary>
    /// Encrypt plain text data using the given secret key.
    /// </summary>
    /// <param name="key">Secret key.</param>
    /// <param name="plainText">Plain text data.</param>
    /// <returns>Generated nonce plus encrypted cipher text in one single cipher block.</returns>
    public override byte[] Encrypt(byte[] key, byte[] plainText)
    {
      // Create nonce having all bytes set to zero.
      byte[] nonce = new byte[XSalsa20Poly1305.NonceLength];

      // All of the secret box documentation states that it is important to use a random nonce (or avoid reusing nonces completely).
      // But the original C implemention ignores this so it is made configurable in order to be compatible with that implementation.
      if (UseRandomNonce)
      {
        using (RNGCryptoServiceProvider rng = new RNGCryptoServiceProvider())
          rng.GetBytes(nonce);
      }

      // Created encrypted data including N * zero bytes padding from the secret box algorithm
      byte[] cipherTextPadded = new byte[plainText.Length + XSalsa20Poly1305.TagLength];

      using (var xSalsa20Poly1305 = new XSalsa20Poly1305(key))
      {
        xSalsa20Poly1305.Encrypt(cipherTextPadded, plainText, nonce);
      }

      using (var xSalsa20Poly1305 = new XSalsa20Poly1305(key))
      {
        var result = new byte[cipherTextPadded.Length];
        bool ok = xSalsa20Poly1305.TryDecrypt(result, cipherTextPadded, nonce);
        if (!ok)
          throw new ApplicationException("Could not decrypt"); // FIXME: Improve.
      }

      byte[] cipherBlock = new byte[nonce.Length + cipherTextPadded.Length];
      Buffer.BlockCopy(nonce, 0, cipherBlock, 0, nonce.Length);

      Buffer.BlockCopy(cipherTextPadded, 0, cipherBlock, nonce.Length, cipherBlock.Length - nonce.Length);

      return cipherBlock;
    }


    /// <summary>
    /// Decrypt cipher text using the given secret key.
    /// </summary>
    /// <param name="key">Secret key.</param>
    /// <param name="cipherBlock">Nonce and cipher text in one single cipher block.</param>
    /// <returns>Decrypted plain text.</returns>
    public override byte[] Decrypt(byte[] key, byte[] cipherBlock)
    {
      // Extract nonce from cipher block.
      byte[] nonce = new byte[XSalsa20Poly1305.NonceLength];
      Buffer.BlockCopy(cipherBlock, 0, nonce, 0, nonce.Length);

      // Extract cipher text and add padding for the secret box algorithm
      byte[] cipherTextPadded = new byte[cipherBlock.Length - nonce.Length];
      Buffer.BlockCopy(cipherBlock, nonce.Length, cipherTextPadded, 0, cipherBlock.Length - nonce.Length);

      // Decrypt the cipher text
      using (var xSalsa20Poly1305 = new XSalsa20Poly1305(key))
      {
        var result = new byte[cipherTextPadded.Length];
        bool ok = xSalsa20Poly1305.TryDecrypt(result, cipherTextPadded, nonce);
        if (!ok)
          throw new ApplicationException("Could not decrypt"); // FIXME: Improve.

        return result;
      }
    }
  }
}
