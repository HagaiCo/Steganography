using System;
using System.Collections.Generic;

namespace WebApplication.Utilities
{
    public class MetaDataAudio
    {
        
        public byte[] GenerateFrames(byte[] audio)
        {
            var flagg = 1;
            var index = 0;
            for (var i = 0; i < audio.Length; i++)
            {
                if ((audio[i] == 255) && (audio[i+1] == 251))
                {
                    if (flagg > 0)
                    {
                        flagg--;
                        continue;
                    }
                    else
                    {
                        index = i+2;
                        break;
                    }
                }
            }
            byte[] temp = new byte[index * 3];
            for (var i = 0; i < index; i++)
            { 
                temp[i] = audio[i]; 
            }

            int j = 0;
            for (var i = index; i < index * 2; i++)
            { 
                temp[i] = audio[j++]; 
            }

            j = 0;
            for (var i = index * 2; i < index * 3; i++)
            { 
                temp[i] = audio[j++]; 
            }
            byte[] newfile = new byte[(index*3) + audio.Length];
            for (var i = 0; i < (index*3); i++)
            {
                newfile[i] = temp[i];
            }

            for (var i = (index*3); i < audio.Length; i++)
            {
                newfile[i] = audio[i];
            }
            return newfile;
        }

        public int indexOfSecret(byte[] audio)
        {
            int skeep = 2;
            for (int i = 0; i < audio.Length; i++)
            {
                if ((audio[i] == 255) && (audio[i + 1] == 251))
                {
                    if (skeep > 0)
                    {
                        skeep--;
                        continue;
                    }

                    return i + 2;
                }
            }

            return 0;
        }

        public long IndexOfSecretFromEnd(byte[] audio)
        {
            var index = 0;
            int flagg = 0;
            for (var i = audio.Length - 1; i > 0; i--)
            {
                if ((audio[i] == 226) && (audio[i - 1] == 251) && (audio[i - 2] == 255)) 
                {
                    index = i + 1;
                    break;
                }
            }

            return index;
        }
        public void HideMp3(byte[] audio, byte [] encryptedMessage)
        {
            var audioMetaData = IndexOfSecretFromEnd(audio);
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
        
        public byte[] SeekMp3(byte[] audio)
        {
            int j = 0;
            var audioMetaData = IndexOfSecretFromEnd(audio);
            byte[] lengthBytes = {audio[audioMetaData],audio[audioMetaData+1],audio[audioMetaData+2],audio[audioMetaData+3]};
            int length = BitConverter.ToInt32(lengthBytes, 0);
            byte[] encryptedData=new byte[length];
            for (var i = audioMetaData + 4; i < audioMetaData + 4 + length; i++) //first 4 bytes for length
            {
                encryptedData[j++] = audio[i];
            }
            return encryptedData;
            
        }
        
        public  byte[] ExtractKeyMp3(byte [] audio)
        {
            byte [] key = new byte[16];
            int j = 0;
            var audioMetaData = IndexOfSecretFromEnd(audio);
            byte[] lengthBytes = {audio[audioMetaData],audio[audioMetaData+1],audio[audioMetaData+2],audio[audioMetaData+3]};
            int length = BitConverter.ToInt32(lengthBytes, 0);
            var index = audioMetaData + 4 + length;
            for (var i = index; i < index + 16; i++)
            {
                key[j++] = audio[i];
            }

            return key;
        }
        
        public  byte[] ExtractIvMp3(byte [] audio)
        {
            byte [] iv = new byte[16];
            int j = 0;
            var audioMetaData = IndexOfSecretFromEnd(audio);
            byte[] lengthBytes = {audio[audioMetaData],audio[audioMetaData+1],audio[audioMetaData+2],audio[audioMetaData+3]};
            int length = BitConverter.ToInt32(lengthBytes, 0);
            var index = audioMetaData + 4 + length + 16;
            for (var i = index; i < index + 16; i++)
            {
                iv[j++] = audio[i];
            }
            return iv;
        }
        
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