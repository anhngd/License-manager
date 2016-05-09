using System;
using System.Runtime.InteropServices;

namespace LicenseManager.Library
{
    class CRC16
    {
#if NO_SECUMOD
        const ushort polynomial = 0xA001;
        ushort[] table = new ushort[256];

        public ushort ComputeChecksum(byte[] bytes)
        {
            ushort crc = 0;
            for (int i = 0; i < bytes.Length; ++i)
            {
                byte index = (byte)(crc ^ bytes[i]);
                crc = (ushort)((crc >> 8) ^ table[index]);
            }
            return crc;
        }

        public byte[] ComputeChecksumBytes(byte[] bytes)
        {
            ushort crc = ComputeChecksum(bytes);
            return BitConverter.GetBytes(crc);
        }

        public CRC16()
        {
            ushort value;
            ushort temp;
            for (ushort i = 0; i < table.Length; ++i)
            {
                value = 0;
                temp = i;
                for (byte j = 0; j < 8; ++j)
                {
                    if (((value ^ temp) & 0x0001) != 0)
                    {
                        value = (ushort)((value >> 1) ^ polynomial);
                    }
                    else
                    {
                        value >>= 1;
                    }
                    temp >>= 1;
                }
                table[i] = value;
            }
        }
#else
        [DllImport("secumod.dll")]
        static extern int ComputeChecksum(IntPtr data, int len, out ushort res);

        public ushort ComputeChecksum(byte[] bytes)
        {
            ushort chkSome;

            int size = bytes.Length;
            IntPtr pnt = Marshal.AllocHGlobal(size);
            Marshal.Copy(bytes, 0, pnt, size);

            int check = ComputeChecksum(pnt, size, out chkSome);
            if (check==1) { }
            return chkSome;
        }

#endif
    }
}
