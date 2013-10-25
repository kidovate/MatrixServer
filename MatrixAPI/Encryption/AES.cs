using System;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Engines;
using Org.BouncyCastle.Crypto.Modes;
using Org.BouncyCastle.Crypto.Parameters;

namespace MatrixAPI.Encryption
{
    public class AES
    {
        private readonly Encoding encoding;

        private SicBlockCipher mode;

        private byte[] key = new byte[32];
        private byte[] iv = new byte[32];

        /// <summary>
        /// Instance of AES with pre-defined key and IV.
        /// </summary>
        /// <param name="encoding"></param>
        /// <param name="key"></param>
        /// <param name="iv"></param>
        public AES(Encoding encoding, byte[] key, byte[] iv)
        {
            this.encoding = encoding;
            this.mode = new SicBlockCipher(new AesFastEngine());
            this.key = key;
            this.iv = iv;
        }

        /// <summary>
        /// Load the key and IV from a file.
        /// </summary>
        /// <param name="encoding"></param>
        /// <param name="encryptionFile"></param>
        public AES(Encoding encoding, string encryptionFile)
        {
            this.mode = new SicBlockCipher(new AesFastEngine());
            byte[] combinedKey = new byte[32 * 2];
            combinedKey = File.ReadAllBytes(encryptionFile);
            combinedKey.Take(32).ToArray().CopyTo(key, 0);
            combinedKey.Skip(32).Take(32).ToArray().CopyTo(iv, 0);
            this.encoding = encoding;
        }

        /// <summary>
        /// Generate a new key and IV.
        /// </summary>
        /// <param name="encoding"></param>
        public AES(Encoding encoding)
        {
            this.mode = new SicBlockCipher(new AesFastEngine());
            RNGCryptoServiceProvider rngCsp = new RNGCryptoServiceProvider();
            // Generate random key and IV
            rngCsp.GetBytes(key);
            rngCsp.GetBytes(iv);
            this.encoding = encoding;
        }

        /// <summary>
        /// Save the key and IV to a file.
        /// </summary>
        /// <param name="filePath"></param>
        public void KeyToFile(string filePath)
        {
            byte[] combinedKey = new byte[key.Length + iv.Length];
            key.CopyTo(combinedKey, 0);
            iv.CopyTo(combinedKey, key.Length);
            File.WriteAllBytes(filePath, combinedKey);
        }

        public static string ByteArrayToString(byte[] bytes)
        {
            return BitConverter.ToString(bytes).Replace("-", string.Empty);
        }

        public static byte[] StringToByteArray(string hex)
        {
            int numberChars = hex.Length;
            byte[] bytes = new byte[numberChars/2];

            for (int i = 0; i < numberChars; i += 2)
            {
                bytes[i/2] = Convert.ToByte(hex.Substring(i, 2), 16);
            }

            return bytes;
        }


        public string Encrypt(string plain)
        {
            byte[] input = this.encoding.GetBytes(plain);

            byte[] bytes = this.BouncyCastleCrypto(true, input);

            string result = ByteArrayToString(bytes);

            return result;
        }


        public string Decrypt(string cipher)
        {
            byte[] bytes = this.BouncyCastleCrypto(false, StringToByteArray(cipher));

            string result = this.encoding.GetString(bytes);

            return result;
        }


        private byte[] BouncyCastleCrypto(bool forEncrypt, byte[] input)
        {
            try
            {
                this.mode.Init(forEncrypt, new ParametersWithIV(new KeyParameter(key), iv));

                BufferedBlockCipher cipher = new BufferedBlockCipher(this.mode);

                return cipher.DoFinal(input);
            }
            catch (CryptoException)
            {
                throw;
            }
        }
    }
}