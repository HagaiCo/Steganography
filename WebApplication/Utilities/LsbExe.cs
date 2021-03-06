﻿using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Web.UI.WebControls.Adapters;

namespace WebApplication.Utilities
{
    public class LsbExe
    {
        
        /// **********************************************
        /// PE File Steganography Methods
        /// **********************************************
        public int GetIndexOfJunkkPE(byte[] file)
        {
            var j = 0;
            for (j = 0; j < file.Length; j++)
            {
                if ((file[j] == 46) && (file[j + 1] == 116))
                {
                    if ((file[j+2] == 101) && (file[j+3] == 120) && (file[j+4] == 116))
                    {
                        break;
                    }
                }
            }

            while (file[j] != 0)
            {
                j += 40;
            }
            return j+1;
        }
        
        public void HidePE(byte[] file, String bin)
        {
            var indexSecret = GetIndexOfJunkkPE(file);
            for (int j = 0; j < bin.Length; j++)
            {
                if (bin[j] == '1')
                {
                    if (file[indexSecret + j] % 2 == 0)
                    {
                        file[indexSecret + j]++;
                    }
                }
                else
                {
                    if (file[indexSecret + j] % 2 == 1)
                    {
                        file[indexSecret + j]--;
                    }
                }
            }
        }
        
        public byte[] SeekPE(byte[] file)
        {
            var bitsToProcess = GetByteCountPE(file)*8;
            string binText=null;
            var list= new List<int>();
            var indexSecret = GetIndexOfJunkkPE(file) + 16; //16 for first 16 bits of the length of the message
            for (int i = 0; i < bitsToProcess; i++)
            {
                list.Add(file[indexSecret + i] % 2 == 0 ? 0 : 1);
            }
            foreach (var n in list)
            {
                binText += n % 2 == 1 ? 1 : 0;
            }
            return BinToByte(binText);
        }
        
        public  byte[] ExtractKeyPE(byte [] file)
        {
            string binText = null;
            var bytesToSkip = GetByteCountPE(file);
            var indexSecret = GetIndexOfJunkkPE(file) + (bytesToSkip * 8) + 16;
            var list = new List<int>();
            for (int i = 0; i < 128; i++)
            {
                list.Add(file[indexSecret + i] % 2 == 0 ? 0 : 1); 
            }

            foreach (var n in list)
            {
                binText += n % 2 == 1 ? 1 : 0;
            }
            return BinToByte(binText);
        }

        public int GetByteCountPE(byte[] file)
        {
            var firstByteList = new List<int>();
            string bin = null;
            var indexSecret = GetIndexOfJunkkPE(file);
            for (int i = 0; i < 16; i++)
            {
                firstByteList.Add(file[indexSecret + i] % 2 == 1 ? 1 : 0);
            }
            foreach (var n in firstByteList)
            {
                bin += n % 2 == 1 ? 1 : 0;
            }

            return Convert.ToInt32(bin, 2);
        }
        
        public  byte[] ExtractIvPE(byte [] file)
        {
            string binText = null;
            var bytesToSkip = GetByteCountPE(file);
            int indexSecret = GetIndexOfJunkkPE(file) + 16 + (bytesToSkip * 8) + 128;
            var list = new List<int>();
            for (int i = 0; i < 128; i++)
            {
                list.Add(file[indexSecret + i] % 2 == 0 ? 0 : 1);
            }
            foreach (var n in list)
            {
                binText += n % 2 == 1 ? 1 : 0;
            }
            return BinToByte(binText);
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
}