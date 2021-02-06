using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Reflection;

namespace WebApplication.Utilities
{
    public class MetaDataPicture
    {
        public byte[] HideJpeg (byte[] jpegByteArray, byte[] encryptedData)
        {
            //Image jpeg = (Bitmap)((new ImageConverter()).ConvertFrom(jpegByteArray));
            MemoryStream ms1 = new MemoryStream(jpegByteArray);
            Image jpeg = Image.FromStream(ms1);
            byte[] length = BitConverter.GetBytes(encryptedData.Length) ;
            PropertyItem pi = (PropertyItem)typeof(PropertyItem).GetConstructor(BindingFlags.Instance | BindingFlags.NonPublic, null, new Type[] { }, null)?.Invoke(null);
            pi.Type = 3;
            pi.Id = 1;
            pi.Len = encryptedData.Length;
            pi.Value = length.Concat(encryptedData).ToArray();
            jpeg.SetPropertyItem(pi);
            using (var ms = new MemoryStream())
            {
                jpeg.Save(ms,ImageFormat.Jpeg);
                return  ms.ToArray();
            }
           
        }

        public byte[] SeekJpeg(byte[] jpegByteArray)
        {
            int j = 0;
            //Image jpeg = (Bitmap)((new ImageConverter()).ConvertFrom(jpegByteArray));
            MemoryStream ms1 = new MemoryStream(jpegByteArray);
            Image jpeg = Image.FromStream(ms1);
            PropertyItem pi = jpeg.GetPropertyItem(1);
            byte [] data = pi.Value;
            byte[] lengthBytes = {data[0],data[1],data[2],data[3]};
            int length = BitConverter.ToInt32(lengthBytes, 0);
            byte [] encryptedData =new byte[length-32];
            for (int i = 4; i < length -32 +4;  i++)
            {
                encryptedData[j++] = data[i];
            }

            return encryptedData;
        }

        public byte[] ExtractKeyJpeg(byte[] jpegByteArray)
        {
            int j = 0;
            //Image jpeg = (Bitmap)((new ImageConverter()).ConvertFrom(jpegByteArray));
            MemoryStream ms1 = new MemoryStream(jpegByteArray);
            Image jpeg = Image.FromStream(ms1);
            byte [] data = jpeg.GetPropertyItem(1).Value;
            byte[] lengthBytes = {data[0],data[1],data[2],data[3]};
            int length = BitConverter.ToInt32(lengthBytes, 0);
            byte [] key = new byte[16];
            for (int i = length-32+4 ; i < length-16 +4; i++)
            {
                key[j++] = data[i];
            }

            return key;
        }
        
        public byte[] ExtractIvJpeg(byte[] jpegByteArray)
        {
            int j = 0;
            //Image jpeg = (Bitmap)((new ImageConverter()).ConvertFrom(jpegByteArray));
            MemoryStream ms1 = new MemoryStream(jpegByteArray);
            Image jpeg = Image.FromStream(ms1);
            byte [] data = jpeg.GetPropertyItem(1).Value;
            byte[] lengthBytes = {data[0],data[1],data[2],data[3]};
            int length = BitConverter.ToInt32(lengthBytes, 0);
            byte [] iv = new byte[16];
            for (int i = length-16+4; i < length+4 ; i++)
            {
                iv[j++] = data[i];
            }

            return iv;
        }
        
    }
}