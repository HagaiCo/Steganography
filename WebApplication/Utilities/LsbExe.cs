using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Web.UI.WebControls.Adapters;

namespace WebApplication.Utilities
{
    public class LsbExe
    {
        public byte[] HideSecretMessage(byte[] batchFileAsBytes, string encryptedMessage)
        {
            encryptedMessage = "::" + encryptedMessage;
            byte[] encryptedMessageAsByte = Encoding.ASCII.GetBytes(encryptedMessage);


            byte[] result = new byte[batchFileAsBytes.Length + encryptedMessageAsByte.Length];
            Buffer.BlockCopy(batchFileAsBytes, 0, result, 0, batchFileAsBytes.Length);
            Buffer.BlockCopy(encryptedMessageAsByte, 0, result, batchFileAsBytes.Length, encryptedMessageAsByte.Length);
            return result;
        }

        public int GetSecretMessageIndex(byte[] fileAsBytes)
        {
            int indexOfSecretMassage = 0;
            for (int j = 0; j < fileAsBytes.Length; j++)
            {
                if (fileAsBytes[j] == 58 && fileAsBytes[j - 1] == 58)
                {
                    indexOfSecretMassage = j+1;
                }
            }
            return indexOfSecretMassage;
        }
        
        public byte[] Seek(byte[] fileAsBytes)
        {
            int indexOfSecretMassage = GetSecretMessageIndex(fileAsBytes);

            var bitsToProcess = GetByteCount(fileAsBytes, indexOfSecretMassage)*8;
            string binText = null;
            int i = indexOfSecretMassage+16;
            int bitsProcessed = 0;
            var list = new List<int>();
            while (bitsProcessed < bitsToProcess)
            {
                list.Add(fileAsBytes[i] % 2 == 0 ? 0 : 1);
                i++;
                bitsProcessed++;
            }
            foreach (var n in list)
            {
                binText += n % 2 == 1 ? 1 : 0;
            }
            return BinToByte(binText);
        }
        
        static int GetByteCount(byte[] fileAsBytes, int indexOfSecretMassage)
        {
            var firstByteList = new List<int>();
            string bin = null;
            for (int j = 0; j < 16; j++)
            {
                firstByteList.Add(fileAsBytes[indexOfSecretMassage + j] % 2 == 1 ? 1 : 0);
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

        public byte[] ExtractKey(byte[] fileAsBytes)
        {
            string binText = null;
            var indexOfMessage = GetSecretMessageIndex(fileAsBytes);
            var bytesToSkip = GetByteCount(fileAsBytes, indexOfMessage);
            int indexOfKey = indexOfMessage + 16 + bytesToSkip * 8;
            int i;
            var list = new List<int>();
            for (i = 0; i < 128; i++)
            {
                list.Add(fileAsBytes[indexOfKey + i] % 2 == 0 ? 0 : 1);
            }
            foreach (var n in list)
            {
                binText += n % 2 == 1 ? 1 : 0;
            }
            return BinToByte(binText);
        }

        public byte[] ExtractIv(byte[] fileAsBytes)
        {
            string binText = null;
            var indexOfMessage = GetSecretMessageIndex(fileAsBytes);
            var bytesToSkip = GetByteCount(fileAsBytes, indexOfMessage);
            int indexOfIv = indexOfMessage + 16 + bytesToSkip * 8 + 128;
            var list = new List<int>();
                
            for (int i = 0; i < 128; i++)
            {
                list.Add(fileAsBytes[indexOfIv + i] % 2 == 0 ? 0 : 1);
                
            }
            foreach (var n in list)
            {
                binText += n % 2 == 1 ? 1 : 0;
            }
            return BinToByte(binText);
        }
    }
}