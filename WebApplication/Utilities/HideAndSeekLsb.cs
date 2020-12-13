/*using System;
using System.Collections.Generic;
using System.Drawing;

namespace WebApplication.Utilities
{
    public class HideAndSeekLsb
    {
        public void Clean(Bitmap bmp, int textLength) // Cleaning all LSB to 0's
        {
            int i;  // row
            int j; // col
            var R = 0;
            var G = 0;
            var B = 0;
            for (i = 0; i < bmp.Height; i+=1)
            {
                for (j = 0; j < bmp.Width; j+=1)
                {
                    Color pixel = bmp.GetPixel(j,i);
                    R = pixel.R;
                    R = pixel.R - pixel.R % 2;
                    G = pixel.G;
                    G = pixel.G - pixel.G % 2;
                    B = pixel.B;
                    B = pixel.B - pixel.B % 2;
                    bmp.SetPixel(j,i, Color.FromArgb(R,G,B));
                }
            }
        }

        public void Hide(Bitmap bmp,String bin)
        {
            
            var j = 0;
            var i = 0;
            var R = 0;
            var G = 0;
            var B = 0;
            var iterantions = 0;
            for (i = 0; i < bmp.Height; i+=1)
            {
                for (j = 0; j < bmp.Width; j+=1)
                {
                    var pixel = bmp.GetPixel(j, i);
                    switch ((i+j) % 3) // %3 of i+j determines if we use R/G/B
                    {
                        case 0:
                        {
                            R = pixel.R;
                            if (bin[iterantions] == '1')
                            {
                                if (R % 2 == 0)
                                    R += 1;
                            }
                            else
                            {
                                if (R % 2 == 1)
                                    R -= 1;
                            }
                            bmp.SetPixel(j, i, Color.FromArgb(R, pixel.G, pixel.B));
                            break;
                        }
                        case 1:
                        {
                            G = pixel.G;
                            if (bin[iterantions] == '1')
                            {
                                if (G % 2 == 0)
                                    G += 1;
                            }
                            else
                            {
                                if (G % 2 == 1)
                                    G -= 1;
                            }
                            bmp.SetPixel(j, i, Color.FromArgb(pixel.R, G, pixel.B));
                            break;
                        }
                        case 2:
                        {
                            B = pixel.B;
                            if (bin[iterantions] == '1')
                            {
                                if (B % 2 == 0)
                                    B += 1;
                            }
                            else
                            {
                                if (B % 2 == 1)
                                    B -= 1;
                            }
                            
                            bmp.SetPixel(j, i, Color.FromArgb(pixel.R, pixel.G, B));
                            break;
                            
                        }
                    }
                    iterantions++;
                    if (iterantions == bin.Length) //check if finished hiding
                    {
                        j = bmp.Width;
                        i = bmp.Height;
                        break;
                    }
                }
            }
        }

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
        
        static int GetByteCount(Bitmap bmp) //returns decimal value of first 16 bits - length of cypher.
        {
            var firstByteList = new List<int>();
            string bin = null;
            
            int i = 0, j;
            for (j = 0; j < 16; j++)
            {
                
                
                Color pixel = bmp.GetPixel(j, i);
                switch (i+j%3)
                {
                    case 0:
                    {
                        firstByteList.Add(pixel.R % 2 == 1 ? 1 : 0);
                        break;
                    }
                    case 1:
                    {
                        firstByteList.Add(pixel.G % 2 == 1 ? 1 : 0);
                        break;
                    }
                    case 2:
                    {
                        firstByteList.Add(pixel.B % 2 == 1 ? 1 : 0);
                        break;
                    }
                }
            }
            foreach (var n in firstByteList)
            {
                bin += n % 2 == 1 ? 1 : 0;
            }

            return Convert.ToInt32(bin, 2);

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
        
        public byte [] Seek(Bitmap bmp)
        {
            var bitsToProcess = GetByteCount(bmp)*8;
            string binText=null;
            var j = 0;
            var i = 0;
            int bitsProcessed = 0;
            var list= new List<int>();
            for (i = 0; i < bmp.Height; i += 1)
            {
                for (j = i == 0 ? 16 : 0; j < bmp.Width; j += 1)  // starts from 3rd byte because 2st byte is length
                {
                    Color pixel = bmp.GetPixel(j, i);
                    switch ((i + j) % 3)
                    {
                        case 0:
                        {
                            list.Add(pixel.R % 2 == 1 ? 1 : 0); ;
                            break;
                        }
                        case 1:
                        {
                            list.Add(pixel.G % 2 == 1 ? 1 : 0);
                            break;
                        }
                        case 2:
                        {
                            list.Add(pixel.B % 2 == 1 ? 1 : 0);
                            break;
                        }
                    }

                    bitsProcessed++;
                    if (bitsProcessed == bitsToProcess) 
                    {
                        j = bmp.Width;
                        i = bmp.Height;
                        break;
                    }


                }
            }
            foreach (var n in list)
            {
                binText += n % 2 == 1 ? 1 : 0;
            }
            return BinToByte(binText);
            

        }

        public byte[] ExtractKey(Bitmap bmp)
        {
            string binText = null;
            var bitsToSkip = GetByteCount(bmp) * 8 + 16; //Starts reading key after cypherText is over
            int iterations = 0, i, j;
            var list = new List<int>();
            for (i=bitsToSkip / bmp.Width; i < bmp.Height; i++)
            {
                for (j = i == bitsToSkip / bmp.Width ? bitsToSkip % bmp.Width : 0; j < bmp.Width; j++) 
                {
                    Color pixel = bmp.GetPixel(j, i);
                    switch ((i + j) % 3)
                    {
                        case 0:
                        {
                            list.Add(pixel.R % 2 == 1 ? 1 : 0); ;
                            break;
                        }
                        case 1:
                        {
                            list.Add(pixel.G % 2 == 1 ? 1 : 0);
                            break;
                        }
                        case 2:
                        {
                            list.Add(pixel.B % 2 == 1 ? 1 : 0);
                            break;
                        }
                    }
                    
                    iterations++;
                    if(iterations==120)
                        Console.WriteLine();
                    if (iterations == 128)
                    {
                        j = bmp.Width;
                        i = bmp.Height;
                        break;
                    }
                }
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
        public byte[] ExtractIv(Bitmap bmp)
        {
            string binText = null;
            var bitsToSkip = GetByteCount(bmp) * 8 + 144; // starts reading IV after key is over
            int iterations = 0, i, j;
            var list = new List<int>();
            for (i=bitsToSkip / bmp.Width; i < bmp.Height; i++)
            {
                for (j = i == bitsToSkip / bmp.Width ? bitsToSkip % bmp.Width : 0; j < bmp.Width; j++) 
                {
                    Color pixel = bmp.GetPixel(j, i);
                    switch ((i + j) % 3)
                    {
                        case 0:
                        {
                            list.Add(pixel.R % 2 == 1 ? 1 : 0); ;
                            break;
                        }
                        case 1:
                        {
                            list.Add(pixel.G % 2 == 1 ? 1 : 0);
                            break;
                        }
                        case 2:
                        {
                            list.Add(pixel.B % 2 == 1 ? 1 : 0);
                            break;
                        }
                    }
                    
                    iterations++;
                    if (iterations == 128)
                    {
                        j = bmp.Width;
                        i = bmp.Height;
                        break;
                    }
                }
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
    }
}*/