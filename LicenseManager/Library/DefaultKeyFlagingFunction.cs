using System;

namespace LicenseManager.Library
{
    class DefaultKeyFlagingFunction :IKeyFlagingFunction
    {
#if _KEYGEN
        public DefaultKeyFlagingFunction() { }

        public DefaultKeyFlagingFunction(Random rd) {
            _rd = rd;
        }

        private Random _rd;

        public byte[] GenerateBlockFlags()
        {
            if (_rd == null) _rd = new Random((int)DateTime.Now.Ticks);

            byte[] res = new byte[5];

            do{
                _rd.NextBytes(res);
                for (int i = 0; i < 5; i++)
                    if (res[i] > 15)
                        res[i] = (byte)(res[i] % 15);

                if (res[1] < res[0])
                    SwapBytes(ref res[1], ref res[0]);

                if (res[2] < res[0])
                    SwapBytes(ref res[2], ref res[0]);

                if (res[3] < res[0])
                    SwapBytes(ref res[3], ref res[0]);

                if (res[4] < res[0])
                    SwapBytes(ref res[4], ref res[0]);

                if(res[4]<res[1])
                    SwapBytes(ref res[4], ref res[1]);

                if (res[4] < res[2])
                    SwapBytes(ref res[4], ref res[2]);

                if (res[4] < res[3])
                    SwapBytes(ref res[4], ref res[3]);


                if (res[0] == res[1]) continue;
                if (res[0] == res[2]) continue;
                if (res[0] == res[3]) continue;
                if (res[0] == res[4]) continue;
                if (res[1] == res[4]) continue;
                if (res[2] == res[4]) continue;
                if (res[3] == res[4]) continue;

                break;
            }
            while(true);

            return res;
        }

        private static void SwapBytes(ref byte a, ref byte b) {
            byte tmp = a;
            a = b;
            b = tmp;
        }

#endif

        public void DetectKeyIndex(byte[] flags, out int key, out int days)
        {
            byte min = flags[0];
            byte max = flags[0];
            key = 0;
            days = 0;

            for (int i = 1; i < flags.Length; i++) {
                if (flags[i] < min)
                {
                    min = flags[i];
                    key = i;
                }
                else if (flags[i] > max)
                {
                    max = flags[i];
                    days = i;
                }
            }
        }
    }
}
