using CodeChat.Services.Encryption;
namespace CodeChatTest
{
    [TestClass]
    public sealed class RoomEncryptionServiceTest
    {
        [TestMethod]
        public void TestGenerateKeyWillProduceDifferentByteArrays()
        {
            var cryptoService = new RoomEncryptionService();
            var key1 = cryptoService.GenerateRoomKey();
            var key2 = cryptoService.GenerateRoomKey();

            //verifying that the keys are different
            CollectionAssert.AreNotEqual(key1, key2);

            //verifying that the length is 256 bits (32 bytes)
            Assert.AreEqual(32, key1.Length, "Key1 should be 256 bits (32 bytes)");
            Assert.AreEqual(32, key2.Length, "Key2 should be 256 bits (32 bytes)");
        }


        [TestMethod]
        public void RoundTripGivenTheSameSymmetricKeyWorks()
        {
            var cryptoService = new RoomEncryptionService();
            var key1 = cryptoService.GenerateRoomKey();
            string msg = "Hello World";

            // Encrypt the message
            (byte[] CipherText, byte[] VI) = cryptoService.EncryptMessage(msg, key1);

            // Decrypt the message
            string decrypted = cryptoService.DecryptMessage(CipherText, VI, key1);

            //check if the decrypted message is the same as the original
            Assert.AreEqual(decrypted, msg);
        }

        [TestMethod]
        public void IVShouldBeUniqueForEachMessage()
        {
            var cryptoService = new RoomEncryptionService();
            var key1 = cryptoService.GenerateRoomKey();
            string msg = "Hello World";
            // Encrypt the message
            (byte[] CipherText1, byte[] IV1) = cryptoService.EncryptMessage(msg, key1);
            (byte[] CipherText2, byte[] IV2) = cryptoService.EncryptMessage(msg, key1);
            //check if the IVs are different
            CollectionAssert.AreNotEqual(IV1, IV2);
        }

        [TestMethod]
        public void IVShouldBeUniqueTest2()
        {
            var cryptoService = new RoomEncryptionService();
            var key = cryptoService.GenerateRoomKey();
            int messageCount = 100;
            HashSet<string> ivSet = new HashSet<string>();

            for (int i = 0; i < messageCount; i++)
            {
                (byte[] CipherText, byte[] IV) = cryptoService.EncryptMessage($"Message {i}", key);

                //store the IV as a hex string for comparison
                string ivHex = BitConverter.ToString(IV);
                Assert.IsFalse(ivSet.Contains(ivHex), $"IV Collision detected for message {i}");
                ivSet.Add(ivHex);

            }
            // Check that all IVs are unique
            Assert.AreEqual(messageCount, ivSet.Count, "IVs should be unique for each message");
        }

    }
}
