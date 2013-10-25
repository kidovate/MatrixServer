using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using MatrixAPI.Encryption;

namespace EncryptionTest
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.Write("Enter your input string: ");
            string inputString = Console.ReadLine();

            AES aes;
            if (File.Exists("EncryptionKey"))
            {
                aes = new AES(Encoding.UTF8, "EncryptionKey");
                Console.WriteLine("Key loaded from file.");
            }
            else
            {
                aes = new AES(Encoding.UTF8);
                aes.KeyToFile("EncryptionKey");
                Console.WriteLine("Key generated and saved to file.");
            }
            //AES
            Console.WriteLine("\n===AES===");
            string aesResult = aes.Encrypt(inputString);
            Console.WriteLine(aesResult);
            Console.WriteLine("\n==Decryp==");
            Console.WriteLine(aes.Decrypt(aesResult));
            Console.ReadLine();
        }
    }
}
