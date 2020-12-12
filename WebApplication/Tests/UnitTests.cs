using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using NUnit.Framework;
using WebApplication.Utilities;

namespace WebApplication.Tests
{
    [TestFixture]
    public class UnitTests
    {
        HideAndSeekLsb _hideAndSeekLsb = new HideAndSeekLsb();
        HideAndSeekMetaData _hideAndSeekMetaData=new HideAndSeekMetaData();
        [Test]
        public void bmpHide()
        {
            AesAlgo aesAlgo = new AesAlgo();
            using (AesManaged aes = new AesManaged())
            {
                aes.KeySize = 128;
                aes.Padding = PaddingMode.PKCS7;
                
                var path = @"C:/Users/mamis/Desktop/pica.jpg";
                
                var bmp = (Bitmap) Image.FromFile(path);

                string message = "Is this Working??";
                
                byte[] encryptedData = aesAlgo.EncryptStringToBytes_Aes(message, aes.Key, aes.IV);

                var binMessage = _hideAndSeekLsb.EncryptedDataToBin(encryptedData, aes.Key, aes.IV);
                
                _hideAndSeekLsb.Hide(bmp, binMessage);
                
                byte[] cypherData = _hideAndSeekLsb.Seek(bmp);
                byte[] key = _hideAndSeekLsb.ExtractKey(bmp);
                byte[] iv = _hideAndSeekLsb.ExtractIv(bmp);

                var decryptedMessage = aesAlgo.DecryptStringFromBytes_Aes(cypherData, key, iv);

                //Console.WriteLine("Secret Massage Is: \n" + decryptedMessage);
                bmp.Save("C:/Users/mamis/Desktop/pica1.jpg", ImageFormat.Tiff);



            }
        }

        [Test]
        public void bmpSeek()
        {
            AesAlgo aesAlgo = new AesAlgo();
            var path1 = "C:/Users/mamis/Desktop/pica1.jpg";
            var bmp = (Bitmap) Image.FromFile(path1);
            byte[] cypherData = _hideAndSeekLsb.Seek(bmp);
            byte[] key = _hideAndSeekLsb.ExtractKey(bmp);
            byte[] iv = _hideAndSeekLsb.ExtractIv(bmp);

            var decryptedMessage = aesAlgo.DecryptStringFromBytes_Aes(cypherData, key, iv);

            Console.WriteLine("Secret Massage Is: \n" + decryptedMessage);
        }

        [Test]
        public void AviTestLsb()
        {
            AesAlgo aesAlgo = new AesAlgo();
            using (AesManaged aes = new AesManaged())
            {
                aes.KeySize = 128;
                aes.Padding = PaddingMode.PKCS7;
                
                var path = @"C:/Users/mamis/Desktop/1280.avi";
                
                FileStream file = File.OpenRead(path);
                byte[] byteVideo = File.ReadAllBytes(path);

                string message = "Lebron is the goat, It is well known all the seven sees??  Lebron is the goat, It is well known all the seven sees?? Lebron is the goat, It is well known all the seven sees?? Lebron is the goat, It is well known all the seven sees??";
                
                byte[] encryptedData = aesAlgo.EncryptStringToBytes_Aes(message, aes.Key, aes.IV);

                var binMessage = _hideAndSeekLsb.EncryptedDataToBin(encryptedData, aes.Key, aes.IV);
                
                _hideAndSeekLsb.Hide(byteVideo, binMessage);
                
                byte[] cypherData = _hideAndSeekLsb.Seek(byteVideo);
                byte[] key = _hideAndSeekLsb.ExtractKey(byteVideo);
                byte[] iv = _hideAndSeekLsb.ExtractIv(byteVideo);

                var decryptedMessage = aesAlgo.DecryptStringFromBytes_Aes(cypherData, key, iv);

                Console.WriteLine("Secret Massage Is: \n" + decryptedMessage);
                File.WriteAllBytes("C:/Users/mamis/Desktop/outputTest.avi", byteVideo);



            }
        }
        
        [Test]
        public void AviTestMetaData()
        {
            AesAlgo aesAlgo = new AesAlgo();
            using (AesManaged aes = new AesManaged())
            {
                aes.KeySize = 128;
                aes.Padding = PaddingMode.PKCS7;
                
                var path = @"C:/Users/mamis/Desktop/1280.avi";
                
                FileStream file = File.OpenRead(path);
                byte[] byteVideo = File.ReadAllBytes(path);
                string message = "  the seven sees?? Lebron is the goat, It is well known all the seven sees." +
                                 "the seven sees?? Lebron is the goat, It is well known all the seven sees" +
                                 "the seven sees?? Lebron is the goat, It is well known all the seven sees" +
                                 "the seven sees?? Lebron is the goat, It is well known all the seven sees" +
                                 "the seven sees?? Lebron is the goat, It is well known all the seven sees??";

                byte[] encryptedData = aesAlgo.EncryptStringToBytes_Aes(message, aes.Key, aes.IV).Concat(aes.Key).Concat(aes.IV)
                    .ToArray();
                //byte[] b = encryptedData.Concat(aes.Key).ToArray();
                _hideAndSeekMetaData.hide(byteVideo, encryptedData);
                
                
                
                
                byte[] cypherData = _hideAndSeekMetaData.Seek(byteVideo);
                byte[] key = _hideAndSeekMetaData.ExtractKey(byteVideo);
                byte[] iv = _hideAndSeekMetaData.ExtractIv(byteVideo);

                var decryptedMessage = aesAlgo.DecryptStringFromBytes_Aes(cypherData, aes.Key, aes.IV);

                Console.WriteLine("Secret Massage Is: \n" + decryptedMessage);
                File.WriteAllBytes("C:/Users/mamis/Desktop/vidOutput.avi", byteVideo);



            }
        }
    }
}