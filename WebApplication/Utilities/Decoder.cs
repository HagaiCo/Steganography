using System;
using System.Collections.Generic;

namespace WebApplication.Utilities
{
    public class Decoder
    {
        public string EncryptedByteArrayToBinary(byte [] encryptedData)
        {
            string binText = null;
            binText=Convert.ToString(encryptedData.Length-32, 2).PadLeft(16, '0'); //first 2 byte is the length of byts to read.
            foreach (var byt in encryptedData)
            {
                binText += Convert.ToString(byt, 2).PadLeft(8, '0');
            }
            
            
            return binText;
        }
        static byte [] BinaryToByte(string bin)
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