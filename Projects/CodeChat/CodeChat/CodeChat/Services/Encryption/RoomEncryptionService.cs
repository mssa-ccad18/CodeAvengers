using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using CodeChat.Services.Interfaces;

namespace CodeChat.Services.Encryption
{
    public class RoomEncryptionService : IRoomEncryptionService  // class declaration, implementing IRoomEncryptionService
    {
        private const int keySize = 32; // 256 bits
        private const int NonceSize = 12; // 96 bits, recommended for ChaCha20-Poly1305
        private const int TagSize = 16; // 128 bits, authentication tag - ensures no tampering has occurred


        //Created by the Poly1305 message authentication code (MAC) function
        //tag must match during decryption, or the message is rejected
        //ChaCha20-Poly1305 is an authenticated encryption algorithm that combines the ChaCha20 stream cipher with the Poly1305 MAC


        public byte[] GenerateRoomKey()
        {
            byte[] key = new byte[keySize];
            using var rng = RandomNumberGenerator.Create();
            rng.GetBytes(key);
            return key;
        }
        public (byte[] CipherText, byte[] Nonce, byte[] Tag) EncryptMessage(string message, byte[] key) //method 2: EncryptMessage(encrypts PT, returns CT and nonce sep) 
        {
            byte[] nonce = new byte[NonceSize];  // Must be unique for each encryption - apends to the ciphertext (message integrity)
            using var rng = RandomNumberGenerator.Create();
            rng.GetBytes(nonce);  //Creates a random nonce for each encryption operation, possibly use XChacha20 for nonce generation

            byte[] plaintextBytes = Encoding.UTF8.GetBytes(message);
            byte[] cipherText = new byte[plaintextBytes.Length];
            byte[] tag = new byte[TagSize];

            using var chacha = new ChaCha20Poly1305(key);
            chacha.Encrypt(nonce, plaintextBytes, cipherText, tag);

            return (cipherText, nonce, tag);

        }

        public string DecryptMessage(byte[] cipherText, byte[] nonce, byte[] tag, byte[] key)
        {
            byte[] plaintextBytes = new byte[cipherText.Length];

            using var chacha = new ChaCha20Poly1305(key);

            try
            {
                chacha.Decrypt(nonce, cipherText, tag, plaintextBytes);
            }
            catch (CryptographicException)
            {
                throw new InvalidOperationException("Decryption failed - possible tampering detected.");
            }

            return Encoding.UTF8.GetString(plaintextBytes);
        }

    }






}