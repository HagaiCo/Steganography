using System;
using System.Collections.Generic;

namespace WebApplication.Utilities
{
    public class LsbVideo
    {
        public void Hide(byte[] vid, String bin)
        {
            var dataChunk = findMOVI(vid);
            var iterations = 0;
            while (iterations<bin.Length)
            {
                if (bin[iterations] == '1')
                {
                    if (vid[dataChunk + iterations] % 2 == 0)
                        vid[dataChunk + iterations]++;
                }
                else
                {
                    if (vid[dataChunk + iterations] % 2 == 1)
                        vid[dataChunk + iterations]--;
                }

                iterations++;
            }
        }

        public void HideMov(byte[] mov, String bin)
        {
            var i = findMoov(mov);
            foreach (var bit in bin)
            {
                if (bit == '1')
                {
                    if (mov[i] % 2 == 0)
                        mov[i]++;
                }
                else
                {
                    if (mov[i] % 2 == 1)
                        mov[i]--;
                }
                i--;
            }
            
        }
        
        public static int findMoov(byte[] b)
        {
            for (int i = 12; i < b.Length; i++)
            {
                if(b[i]!=109)
                    continue;
                else
                {
                    if (b[i + 1] == 111 && b[i + 2] == 111 && b[i + 3] == 118)
                        return i-10;
                }
            }

            return 0;
        }
        
        public byte[] Seek(byte[] vid)
        {
            var bitsToProcess = GetByteCount(vid)*8;
            string binText=null;
            int i = findMOVI(vid)+16;
            int bitsProcessed = 0;
            var list= new List<int>();
            while (bitsProcessed<bitsToProcess)
            {
                list.Add(vid[i] % 2 == 0 ? 0 : 1);
                i++;
                bitsProcessed++;
            }
            foreach (var n in list)
            {
                binText += n % 2 == 1 ? 1 : 0;
            }
            return BinToByte(binText);
        }

        public byte[] SeekMov(byte[] mov)
        {
            String binText = null;
            var bitsToProcess = GetByteCountMov(mov)*8;
            var i = findMoov(mov)-16;
            var list = new List<int>();
            for (int j = i; j > i - bitsToProcess; j--)
            {
                list.Add(mov[j] % 2 == 0 ? 0 : 1);
            }
            foreach (var n in list)
            {
                binText += n % 2 == 1 ? 1 : 0;
            }
            return BinToByte(binText);
        }
        
        public byte[] ExtractKey(byte [] vid)
        {
            string binText = null;
            var bytesToSkip = GetByteCount(vid);
            int index = findMOVI(vid) + 16 + (bytesToSkip*8);
            int iterations, i;
            var list = new List<int>();
            for (i = 0; i < 128; i++)
            {
                list.Add(vid[index+i] % 2 == 0 ? 0 : 1);
                
            }
            foreach (var n in list)
            {
                binText += n % 2 == 1 ? 1 : 0;
            }
            return BinToByte(binText);
        }
        
        public byte[] ExtractKeyMov(byte [] vid)
        {
            string binText = null;
            var bytesToSkip = GetByteCountMov(vid);
            int index = findMoov(vid) - 16 - (bytesToSkip*8);
            int iterations, i;
            var list = new List<int>();
            for (i = 0; i < 128; i++)
            {
                list.Add(vid[index--] % 2 == 0 ? 0 : 1);
                
            }
            foreach (var n in list)
            {
                binText += n % 2 == 1 ? 1 : 0;
            }
            return BinToByte(binText);
        }
        
        
        
        public byte[] ExtractIv(byte [] vid)
        {
            string binText = null;
            var bytesToSkip = GetByteCount(vid);
            int index = findMOVI(vid) + 16 + (bytesToSkip*8) + 128;
            int iterations, i;
            var list = new List<int>();
            for (i = 0; i < 128; i++)
            {
                list.Add(vid[index+i] % 2 == 0 ? 0 : 1);
                
            }
            foreach (var n in list)
            {
                binText += n % 2 == 1 ? 1 : 0;
            }
            return BinToByte(binText);
        }
        
        public byte[] ExtractIvMov(byte [] vid)
        {
            string binText = null;
            var bytesToSkip = GetByteCountMov(vid);
            int index = findMoov(vid) - 16 - (bytesToSkip*8) - 128;
            int iterations, i;
            var list = new List<int>();
            for (i = 0; i < 128; i++)
            {
                list.Add(vid[index--] % 2 == 0 ? 0 : 1);
                
            }
            foreach (var n in list)
            {
                binText += n % 2 == 1 ? 1 : 0;
            }
            return BinToByte(binText);
        }
        public static int findMOVI(byte[] b)
        {
            for (int i = 12; i < b.Length; i++)
            {
                if(b[i]!=109)
                    continue;
                else
                {
                    if (b[i + 1] == 111 && b[i + 2] == 118 && b[i + 3] == 105)
                        return i+100000;
                }
            }

            return 0;
        }
        static int GetByteCount(byte[] vid)
        {
            var firstByteList = new List<int>();
            string bin = null;
            var datachunk = findMOVI(vid);
            for (int j = 0; j < 16; j++)
            {
                firstByteList.Add(vid[datachunk + j] % 2 == 1 ? 1 : 0);
            }
            
            foreach (var n in firstByteList)
            {
                bin += n % 2 == 1 ? 1 : 0;
            }

            return Convert.ToInt32(bin, 2);
        }
        
        static int GetByteCountMov(byte[] vid)
        {
            var firstByteList = new List<int>();
            string bin = null;
            var moovChunk = findMoov(vid);
            for (int j = 0; j < 16; j++)
            {
                firstByteList.Add(vid[moovChunk--] % 2 == 1 ? 1 : 0);
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
        
        public string EncryptedDataToBin(byte [] encryptedData,byte [] key, byte[] iv)
        {
            string binText = null;
            binText=Convert.ToString(encryptedData.Length, 2).PadLeft(16, '0'); //first 2 byte is the length of byts to read.
            foreach (var byt in encryptedData)
            {
                binText += Convert.ToString(byt, 2).PadLeft(8, '0');
            }
            foreach (var byt in key)
            {
                binText += Convert.ToString(byt, 2).PadLeft(8, '0');
            }
            foreach (var byt in iv)
            {
                binText += Convert.ToString(byt, 2).PadLeft(8, '0');
            }
            
            
            
            return binText;
        }
    }
}