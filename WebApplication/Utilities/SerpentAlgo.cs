using System.Text;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Engines;
using Org.BouncyCastle.Crypto.Paddings;
using Org.BouncyCastle.Crypto.Parameters;

namespace WebApplication.Utilities
{
    public class SerpentAlgo
    {
        private readonly Encoding _encoding;
        private readonly IBlockCipher _blockCipher;
        private PaddedBufferedBlockCipher _cipher;
        private IBlockCipherPadding _padding;

        public SerpentAlgo(IBlockCipher blockCipher, Encoding encoding)
        {
            _blockCipher = blockCipher;
            _encoding = encoding;
        }

        public void SetPadding(IBlockCipherPadding padding)
        {
            if (padding != null)
                _padding = padding;
        }

        public byte[] Encrypt(string plain, byte[] key)
        {
            byte[] result = BouncyCastleCrypto(true, ASCIIEncoding.ASCII.GetBytes(plain), key);
            //return Convert.ToBase64String(result);
            return result;
        }

        public string Decrypt(byte[] cipher, byte[] key)
        {
            byte[] result = BouncyCastleCrypto(false, cipher, key);
            return ASCIIEncoding.ASCII.GetString(result);
        }

        private byte[] BouncyCastleCrypto(bool forEncrypt, byte[] input, byte[] key)
        {
            try
            {
                _cipher = _padding == null
                    ? new PaddedBufferedBlockCipher(_blockCipher)
                    : new PaddedBufferedBlockCipher(_blockCipher, _padding);
                //byte[] keyByte = _encoding.GetBytes(key);
                _cipher.Init(forEncrypt, new KeyParameter(key));
                return _cipher.DoFinal(input);
            }
            catch (Org.BouncyCastle.Crypto.CryptoException ex)
            {
                throw new CryptoException(ex.Message);
            }
        }
        
        public static byte[] SerpentEncryption(string plain, byte[] key)
        {
            SerpentAlgo bcEngine = new SerpentAlgo(new SerpentEngine(), ASCIIEncoding.ASCII);
            bcEngine.SetPadding(new Pkcs7Padding());
            return bcEngine.Encrypt(plain, key);
        }

        public static string SerpentDecryption(byte[] cipher, byte[] key)
        {
            SerpentAlgo bcEngine = new SerpentAlgo(new SerpentEngine(), ASCIIEncoding.ASCII);
            bcEngine.SetPadding(new Pkcs7Padding());
            return bcEngine.Decrypt(cipher, key);
        }
    }
}