using System;
using System.Collections.Generic;
using System.Text;

namespace LicenseManager.Library
{
    /// <summary>
    /// Key san pham
    /// </summary>
    public class ProductKey
    {
        private ProductKey() {
            _blocks = new KeyBlock[5];
        }

        private KeyBlock[] _blocks;

        private UInt16 _keyValue;

        /// <summary>
        /// Ngay het han
        /// </summary>
        public DateTime ExpireDate { get; private set; }

        public uint NumberOfUser { get; set; }

        public override string ToString()
        {
            return string.Join("-", new string[] { _blocks[0].ToString(), _blocks[1].ToString(), _blocks[2].ToString(), _blocks[3].ToString(), _blocks[4].ToString() });
        }

        private void Decode(IKeyFlagingFunction fef)
        {
            if (fef == null)
                fef = new DefaultKeyFlagingFunction();

            byte[] flags = new byte[5];

            flags[0] = _blocks[0].Flag;
            flags[1] = _blocks[1].Flag;
            flags[2] = _blocks[2].Flag;
            flags[3] = _blocks[3].Flag;
            flags[4] = _blocks[4].Flag;

            int daysBlkIdx;
            int keyBlkIdx;
            int numOfUserIdx;
            UInt16 flagsChecksum;

            CRC16 crc = new CRC16();

            fef.DetectKeyIndex(flags, out keyBlkIdx, out daysBlkIdx);
            numOfUserIdx = 0;
            while (numOfUserIdx == daysBlkIdx || numOfUserIdx == keyBlkIdx)
                numOfUserIdx++;

            this._keyValue = _blocks[keyBlkIdx].GetUint16Value();
            flagsChecksum = crc.ComputeChecksum(flags);
            UInt16 tmp=DeCircle(_blocks[numOfUserIdx].GetUint16Value(),flagsChecksum);

            if ((tmp & 0x8000) == 0x8000)
                this.NumberOfUser = (uint)Math.Pow(2, tmp & 0x00ff);
            else this.NumberOfUser = 0;

            try
            {
                this.ExpireDate = new DateTime(2000, 1, 1).
                    AddDays(DeCircle(_blocks[daysBlkIdx].GetUint16Value(), flagsChecksum));
            }
            catch {
                throw new InvalidProductKeyFormatException();
            }
        }

        public bool Verify(DateTime dateCheck, string id)
        {
            return Verify(dateCheck, id, null);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="dateCheck"></param>
        /// <param name="id"></param>
        /// <returns></returns>
        public bool Verify(DateTime dateCheck, string id, IKeyFlagingFunction fef)
        {
            CRC16 rcr = new CRC16();

            byte[] dataToCheck = new byte[15];
            byte[] flags = new byte[5];

            int daysBlkIdx;
            int keyBlkIdx;
            int idChecksumIdx = 0;
            int dateChecksumIdx = 0;

            UInt16 dateChecksum;
            UInt16 idChecksum;
            UInt16 tmpChecksum;

            dataToCheck[0] = _blocks[0].Flag;
            dataToCheck[1] = _blocks[0].HightBits;
            dataToCheck[2] = _blocks[0].LowBits;

            dataToCheck[3] = _blocks[1].Flag;
            dataToCheck[4] = _blocks[1].HightBits;
            dataToCheck[5] = _blocks[1].LowBits;

            dataToCheck[6] = _blocks[2].Flag;
            dataToCheck[7] = _blocks[2].HightBits;
            dataToCheck[8] = _blocks[2].LowBits;

            dataToCheck[9] = _blocks[3].Flag;
            dataToCheck[10] = _blocks[3].HightBits;
            dataToCheck[11] = _blocks[3].LowBits;

            dataToCheck[12] = _blocks[4].Flag;
            dataToCheck[13] = _blocks[4].HightBits;
            dataToCheck[14] = _blocks[4].LowBits;

            flags[0] = _blocks[0].Flag;
            flags[1] = _blocks[1].Flag;
            flags[2] = _blocks[2].Flag;
            flags[3] = _blocks[3].Flag;
            flags[4] = _blocks[4].Flag;

            if (fef == null)
                fef = new DefaultKeyFlagingFunction();

            fef.DetectKeyIndex(flags, out keyBlkIdx, out daysBlkIdx);

            // get date checksum index
            while (dateChecksumIdx == daysBlkIdx || dateChecksumIdx == keyBlkIdx)
            {
                dateChecksumIdx++;
            }
            dateChecksumIdx++;
            while (dateChecksumIdx == daysBlkIdx || dateChecksumIdx == keyBlkIdx)
            {
                dateChecksumIdx++;
            }

            // get id checksum index
            idChecksumIdx = dateChecksumIdx + 1;
            while (idChecksumIdx == daysBlkIdx || idChecksumIdx == keyBlkIdx)
            {
                idChecksumIdx++;
            }

            // get date checksum value
            dateChecksum = _blocks[dateChecksumIdx].GetUint16Value();
            // get id checksum value
            idChecksum = _blocks[idChecksumIdx].GetUint16Value();

            // adjust data
            dataToCheck[dateChecksumIdx * 3 + 1] = 0;
            dataToCheck[dateChecksumIdx * 3 + 2] = 0;

            dataToCheck[idChecksumIdx * 3 + 1] = 0;
            dataToCheck[idChecksumIdx * 3 + 2] = 0;

            // checksum date
            dataToCheck[keyBlkIdx * 3 + 1] = 0;
            dataToCheck[keyBlkIdx * 3 + 2] = 0;

            tmpChecksum = rcr.ComputeChecksum(dataToCheck);
            if (tmpChecksum != dateChecksum)
                return false;

            // checksum id
            dataToCheck[keyBlkIdx * 3 + 1] = _blocks[keyBlkIdx].HightBits;
            dataToCheck[keyBlkIdx * 3 + 2] = _blocks[keyBlkIdx].LowBits;

            tmpChecksum = rcr.ComputeChecksum(dataToCheck);
            if (tmpChecksum != idChecksum)
                return false;

            // check expire date
            if (dateCheck > this.ExpireDate)
                return false;

            // check id
            if (!string.IsNullOrEmpty(id))
            {
                string flagedId = string.Concat(Encoding.ASCII.GetString(flags), id);
                tmpChecksum = rcr.ComputeChecksum(Encoding.UTF8.GetBytes(flagedId));
                if (tmpChecksum != this._keyValue)
                    return false;
            }

            // pass all
            return true;
        }

        //private byte[] mergeBytes(byte[] a, byte[] b)
        //{
        //    byte[] tmp = new byte[a.Length + b.Length];

        //    for (int i = 0; i < a.Length; i++)
        //        tmp[i] = a[i];

        //    for (int i = 0; i < b.Length; i++)
        //    {
        //        tmp[a.Length + i] = b[i];
        //    }

        //    return tmp;
        //}

        public bool Validate(DateTime dateCheck)
        {
            return Verify(dateCheck, null, null);
        }

#if _KEYGEN

        public static ProductKey GenKey(DateTime expireDate, string keyData)
        {
            return GenKey(expireDate, keyData,0, null, null);
        }

        public static ProductKey GenKey(DateTime expireDate, string keyData, uint numOfUser)
        {
            return GenKey(expireDate, keyData, numOfUser, null, null);
        }

        public static ProductKey GenKey(DateTime expireDate, string id, uint numOfUser, IKeyFlagingFunction fef, Random rd)
        {
            CRC16 rcr = new CRC16();

            ProductKey res = new ProductKey();
            res.ExpireDate = expireDate;

            if(rd==null)
                rd = new Random((int)DateTime.Now.Ticks);

            if (fef == null)
                fef = new DefaultKeyFlagingFunction(rd);

            DateTime rootDate = new DateTime(2000, 1, 1);
            if (expireDate < rootDate)
                throw new Exception("Ngày hết hạn không được nhỏ hơn 2000-01-01.");

            UInt16 dd = (UInt16)(expireDate - rootDate).TotalDays;
            UInt16 keyBytes;
            UInt16 nouVal;

            int daysBlkIdx;
            int keyBlkIdx;
            
            byte[] flags = fef.GenerateBlockFlags();

            do
            {
                daysBlkIdx = rd.Next(0, 5);
                keyBlkIdx = rd.Next(0, 5);
            }
            while (daysBlkIdx == keyBlkIdx);

            if (daysBlkIdx != 4)
                SwapBytes(ref flags[4], ref flags[daysBlkIdx]);

            if (keyBlkIdx != 0)
            {
                if (daysBlkIdx == 0)
                    SwapBytes(ref flags[4], ref flags[keyBlkIdx]);
                else
                    SwapBytes(ref flags[0], ref flags[keyBlkIdx]);
            }

            int nbi = 0;

            int dateChecksumIndex = 0;
            int idChecksumIndex = 0;

            UInt16 flagsChecksum = rcr.ComputeChecksum(flags);

            for (int i = 0; i < 5; i++)
            {
                if (i == daysBlkIdx)
                    res._blocks[i] = KeyBlock.Create(flags[i], Circle(dd, flagsChecksum));
                else if (i == keyBlkIdx)
                    res._blocks[i] = KeyBlock.Create(flags[i], 0);
                else if (nbi == 0)
                {
                    if (numOfUser == 0)
                    {
                        byte[] buf = new byte[2];
                        rd.NextBytes(buf);
                        nouVal = (UInt16)(buf[0] & 0x7F);
                        nouVal = (UInt16)((nouVal << 8) | buf[1]);
                    }
                    else
                    {
                        if (numOfUser < 2) numOfUser = 2;
                        double log = Math.Log(numOfUser, 2);
                        byte nuPow = (byte)log;
                        if (log > nuPow)
                            nuPow = (byte)(nuPow + 1);
                        res.NumberOfUser = (uint)(Math.Pow(2, nuPow));

                        byte[] buf = new byte[1];
                        rd.NextBytes(buf);
                        nouVal = (UInt16)(((buf[0] | 0x80) << 8) | nuPow);
                    }

                    res._blocks[i] = KeyBlock.Create(flags[i], Circle(nouVal, flagsChecksum));

                    nbi++;
                }
                else if (nbi == 1)
                {
                    dateChecksumIndex = i;
                    nbi++;
                    res._blocks[i] = KeyBlock.Create(flags[i], 0);
                }
                else if (nbi == 2)
                {
                    idChecksumIndex = i;
                    nbi++;
                    res._blocks[i] = KeyBlock.Create(flags[i], 0);
                }
            }

            byte[] dataToCheck = new byte[15];

            dataToCheck[0] = res._blocks[0].Flag;
            dataToCheck[1] = res._blocks[0].HightBits;
            dataToCheck[2] = res._blocks[0].LowBits;

            dataToCheck[3] = res._blocks[1].Flag;
            dataToCheck[4] = res._blocks[1].HightBits;
            dataToCheck[5] = res._blocks[1].LowBits;

            dataToCheck[6] = res._blocks[2].Flag;
            dataToCheck[7] = res._blocks[2].HightBits;
            dataToCheck[8] = res._blocks[2].LowBits;

            dataToCheck[9] = res._blocks[3].Flag;
            dataToCheck[10] = res._blocks[3].HightBits;
            dataToCheck[11] = res._blocks[3].LowBits;

            dataToCheck[12] = res._blocks[4].Flag;
            dataToCheck[13] = res._blocks[4].HightBits;
            dataToCheck[14] = res._blocks[4].LowBits;

            // expire date checksum
            res._blocks[dateChecksumIndex] = KeyBlock.Create(flags[dateChecksumIndex], rcr.ComputeChecksum(dataToCheck));

            // push id into key
            string flagedId = string.Concat(Encoding.ASCII.GetString(flags), id);
            keyBytes = rcr.ComputeChecksum(Encoding.UTF8.GetBytes(flagedId));
            res._keyValue = keyBytes;
            res._blocks[keyBlkIdx] = KeyBlock.Create(flags[keyBlkIdx], keyBytes);
            dataToCheck[keyBlkIdx * 3] = res._blocks[keyBlkIdx].Flag;
            dataToCheck[keyBlkIdx * 3 + 1] = res._blocks[keyBlkIdx].HightBits;
            dataToCheck[keyBlkIdx * 3 + 2] = res._blocks[keyBlkIdx].LowBits;

            // id checksum
            res._blocks[idChecksumIndex] = KeyBlock.Create(flags[idChecksumIndex], rcr.ComputeChecksum(dataToCheck));

            return res;
        }

        private static UInt16 Circle(UInt16 data, UInt16 key)
        {
            if(key== UInt16.MaxValue)return data;

            UInt32 len = UInt16.MaxValue + 1;

            UInt32 tmp = (UInt32)data + key + 1;
            if (tmp > len) tmp = (UInt16)(tmp % len);

            tmp = tmp - 1;

            return (UInt16)tmp;
        }

#endif

        private static UInt16 DeCircle(UInt16 data, UInt16 key)
        {
            //if (key == UInt16.MaxValue) return data;

            //UInt32 tmp = (UInt32)data - key;
            //UInt16 res;
            //if (tmp < 0) res = (UInt16)((UInt16.MaxValue - tmp) + 1);
            //else res = (UInt16)tmp;

            //return res;

            if (key == UInt16.MaxValue) return data;

            UInt32 len = UInt16.MaxValue + 1;

            UInt32 tmp = (UInt32)(data + 1) - key;
            if (tmp <= 0) tmp = len + tmp;

            tmp = tmp - 1;

            return (UInt16)tmp;
        }

        //private static UInt16 GetKeyBytes(string key) {
        //    UInt16 res = 0;
        //    byte[] keyData = Encoding.UTF8.GetBytes(key);
        //    foreach (byte b in keyData) {
        //        res = (UInt16)((res + b) % UInt16.MaxValue);
        //    }

        //    return res;
        //}

        public static ProductKey Parse(string key)
        {
            return Parse(key, null);
        }

        public static ProductKey Parse(string key, IKeyFlagingFunction fef)
        {
            try
            {
                string[] keys = key.ToUpper().Trim().Split(new char[] { '-' });

                ProductKey pk = new ProductKey();

                pk._blocks[0] = KeyBlock.Parse(keys[0]);
                pk._blocks[1] = KeyBlock.Parse(keys[1]);
                pk._blocks[2] = KeyBlock.Parse(keys[2]);
                pk._blocks[3] = KeyBlock.Parse(keys[3]);
                pk._blocks[4] = KeyBlock.Parse(keys[4]);

                pk.Decode(fef);

                return pk;
            }
            catch {
                throw new InvalidProductKeyFormatException();
            }
        }

        /// <summary>
        /// Cat chuoi thanh cac doan bang nhau
        /// </summary>
        /// <param name="str"></param>
        /// <param name="chunkSize"></param>
        /// <returns></returns>
        private static string[] ChunkSplit(string str, int chunkSize)
        {
            List<string> res = new List<string>();
            int stringLength = str.Length;
            for (int i = 0; i < stringLength; i += chunkSize)
            {
                if (i + chunkSize > stringLength) chunkSize = stringLength - i;
                res.Add(str.Substring(i, chunkSize));
            }
            return res.ToArray();
        }

        private static void SwapBytes(ref byte a, ref byte b)
        {
            byte tmp = a;
            a = b;
            b = tmp;
        }
    }

    class KeyBlock
    {
        const string _index = "0123456789ABCDEF";

        private KeyBlock() {
            Data = new byte[3];
        }

        public byte[] Data { get; private set; }

        public byte Flag
        {
            get
            {
                return Data[0];
            }
        }

        public byte HightBits
        {
            get
            {
                return Data[1];
            }
        }

        public byte LowBits
        {
            get
            {
                return Data[2];
            }
        }

        public static KeyBlock Empty
        {
            get {
                return new KeyBlock();
            }
        }

        public static KeyBlock Parse(string key) {
            KeyBlock kb = new KeyBlock();
            kb.Data[0] = (byte)_index.IndexOf(key[0]);

            kb.Data[1] = (byte)((_index.IndexOf(key[1]) << 4) | _index.IndexOf(key[2]));
            kb.Data[2] = (byte)((_index.IndexOf(key[3]) << 4) | _index.IndexOf(key[4]));

            return kb;
        }

#if _KEYGEN

        public static KeyBlock Create(byte flag, UInt16 data)
        {
            KeyBlock res = new KeyBlock();
            res.Data[0] = (byte)(flag & 0x00ff);
            res.Data[1] = (byte)(data >> 8);
            res.Data[2] = (byte)(data & 0x00ff);
            return res;
        }

        public static KeyBlock Create(byte flag, byte hightBits, byte lowBits) {
            KeyBlock res = new KeyBlock();
            res.Data[0] = (byte)(flag & 0x00ff);
            res.Data[1] = hightBits;
            res.Data[2] = lowBits;
            return res;
        }

        public static KeyBlock Create(byte flag,Random rd) {
            KeyBlock res = new KeyBlock();
            rd.NextBytes(res.Data);
            res.Data[0] = (byte)(flag & 0x00ff);
            return res;
        }

#endif

        public UInt16 GetUint16Value() {
            return (UInt16)((Data[1] << 8) | Data[2]);
        }

        public override string ToString()
        {
            return string.Concat(_index[Data[0] & 0x0f],
                _index[Data[1] >> 4], _index[Data[1] & 0x0f],
                _index[Data[2] >> 4], _index[Data[2] & 0x0f]);
        }
    }
}
