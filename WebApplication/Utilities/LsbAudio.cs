using System;
using System.Collections.Generic;

namespace WebApplication.Utilities
{
    public class LsbAudio
    {
        public void Hide(byte[] audio, String bin)
        {
            var audioData = 45;
            var index = 0;
            while (index < bin.Length)
            {
                if (bin[index] == '1')
                {
                    if (audio[audioData + index] % 2 == 0)
                    {
                        audio[audioData + index]++;
                    }
                }
                else
                {
                    if (audio[audioData + index] % 2 == 1)
                    {
                        audio[audioData + index]--;
                    }
                }

                index++;
            }
        }

        public byte[] Seek(byte[] audio)
        {
            var bitsToProcess = GetByteCount(audio)*8;
            string binText=null;
            int i = 45+16;
            int bitsProcessed = 0;
            var list= new List<int>();
            while (bitsProcessed<bitsToProcess)
            {
                list.Add(audio[i] % 2 == 0 ? 0 : 1);
                i++;
                bitsProcessed++;
            }
            foreach (var n in list)
            {
                binText += n % 2 == 1 ? 1 : 0;
            }
            return BinToByte(binText);
            
        }
        
        static int GetByteCount(byte[] audio)
        {
            var firstByteList = new List<int>();
            string bin = null;
            var audioData = 45;
            for (int j = 0; j < 16; j++)
            {
                firstByteList.Add(audio[audioData + j] % 2 == 1 ? 1 : 0);
            }
            
            foreach (var n in firstByteList)
            {
                bin += n % 2 == 1 ? 1 : 0;
            }

            return Convert.ToInt32(bin, 2);
        }
        
        static byte [] BinToByte(string bin)
        {
            var list= new List<byte>();
            for (var i = 0; i < bin.Length; i += 8)
            {
                var t = bin.Substring(i, 8);
                list.Add(Convert.ToByte(t,2));
            }
            
            return list.ToArray(); ;
        }
        
        public  byte[] ExtractKey(byte [] audio)
        {
            string binText = null;
            var bytesToSkip = GetByteCount(audio);
            int index = 45 + 16 + (bytesToSkip*8);
            int iterations, i;
            var list = new List<int>();
            for (i = 0; i < 128; i++)
            {
                list.Add(audio[index+i] % 2 == 0 ? 0 : 1);
                
            }
            foreach (var n in list)
            {
                binText += n % 2 == 1 ? 1 : 0;
            }
            return BinToByte(binText);
        }
        
        public  byte[] ExtractIv(byte [] audio)
        {
            string binText = null;
            var bytesToSkip = GetByteCount(audio);
            int index = 45 + 16 + (bytesToSkip*8) + 128;
            int iterations, i;
            var list = new List<int>();
            for (i = 0; i < 128; i++)
            {
                list.Add(audio[index+i] % 2 == 0 ? 0 : 1);
                
            }
            foreach (var n in list)
            {
                binText += n % 2 == 1 ? 1 : 0;
            }
            return BinToByte(binText);
        }
    }
}