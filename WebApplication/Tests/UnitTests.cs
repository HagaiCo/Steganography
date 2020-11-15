using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Security.Cryptography;
using System.Text;
using NUnit.Framework;
using WebApplication.Utilities;

namespace WebApplication.Tests
{
    [TestFixture]
    public class UnitTests
    {
        HideAndSeek _hideAndSeek = new HideAndSeek();
        [Test]
        public void bmpHide()
        {
            AesAlgo aesAlgo = new AesAlgo();
            using (AesManaged aes = new AesManaged())
            {
                aes.KeySize = 128;
                aes.Padding = PaddingMode.PKCS7;
                
                var path = @"C:/Users/mamis/RiderProjects/ConsoleApp1/ConsoleApp1/assets/NewBmp.bmp";
                
                var bmp = (Bitmap) Image.FromFile(path);

                string message = "Lebron is the Goat ! Lebron is the Goat ! Lebron is the Goat ! Lebron is the Goat ! Lebron is the Goat ! Lebron is the Goat ! Lebron is the Goat ! Lebron is the Goat ! Lebron is the Goat ! Lebron is the Goat ! Lebron is the Goat ! Lebron is the Goat ! Lebron is the Goat !  Lebron is the Goat ! Lebron is the Goat ! Lebron is the Goat ! Lebron is the Goat ! Lebron is the Goat ! Lebron is the Goat ! Lebron is the Goat ! ";
                
                byte[] encryptedData = aesAlgo.EncryptStringToBytes_Aes(message, aes.Key, aes.IV);

                var binMessage = _hideAndSeek.EncryptedDataToBin(encryptedData, aes.Key, aes.IV);

                _hideAndSeek.Clean(bmp, binMessage.Length);
                _hideAndSeek.Hide(bmp, binMessage);
                
                bmp.Save("C:/Users/mamis/Desktop/newBmp.Bmp", ImageFormat.Bmp);
               
               
                
            }
        }

        [Test]
        public void bmpSeek()
        {
            AesAlgo aesAlgo = new AesAlgo();
            var path1 = "C:/Users/mamis/Desktop/newBmp.Bmp";
            var bmp = (Bitmap) Image.FromFile(path1);
            byte[] cypherData = _hideAndSeek.Seek(bmp);
            byte[] key = _hideAndSeek.ExtractKey(bmp);
            byte[] iv = _hideAndSeek.ExtractIv(bmp);

            var decryptedMessage = aesAlgo.DecryptStringFromBytes_Aes(cypherData, key, iv);

            Console.WriteLine("Secret Massage Is: \n" + decryptedMessage);
        }
    }
}