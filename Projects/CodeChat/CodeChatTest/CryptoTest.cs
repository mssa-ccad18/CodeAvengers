//using Microsoft.VisualStudio.TestTools.UnitTesting;
//using Microsoft.JSInterop;
//using System;
//using System.Text;
//using System.Diagnostics.CodeAnalysis;
//using CodeChat.Client.Components.Models;
//using CodeChat.Data;
//using CodeChat.Services;
//using CodeChat.Services.Encryption;
//using Microsoft.EntityFrameworkCore;
//using Microsoft.AspNetCore.SignalR;
//using CodeChat.Hubs;
//using Microsoft.AspNetCore.Mvc.RazorPages;
//using Microsoft.AspNetCore.SignalR.Client;

//namespace CodeChat.Tests
//{
//    [TestClass]
//    public class CryptoTests
//    {
//        private IJSRuntime _jsRuntime;
//        private string testConnectionString = "Data Source=memory";
//        private DbContextOptions<ChatDbContext> options;
//        private ChatDbContext _dbContext;
//        private RoomService _roomService;
//        private ChatHub _chatHub;
//        private HubConnection _hubConnection;

//        [TestInitialize]
//        public void TestInitialize() {

//            //// Initialize the database context with an in-memory database for testing
//            //options = new DbContextOptionsBuilder<ChatDbContext>()
//            //    .UseInMemoryDatabase(databaseName: "TestDatabase")
//            //    .Options;
//            // Initialize RoomService  
//            _roomService = new RoomService(new RoomEncryptionService());

//            // Initialize JSRuntime (mock or real instance depending on your setup)  
//            _jsRuntime = new MockJSRuntime();
//            // Initialize ChatHub with the mock DbContext
//            _chatHub = new ChatHub(_dbContext);
//            // Initialize Mock hubContext
//            var mockHubContext = new MockHubContext();
            
//            // Initialize Mock hubConnection
//            _hubConnection = new HubConnectionBuilder()
//                .WithUrl("https://localhost:5001/testchathub")
//                .Build();
//        }

//        [TestMethod]
//        public async Task TestEncryptMessage() {
//            // Arrange
//            const string testMessage = "Hello, CodeChat!";

//            // initialize the database
//            _dbContext.Database.EnsureDeleted();
//            _dbContext.Database.EnsureCreated();

//            // connect to mock hub 
//            _hubConnection = new HubConnectionBuilder()
//                .WithUrl("https://localhost:5001/testchathub")
//                .Build();
//            var userList = await CreateTestUsersAsync();

//            TestPasswordVerification(userList);

//            // create test room
//            var roomCreated = await _chatHub.CreateRoom(userList[0].Username, new List<string> { userList[1].Username });
//            Assert.IsTrue(roomCreated, "room not created");

//            //// generate the public key as a JSON Web Key (JWK) string
//            //var pubKey = await _jsRuntime.InvokeAsync<string>("generateKeyPair");
//            //Assert.IsNotNull(pubKey, "Public key should not be null.");
//            //Assert.IsTrue(pubKey.Length > 0, "Public key should not be empty.");
//            //Assert.IsTrue(pubKey.Contains("kty"), "Public key should be a valid JWK.");
//            //Assert.IsTrue(pubKey.Contains("alg"), "Public key should contain algorithm information.");
//            //Assert.IsTrue(pubKey.Contains("crv"), "Public key should contain curve information.");
//            //Assert.IsTrue(pubKey.Contains("x"), "Public key should contain x coordinate.");
//            //Assert.IsTrue(pubKey.Contains("y"), "Public key should contain y coordinate.");
//            //Assert.IsTrue(pubKey.Contains("kid"), "Public key should contain key ID.");
//            //Assert.IsTrue(pubKey.Contains("use"), "Public key should contain usage information.");
//            //Assert.IsTrue(pubKey.Contains("key_ops"), "Public key should contain key operations information.");

//            //// convert the public key to a byte array
//            //var publicKeyBytes = Encoding.UTF8.GetBytes(pubKey);
//            //Assert.IsNotNull(publicKeyBytes, "Public key bytes should not be null.");
//            //Assert.IsTrue(publicKeyBytes.Length > 0, "Public key bytes should not be empty.");



//            //// Assert
//            //Assert.IsNotNull(encryptedMessage, "Encrypted message should not be null.");
//            //Assert.IsTrue(encryptedMessage.Length > 0, "Encrypted message should not be empty.");

//            //// Optionally, decrypt the message and verify it matches the original
//            //var decryptedMessage = await DecryptMessage(encryptedMessage);
//            //Assert.AreEqual(testMessage, decryptedMessage, "Decrypted message should match the original message.");
//        }

//        public async Task<List<User>> CreateTestUsersAsync() {
//            // create test users
//            Assert.IsNotNull(_dbContext, "Database context should not be null.");
//            Assert.IsNotNull(_chatHub, "ChatHub should not be null.");
//            var user1Created = await _chatHub.CreateUser(new User { Id = 1, Username = "andrew", Password = "test", Email = "me@me.gmail.com" });
//            var user2Created = await _chatHub.CreateUser(new User { Id = 2, Username = "bob", Password = "test123", Email = "bob@me.gmail.com" });
//            await Task.CompletedTask; // Simulate async operation
//            await Task.Delay(1000); // Simulate some delay
//            Assert.IsTrue(user1Created, "User 1 not created");
//            Assert.IsTrue(user2Created, "User 2 not created");
//            var user = await _dbContext.Users.FirstOrDefaultAsync(u => u.Username == "andrew");
//            var user2 = await _dbContext.Users.FirstOrDefaultAsync(u => u.Username == "bob");
//            Assert.IsNotNull(user, "User 1 not found in the database.");
//            Assert.IsNotNull(user2, "User 2 not found in the database.");
//            Assert.AreEqual(user.Username, "andrew", "User 1 username mismatch.");
//            Assert.AreEqual(user2.Username, "bob", "User 2 username mismatch.");

//            return new List<User> { user, user2 };
//        }

//        public void TestPasswordVerification(List<User> userList) {
//            // verify passwords
//            Assert.IsTrue(userList[0].VerifyPassword("test"), "User password verification failed.");
//            Assert.IsTrue(userList[1].VerifyPassword("test123"), "User password verification failed.");
//        }


//        private async Task<byte[]> EncryptMessage(string roomID, string message) {
//            // Call the JavaScript function to encrypt the message
//            return await _jsRuntime.InvokeAsync<byte[]>("encryptMessage", roomID, message);
//        }

//        private async Task<string> DecryptMessage(byte[] encryptedMessage) {
//            // Retrieve the private key from IndexedDB
//            var privateKey = await _jsRuntime.InvokeAsync<object>("getPrivateKey");

//            // Call the JavaScript function to decrypt the message
//            var decryptedMessage = await _jsRuntime.InvokeAsync<byte[]>("decryptMessage", privateKey, encryptedMessage);

//            // Convert the decrypted message to a string
//            return Encoding.UTF8.GetString(decryptedMessage);
//        }

//        public class MockJSRuntime : IJSRuntime
//        {
//            public ValueTask<TValue> InvokeAsync<TValue>(string identifier, object?[]? args) {
//                // Mock the behavior of JavaScript functions
//                if (identifier == "generateKeyPair") {
//                    // Return a mock key pair
//                    return new ValueTask<TValue>((TValue)(object)new Dictionary<string, object>
//                    {
//                        { "publicKey", "mockPublicKey" }
//                    });
//                } else if (identifier == "encryptMessage") {
//                    // Return a mock encrypted message
//                    return new ValueTask<TValue>((TValue)(object)new byte[] { 1, 2, 3, 4 });
//                } else if (identifier == "decryptMessage") {
//                    // Return the original message for testing
//                    return new ValueTask<TValue>((TValue)(object)System.Text.Encoding.UTF8.GetBytes("Hello, CodeChat!"));
//                }

//                throw new NotImplementedException($"Mock for {identifier} is not implemented.");
//            }

//            public ValueTask<TValue> InvokeAsync<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors | DynamicallyAccessedMemberTypes.PublicFields | DynamicallyAccessedMemberTypes.PublicProperties)] TValue>(string identifier, CancellationToken cancellationToken, object?[]? args) {
//                throw new NotImplementedException();
//            }
//        }

//        public class MockHubContext : IHubContext<ChatHub>
//        {
//            public IHubClients Clients => throw new NotImplementedException();
//            public IGroupManager Groups => throw new NotImplementedException();
//            public IServiceProvider ServiceProvider => throw new NotImplementedException();
//            public Task SendAsync(string methodName, object? arg1, CancellationToken cancellationToken = default) {
//                throw new NotImplementedException();
//            }
//        }

//        public class MockPageModel : PageModel
//        {
//            public void OnGet() {
//                // Mock implementation for testing
//            }
//        }

//        public class MockIndexedDB
//        {
//            private readonly Dictionary<string, MockDatabase> _databases;

//            public MockIndexedDB() {
//                _databases = new Dictionary<string, MockDatabase>();
//            }

//            public MockDatabase Open(string name, int version) {
//                if (!_databases.ContainsKey(name)) {
//                    _databases[name] = new MockDatabase(name, version);
//                }
//                return _databases[name];
//            }
//        }

//        public class MockDatabase
//        {
//            public string Name { get; }
//            public int Version { get; }
//            private readonly Dictionary<string, MockObjectStore> _objectStores;

//            public MockDatabase(string name, int version) {
//                Name = name;
//                Version = version;
//                _objectStores = new Dictionary<string, MockObjectStore>();
//            }

//            public MockObjectStore CreateObjectStore(string name) {
//                if (!_objectStores.ContainsKey(name)) {
//                    _objectStores[name] = new MockObjectStore(name);
//                }
//                return _objectStores[name];
//            }

//            public MockTransaction Transaction(string storeName, string mode) {
//                if (!_objectStores.ContainsKey(storeName)) {
//                    throw new KeyNotFoundException($"Object store '{storeName}' does not exist.");
//                }
//                return new MockTransaction(_objectStores[storeName], mode);
//            }
//        }

//        public class MockTransaction
//        {
//            private readonly MockObjectStore _objectStore;
//            public string Mode { get; }

//            public MockTransaction(MockObjectStore objectStore, string mode) {
//                _objectStore = objectStore;
//                Mode = mode;
//            }

//            public MockObjectStore ObjectStore(string name) {
//                return _objectStore;
//            }
//        }

//        public class MockObjectStore
//        {
//            public string Name { get; }
//            private readonly Dictionary<string, object> _data;

//            public MockObjectStore(string name) {
//                Name = name;
//                _data = new Dictionary<string, object>();
//            }

//            public void Put(string key, object value) {
//                _data[key] = value;
//            }

//            public object Get(string key) {
//                if (_data.ContainsKey(key)) {
//                    return _data[key];
//                }
//                return null;
//            }

//            public List<object> GetAll() {
//                return new List<object>(_data.Values);
//            }
//        }
//    }
//}

