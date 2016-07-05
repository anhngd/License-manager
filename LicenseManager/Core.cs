using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Management;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;

namespace LicenseManager
{
    public class Core
    {
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public static string GetServerProductId()
        {
            
            var curVersion = Registry.LocalMachine.OpenSubKey("SOFTWARE\\Microsoft\\Windows NT\\CurrentVersion");
            
            if (curVersion == null) return string.Empty;
            
            try
            {
                var a = curVersion.GetSubKeyNames();
                var pid = curVersion.GetValue("DigitalProductId").ToString();
                return pid;
            }
            catch (Exception ex)
            {
                return string.Empty;
            }
        }

        public static string GetMacAddress()
        {
            var objMos = new ManagementObjectSearcher("Win32_NetworkAdapterConfiguration");
            var objMoc = objMos.Get();
            var MACAddress = String.Empty;
            foreach (var o in objMoc)
            {
                var objMo = (ManagementObject) o;
                if (MACAddress == String.Empty) // only return MAC Address from first card   
                {
                    MACAddress = objMo["MacAddress"].ToString();
                }
                objMo.Dispose();
            }
            MACAddress = MACAddress.Replace(":", "");
            return MACAddress;
        }

        public static string GetCpuId()
        {
            return CpuId.ProcessorId();
        }
        
        /// <summary>
        /// Lấy tất cả thông tin của ổ cứng
        /// </summary>
        /// <returns></returns>
        static public HardDrive GetAllDiskDrives()
        {
            var hd = new HardDrive();
            var searcher = new ManagementObjectSearcher("SELECT * FROM Win32_DiskDrive");

            foreach (var wmiHd in searcher.Get().Cast<ManagementObject>())
            {
                hd.Model = wmiHd["Model"].ToString().Trim();
                hd.InterfaceType = wmiHd["InterfaceType"].ToString().Trim();
                hd.Caption = wmiHd["Caption"].ToString().Trim();
                hd.SerialNo = wmiHd.GetPropertyValue("SerialNumber").ToString().Trim();//get the serailNumber of diskdrive
                return hd;
            }
            return hd;
        }

        /// <summary>
        /// Get model of hdd
        /// </summary>
        /// <returns></returns>
        static public string GetHddModel()
        {
            return GetAllDiskDrives().Model;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        static public string GetHddInterfaceType()
        {
            return GetAllDiskDrives().InterfaceType;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        static public string GetHddCaption()
        {
            return GetAllDiskDrives().Caption;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        static public string GetHddSerialNo()
        {
            return GetAllDiskDrives().SerialNo;
        }
        #region

        /// <summary>
        /// Get Hash
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static string EncryptMd5(string value)
        {
            if (string.IsNullOrEmpty(value))
                return string.Empty;
            var md5 = new MD5CryptoServiceProvider();
            var valueArray = Encoding.ASCII.GetBytes(value);
            valueArray = md5.ComputeHash(valueArray);
            var sb = new StringBuilder();
            foreach (var t in valueArray)
                sb.Append(t.ToString("x2").ToLower());
            return sb.ToString();
        }

        /// <summary>
        /// Mã hóa bằng SHA1
        /// </summary>
        /// <param name="inputValue">chuỗi gốc</param>
        /// <returns>Chuỗi mã hóa</returns>
        public static string EncryptSha1(string inputValue)
        {
            if (string.IsNullOrEmpty(inputValue))
                return string.Empty;
            var sha = new SHA1CryptoServiceProvider();
            var valueArray = Encoding.ASCII.GetBytes(inputValue);
            valueArray = sha.ComputeHash(valueArray);
            var outputValue = new StringBuilder();
            foreach (var t in valueArray)
                outputValue.Append(t.ToString().ToLower());
            return outputValue.ToString();
        }
        #endregion
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        static public string GetServerId()
        {
            //var productId = GetServerProductId();
            //var macAdd = GetMacAddress();
            var cpuid = GetCpuId();
            var hddNo = GetHddSerialNo();
            var temp = cpuid + "TDSECURITY" + hddNo;
            var hash = EncryptSha1(temp);
            var id = string.Empty;
            for (var i = 0; i < hash.Length; i += 5)
            {
                if (i > 20) break;
                var t = hash.Substring(i, 5);
                id += string.IsNullOrEmpty(id) ? t : "-" + t;
            }
            return id;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        static public string GetProductId(string serverId, string aid)
        {
            var temp = serverId + "TDAPP" + aid;
            var hash = EncryptSha1(temp);
            var id = string.Empty;
            for (var i = 0; i < hash.Length; i += 5)
            {
                if (i > 20) break;
                var t = hash.Substring(i, 5);
                id += string.IsNullOrEmpty(id) ? t : "-" + t;
            }
            return id;
        }
    }

    /// <summary>
    /// 
    /// </summary>
    public class HardDrive
    {
        /// <summary>
        /// 
        /// </summary>
        public string Model { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string InterfaceType { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string Caption { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string SerialNo { get; set; }
    }

    public class CpuId
    {
        [DllImport("user32", EntryPoint = "CallWindowProcW", CharSet = CharSet.Unicode, SetLastError = true, ExactSpelling = true)]
        private static extern IntPtr CallWindowProcW([In] byte[] bytes, IntPtr hWnd, int msg, [In, Out] byte[] wParam, IntPtr lParam);

        [return: MarshalAs(UnmanagedType.Bool)]
        [DllImport("kernel32", CharSet = CharSet.Unicode, SetLastError = true)]
        public static extern bool VirtualProtect([In] byte[] bytes, IntPtr size, int newProtect, out int oldProtect);

        const int PAGE_EXECUTE_READWRITE = 0x40;



        public static string ProcessorId()
        {
            var sn = new byte[8];

            return !ExecuteCode(ref sn) ? "ND" : string.Format("{0}{1}", BitConverter.ToUInt32(sn, 4).ToString("X8"), BitConverter.ToUInt32(sn, 0).ToString("X8"));
        }

        private static bool ExecuteCode(ref byte[] result)
        {
            int num;

            /* The opcodes below implement a C function with the signature:
             * __stdcall CpuIdWindowProc(hWnd, Msg, wParam, lParam);
             * with wParam interpreted as an 8 byte unsigned character buffer.
             * */

            var codeX86 = new byte[] {
            0x55,                      /* push ebp */
            0x89, 0xe5,                /* mov  ebp, esp */
            0x57,                      /* push edi */
            0x8b, 0x7d, 0x10,          /* mov  edi, [ebp+0x10] */
            0x6a, 0x01,                /* push 0x1 */
            0x58,                      /* pop  eax */
            0x53,                      /* push ebx */
            0x0f, 0xa2,                /* cpuid    */
            0x89, 0x07,                /* mov  [edi], eax */
            0x89, 0x57, 0x04,          /* mov  [edi+0x4], edx */
            0x5b,                      /* pop  ebx */
            0x5f,                      /* pop  edi */
            0x89, 0xec,                /* mov  esp, ebp */
            0x5d,                      /* pop  ebp */
            0xc2, 0x10, 0x00,          /* ret  0x10 */
        };
            var codeX64 = new byte[] {
            0x53,                                     /* push rbx */
            0x48, 0xc7, 0xc0, 0x01, 0x00, 0x00, 0x00, /* mov rax, 0x1 */
            0x0f, 0xa2,                               /* cpuid */
            0x41, 0x89, 0x00,                         /* mov [r8], eax */
            0x41, 0x89, 0x50, 0x04,                   /* mov [r8+0x4], edx */
            0x5b,                                     /* pop rbx */
            0xc3,                                     /* ret */
        };

            var code = IsX64Process() ? codeX64 : codeX86;

            var ptr = new IntPtr(code.Length);

            if (!VirtualProtect(code, ptr, PAGE_EXECUTE_READWRITE, out num))
                Marshal.ThrowExceptionForHR(Marshal.GetHRForLastWin32Error());

            ptr = new IntPtr(result.Length);

            try
            {
                return (CallWindowProcW(code, IntPtr.Zero, 0, result, ptr) != IntPtr.Zero);
            }
            catch { return false; }
        }

        private static bool IsX64Process()
        {
            return IntPtr.Size == 8;
        }

    }
}
