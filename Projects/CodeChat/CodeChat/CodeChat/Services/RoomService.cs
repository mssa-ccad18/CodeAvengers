using CodeChat.Services.Encryption;

namespace CodeChat.Services
{
    public class RoomService 
    {
        private readonly RoomEncryptionService _encryptionService;
        private readonly string storagePath = "ChatAppStorage/rooms/";
        
        public RoomEncryptionService EncryptionService => _encryptionService;
        public RoomService(RoomEncryptionService encryptionService)
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

        public (byte[] CipherText, byte[] IV) EncryptPublicRoomKey(string roomKey, byte[] userPublicKey) {
            // Assuming the EncryptPublicRoomKey method in IRoomEncryptionService should return a byte array.
            // Adjusting the call to match the expected return type.
            var encryptedKey = _encryptionService.EncryptMessage(roomKey, userPublicKey);
            return encryptedKey;
        }

        public byte[] GetRoomKey(string roomId)
        {
            string keyFilePath = Path.Combine(storagePath, $"{roomId}.key");
            if (!File.Exists(keyFilePath))
                throw new InvalidOperationException("Room key not found.");

            return File.ReadAllBytes(keyFilePath);
        }

        public (byte[] CipherText, byte[] IV) EncryptMessage(string roomId, string message)
        {
            byte[] key = GetRoomKey(roomId);

            // Assuming the EncryptMessage method in IRoomEncryptionService should return a tuple (byte[] CipherText, byte[] IV).
            // Adjusting the call to match the expected return type.
            var encryptionResult = _encryptionService.EncryptMessage(message: message, key);

            if (encryptionResult is (byte[] cipherText, byte[] iv))
            {
                return (cipherText, iv);
            }

            throw new InvalidOperationException("Encryption result is not in the expected format.");
        }

        public string DecryptMessage(string roomId, byte[] cipherText, byte[] IV)
        {
            byte[] key = GetRoomKey(roomId);
            return _encryptionService.DecryptMessage(cipherText, IV, key);
        }
    }
}
