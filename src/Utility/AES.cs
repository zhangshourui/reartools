using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace Utility
{
    public class AES : IDisposable
    {
        private static Log5 log = new Log5();
        private System.Security.Cryptography.SymmetricAlgorithm aes = null;
        private ICryptoTransform decryptor = null;
        private ICryptoTransform encryptor = null;

        public byte[] Key { get; set; }
        public byte[] IV { get; set; }
        public Encoding TextEncode = Encoding.UTF8;

        public AES(string key, string iv)
        {
            this.Key = this.TextEncode.GetBytes(key);
            this.IV = this.TextEncode.GetBytes(iv);

            //System.Security.Cryptography.Aes aes = new System.Security.Cryptography.AesCryptoServiceProvider();
            //aes = new System.Security.Cryptography.AesCryptoServiceProvider();
            aes = new System.Security.Cryptography.RijndaelManaged();
            aes.Key = Key;
            aes.IV = IV;
            aes.KeySize = 128;
            aes.Mode = CipherMode.CBC;
            aes.BlockSize = 128;
            aes.Padding = PaddingMode.Zeros;

            try
            {
                decryptor = aes.CreateDecryptor(this.Key, this.IV);
                encryptor = aes.CreateEncryptor(this.Key, this.IV);

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

                lock (this)
                {
                    byte[] result = encryptor.TransformFinalBlock(srcBuffer, 0, bufferlen);

                    aes.Clear();


                    return result;
                }
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
            {
                return Convert.ToBase64String(result);
            }
            else
                return null;

        }

        public byte[] Decrypt(byte[] srcBuffer)
        {
            try
            {
                MemoryStream mStream = new MemoryStream();
                lock (this)
                {
                    int bufferlen = srcBuffer.Length;
                    byte[] result = decryptor.TransformFinalBlock(srcBuffer, 0, bufferlen);
                    //decryptor.TransformFinalBlock()
                    aes.Clear();

                    return result;
                }
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
                log.Error(ex.ToString());
#if DEBUG
				throw ex;
#else
                return null;
#endif

            }

        }

        public void Dispose()
        {
            decryptor.Dispose();
            encryptor.Dispose();
            aes.Dispose();
        }
    }
}
