// Encrypt a message using the stored decrypted room key 
async function encryptMessage(roomID, message) {
    console.log(`Attempting to encrypt message for room: ${roomID}`);
    
    if (!roomID) {
        console.error("Room ID is required for encryption");
        throw new Error("Room ID is required for encryption");
    }
    
    // get the room key from IndexedDB
    let roomKey = await getDecryptedRoomKey(roomID);
    
    // If the key is not found, log a clear warning
    if (!roomKey) {
        console.error(`No room key found for ${roomID}. Encryption is not possible without the proper key.`);
        throw new Error(`No room key found for room ${roomID}. Please request the key from the server.`);
    }
    
    console.log(`Retrieved room key for room: ${roomID}`);
    
    try {
        // Encrypt the message using the room key
        const encoder = new TextEncoder();
        const encodedMessage = encoder.encode(message);
        const iv = window.crypto.getRandomValues(new Uint8Array(12)); // Initialization vector (IV) for AES-GCM
        
        console.log("Encryption parameters prepared, encrypting message");
        
        const encryptedMessage = await window.crypto.subtle.encrypt(
            {
                name: "AES-GCM",
                iv: iv,
            },
            roomKey,
            encodedMessage
        );
        
        // Combine the IV and encrypted message
        const combinedMessage = new Uint8Array(iv.length + encryptedMessage.byteLength);
        combinedMessage.set(iv);
        combinedMessage.set(new Uint8Array(encryptedMessage), iv.length);
        
        console.log(`Message encrypted successfully, combined length: ${combinedMessage.length}`);
        
        return combinedMessage;
    } catch (error) {
        console.error(`Error during message encryption: ${error}`);
        throw error;
    }
}

// Decrypt a message using the roomKey stored in IndexedDB
async function decryptMessage(encryptedMessage, roomID) {
    // get the room key from IndexedDB
    const roomKey = await getDecryptedRoomKey(roomID);
    if (!roomKey) {
        throw new Error("Room key not found in IndexedDB.");
    }
    // Extract the IV from the encrypted message
    const iv = encryptedMessage.slice(0, 12); // First 12 bytes are the IV
    const encryptedData = encryptedMessage.slice(12); // Remaining bytes are the encrypted data
    // Decrypt the message using the room key
    const decryptedMessage = await window.crypto.subtle.decrypt(
        {
            name: "AES-GCM",
            iv: iv,
        },
        roomKey,
        encryptedData
    );
    // Convert the decrypted message to a string

    // Check if the decrypted message is null
    if (!decryptedMessage) {
        throw new Error("Decryption failed.");
    }
    // Convert the decrypted message to a string
    const decoder = new TextDecoder();
    const decodedMessage = decoder.decode(decryptedMessage);
    // Return the decoded message
    return decodedMessage;
}

// decrypt the room key
async function decryptRoomKey(encryptedRoomKey, roomID) {
    console.log(`Decrypting room key for room: ${roomID}`);
    console.log(`Encrypted key length: ${encryptedRoomKey ? encryptedRoomKey.length : 'undefined'}`);
    
    if (!roomID) {
        console.error("Room ID is required to decrypt and store the room key.");
    }
    
    if (!encryptedRoomKey || encryptedRoomKey.length === 0) {
        console.error("Encrypted room key is required for decryption.");
    }
    
    try {
        // Check if we already have a key for this room
        const existingKey = await getDecryptedRoomKey(roomID);
        if (existingKey) {
            console.log(`Key already exists for room ${roomID}. Using existing key.`);
            return existingKey;
        }
        
        console.log(`No existing key found for room ${roomID}. Decrypting from server key.`);
        
        // First try to get our private key
        const privateKey = await getPrivateKey();
        if (!privateKey) {
            console.error("Private key not found in IndexedDB");
        }
        
        // Get the private key
        console.log(`Private key retrieved, key algorithm: ${privateKey.algorithm.name}, usages: ${privateKey.usages.join(', ')}`);
        
        // Check if the key is actually suitable for decryption
        if (!privateKey.usages.includes("decrypt")) {
            console.error("ERROR: Private key does not have decrypt usage");
        }
        
        // We'll do thorough diagnostics to understand the issue
        let encryptedData;
        
        // Check if we have a string (possibly base64 encoded)
        if (typeof encryptedRoomKey === 'string') {
            console.log("Encrypted key is a string, converting to binary...");
            // Try to convert from base64 if it looks like base64
            try {
                // Convert base64 to binary
                const binaryString = atob(encryptedRoomKey);
                encryptedData = new Uint8Array(binaryString.length);
                for (let i = 0; i < binaryString.length; i++) {
                    encryptedData[i] = binaryString.charCodeAt(i);
                }
                console.log("Successfully converted base64 string to binary data");
            } catch (e) {
                console.warn("Not a valid base64 string, treating as binary string");
                // If not base64, treat as binary string
                encryptedData = new Uint8Array(encryptedRoomKey.length);
                for (let i = 0; i < encryptedRoomKey.length; i++) {
                    encryptedData[i] = encryptedRoomKey.charCodeAt(i);
                }
            }
        } else if (encryptedRoomKey instanceof Uint8Array) {
            // Already a Uint8Array
            encryptedData = encryptedRoomKey;
            console.log("Encrypted key is already a Uint8Array");
        } else if (encryptedRoomKey instanceof ArrayBuffer) {
            // Convert ArrayBuffer to Uint8Array
            encryptedData = new Uint8Array(encryptedRoomKey);
            console.log("Converted ArrayBuffer to Uint8Array");
        } else {
            // Try to handle as array-like object
            encryptedData = new Uint8Array(encryptedRoomKey);
            console.log("Converted array-like object to Uint8Array");
        }
        
        // Log detailed information about the encrypted data
        console.log(`Prepared encrypted data for decryption, length: ${encryptedData.byteLength}`);
        console.log(`First 16 bytes: ${Array.from(encryptedData.slice(0, 16)).join(', ')}`);
        console.log(`Last 16 bytes: ${Array.from(encryptedData.slice(-16)).join(', ')}`);
        
        // Log detailed information about the private key
        console.log(`Private key algorithm: ${privateKey.algorithm.name}`);
        console.log(`Private key modulus length: ${privateKey.algorithm.modulusLength}`);
        console.log(`Private key hash: ${privateKey.algorithm.hash.name}`);
        
        // Try RSA-OAEP decryption with exact parameters - NO FALLBACKS
        console.log("Attempting decryption with RSA-OAEP and SHA-256...");
        // Attempt decryption
        let decryptedRoomKey;
        try {
            decryptedRoomKey = await window.crypto.subtle.decrypt(
                {
                    name: "RSA-OAEP",
                    hash: { name: "SHA-256" },
                },
                privateKey,
                encryptedData
            );
            console.log("Decryption successful with SHA-256!");
        } catch (error) {
            console.error("Decryption failed with RSA-OAEP/SHA-256:", error);
            
            // Try with SHA-1 as a last resort
            console.log("Attempting decryption with RSA-OAEP and SHA-1...");
            try {
                decryptedRoomKey = await window.crypto.subtle.decrypt(
                    {
                        name: "RSA-OAEP",
                        hash: { name: "SHA-1" }, 
                    },
                    privateKey,
                    encryptedData
                );
                console.log("Decryption successful with SHA-1!");
            } catch (error2) {
                console.error("Decryption also failed with RSA-OAEP/SHA-1:", error2);
                console.error("DIAGNOSIS: RSA decryption failed with both SHA-256 and SHA-1");
                
            }
        }
        
        if (!decryptedRoomKey) {
            console.error("Decryption returned null or undefined result");
        }
        
        console.log("Room key decrypted successfully, importing as CryptoKey");
        console.log(`Decrypted key length: ${decryptedRoomKey.byteLength}`);
        console.log(`Decrypted key bytes: ${Array.from(new Uint8Array(decryptedRoomKey)).join(', ')}`);
        
        // Convert the decrypted room key to a CryptoKey object
        const roomKeyCryptoKey = await window.crypto.subtle.importKey(
            "raw",
            decryptedRoomKey,
            {
                name: "AES-GCM",
                length: 256,
            },
            false,
            ["encrypt", "decrypt"]
        );
        
        console.log(`Room key imported as CryptoKey, storing for room ${roomID}`);
        
        // Store the decrypted key in IndexedDB
        await storeDecryptedKey(roomID, roomKeyCryptoKey);
        console.log(`Room key stored successfully in IndexedDB for room ${roomID}`);
        
        // Return the CryptoKey object
        return roomKeyCryptoKey;
    } catch (error) {
        console.error("Error in decryptRoomKey:", error);
        console.error("Stack trace:", error.stack);
    }
}

// Store the decrypted room key in IndexedDB
async function storeDecryptedKey(roomID, decryptedKey) {
    console.log(`Attempting to store key for room: ${roomID}`);
    
    if (!roomID) {
        throw new Error("Room ID is required to store the key.");
    }
    
    const dbRequest = indexedDB.open("KeyStorage", 1);
    if (!dbRequest) {
        throw new Error("Failed to open IndexedDB.");
    }
    
    dbRequest.onupgradeneeded = (event) => {
        console.log("Creating or upgrading database");
        const db = event.target.result;
        if (!db.objectStoreNames.contains("keys")) {
            db.createObjectStore("keys", { keyPath: "id" });
        }
    }
    
    try {
        const db = await new Promise((resolve, reject) => {
            dbRequest.onsuccess = (event) => resolve(event.target.result);
            dbRequest.onerror = (event) => {
                console.error("Error opening database:", event.target.error);
                reject(event.target.error);
            };
        });
        
        const transaction = db.transaction(["keys"], "readwrite");
        const store = transaction.objectStore("keys");
        
        // Store the decrypted key as a CryptoKey object
        const request = store.put({ id: roomID, decryptedKey: decryptedKey });
        
        // Wait for both the request and the transaction to complete
        await new Promise((resolve, reject) => {
            request.onsuccess = () => {
                console.log(`Key stored successfully for room: ${roomID}`);
                resolve();
            };
            request.onerror = (event) => {
                console.error("Error storing key:", event.target.error);
                reject(event.target.error);
            };
        });
        
        // Also wait for the transaction to complete
        await new Promise((resolve, reject) => {
            transaction.oncomplete = () => {
                console.log("Transaction completed successfully");
                resolve();
            };
            transaction.onerror = (event) => {
                console.error("Transaction error:", event.target.error);
                reject(event.target.error);
            };
        });
        
        return true;
    } catch (error) {
        console.error("Error in storeDecryptedKey:", error);
        throw error;
    }
}

// Retrieve the decrypted key from IndexedDB based on the roomID
async function getDecryptedRoomKey(roomID) {
    console.log(`Attempting to retrieve key for room: ${roomID}`);
    
    if (!roomID) {
        throw new Error("Room ID is required to retrieve the key.");
    }
    
    const dbRequest = indexedDB.open("KeyStorage", 1);
    if (!dbRequest) {
        throw new Error("Failed to open IndexedDB.");
    }
    
    try {
        const db = await new Promise((resolve, reject) => {
            dbRequest.onsuccess = (event) => resolve(event.target.result);
            dbRequest.onerror = (event) => {
                console.error("Error opening database:", event.target.error);
                reject(event.target.error);
            };
        });
        
        const transaction = db.transaction(["keys"], "readonly");
        const store = transaction.objectStore("keys");
        
        const result = await new Promise((resolve, reject) => {
            const request = store.get(roomID);
            request.onsuccess = (event) => {
                if (event.target.result) {
                    console.log(`Key found for room: ${roomID}`);
                } else {
                    console.warn(`No key found for room: ${roomID}`);
                }
                resolve(event.target.result);
            };
            request.onerror = (event) => {
                console.error("Error retrieving key:", event.target.error);
                reject(event.target.error);
            };
        });
        
        if (!result) {
            console.error(`No data found for room: ${roomID}`);
            return null;
        }
        
        if (!result.decryptedKey) {
            console.error(`Retrieved data does not contain decryptedKey for room: ${roomID}`);
            return null;
        }
        
        return result.decryptedKey;
    } catch (error) {
        console.error("Error in getDecryptedRoomKey:", error);
        throw error;
    }
}

// Generate an RSA key pair with a non-extractable private key
async function generateKeyPair() {
    console.log("DIAGNOSTICS: Generating new RSA-OAEP key pair");
    
    try {
        // First check if we already have a private key
        console.log("Checking for existing private key in storage...");
        const existingKey = await getPrivateKey();
        
        if (existingKey) {
            console.log("Found existing private key in storage");
            
            // Generate a new key pair for complete diagnostics
            console.log("DIAGNOSTICS: Generating completely new key pair");
        } else {
            console.log("No existing private key found in storage, will generate new one");
        }
        
        // Define RSA-OAEP parameters for maximum compatibility
        const rsaParams = {
            name: "RSA-OAEP",
            modulusLength: 2048,  // Standard RSA key size
            publicExponent: new Uint8Array([1, 0, 1]),  // 65537
            hash: { name: "SHA-256" },  // Use SHA-256 for better security
        };
        
        console.log(`DIAGNOSTICS: Key generation parameters:`);
        console.log(`- Algorithm: ${rsaParams.name}`);
        console.log(`- Modulus Length: ${rsaParams.modulusLength} bits`);
        console.log(`- Hash: ${rsaParams.hash.name}`);
        console.log(`- Public Exponent: ${Array.from(rsaParams.publicExponent).join(', ')}`);
        
        // Generate a new key pair with proper parameters
        console.log("Starting key generation...");
        const keyPair = await window.crypto.subtle.generateKey(
            rsaParams,
            true, // Allow export of keys
            ["encrypt", "decrypt"]
        );
        
        console.log("RSA key pair generated successfully");
        console.log(`Private key algorithm: ${keyPair.privateKey.algorithm.name}`);
        console.log(`Private key modulus length: ${keyPair.privateKey.algorithm.modulusLength}`);
        console.log(`Private key hash: ${keyPair.privateKey.algorithm.hash.name}`);
        console.log(`Private key usages: ${keyPair.privateKey.usages.join(', ')}`);
        
        // Store the private key in IndexedDB
        await savePrivateKey(keyPair.privateKey);
        console.log("Private key stored in IndexedDB");
        
        // Export the public key to raw format
        console.log("Exporting public key...");
        const publicKeyExported = await window.crypto.subtle.exportKey("spki", keyPair.publicKey);
        console.log(`Public key exported successfully, length: ${publicKeyExported.byteLength} bytes`);
        
        // Print detailed public key info
        const pubKeyBytes = new Uint8Array(publicKeyExported);
        console.log(`Public key first 16 bytes: ${Array.from(pubKeyBytes.slice(0, 16)).join(', ')}`);
        console.log(`Public key last 16 bytes: ${Array.from(pubKeyBytes.slice(-16)).join(', ')}`);
        
        // Convert to byte array for return
        return pubKeyBytes;
    } catch (error) {
        console.error("Error generating key pair:", error);
        console.error("Stack trace:", error.stack);
        throw error;
    }
}

// Save the private key in IndexedDB
async function savePrivateKey(privateKey) {
    console.log("Saving private key to IndexedDB");
    
    const dbRequest = indexedDB.open("KeyStorage", 1);
    if (!dbRequest) {
        console.error("Failed to open IndexedDB for saving private key");
        throw new Error("Failed to open IndexedDB.");
    }
    
    try {
        dbRequest.onupgradeneeded = (event) => {
            console.log("Creating or upgrading database for storing private key");
            const db = event.target.result;
            if (!db.objectStoreNames.contains("keys")) {
                db.createObjectStore("keys", { keyPath: "id" });
            }
        };
        
        const db = await new Promise((resolve, reject) => {
            dbRequest.onsuccess = (event) => resolve(event.target.result);
            dbRequest.onerror = (event) => {
                console.error("Error opening database for storing private key:", event.target.error);
                reject(event.target.error);
            };
        });
        
        const transaction = db.transaction(["keys"], "readwrite");
        const store = transaction.objectStore("keys");
        
        // Store the private key as a CryptoKey object
        const request = store.put({ id: "privateKey", key: privateKey });
        
        // Wait for both the request to complete
        await new Promise((resolve, reject) => {
            request.onsuccess = () => {
                console.log("Private key stored successfully");
                resolve();
            };
            request.onerror = (event) => {
                console.error("Error storing private key:", event.target.error);
                reject(event.target.error);
            };
        });
        
        // Wait for the transaction to complete
        await new Promise((resolve, reject) => {
            transaction.oncomplete = () => {
                console.log("Private key transaction completed successfully");
                resolve();
            };
            transaction.onerror = (event) => {
                console.error("Private key transaction error:", event.target.error);
                reject(event.target.error);
            };
        });
        
        return true;
    } catch (error) {
        console.error("Error saving private key:", error);
        throw error;
    }
}

// Retrieve the private key from IndexedDB
async function getPrivateKey() {
    console.log("Attempting to retrieve private key from IndexedDB");
    
    const dbRequest = indexedDB.open("KeyStorage", 1);
    if (!dbRequest) {
        console.error("Failed to open IndexedDB for private key retrieval");
        throw new Error("Failed to open IndexedDB.");
    }
    
    try {
        // Handle database creation/upgrade
        dbRequest.onupgradeneeded = (event) => {
            console.log("Creating or upgrading database for private key");
            const db = event.target.result;
            if (!db.objectStoreNames.contains("keys")) {
                db.createObjectStore("keys", { keyPath: "id" });
            }
        };
        
        const db = await new Promise((resolve, reject) => {
            dbRequest.onsuccess = (event) => resolve(event.target.result);
            dbRequest.onerror = (event) => {
                console.error("Error opening database for private key:", event.target.error);
                reject(event.target.error);
            };
        });
        
        const transaction = db.transaction(["keys"], "readonly");
        const store = transaction.objectStore("keys");
        
        const result = await new Promise((resolve, reject) => {
            const request = store.get("privateKey");
            request.onsuccess = (event) => {
                if (event.target.result) {
                    console.log("Private key found in IndexedDB");
                } else {
                    console.warn("No private key found in IndexedDB");
                }
                resolve(event.target.result);
            };
            request.onerror = (event) => {
                console.error("Error retrieving private key:", event.target.error);
                reject(event.target.error);
            };
        });
        
        if (!result) {
            console.error("No private key data found in IndexedDB");
            return null;
        }
        
        if (!result.key) {
            console.error("Retrieved data does not contain private key");
            return null;
        }
        
        console.log("Private key successfully retrieved from IndexedDB");
        return result.key;
    } catch (error) {
        console.error("Error in getPrivateKey:", error);
        throw error;
    }
}

// Check if a key exists for a given room ID
async function checkKeyExists(roomID) {
    console.log(`Checking if key exists for room: ${roomID}`);
    
    if (!roomID) {
        console.error("Room ID is required to check key existence");
        return false;
    }
    
    try {
        const key = await getDecryptedRoomKey(roomID);
        const exists = key !== null;
        console.log(`Key exists for room ${roomID}: ${exists}`);
        return exists;
    } catch (error) {
        console.error(`Error checking key existence: ${error}`);
        return false;
    }
}
