using System;
using System.IO;
using MatrixAPI.Encryption;
using System.Collections.Generic;
using System.Text;
using System.Security.Cryptography;
using System.Linq;

namespace MMOController
{
	/// <summary>
	/// Loads encryption keys from the filesystem
	/// </summary>
	public static class MmoEncrypt
	{

		private static Dictionary<byte[], AES> keys = new Dictionary<byte[], AES>(); 
		private static string folderName = "ClientKeys";

		/// <summary>
		/// All encryption keys loaded from filesystem.
		/// </summary>
		/// <value>The keys.</value>
		public static Dictionary<byte[], AES> Keys {
			get{
				return keys;
			}
		}

		static MmoEncrypt(){
			IndexKeys();
		}

		/// <summary>
		/// This is automatically called when you first access Keys.
		/// </summary>
		public static void IndexKeys(){
			keys.Clear();
			if(!Directory.Exists(folderName)) Directory.CreateDirectory(folderName);

			var files = Directory.GetFiles(folderName, "*.mek");
			if(files.Length == 0)
			{
				//Generate a new key
				AES newKey = new AES(Encoding.UTF8);
				newKey.KeyToFile(folderName+"/EncryptionKey.mek");
				File.WriteAllText(folderName+"/EncryptionKey.txt", "There were no keys, so this default key was generated for you. Note this key will be erased the next time nodes are synced and this is NOT the proper way to add client keys.");
				files = Directory.GetFiles(folderName, "*.mek");
			}
			foreach(var file in files)
			{
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

		/// <summary>
		/// Get an encryption pair by md5 hash.
		/// </summary>
		/// <returns>The hash.</returns>
		/// <param name="keymd5">Keymd5.</param>
		public static AES ByHash(byte[] keymd5)
		{
			return (from key in keys.Keys where key.SequenceEqual(keymd5) select keys[key]).FirstOrDefault();
		}
	}
}

