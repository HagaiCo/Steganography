using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Xml.Linq;
using cs_ffmpeg_mp3_converter;
using FFMpegCore;
using FFMpegCore.Pipes;
using NAudio.Lame;
using NAudio.Wave;
using NReco.VideoConverter;
using NUnit.Framework;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Security;
using RestSharp.Extensions;
using WebApplication.ResponseModel;
using WebApplication.Services;
using WebApplication.Services;
using WebApplication.Utilities;
using Xabe.FFmpeg;
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
        LsbExe _lsbPe = new LsbExe();
        HomeService _homeService = new HomeService();
        Decoder _decoder = new Decoder();
        
        MetaDataAudio _metaDataAudio = new MetaDataAudio();


        [Test]
        public void SerpentTest()
        {
            
            CipherKeyGenerator cipherKeyGenerator = new CipherKeyGenerator();
            cipherKeyGenerator.Init(new KeyGenerationParameters(new SecureRandom(),128 ));
            byte[] key = cipherKeyGenerator.GenerateKey();
            string message = "Hello World!";

            // Encrypt the string to an in-memory buffer.
            
            byte[] encrptedDAta = SerpentAlgo.SerpentEncryption(message, key);

            // Decrypt the buffer back to a string.
            string plainText = SerpentAlgo.SerpentDecryption(encrptedDAta, key);

            // Display the decrypted string to the console.
            
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
                byte[] key = _metaDataPicture.ExtractKeyJpeg(jpeg);
                byte[] iv = _metaDataPicture.ExtractIvJpeg(jpeg);

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
                
                _lsbPicture.HideBitmap(bmp, binMessage);
                
                byte[] cypherData = _lsbPicture.SeekBitmap(bmp);
                byte[] key = _lsbPicture.ExtractKeyBitmap(bmp);
                byte[] iv = _lsbPicture.ExtractIvBitmap(bmp);

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
            byte[] cypherData = _lsbPicture.SeekBitmap(bmp);
            byte[] key = _lsbPicture.ExtractKeyBitmap(bmp);
            byte[] iv = _lsbPicture.ExtractIvBitmap(bmp);

            var decryptedMessage = aesAlgo.DecryptStringFromBytes_Aes(cypherData, key, iv);

            Console.WriteLine("Secret Massage Is: \n" + decryptedMessage);
        }
        
        [Test]
        public void Mp3TestLsb()
        {
            AesAlgo aesAlgo = new AesAlgo();
            using (AesManaged aes = new AesManaged())
            {
                aes.KeySize = 128;
                aes.Padding = PaddingMode.PKCS7;
                var path = @"C:/Users/Mike/Desktop/pruducta/file_example_MP3_2MG - Copy.mp3";
                byte[] byteAudio = File.ReadAllBytes(path);
                string message = "tmkescht";
                byte[] encryptedMessage = aesAlgo.EncryptStringToBytes_Aes(message, aes.Key, aes.IV).Concat(aes.Key)
                    .Concat(aes.IV).ToArray();
                //var binMessage = _decoder.EncryptedByteArrayToBinary(encryptedMessage);
                //byteAudio = _metaDataAudio.GenerateJunk(byteAudio);
                byteAudio = _metaDataAudio.GenerateFramesMp3(byteAudio);
                _metaDataAudio.HideMp3(byteAudio, encryptedMessage);
                byte[] cypherData = _metaDataAudio.SeekMp3(byteAudio);
                byte[] key = _metaDataAudio.ExtractKeyMp3(byteAudio);
                byte[] iv = _metaDataAudio.ExtractIvMp3(byteAudio);
                // byteAudio = _lsbAudio.GenerateFrames(byteAudio);
                // _lsbAudio.HideMp3(byteAudio, binMessage);
                //
                // byte[] cypherData = _lsbAudio.SeekMp3(byteAudio);
                // byte[] key = _lsbAudio.ExtractKeyMp3(byteAudio);
                // byte[] iv = _lsbAudio.ExtractIvMp3(byteAudio);
                
                var decryptedMessage = aesAlgo.DecryptStringFromBytes_Aes(cypherData, key, iv);
                
                Console.WriteLine("Secret Massage Is: \n" + decryptedMessage);
                File.WriteAllBytes("C:/Users/Mike/Desktop/pruducta/TEST.mp3", byteAudio);
            }
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
                string message = "TEST";
                byte[] encryptedMessage = aesAlgo.EncryptStringToBytes_Aes(message, aes.Key, aes.IV).Concat(aes.Key)
                    .Concat(aes.IV).ToArray();
                var binMessage = _decoder.EncryptedByteArrayToBinary(encryptedMessage);
                
                
                _lsbAudio.HideWave(byteAudio, binMessage);
                
                byte[] cypherData = _lsbAudio.SeekWave(byteAudio);
                byte[] key = _lsbAudio.ExtractKeyWave(byteAudio);
                byte[] iv = _lsbAudio.ExtractIvWave(byteAudio);

                var decryptedMessage = aesAlgo.DecryptStringFromBytes_Aes(cypherData, key, iv);

                Console.WriteLine("Secret Massage Is: \n" + decryptedMessage);
                File.WriteAllBytes("C:/Users/Mike/Desktop/pruducta/file_example_WAV_1MG - Copy.wav", byteAudio);

            }
        }
        
        
        [Test]
        public void PETestLsb()
        {
            AesAlgo aesAlgo = new AesAlgo();
            using (AesManaged aes = new AesManaged())
            {
                aes.KeySize = 128;
                aes.Padding = PaddingMode.PKCS7;
                var path = @"C:\testfolder\exetest.exe";
                //FileStream file = File.OpenRead(path);
                byte[] byteAudio = File.ReadAllBytes(path);
                string message = "TEST lsb aes";
                byte[] encryptedMessage = aesAlgo.EncryptStringToBytes_Aes(message, aes.Key, aes.IV).Concat(aes.Key)
                    .Concat(aes.IV).ToArray();
                var binMessage = _decoder.EncryptedByteArrayToBinary(encryptedMessage);
                
                
                _lsbPe.HidePE(byteAudio, binMessage);
                
                byte[] cypherData = _lsbPe.SeekPE(byteAudio);
                byte[] key = _lsbPe.ExtractKeyPE(byteAudio);
                byte[] iv = _lsbPe.ExtractIvPE(byteAudio);

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
                
                _lsbVideo.HideAvi(byteVideo, binMessage);
                
                byte[] cypherData = _lsbVideo.SeekAvi(byteVideo);
                byte[] key = _lsbVideo.ExtractKeyAvi(byteVideo);
                byte[] iv = _lsbVideo.ExtractIvAvi(byteVideo);

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
                _metaDataVideo.hideAvi(byteVideo, encryptedData);
                
                
                
                
                byte[] cypherData = _metaDataVideo.SeekAvi(byteVideo);
                byte[] key = _metaDataVideo.ExtractKeyAvi(byteVideo);
                byte[] iv = _metaDataVideo.ExtractIvAvi(byteVideo);

                var decryptedMessage = aesAlgo.DecryptStringFromBytes_Aes(cypherData, aes.Key, aes.IV);

                Console.WriteLine("Secret Massage Is: \n" + decryptedMessage);
                File.WriteAllBytes("C:/Users/mamis/Desktop/vidOutput.avi", byteVideo);



            }
        }
    }
}