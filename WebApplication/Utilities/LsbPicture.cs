using System;
using System.Collections.Generic;
using System.Drawing;

namespace WebApplication.Utilities
{
    public class LsbPicture
    {
        public void HideBitmap(Bitmap bmp,String bin)
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
        
        static int GetByteCountBitmap(Bitmap bmp) //returns decimal value of first 16 bits - length of cypher.
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
        
        public byte [] SeekBitmap(Bitmap bmp)
        {
            var bitsToProcess = GetByteCountBitmap(bmp)*8;
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
            return BinToByteBitmap(binText);
            

        }

        public byte[] ExtractKeyBitmap(Bitmap bmp)
        {
            string binText = null;
            var bitsToSkip = GetByteCountBitmap(bmp) * 8 + 16; //Starts reading key after cypherText is over
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
            return BinToByteBitmap(binText);
            

        }
        
        public byte[] ExtractIvBitmap(Bitmap bmp)
        {
            string binText = null;
            var bitsToSkip = GetByteCountBitmap(bmp) * 8 + 144; // starts reading IV after key is over
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
            return BinToByteBitmap(binText);
        }
        
        static byte [] BinToByteBitmap(string bin)
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
}