using System;

namespace LicenseManager.Library
{
    class CyclicEncrypting
    {
        public byte[] Encrypt(byte[] data, UInt16 key) { 
            key=(UInt16)(key%8);
            byte[] res=new byte[data.Length];
            UInt16 tmp;
            for (var i = 0; i < data.Length; i++) {
                tmp = (UInt16)((data[i] << 8) >> key);
                res[i] = (byte)((tmp & 0x00ff) | (tmp >> 8));
            }

            return res;
        }

        public byte[] Decrypt(byte[] data, UInt16 key)
        {
            key = (UInt16)(key % 8);
            byte[] res = new byte[data.Length];
            UInt16 tmp;
            for (var i = 0; i < data.Length; i++)
            {
                tmp = (UInt16)(data[i] << key);
                res[i] = (byte)((tmp & 0x00ff) | (tmp >> 8));
            }

            return res;
        }
    }
}
