using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using MatrixAPI.Encryption;

namespace MatrixMaster.Encryption
{
    /// <summary>
    /// Encryption keys loaded from the filesystem.
    /// </summary>
    public class EncryptionKeyDB
    {
        //todo: Needs to have a logger.
        public static EncryptionKeyDB Instance;

        private string folderName;
        /// <summary>
        /// Keys, paired by hash -> md5
        /// </summary>
        private Dictionary<byte[], AES> keys = new Dictionary<byte[], AES>(); 

        /// <summary>
        /// Create a new key database in a location.
        /// </summary>
        public EncryptionKeyDB(string folderName)
        {
            Instance = this;
            this.folderName = folderName;
            //Load mek files (matrix encryption keys)
            if (!Directory.Exists(folderName)) Directory.CreateDirectory(folderName);
            var files = Directory.GetFiles(this.folderName, "*.mek");
            if(files.Length == 0)
            {
                //Generate a new key
                AES newKey = new AES(Encoding.UTF8);
                newKey.KeyToFile(this.folderName+"/DefaultKey.mek");
                File.WriteAllText(this.folderName+"/DefaultKey.txt", "There were no keys, so this default key was generated for you.");
                files = Directory.GetFiles(this.folderName, "*.mek");
            }
            foreach(var file in files)
            {
                //Hash the file to a all lowercase string
                using (var md5 = MD5.Create())
                {
                    using (var stream = File.OpenRead(file))
                    {
                        var hash = md5.ComputeHash(stream);
                        keys[hash] = new AES(Encoding.UTF8, file);
                    }
                }
            }
        }

        public Dictionary<byte[], AES> Keys
        {
            get { return keys; }
        }

        /// <summary>
        /// Get the encryptor by hash, or null if no entry.
        /// </summary>
        /// <param name="keymd5"></param>
        /// <returns></returns>
        public AES ByHash(byte[] keymd5)
        {
            return (from key in keys.Keys where key.SequenceEqual(keymd5) select keys[key]).FirstOrDefault();
        }
    }
}
