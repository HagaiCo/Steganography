using System;
using System.Collections.Generic;
using NAudio.Wave;

namespace WebApplication.Utilities
{
    public class LsbAudio
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
        
        public void HideMp3(byte[] audio, String bin)
        {
            // audio = GenerateFrames(audio);
            var indexSecret = indexOfSecret(audio);
            for (int j = 0; j < bin.Length; j++)
            {
                if (bin[j] == '1')
                {
                    if (audio[indexSecret + j] % 2 == 0)
                    {
                        audio[indexSecret + j]++;
                    }
                }
                else
                {
                    if (audio[indexSecret + j] % 2 == 1)
                    {
                        audio[indexSecret + j]--;
                    }
                }
            }
        }

        public byte[] SeekMp3(byte[] audio)
        {
            var bitsToProcess = GetByteCountMp3(audio)*8;
            string binText=null;
            var list= new List<int>();
            var indexSecret = indexOfSecret(audio) + 16; //16 for first 16 bits of the length of the message
            for (int i = 0; i < bitsToProcess; i++)
            {
                list.Add(audio[indexSecret + i] % 2 == 0 ? 0 : 1);
            }
            foreach (var n in list)
            {
                binText += n % 2 == 1 ? 1 : 0;
            }
            return BinToByte(binText);
        }
        
        public int GetByteCountMp3(byte[] audio)
        {
            var firstByteList = new List<int>();
            string bin = null;
            var indexSecret = indexOfSecret(audio);
            for (int i = 0; i < 16; i++)
            {
                firstByteList.Add(audio[indexSecret + i] % 2 == 1 ? 1 : 0);
            }
            foreach (var n in firstByteList)
            {
                bin += n % 2 == 1 ? 1 : 0;
            }

            return Convert.ToInt32(bin, 2);
        }
        
        public  byte[] ExtractKeyMp3(byte [] audio)
        {
            string binText = null;
            var bytesToSkip = GetByteCountMp3(audio);
            var indexSecret = indexOfSecret(audio) + (bytesToSkip * 8) + 16;
            var list = new List<int>();
            for (int i = 0; i < 128; i++)
            {
                list.Add(audio[indexSecret + i] % 2 == 0 ? 0 : 1); 
            }

            foreach (var n in list)
            {
                binText += n % 2 == 1 ? 1 : 0;
            }
            return BinToByte(binText);
        }
        
        public  byte[] ExtractIvMp3(byte [] audio)
        {
            string binText = null;
            var bytesToSkip = GetByteCountMp3(audio);
            int indexSecret = indexOfSecret(audio) + 16 + (bytesToSkip * 8) + 128;
            var list = new List<int>();
            for (int i = 0; i < 128; i++)
            {
                list.Add(audio[indexSecret + i] % 2 == 0 ? 0 : 1);
            }
            foreach (var n in list)
            {
                binText += n % 2 == 1 ? 1 : 0;
            }
            return BinToByte(binText);
        }

        public int WavStartPos(byte[] audio)
        {
            for (int i = 0; i < audio.Length; i++)
            {
                if ((audio[i].ToString() == "d") && audio[i + 1].ToString() == "a")
                {
                    if ((audio[i + 2].ToString() == "t") && audio[i + 3].ToString() == "a")
                        return i + 8;
                }
            }
            return 47;
        }
        
        public void Hide(byte[] audio, String bin)
        {
            var audioData = WavStartPos(audio);
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
            int i = WavStartPos(audio)+16;
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

        public int GetByteCount(byte[] audio)
        {
            var firstByteList = new List<int>();
            string bin = null;
            var audioData = WavStartPos(audio);
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
        
        public byte [] BinToByte(string bin)
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
            int index = WavStartPos(audio) + 16 + (bytesToSkip*8);
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
            int index = WavStartPos(audio) + 16 + (bytesToSkip*8) + 128;
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