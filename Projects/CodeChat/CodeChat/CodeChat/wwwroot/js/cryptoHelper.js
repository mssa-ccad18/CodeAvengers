window.cryptoHelper = {
    async generateKey() {
        const key = await window.crypto.subtle.generateKey(
            { name: "AES-GCM", length: 256 },
            true,
            ["encrypt", "decrypt"]
        );
        const exportedKey = await window.crypto.subtle.exportKey("raw", key);
        const keyId = cryptoHelper.storeKey(exportedKey);
        return keyId;
    },

    storeKey(key) {
        const keyId = "my-secret-key";
        indexedDB.open("CryptoDB").onsuccess = function(event) {
            const db = event.target.result;
            const tx = db.transaction("keys", "readwrite");
            tx.objectStore("keys").put(key, keyId);
        };
        return keyId;
    },

    async encrypt(data, keyId) {
        const key = await cryptoHelper.getKey(keyId);
        const iv = window.crypto.getRandomValues(new Uint8Array(12));
        const encryptedData = await window.crypto.subtle.encrypt(
            { name: "AES-GCM", iv },
            key,
            new TextEncoder().encode(data)
        );
        return { iv, encryptedData };
    },

    async decrypt(encryptedData, keyId) {
        const key = await cryptoHelper.getKey(keyId);
        const decryptedData = await window.crypto.subtle.decrypt(
            { name: "AES-GCM", iv: encryptedData.iv },
            key,
            encryptedData.encryptedData
        );
        return new TextDecoder().decode(decryptedData);
    },

    async getKey(keyId) {
        return new Promise((resolve) => {
            indexedDB.open("CryptoDB").onsuccess = function(event) {
                const db = event.target.result;
                const tx = db.transaction("keys", "readonly");
                const request = tx.objectStore("keys").get(keyId);
                request.onsuccess = async function() {
                    resolve(await window.crypto.subtle.importKey(
                        "raw",
                        request.result,
                        { name: "AES-GCM" },
                        true,
                        ["encrypt", "decrypt"]
                    ));
                };
            };
        });
    }
};