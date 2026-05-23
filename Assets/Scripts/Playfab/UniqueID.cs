using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using UnityEngine;

public class UniqueID 
{
    private string pw = "fiu45lwaefi032";
    public string ID { get; private set; }
    public UniqueID()
    {
       // Debug.Log("current ID is:" + PlayerPrefs.GetString("uniqueID", string.Empty));
        ID = PlayerPrefs.GetString("uniqueID", string.Empty);
        
        if (string.IsNullOrEmpty(ID))
        {
            ID = EncryptString(System.Guid.NewGuid().ToString(), pw);
            //Debug.Log("encrypted Id is:" + ID);
            PlayerPrefs.SetString("uniqueID", ID);
            PlayerPrefs.Save();
        }

        ID = DecryptString(ID, pw);
       // Debug.Log("decrypted Id is:" + ID);
    }

    static string EncryptString(string plaintext, string password)
    {
        byte[] salt = GenerateRandomSalt(16); // Generate a new salt for each encryption
        using (var passwordBytes = new Rfc2898DeriveBytes(password, salt, 10000))
        {
            byte[] key = passwordBytes.GetBytes(32);
            byte[] iv = passwordBytes.GetBytes(16);

            using (Aes encryptor = Aes.Create())
            {
                encryptor.Key = key;
                encryptor.IV = iv;
                using (var ms = new MemoryStream())
                {
                    // Write the salt at the beginning of the stream
                    ms.Write(salt, 0, salt.Length);

                    using (var cs = new CryptoStream(ms, encryptor.CreateEncryptor(), CryptoStreamMode.Write))
                    {
                        byte[] plaintextBytes = Encoding.UTF8.GetBytes(plaintext);
                        cs.Write(plaintextBytes, 0, plaintextBytes.Length);
                        cs.FlushFinalBlock();
                    }

                    return Convert.ToBase64String(ms.ToArray());
                }
            }
        }
    }

    static string DecryptString(string encrypted, string password)
    {
        byte[] encryptedBytes = Convert.FromBase64String(encrypted);

        using (var ms = new MemoryStream(encryptedBytes))
        {
            byte[] salt = new byte[16];
            ms.Read(salt, 0, salt.Length); // Extract the salt from the beginning of the stream

            using (var passwordBytes = new Rfc2898DeriveBytes(password, salt, 10000))
            {
                byte[] key = passwordBytes.GetBytes(32);
                byte[] iv = passwordBytes.GetBytes(16);

                using (Aes decryptor = Aes.Create())
                {
                    decryptor.Key = key;
                    decryptor.IV = iv;

                    using (var cs = new CryptoStream(ms, decryptor.CreateDecryptor(), CryptoStreamMode.Read))
                    {
                        using (var outputMs = new MemoryStream())
                        {
                            cs.CopyTo(outputMs);
                            return Encoding.UTF8.GetString(outputMs.ToArray());
                        }
                    }
                }
            }
        }
    }

    static byte[] GenerateRandomSalt(int size)
    {
        byte[] salt = new byte[size];
        using (var rng = RandomNumberGenerator.Create())
        {
            rng.GetBytes(salt);
        }
        return salt;
    }
}
