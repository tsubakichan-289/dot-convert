module AES

open System
open System.IO
open System.Security.Cryptography
open System.Text

// Static IV used for AES-CBC operations.
let iv : byte[] 
    = [| 0xb8uy; 0xf9uy; 0x0euy; 0x78uy
         0x6euy; 0xccuy; 0xbbuy; 0xdfuy
         0x27uy; 0x39uy; 0x7euy; 0x01uy
         0xf5uy; 0xbduy; 0x30uy; 0x13uy |]

// Derive a fixed-length AES key from a string.
let private deriveKey : string -> byte[]
    = fun key ->
        use sha = SHA256.Create()
        Encoding.UTF8.GetBytes(key) |> sha.ComputeHash

// Encrypt a UTF-8 message using the derived key.
let encryptMessage : string -> string -> byte[]
    = fun message key ->
        let encryptKey = deriveKey key
        use aes = Aes.Create()
        aes.Key <- encryptKey
        aes.IV <- iv
        aes.Mode <- CipherMode.CBC
        aes.Padding <- PaddingMode.PKCS7

        use encryptor = aes.CreateEncryptor(aes.Key, aes.IV)
        let messageBytes = Encoding.UTF8.GetBytes(message)

        use ms = new MemoryStream()
        use cs = new CryptoStream(ms, encryptor, CryptoStreamMode.Write)
        cs.Write(messageBytes, 0, messageBytes.Length)
        cs.FlushFinalBlock()

        ms.ToArray()

// Decrypt a payload to UTF-8 using the derived key.
let decryptMessage : byte[] -> string -> string
    = fun encryptedData key ->
        let decryptKey = deriveKey key
        use aes = Aes.Create()
        aes.Key <- decryptKey
        aes.IV <- iv
        aes.Mode <- CipherMode.CBC
        aes.Padding <- PaddingMode.PKCS7

        use decryptor = aes.CreateDecryptor(aes.Key, aes.IV)

        use ms = new MemoryStream(encryptedData)
        use cs = new CryptoStream(ms, decryptor, CryptoStreamMode.Read)
        use sr = new StreamReader(cs, Encoding.UTF8)
        sr.ReadToEnd()
