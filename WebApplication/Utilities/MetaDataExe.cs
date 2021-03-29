using System;
using System.Collections.Generic;
using System.Text;

namespace WebApplication.Utilities
{
    public class MetaDataExe
    {
        
        /// **********************************************
        /// PE  File Steganography Methods
        /// **********************************************
        public int GetIndexOfJunkkPE(byte[] file)
        {
            int j = 0;
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
        
        public void HideMetaDataPE(byte[] file, byte [] encryptedMessage)
        {
            var PEMetaDataIndex = GetIndexOfJunkkPE(file);
            byte[] length = BitConverter.GetBytes(encryptedMessage.Length-32) ;
            file[PEMetaDataIndex++] = length[0];
            file[PEMetaDataIndex++] = length[1];
            file[PEMetaDataIndex++] = 0;
            file[PEMetaDataIndex++] = 0;
            foreach (var b in encryptedMessage)
            {
                file[PEMetaDataIndex++] = b;
            }
        }
        
        public byte[] SeekPE(byte[] file)
        {
            int j = 0;
            var PEMetaDataIndex = GetIndexOfJunkkPE(file);
            byte[] lengthBytes = {file[PEMetaDataIndex],file[PEMetaDataIndex+1],file[PEMetaDataIndex+2],file[PEMetaDataIndex+3]};
            int length = BitConverter.ToInt32(lengthBytes, 0);
            byte[] encryptedData=new byte[length];
            for (int i = PEMetaDataIndex + 4; i < PEMetaDataIndex + 4 + length; i++) //first 4 bytes for length
            {
                encryptedData[j++] = file[i];
            }
            return encryptedData;
            
        }
        
        public  byte[] ExtractKeyPE(byte [] file)
        {
            byte [] key = new byte[16];
            int j = 0;
            var PEMetaDataIndex = GetIndexOfJunkkPE(file);
            byte[] lengthBytes = {file[PEMetaDataIndex],file[PEMetaDataIndex+1],file[PEMetaDataIndex+2],file[PEMetaDataIndex+3]};
            int length = BitConverter.ToInt32(lengthBytes, 0);
            int index = PEMetaDataIndex + 4 + length;
            for (int i = index; i < index + 16; i++)
            {
                key[j++] = file[i];
            }

            return key;
        }
        
        public  byte[] ExtractIvPE(byte [] file)
        {
            byte [] iv = new byte[16];
            int j = 0;
            var PEMetaDataIndex = GetIndexOfJunkkPE(file);
            byte[] lengthBytes = {file[PEMetaDataIndex],file[PEMetaDataIndex+1],file[PEMetaDataIndex+2],file[PEMetaDataIndex+3]};
            int length = BitConverter.ToInt32(lengthBytes, 0);
            int index = PEMetaDataIndex + 4 + length + 16;
            for (int i = index; i < index + 16; i++)
            {
                iv[j++] = file[i];
            }
            return iv;
        }
        
        /// **********************************************
        /// Batch .bat File Steganography Methods
        /// **********************************************
        
        public byte [] HideBatch(byte[] batchFileAsBytes, string encryptedMessage)
        {
            encryptedMessage = "::" + encryptedMessage;
            byte[] encryptedMessageAsByte = Encoding.ASCII.GetBytes(encryptedMessage);


            byte[] result = new byte[batchFileAsBytes.Length + encryptedMessageAsByte.Length];
            Buffer.BlockCopy(batchFileAsBytes, 0, result, 0, batchFileAsBytes.Length);
            Buffer.BlockCopy(encryptedMessageAsByte, 0, result, batchFileAsBytes.Length, encryptedMessageAsByte.Length);
            return result;

        }

        public int GetSecretMessageIndexBatch(byte[] fileAsBytes)
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
        
        public byte[] SeekBatch(byte[] fileAsBytes)
        {
            int indexOfSecretMassage = GetSecretMessageIndexBatch(fileAsBytes);
            byte[] encryptedData=new byte[fileAsBytes.Length-indexOfSecretMassage-24];
            var j = 0;
            for (int i = indexOfSecretMassage; i < fileAsBytes.Length - 24; i++)
            {
                encryptedData[j++] = fileAsBytes[i];
            }

            return encryptedData;
            /*var bitsToProcess = GetByteCountBatch(fileAsBytes, indexOfSecretMassage)*8;
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
            return BinToByte(binText);*/
        }
        
        static int GetByteCountBatch(byte[] fileAsBytes, int indexOfSecretMassage)
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

        public byte[] ExtractKeyBatch(byte[] fileAsBytes)
        {
            /*string binText = null;
            var indexOfMessage = GetSecretMessageIndexBatch(fileAsBytes);
            var bytesToSkip = GetByteCountBatch(fileAsBytes, indexOfMessage);
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
            return BinToByte(binText);*/
            int indexOfSecretMassage = GetSecretMessageIndexBatch(fileAsBytes);
            byte[] key=new byte[16];
            var j = 0;
            for (int i = fileAsBytes.Length - 24 ; i < fileAsBytes.Length - 8; i++)
            {
                key[j++] = fileAsBytes[i];
            }

            return key;
        }

        public byte[] ExtractIvBatch(byte[] fileAsBytes)
        {
            byte[] IV=new byte[8];
            var j = 0;
            for (int i = fileAsBytes.Length - 8 ; i < fileAsBytes.Length; i++)
            {
                IV[j++] = fileAsBytes[i];
            }

            return IV;
            /*string binText = null;
            var indexOfMessage = GetSecretMessageIndexBatch(fileAsBytes);
            var bytesToSkip = GetByteCountBatckeyh(fileAsBytes, indexOfMessage);
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
            return BinToByte(binText);*/
            
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