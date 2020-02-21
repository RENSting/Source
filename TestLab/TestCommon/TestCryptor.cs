using System;
using System.Collections.Generic;
using System.Text;
using Cnf.CodeBase.Secure;

namespace TestLab.TestCommon
{
    class TestCryptor
    {
        byte[] _key, _iv;
        string _password = "sting1973Sting163helloworld,&#$Iamabigboss,hellokitty,BigBen%&^123456197310260";

        public TestCryptor()
        {
            CryptoHelper.GenerateAesKeyAndIV(out _key, out _iv);
        }

        public void Start()
        {
            if(CreateDigest())
            {
                Console.WriteLine($"生成MD5摘要的函数运行正确");
            }

            string ciphertext = Encrypt();
            Console.WriteLine($"AES ciphertext is: {ciphertext}");

            string plaintext = Decrypt(ciphertext);
            Console.WriteLine($"AES plain text is: {plaintext}");

            string credential = GetCredential();
            Console.WriteLine($"经过自加密后的证书是：{credential}");

            if (CryptoHelper.VerifyCredential(_password, credential))
                Console.WriteLine("自加密证书功能正常");

            if(plaintext.Equals("Hello world!", StringComparison.OrdinalIgnoreCase))
            {
                Console.WriteLine("AES加密解密函数运行正确");
            }
            else
            {
                Console.WriteLine("AES加密后解密没有得到预期的效果");
            }
        }

        string GetCredential()
        {
            return CryptoHelper.CreateCredential(_password);
        }

        bool CreateDigest()
        {
            string srcData = "Hello world!";
            string md5 = CryptoHelper.CreateMD5Digest(srcData);
            Console.WriteLine(md5);
            return true;
        }

        string Encrypt()
        {
            string plain = "Hello world!";
            return CryptoHelper.AESEncrypt(plain, _key, _iv);
        }

        string Decrypt(string ciphertext)
        {
            return CryptoHelper.AESDecrypt(ciphertext, _key, _iv);
        }
    }
}
