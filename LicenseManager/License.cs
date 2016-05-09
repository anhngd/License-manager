using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace LicenseManager
{
    public class License
    {
        #region Properties

        /// <summary>
        /// the license terms. obscured.
        /// </summary>
        public string LicenseTerms { get; set; }

        /// <summary>
        /// the signature.
        /// </summary>
        public string Signature { get; set; }

        #endregion

        #region Methods

        /// <summary>
        /// saves the license to an xml file.
        /// </summary>
        /// <param name="fileName"></param>
        public void Save(String fileName)
        {
            Serializer.Save<License>(this, fileName);
        }

        /// <summary>
        /// saves the license to a stream as xml.
        /// </summary>
        /// <param name="stream"></param>
        public void Save(Stream stream)
        {
            Serializer.Save<License>(this, stream);
        }

        /// <summary>
        /// create a license object from a license file.
        /// </summary>
        /// <param name="fileName"></param>
        /// <returns></returns>
        public static License Load(String fileName)
        {
            // read the filename:
            return Serializer.Load<License>(new FileInfo(fileName));
        }

        /// <summary>
        /// load a license from stream xml data.
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public static License Load(Stream data)
        {
            // read the data stream:
            return Serializer.Load<License>(data);
        }

        #endregion
    }
}
