using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Management;
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
            var pid = curVersion?.GetValue("ProductId").ToString();
            return pid;
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
            var productId = GetServerProductId();
            var hddNo = GetHddSerialNo();
            var temp = productId + "TDSECURITY" + hddNo;
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
}
