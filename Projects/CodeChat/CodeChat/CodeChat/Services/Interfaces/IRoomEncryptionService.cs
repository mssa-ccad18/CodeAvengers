using System.Security.Cryptography;

namespace CodeChat.Services.Interfaces
{
    public interface IRoomEncryptionService
    {
        byte[] GenerateRoomKey();

        (byte[] CipherText, byte[] IV) EncryptMessage(string message, byte[] key);

        string DecryptMessage(byte[] cipherText, byte[] iv, byte[] key);
    }

}

