using System;
using System.Collections.Generic;

namespace WebApplication.Utilities
{
    public class MetaDataAudio
    {
        public void Hide(byte[] audio, byte [] encryptedMessage)
        {
            var audioMetaData = JunkPosition(audio);
            byte[] length = BitConverter.GetBytes(encryptedMessage.Length-32) ;
            audio[audioMetaData++] = length[0];
            audio[audioMetaData++] = length[1];
            audio[audioMetaData++] = 0;
            audio[audioMetaData++] = 0;
            foreach (var b in encryptedMessage)
            {
                audio[audioMetaData++] = b;
            }
        }

        public byte[] Seek(byte[] audio)
        {
            int j = 0;
            var audioMetaData = JunkPosition(audio);
            byte[] lengthBytes = {audio[audioMetaData],audio[audioMetaData+1],audio[audioMetaData+2],audio[audioMetaData+3]};
            int length = BitConverter.ToInt32(lengthBytes, 0);
            byte[] encryptedData=new byte[length];
            for (int i = audioMetaData + 4; i < audioMetaData + 4 + length; i++) //first 4 bytes for length
            {
                encryptedData[j++] = audio[i];
            }
            return encryptedData;
            
        }
        
        
        public  byte[] ExtractKey(byte [] audio)
        {
            byte [] key = new byte[16];
            int j = 0;
            var audioMetaData = JunkPosition(audio);
            byte[] lengthBytes = {audio[audioMetaData],audio[audioMetaData+1],audio[audioMetaData+2],audio[audioMetaData+3]};
            int length = BitConverter.ToInt32(lengthBytes, 0);
            int index = audioMetaData + 4 + length;
            for (int i = index; i < index + 16; i++)
            {
                key[j++] = audio[i];
            }

            return key;
        }
        
        public  byte[] ExtractIv(byte [] audio)
        {
            byte [] iv = new byte[16];
            int j = 0;
            var audioMetaData = JunkPosition(audio);
            byte[] lengthBytes = {audio[audioMetaData],audio[audioMetaData+1],audio[audioMetaData+2],audio[audioMetaData+3]};
            int length = BitConverter.ToInt32(lengthBytes, 0);
            int index = audioMetaData + 4 + length + 16;
            for (int i = index; i < index + 16; i++)
            {
                iv[j++] = audio[i];
            }
            return iv;
        }
        
        public static int JunkPosition(byte[] audio)
        {
            string datal1 = audio[40].ToString("X").PadLeft(0);
            string datal2 = audio[41].ToString("X").PadLeft(0);
            string datal3 = audio[42].ToString("X").PadLeft(0);
            string datal4 = audio[43].ToString("X").PadLeft(0);// + audio[41] + audio[42] + audio[43];
            string dataString = datal4 + datal3 + datal2 + datal1;
            int datalengthnum = Convert.ToInt32(dataString, 16) + 44;
            return datalengthnum;
        }
    }
}