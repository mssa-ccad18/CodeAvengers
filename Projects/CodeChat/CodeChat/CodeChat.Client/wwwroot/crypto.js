// Encrypt a message using the stored decrypted room key 
async function encryptMessage(roomID, message) {
    // get the room key from IndexedDB
    const roomKey = await getDecryptedRoomKey(roomID);
    if (!roomKey) {
        throw new Error("Room key not found in IndexedDB.");
    }
    // Convert the room key Uint8Array to a CryptoKey object
    const roomKeyCryptoKey = await window.crypto.subtle.importKey(
        "raw",
        roomKey,
        {
            name: "AES-GCM",
            length: 256,
        },
        false,
        ["encrypt"]
    );
    // Encrypt the message using the room key
    const encoder = new TextEncoder();
    const encodedMessage = encoder.encode(message);
    const encryptedMessage = await window.crypto.subtle.encrypt(
        {
            name: "RSA-OAEP",
            hash: { name: "SHA-256" },
        },
        roomKeyCryptoKey,
        encodedMessage
    );
    return new Uint8Array(encryptedMessage);
}

// Decrypt a message using the roomKey stored in IndexedDB
async function decryptMessage(encryptedMessage, roomID) {
    // get the room key from IndexedDB
    const roomKey = await getDecryptedRoomKey(roomID);
    if (!roomKey) {
        throw new Error("Room key not found in IndexedDB.");
    }
    // Convert the room key Uint8Array to a CryptoKey object
    const roomKeyCryptoKey = await window.crypto.subtle.importKey(
        "raw",
        roomKey,
        {
            name: "AES-GCM",
            length: 256,
        },
        false,
        ["decrypt"]
    );
    // decrypt the message using the room key
    const decryptedMessage = await window.crypto.subtle.decrypt(
        {
            name: "AES-GCM",
            iv: new Uint8Array(12), // Initialization vector (IV) for AES-GCM
        },
        roomKeyCryptoKey,
        encryptedMessage
    );
    const decoder = new TextDecoder();
    return decoder.decode(decryptedMessage);
}

// decrypt the room key
async function decryptRoomKey(encryptedRoomKey) {
    const privateKey = await getPrivateKey();
    if (!privateKey) {
        throw new Error("Private key not found in IndexedDB.");
    }
    // Decrypt the room key using the private key
    const decryptedRoomKey = await window.crypto.subtle.decrypt(
        {
            name: "RSA-OAEP",
            hash: { name: "SHA-256" },
        },
        privateKey,
        encryptedRoomKey
    );
    return new Uint8Array(decryptedRoomKey); // Return the decrypted room key as a Uint8Array
}

// store the decrypted key in the indexedDB based on the roomID
async function storeDecryptedKey(roomID, decryptedKey) {
    const dbRequest = indexedDB.open("KeyStorage", 1); // Open the database
    dbRequest.onupgradeneeded = (event) => {  // Create the object store if it doesn't exist
        const db = event.target.result;
        db.createObjectStore("keys", { keyPath: "roomID" });
    };
    const db = await new Promise((resolve, reject) => {   // Wait for the database to open
        dbRequest.onsuccess = (event) => resolve(event.target.result);
        dbRequest.onerror = (event) => reject(event.target.error);
    });
    const transaction = db.transaction(["keys"], "readwrite"); // Start a transaction
    const store = transaction.objectStore("keys"); // Get the object store
    // Store the decrypted key
    store.put({ roomID, decryptedKey });
    return transaction.complete;
}

// Retrieve the decrypted key from IndexedDB based on the roomID
async function getDecryptedRoomKey(roomID) {
    const dbRequest = indexedDB.open("KeyStorage", 1);
    const db = await new Promise((resolve, reject) => {
        dbRequest.onsuccess = (event) => resolve(event.target.result);
        dbRequest.onerror = (event) => reject(event.target.error);
    });
    const transaction = db.transaction(["keys"], "readonly");
    const store = transaction.objectStore("keys");
    const decryptedKey = await new Promise((resolve, reject) => {
        const request = store.get(roomID);
        request.onsuccess = (event) => resolve(event.target.result?.decryptedKey);
        request.onerror = (event) => reject(event.target.error);
    });
    return decryptedKey; // Return the decrypted key as a Uint8Array
}

// Generate an RSA key pair with a non-extractable private key
async function generateKeyPair() {
    // generate a public key
    const keyPair = await window.crypto.subtle.generateKey(
        {
            name: "RSA-OAEP",
            modulusLength: 2048, // Key size
            publicExponent: new Uint8Array([1, 0, 1]), // 65537
            hash: { name: "SHA-256" },
        },
        false, // Private key is non-extractable
        ["encrypt", "decrypt"]
    );

    // Export the public key as a byte array
    const publicKey = await window.crypto.subtle.exportKey("spki", keyPair.publicKey);

    const publicKeyBytes = new Uint8Array(publicKey);

    // Save the private Key in IndexedDB
    await savePrivateKey(keyPair.privateKey);

    return publicKeyBytes;
}

// Save the private key in IndexedDB
async function savePrivateKey(privateKey) {
    const dbRequest = indexedDB.open("KeyStorage", 1);

    dbRequest.onupgradeneeded = (event) => {
        const db = event.target.result;
        db.createObjectStore("keys", { keyPath: "id" });
    };

    const db = await new Promise((resolve, reject) => {
        dbRequest.onsuccess = (event) => resolve(event.target.result);
        dbRequest.onerror = (event) => reject(event.target.error);
    });

    const transaction = db.transaction(["keys"], "readwrite");
    const store = transaction.objectStore("keys");

    // Store the private key as a CryptoKey object
    store.put({ id: "privateKey", key: privateKey });

    return transaction.complete;
}

// Retrieve the private key from IndexedDB
async function getPrivateKey() {
    const dbRequest = indexedDB.open("KeyStorage", 1);

    const db = await new Promise((resolve, reject) => {
        dbRequest.onsuccess = (event) => resolve(event.target.result);
        dbRequest.onerror = (event) => reject(event.target.error);
    });

    const transaction = db.transaction(["keys"], "readonly");
    const store = transaction.objectStore("keys");

    const privateKey = await new Promise((resolve, reject) => {
        const request = store.get("privateKey");
        request.onsuccess = (event) => resolve(event.target.result?.key);
        request.onerror = (event) => reject(event.target.error);
    });

    return privateKey;  // Return the private key as a CryptoKey object
}

