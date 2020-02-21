using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace Cnf.CodeBase.Secure
{
    /// <summary>
    /// 用于加解密及MD5摘要处理的帮助类
    /// </summary>
    public static class CryptoHelper
    {
        /// <summary>
        /// 计算消息data的MD5摘要
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public static string CreateMD5Digest(string data)
        {
            if (string.IsNullOrEmpty(data))
                return string.Empty;

            using (MD5 md5 = MD5.Create())
            {
                byte[] inputValue = Encoding.UTF8.GetBytes(data);
                var result = md5.ComputeHash(inputValue);
                return (result == null) ? string.Empty : BitConverter.ToString(result);
            }
        }

        /// <summary>
        /// 使用plainText的MD5摘要作为key进行AES加密
        /// </summary>
        /// <param name="plainText"></param>
        /// <returns></returns>
        public static string CreateCredential(string plainText)
        {
            using (MD5 md5 = MD5.Create())
            {
                byte[] key = md5.ComputeHash(Encoding.UTF8.GetBytes(plainText));
                byte[] iv = Encoding.UTF8.GetBytes("1234567890123456");

                return AESEncrypt(plainText, key, iv);
            }
        }

        /// <summary>
        /// 检查文本和证书是否一致
        /// </summary>
        /// <param name="plainText"></param>
        /// <param name="credential"></param>
        /// <returns></returns>
        public static bool VerifyCredential(string plainText, string credential)
        {
            return (credential.Equals(CreateCredential(plainText), StringComparison.Ordinal));
        }

        /// <summary>
        /// 生成一个用于AES加密的随机128位密钥和初始向量
        /// </summary>
        public static void GenerateAesKeyAndIV(out byte[] key, out byte[] iv)
        {
            using (AesManaged aes = new AesManaged())
            {
                key = aes.Key;
                iv = aes.IV;
            }
        }

        /// <summary>
        /// 通过AES算法使用密钥key加密明文plaintext, key是一个byte[16]字节数组（128位）, iv是初始向量
        /// </summary>
        public static string AESEncrypt(string plaintext, byte[] key, byte[] iv)
        {
            byte[] cryptoBuffer = EncryptStringToBytes_Aes(plaintext, key, iv);
            return Convert.ToBase64String(cryptoBuffer);
        }

        /// <summary>
        /// 通过AES算法使用密钥key解密ciphertext, key是一个byte[16]字节数组（128位）, iv是初始向量
        /// </summary>
        public static string AESDecrypt(string ciphertext, byte[] key, byte[] iv)
        {
            byte[] cryptoBuffer = Convert.FromBase64String(ciphertext);

            return DecryptStringFromBytes_Aes(cryptoBuffer, key, iv);
        }

        static byte[] EncryptStringToBytes_Aes(string plainText, byte[] Key, byte[] IV)
        {
            // Check arguments.
            if (plainText == null || plainText.Length <= 0)
                throw new ArgumentNullException("plainText");
            if (Key == null || Key.Length <= 0)
                throw new ArgumentNullException("Key");
            if (IV == null || IV.Length <= 0)
                throw new ArgumentNullException("IV");
            byte[] encrypted;

            // Create an AesManaged object
            // with the specified key and IV.
            using (AesManaged aesAlg = new AesManaged())
            {
                aesAlg.Key = Key;
                aesAlg.IV = IV;

                // Create an encryptor to perform the stream transform.
                ICryptoTransform encryptor = aesAlg.CreateEncryptor(aesAlg.Key, aesAlg.IV);

                // Create the streams used for encryption.
                using (MemoryStream msEncrypt = new MemoryStream())
                {
                    using (CryptoStream csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write))
                    {
                        using (StreamWriter swEncrypt = new StreamWriter(csEncrypt))
                        {
                            //Write all data to the stream.
                            swEncrypt.Write(plainText);
                        }
                        encrypted = msEncrypt.ToArray();
                    }
                }
            }


            // Return the encrypted bytes from the memory stream.
            return encrypted;

        }

        static string DecryptStringFromBytes_Aes(byte[] cipherText, byte[] Key, byte[] IV)
        {
            // Check arguments.
            if (cipherText == null || cipherText.Length <= 0)
                throw new ArgumentNullException("cipherText");
            if (Key == null || Key.Length <= 0)
                throw new ArgumentNullException("Key");
            if (IV == null || IV.Length <= 0)
                throw new ArgumentNullException("IV");

            // Declare the string used to hold
            // the decrypted text.
            string plaintext = null;

            // Create an AesManaged object
            // with the specified key and IV.
            using (AesManaged aesAlg = new AesManaged())
            {
                aesAlg.Key = Key;
                aesAlg.IV = IV;

                // Create a decryptor to perform the stream transform.
                ICryptoTransform decryptor = aesAlg.CreateDecryptor(aesAlg.Key, aesAlg.IV);

                // Create the streams used for decryption.
                using (MemoryStream msDecrypt = new MemoryStream(cipherText))
                {
                    using (CryptoStream csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read))
                    {
                        using (StreamReader srDecrypt = new StreamReader(csDecrypt))
                        {
                            // Read the decrypted bytes from the decrypting stream
                            // and place them in a string.
                            plaintext = srDecrypt.ReadToEnd();
                        }
                    }
                }
            }

            return plaintext;
        }
    }
}
