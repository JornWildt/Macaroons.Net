using System;


namespace Macaroons
{
  public class SecretBoxCryptoAlgorithm : CryptoAlgorithm
  {
    private const int SECRET_BOX_ZERO_BYTES = 16;

    private const int SECRET_BOX_NONCE_BYTES = 24;

    
    public override byte[] Encrypt(byte[] key, byte[] plainText)
    {
      // Create nonce having all bytes set to zero.
      byte[] nonce = new byte[SECRET_BOX_NONCE_BYTES];

      // FIXME: make this optional
      //using (RNGCryptoServiceProvider rng = new RNGCryptoServiceProvider())
      //  rng.GetBytes(enc_nonce);

      // Created encrypted data including N * zero bytes padding from the secret box algorithm
      byte[] cipherTextPadded = Sodium.SecretBox.Create(plainText, nonce, key);

      // Create a cipher block consisting of the nonce and the cipher text excluding the padding
      byte[] cipherBlock = new byte[nonce.Length + cipherTextPadded.Length - SECRET_BOX_ZERO_BYTES];
      Buffer.BlockCopy(nonce, 0, cipherBlock, 0, nonce.Length);
      Buffer.BlockCopy(cipherTextPadded, SECRET_BOX_ZERO_BYTES, cipherBlock, nonce.Length, cipherBlock.Length - nonce.Length);

      return cipherBlock;
    }


    public override byte[] Decrypt(byte[] key, byte[] cipherBlock)
    {
      // Extract nonce from cipher block.
      byte[] nonce = new byte[SECRET_BOX_NONCE_BYTES];
      Buffer.BlockCopy(cipherBlock, 0, nonce, 0, nonce.Length);

      // Extract cipher text and add padding for the secret box algorithm
      byte[] cipherTextPadded = new byte[cipherBlock.Length - nonce.Length + SECRET_BOX_ZERO_BYTES];
      Buffer.BlockCopy(cipherBlock, nonce.Length, cipherTextPadded, SECRET_BOX_ZERO_BYTES, cipherBlock.Length - nonce.Length);

      // Decrypt the cipher text
      byte[] plainText = Sodium.SecretBox.Open(cipherTextPadded, nonce, key);
      return plainText;
    }
  }
}
