using System;
using CodeChat.Client.Components.Models;
using CodeChat.Services;
using CodeChat.Services.Encryption;

namespace CodeChatTest
{
    internal class RoomCreationTest
    {
        private readonly string roomId = Guid.NewGuid().ToString(); // Unique room ID for testing
        private readonly int userId = 1; // Unique user ID for testing
        // This test will check if the room key is generated and stored correctly.
        [TestMethod]
        public void TestRoomKeyGeneration() {
            // Arrange
            var roomService = new RoomService(new RoomEncryptionService());
            string roomName = "testRoom";

            // Act
            byte[] roomKey = roomService.CreateRoomKey(roomId);
            // Assert
            Assert.IsNotNull(roomKey, "Room key should not be null.");
            Assert.IsTrue(roomKey.Length > 0, "Room key should not be empty.");
            // Check if the key file exists
            string keyFilePath = Path.Combine("ChatAppStorage/rooms/", $"{roomId}.key");
            Assert.IsTrue(File.Exists(keyFilePath), "Room key file should exist.");
        }
    }
}
