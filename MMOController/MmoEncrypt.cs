using System;
using System.IO;
using MatrixAPI.Encryption;
using System.Collections.Generic;

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
			public get{
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
				newKey.KeyToFile(folderName+"/GeneratedKey.mek");
				File.WriteAllText(folderName+"/GeneratedKey.txt", "There were no keys, so this default key was generated for you. Note this key will be erased the next time nodes are synced and this is NOT the proper way to add client keys.");
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
	}
}

