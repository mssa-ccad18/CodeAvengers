using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using CodeChat.Services.Interfaces;

namespace CodeChat.Services.Encryption
{
    public class RoomEncryptionService : IRoomEncryptionService
    {
        public byte[] GenerateRoomKey()
        {
            using var aes = Aes.Create();
            aes.KeySize = 256;
            aes.GenerateKey();
            return aes.Key;
        }

        public (byte[] CipherText, byte[] IV) EncryptMessage(string message, byte[] key) //method 2: EncryptMessage(encrypts PT, returns CT and IV sep) 
        {
            byte[] cipherText;
            byte[] iv;

            using var aes = Aes.Create();
            aes.Key = key;
            aes.GenerateIV(); //generate IV for messages
            iv = aes.IV;

            using var encryptor = aes.CreateEncryptor(aes.Key, aes.IV);
            using var ms = new MemoryStream();
            using var cs = new CryptoStream(ms, encryptor, CryptoStreamMode.Write);
            using var sw = new StreamWriter(cs);
            sw.Write(message);
            sw.Flush();
            cs.FlushFinalBlock();

            cipherText = ms.ToArray();
            return (cipherText, iv); //returning CT and IV as tuple

        }

        public string DecryptMessage(byte[] cipherText, byte[] iv, byte[] key)
        {
            using var aes = Aes.Create();
            aes.Key = key;
            aes.IV = iv;

            using var decryptor = aes.CreateDecryptor(aes.Key, aes.IV);
            using var ms = new MemoryStream(cipherText);
            using var cs = new CryptoStream(ms, decryptor, CryptoStreamMode.Read);
            using var sr = new StreamReader(cs);
            return sr.ReadToEnd();
        }



    }



}