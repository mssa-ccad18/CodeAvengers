using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using CodeChat.Services.Interfaces;

namespace CodeChat.Services
{
    public class RoomService
    {
        private readonly IRoomEncryptionService _encryptionService;
        private readonly string storagePath = "ChatAppStorage/rooms/";

        public RoomService(IRoomEncryptionService encryptionService)
        {
            _encryptionService = encryptionService;
            Directory.CreateDirectory(storagePath);
        }

        public byte[] CreateRoomKey(string roomId)
        {
            byte[] key = _encryptionService.GenerateRoomKey();
            string keyFilePath = Path.Combine(storagePath, $"{roomId}.key");
            File.WriteAllBytes(keyFilePath, key);
            return key;
        }

        public byte[] GetRoomKey(string roomId)
        {
            string keyFilePath = Path.Combine(storagePath, $"{roomId}.key");
            if (!File.Exists(keyFilePath))
                throw new InvalidOperationException("Room key not found.");

            return File.ReadAllBytes(keyFilePath);
        }

        public (byte[] CipherText, byte[] Nonce, byte[] Tag) EncryptMessage(string roomId, string message)
        {
            byte[] key = GetRoomKey(roomId);
            return _encryptionService.EncryptMessage(message, key);
        }

        public string DecryptMessage(string roomId, byte[] cipherText, byte[] nonce, byte[] tag)
        {
            byte[] key = GetRoomKey(roomId);
            return _encryptionService.DecryptMessage(cipherText, nonce, tag, key);
        }
    }
}
