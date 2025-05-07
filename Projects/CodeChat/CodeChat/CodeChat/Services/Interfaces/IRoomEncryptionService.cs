using System.Security.Cryptography;

namespace CodeChat.Services.Interfaces
{
    public interface IRoomEncryptionService
    {
        byte[] GenerateRoomKey();
        (byte[] CipherText, byte[] Nonce, byte[] Tag) EncryptMessage(string message, byte[] key);
        string DecryptMessage(byte[] cipherText, byte[] nonce, byte[] tag, byte[] key);
    }

}

