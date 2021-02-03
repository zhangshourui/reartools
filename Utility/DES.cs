using System;
using System.Text;
using System.Security.Cryptography;
using System.IO;

namespace Utility
{
	public class DES : IDisposable
	{
		private static Log5 log = new Log5();
		private System.Security.Cryptography.SymmetricAlgorithm des = null;
		private ICryptoTransform decryptor = null;
		private ICryptoTransform encryptor = null;

		public byte[] Key { get; set; }
		public byte[] IV { get; set; }
		public Encoding TextEncode = Encoding.UTF8;

		public DES(string key, string iv)
		{
			if (key.Length > 8)
				key = key.Substring(0, 8);
			if (iv.Length > 8)
				iv = iv.Substring(0, 8);
			this.Key = this.TextEncode.GetBytes(key);
			this.IV = this.TextEncode.GetBytes(iv);

			try
			{
				des = new System.Security.Cryptography.DESCryptoServiceProvider();
				des.Mode = CipherMode.ECB;
				des.Key = Key;
				des.IV = IV;
				decryptor = des.CreateDecryptor(this.Key, this.IV);
				encryptor = des.CreateEncryptor(this.Key, this.IV);
			}
			catch (System.Exception ex)
			{
				log.Error(ex.ToString());
			}

		}
		public byte[] Encrypt(byte[] srcBuffer)
		{
			try
			{
				byte[] buffer = srcBuffer;// srcStream.GetBuffer();
				int bufferlen = buffer.Length;
				MemoryStream mStream = new MemoryStream();
				CryptoStream cStream = new CryptoStream(mStream, encryptor, CryptoStreamMode.Write);

				cStream.Write(buffer, 0, bufferlen);
				cStream.FlushFinalBlock();

				byte[] result = mStream.ToArray();
				cStream.Close();
				des.Clear();
				return result;

			}
			catch (System.Exception ex)
			{
#if DEBUG
				throw ex;
#else
				log.Error(ex.ToString());
				return null;
#endif
			}
		}

		public string Encrypt(string source, Encoding encoding)
		{
			byte[] srcBuffer = encoding.GetBytes(source);
			byte[] result = Encrypt(srcBuffer);
			if (result != null)
				return Convert.ToBase64String(result);
			else
				return null;

		}

		public byte[] Decrypt(byte[] srcBuffer)
		{
			try
			{
				MemoryStream mStream = new MemoryStream();
				CryptoStream cStream = new CryptoStream(mStream, decryptor, CryptoStreamMode.Write);
				cStream.Write(srcBuffer, 0, srcBuffer.Length);
				cStream.FlushFinalBlock();
				byte[] result = mStream.ToArray();
				mStream.Close();
				des.Clear();

				return result;
			}
			catch (System.Exception ex)
			{
#if DEBUG
				throw ex;
#else
				log.Error(ex.ToString());
				return null;
#endif
			}

		}

		public string Decrypt(string sourceBase64, Encoding encoding)
		{
			try
			{
				byte[] srcBuffer = Convert.FromBase64String(sourceBase64);

				byte[] result = Decrypt(srcBuffer);
				if (result != null)
					return encoding.GetString(result);
				else
					return null;
			}
			catch (System.Exception ex)
			{
#if DEBUG
				throw ex;
#else
				log.Error(ex.ToString());
				return null;
#endif

			}

		}



		public void Dispose()
		{
			decryptor.Dispose();
			encryptor.Dispose();
			des.Dispose();
		}
	}
}
