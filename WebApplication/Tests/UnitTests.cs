﻿using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using NUnit.Framework;
using WebApplication.ResponseModel;
using WebApplication.Services;
using WebApplication.Services;
using WebApplication.Utilities;
using Decoder = WebApplication.Utilities.Decoder;


namespace WebApplication.Tests
{
    [TestFixture]
    public class UnitTests
    {
        
        MetaDataVideo _metaDataVideo = new MetaDataVideo();
        MetaDataPicture _metaDataPicture = new MetaDataPicture();
        LsbPicture _lsbPicture = new LsbPicture();
        LsbVideo _lsbVideo = new LsbVideo();
        LsbAudio _lsbAudio = new LsbAudio();
        HomeService _homeService = new HomeService();
        Decoder _decoder = new Decoder();
        DesAlgo _desAlgo = new DesAlgo();


        [Test]
        public void DesTest()
        {
            
            // Create a new TripleDESCryptoServiceProvider object
            // to generate a key and initialization vector (IV).
            TripleDESCryptoServiceProvider tDESalg = new TripleDESCryptoServiceProvider();
            tDESalg.KeySize = 128;
            tDESalg.Padding = PaddingMode.PKCS7;
            // Create a string to encrypt.
            string message = "Lebron James is the goat !!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!00000u00";

            // Encrypt the string to an in-memory buffer.
            byte[] Data = _desAlgo.EncryptStringToBytes_Des(message, tDESalg.Key, tDESalg.IV);

            // Decrypt the buffer back to a string.
            string plainText = _desAlgo.DecryptStringFromBytes_Des(Data, tDESalg.Key, tDESalg.IV);

            // Display the decrypted string to the console.
            var index = plainText.IndexOf("\u0000", StringComparison.Ordinal);
            if(index>0)
                plainText = plainText.Substring(0, index);
            Console.WriteLine(plainText);

            
        }
        
        
        [Test]
        public void JpegHide()
        {
            AesAlgo aesAlgo = new AesAlgo();
            using (AesManaged aes = new AesManaged())
            {
                aes.KeySize = 128;
                aes.Padding = PaddingMode.PKCS7;

                var path = @"C:/Users/mamis/Desktop/tif.tiff";
                var path2 = @"C:/Users/mamis/Desktop/picOutput.tiff";
                var jpeg = File.ReadAllBytes(path);

                string message = "Lebron james is the greatest player of all times" +
                                 "Lebron james is the greatest player of all times" +
                                 "Lebron james is the greatest player of all times" +
                                 "Lebron james is the greatest player of all times" +
                                 "Lebron james is the greatest player of all times" +
                                 "Lebron james is the greatest player of all times ";
                                  
                                  
                                 
                    

                byte[] encryptedData = aesAlgo.EncryptStringToBytes_Aes(message, aes.Key, aes.IV);
                encryptedData = encryptedData
                    .Concat(aes.Key)
                    .Concat(aes.IV).ToArray();

                jpeg =_metaDataPicture.HideJpeg(jpeg, encryptedData);
                byte[] cypherData = _metaDataPicture.SeekJpeg(jpeg);
                byte[] key = _metaDataPicture.ExtractKey(jpeg);
                byte[] iv = _metaDataPicture.ExtractIv(jpeg);

                var decryptedMessage = aesAlgo.DecryptStringFromBytes_Aes(cypherData, key, iv);

                Console.WriteLine("Secret Massage Is: \n" + decryptedMessage);
                File.WriteAllBytes(path2,jpeg);
            }
        }


        [Test]
        public void bmpHide()
        {
            AesAlgo aesAlgo = new AesAlgo();
            using (AesManaged aes = new AesManaged())
            {
                aes.KeySize = 128;
                aes.Padding = PaddingMode.PKCS7;
                
                var path = @"C:/Users/mamis/Desktop/pic.png";
                
                var bmp = (Bitmap) Image.FromFile(path);

                string message = "Lebron is the Goat  Lebron is the Goat  ";
                
                byte[] encryptedData = aesAlgo.EncryptStringToBytes_Aes(message, aes.Key, aes.IV).Concat(aes.Key).Concat(aes.IV).ToArray();

                var binMessage = _decoder.EncryptedByteArrayToBinary(encryptedData);
                
                _lsbPicture.Hide(bmp, binMessage);
                
                byte[] cypherData = _lsbPicture.Seek(bmp);
                byte[] key = _lsbPicture.ExtractKey(bmp);
                byte[] iv = _lsbPicture.ExtractIv(bmp);

                var decryptedMessage = aesAlgo.DecryptStringFromBytes_Aes(cypherData, key, iv);

                Console.WriteLine("Secret Massage Is: \n" + decryptedMessage);
                bmp.Save("C:/Users/mamis/Desktop/pica1.jpg", ImageFormat.Tiff);



            }
        }

        [Test]
        public void bmpSeek()
        {
            AesAlgo aesAlgo = new AesAlgo();
            var path1 = "C:/Users/mamis/Desktop/pica1.jpg";
            var bmp = (Bitmap) Image.FromFile(path1);
            byte[] cypherData = _lsbPicture.Seek(bmp);
            byte[] key = _lsbPicture.ExtractKey(bmp);
            byte[] iv = _lsbPicture.ExtractIv(bmp);

            var decryptedMessage = aesAlgo.DecryptStringFromBytes_Aes(cypherData, key, iv);

            Console.WriteLine("Secret Massage Is: \n" + decryptedMessage);
        }

        [Test]
        public void WavTestLsb()
        {
            AesAlgo aesAlgo = new AesAlgo();
            using (AesManaged aes = new AesManaged())
            {
                aes.KeySize = 128;
                aes.Padding = PaddingMode.PKCS7;
                var path = @"C:/Users/Mike/Desktop/pruducta/file_example_WAV_1MG - Copy.wav";
                //FileStream file = File.OpenRead(path);
                byte[] byteAudio = File.ReadAllBytes(path);
                string message = "blalalalalalalalalalalalalal !!!!!!!!!!!!1 @#%% secert message";
                byte[] encryptedMessage = aesAlgo.EncryptStringToBytes_Aes(message, aes.Key, aes.IV).Concat(aes.Key)
                    .Concat(aes.IV).ToArray();
                var binMessage = _decoder.EncryptedByteArrayToBinary(encryptedMessage);
                
                
                _lsbAudio.Hide(byteAudio, binMessage);
                
                byte[] cypherData = _lsbAudio.Seek(byteAudio);
                byte[] key = _lsbAudio.ExtractKey(byteAudio);
                byte[] iv = _lsbAudio.ExtractIv(byteAudio);

                var decryptedMessage = aesAlgo.DecryptStringFromBytes_Aes(cypherData, key, iv);

                Console.WriteLine("Secret Massage Is: \n" + decryptedMessage);
                File.WriteAllBytes("C:/Users/Mike/Desktop/pruducta/file_example_WAV_1MG - Copy.wav", byteAudio);

            }
        }

        [Test]
        public void AviTestLsb()
        {
            AesAlgo aesAlgo = new AesAlgo();
            using (AesManaged aes = new AesManaged())
            {
                aes.KeySize = 128;
                aes.Padding = PaddingMode.PKCS7;
                
                var path = @"C:/Users/mamis/Desktop/sample.avi";
                
                FileStream file = File.OpenRead(path);
                byte[] byteVideo = File.ReadAllBytes(path);

                string message = "Lebron is the goat, It is well known all the seven sees??  Lebron is the goat, It is well known all the seven sees?? Lebron is the goat, It is well known all the seven sees?? Lebron is the goat, It is well known all the seven sees??";
                
                byte[] encryptedData = aesAlgo.EncryptStringToBytes_Aes(message, aes.Key, aes.IV).Concat(aes.Key).Concat(aes.IV).ToArray();

                var binMessage = _decoder.EncryptedByteArrayToBinary(encryptedData);
                
                _lsbVideo.Hide(byteVideo, binMessage);
                
                byte[] cypherData = _lsbVideo.Seek(byteVideo);
                byte[] key = _lsbVideo.ExtractKey(byteVideo);
                byte[] iv = _lsbVideo.ExtractIv(byteVideo);

                var decryptedMessage = aesAlgo.DecryptStringFromBytes_Aes(cypherData, key, iv);

                Console.WriteLine("Secret Massage Is: \n" + decryptedMessage);
                File.WriteAllBytes("C:/Users/mamis/Desktop/outputTest.avi", byteVideo);



            }
        }
        
        [Test]
        public void MovTestLsb()
        {
            AesAlgo aesAlgo = new AesAlgo();
            using (AesManaged aes = new AesManaged())
            {
                aes.KeySize = 128;
                aes.Padding = PaddingMode.PKCS7;
                
                var path = @"C:/Users/mamis/Desktop/sample.mov";
                
                FileStream file = File.OpenRead(path);
                byte[] byteVideo = File.ReadAllBytes(path);

                string message = "Im the GOATTTT!!";
                
                byte[] encryptedData = aesAlgo.EncryptStringToBytes_Aes(message, aes.Key, aes.IV).Concat(aes.Key).Concat(aes.IV).ToArray();

                var binMessage = _decoder.EncryptedByteArrayToBinary(encryptedData);
                
                _lsbVideo.HideMov(byteVideo, binMessage);
                
                byte[] cypherData = _lsbVideo.SeekMov(byteVideo);
                byte[] key = _lsbVideo.ExtractKeyMov(byteVideo);
                byte[] iv = _lsbVideo.ExtractIvMov(byteVideo);

                var decryptedMessage = aesAlgo.DecryptStringFromBytes_Aes(cypherData, key, iv);

                Console.WriteLine("Secret Massage Is: \n" + decryptedMessage);
                File.WriteAllBytes("C:/Users/mamis/Desktop/outputTest.mov", byteVideo);



            }
        }
        
        [Test]
        public void MovTestMetaData()
        {
            AesAlgo aesAlgo = new AesAlgo();
            using (AesManaged aes = new AesManaged())
            {
                aes.KeySize = 128;
                aes.Padding = PaddingMode.PKCS7;
                
                var path = @"C:/Users/mamis/Desktop/vid.mov";
                
                
                byte[] byteVideo = File.ReadAllBytes(path);
                string message = "  the seven sees?? Lebron is the goat, It is well known all the seven sees." +
                                 "the seven sees?? Lebron is the goat, It is well known all the seven sees" +
                                 "the seven sees?? Lebron is the goat, It is well known all the seven sees" +
                                 "the seven sees?? Lebron is the goat, It is well known all the seven sees" +
                                 "the seven sees?? Lebron is the goat, It is well known all the seven sees??";

                byte[] encryptedData = aesAlgo.EncryptStringToBytes_Aes(message, aes.Key, aes.IV).Concat(aes.Key).Concat(aes.IV)
                    .ToArray();
                //byte[] b = encryptedData.Concat(aes.Key).ToArray();
                _metaDataVideo.HideMov(byteVideo, encryptedData);
                
                
                
                
                byte[] cypherData = _metaDataVideo.SeekMov(byteVideo);
                byte[] key = _metaDataVideo.ExtractKeyMov(byteVideo);
                byte[] iv = _metaDataVideo.ExtractIvMov(byteVideo);

                var decryptedMessage = aesAlgo.DecryptStringFromBytes_Aes(cypherData, aes.Key, aes.IV);

                Console.WriteLine("Secret Massage Is: \n" + decryptedMessage);
                File.WriteAllBytes("C:/Users/mamis/Desktop/movOutput.mov", byteVideo);



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
                
                var path = @"C:/Users/mamis/Desktop/sample.avi";
                
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
                _metaDataVideo.hide(byteVideo, encryptedData);
                
                
                
                
                byte[] cypherData = _metaDataVideo.Seek(byteVideo);
                byte[] key = _metaDataVideo.ExtractKey(byteVideo);
                byte[] iv = _metaDataVideo.ExtractIv(byteVideo);

                var decryptedMessage = aesAlgo.DecryptStringFromBytes_Aes(cypherData, aes.Key, aes.IV);

                Console.WriteLine("Secret Massage Is: \n" + decryptedMessage);
                File.WriteAllBytes("C:/Users/mamis/Desktop/vidOutput.avi", byteVideo);



            }
        }
    }
}