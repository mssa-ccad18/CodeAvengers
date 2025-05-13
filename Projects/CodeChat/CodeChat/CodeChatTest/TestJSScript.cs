using Microsoft.JSInterop;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System.Text;
using System.Threading;
using System.Diagnostics;

namespace CodeChatTest;

/// <summary>
/// Custom IJSRuntime implementation for testing
/// </summary>
public class TestJSRuntime : IJSRuntime
{
    private readonly Dictionary<string, Delegate> _handlers = new();

    public void SetupHandler<TResult>(string methodName, Func<object[], TResult> handler)
    {
        _handlers[methodName] = handler;
    }

    public ValueTask<TValue> InvokeAsync<TValue>(string identifier, CancellationToken cancellationToken, object[] args)
    {
        Console.WriteLine($"InvokeAsync called: {identifier}");
        Console.WriteLine($"Args count: {args.Length}");
        for (int i = 0; i < args.Length; i++)
        {
            if (args[i] is byte[] bytes)
            {
                Console.WriteLine($"Arg {i}: byte[] of length {bytes.Length}");
            }
            else
            {
                Console.WriteLine($"Arg {i}: {args[i]}");
            }
        }

        if (_handlers.TryGetValue(identifier, out var handlerObj))
        {
            if (handlerObj is Func<object[], TValue> handler)
            {
                try
                {
                    var result = handler(args);
                    Console.WriteLine($"Returning result of type {typeof(TValue).Name}");
                    if (result is byte[] bytes)
                    {
                        Console.WriteLine($"Byte array length: {bytes.Length}");
                    }
                    return ValueTask.FromResult(result);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Exception in handler: {ex}");
                }
            }
            else
            {
                Console.WriteLine($"Handler type mismatch: expected Func<object[], {typeof(TValue).Name}>, got {handlerObj.GetType().Name}");
            }
        }
        else
        {
            Console.WriteLine($"No handler found for: {identifier}");
        }

        // Default value
        return ValueTask.FromResult<TValue>(default);
    }

    public ValueTask<TValue> InvokeAsync<TValue>(string identifier, object[] args)
    {
        return InvokeAsync<TValue>(identifier, CancellationToken.None, args);
    }
}

[TestClass]
public class TestJSScript
{
    private TestJSRuntime jsRuntime;
    private byte[] testRoomKey;
    private byte[] testEncryptedRoomKey;
    private byte[] testPublicKey;
    private string testRoomId = "test-room-123";

    [TestInitialize]
    public void Setup()
    {
        jsRuntime = new TestJSRuntime();
        
        // Create a test room key (32 bytes for AES-256)
        testRoomKey = new byte[32];
        for (int i = 0; i < testRoomKey.Length; i++)
        {
            testRoomKey[i] = (byte)i;
        }

        // Simulate RSA-OAEP encrypted room key (would be longer in reality, around 256 bytes for 2048-bit RSA)
        testEncryptedRoomKey = new byte[256]; // RSA-2048 encrypted data is 256 bytes
        Random rnd = new Random();
        rnd.NextBytes(testEncryptedRoomKey);

        // Simulate a public key (would be ~294 bytes for RSA-2048 in SPKI format)
        testPublicKey = new byte[294];
        rnd.NextBytes(testPublicKey);

        // Setup handlers for our custom JSRuntime
        jsRuntime.SetupHandler<byte[]>("decryptRoomKey", args => {
            Console.WriteLine("decryptRoomKey handler called");
            return testRoomKey;
        });

        jsRuntime.SetupHandler<object>("storeDecryptedKey", args => {
            Console.WriteLine("storeDecryptedKey handler called");
            return new object();
        });

        jsRuntime.SetupHandler<byte[]>("getDecryptedRoomKey", args => {
            Console.WriteLine("getDecryptedRoomKey handler called");
            if (args.Length > 0 && args[0].ToString() == testRoomId)
            {
                return testRoomKey;
            }
            return null;
        });

        jsRuntime.SetupHandler<byte[]>("encryptMessage", args => {
            Console.WriteLine("encryptMessage handler called");
            
            if (args.Length < 2) return null;
            
            string roomId = args[0].ToString();
            string message = args[1].ToString();
            
            // Create a random IV (12 bytes for AES-GCM)
            byte[] iv = new byte[12];
            rnd.NextBytes(iv);
            
            // Simulate encrypted message data (variable length based on message)
            byte[] encryptedData = new byte[message.Length + 16]; // Add some padding for auth tag
            rnd.NextBytes(encryptedData);
            
            // Combine IV and encrypted data like in the actual implementation
            byte[] combined = new byte[iv.Length + encryptedData.Length];
            Array.Copy(iv, 0, combined, 0, iv.Length);
            Array.Copy(encryptedData, 0, combined, iv.Length, encryptedData.Length);
            
            return combined;
        });

        jsRuntime.SetupHandler<string>("decryptMessage", args => {
            Console.WriteLine("decryptMessage handler called");
            
            if (args.Length < 2) return null;
            
            byte[] encryptedMessage = args[0] as byte[];
            string roomId = args[1].ToString();
            
            if (encryptedMessage == null || roomId != testRoomId)
            {
                return null;
            }
            
            // Simulate successful decryption
            return "Decrypted message content";
        });
    }

    [TestMethod]
    public async Task TestDecryptAndStoreRoomKey()
    {
        // Act
        Console.WriteLine("Calling decryptRoomKey");
        var decryptedKey = await jsRuntime.InvokeAsync<byte[]>("decryptRoomKey", testEncryptedRoomKey);
        Console.WriteLine($"decryptedKey length: {decryptedKey?.Length ?? 0}");
        
        // Store the decrypted AES key
        await jsRuntime.InvokeAsync<object>("storeDecryptedKey", testRoomId, decryptedKey);

        // Assert
        Assert.IsNotNull(decryptedKey, "Decrypted key should not be null");
        Assert.AreEqual(32, decryptedKey.Length, "Decrypted AES key should be 32 bytes (256 bits)");
        CollectionAssert.AreEqual(testRoomKey, decryptedKey, "Decrypted key should match the test key");
    }

    [TestMethod]
    public async Task TestEncryptMessage()
    {
        // Arrange
        string testMessage = "Hello, secure world!";

        // Act
        var encryptedMessage = await jsRuntime.InvokeAsync<byte[]>(
            "encryptMessage", 
            testRoomId,
            testMessage);

        // Assert
        Assert.IsNotNull(encryptedMessage, "Encrypted message should not be null");
        Assert.IsTrue(encryptedMessage.Length > 0, "Encrypted message should not be empty");
        Assert.IsTrue(encryptedMessage.Length > 12, "Encrypted message should include IV (12 bytes) plus encrypted data");
    }
    
    [TestMethod]
    public async Task TestFullMessageEncryptionDecryptionFlow()
    {
        // Arrange
        string originalMessage = "This is a confidential message";

        // Step 1: Encrypt the message
        var encryptedMessage = await jsRuntime.InvokeAsync<byte[]>(
            "encryptMessage", 
            testRoomId,
            originalMessage);
            
        Assert.IsNotNull(encryptedMessage, "Encrypted message should not be null");
        Assert.IsTrue(encryptedMessage.Length > 12, "Encrypted message should include IV (12 bytes) plus encrypted data");
        
        // Step 2: Decrypt the message
        var decryptedMessage = await jsRuntime.InvokeAsync<string>(
            "decryptMessage",
            encryptedMessage,
            testRoomId);
            
        // Assert
        Assert.IsNotNull(decryptedMessage, "Decrypted message should not be null");
        Assert.AreEqual("Decrypted message content", decryptedMessage, "Decryption should return the expected message");
    }
}
