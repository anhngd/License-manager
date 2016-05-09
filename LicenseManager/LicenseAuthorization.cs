using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization.Formatters.Binary;
using System.Security;
using System.Security.Cryptography;
using System.Text;

namespace LicenseManager
{
    /// <summary>
    /// handles license authorization.
    /// </summary>
    public abstract class LicenseAuthorization
    {
        /// <summary>
        /// terms of the license agreement: it's not encrypted (but is obscured)
        /// </summary>
        [Serializable]
        internal class LicenseTerms
        {
            /// <summary>
            /// Start date of the license agreement.
            /// </summary>
            public DateTime StartDate { get; set; }

            /// <summary>
            /// The last date on which the software can be used on this license.
            /// </summary>
            public DateTime ExpiryDate { get; set; }

            /// <summary>
            /// Limit number of users for the license agreement
            /// </summary>
            public int NumberOfUsers { get; set; }

            /// <summary>
            /// ID of product that need to get license
            /// </summary>
            public string ProductId { get; set; }

            /// <summary>
            /// Registered Organisation name for the license agreement.
            /// </summary>
            public String OrganisationName { get; set; }

            /// <summary>
            /// returns the license terms as an obscure (not human readable) string.
            /// </summary>
            /// <returns></returns>
            public String GetLicenseString()
            {
                using (var ms = new MemoryStream())
                {
                    var bf = new BinaryFormatter();
                    bf.Serialize(ms, this);
                    return Convert.ToBase64String(ms.GetBuffer());
                }
            }

            /// <summary>
            /// returns a binary representation of the license terms.
            /// </summary>
            /// <returns></returns>
            public byte[] GetLicenseData()
            {
                using (var ms = new MemoryStream())
                {
                    // create a binary formatter:
                    var bnfmt = new BinaryFormatter();

                    // serialize the data to the memory-steam;
                    bnfmt.Serialize(ms, this);

                    // return a base64 string representation of the binary data:
                    return ms.GetBuffer();

                }
            }

            /// <summary>
            /// create a new license-terms object from a string-representation of the binary
            /// serialization of the licence-terms.
            /// </summary>
            /// <param name="licenseTerms"></param>
            /// <returns></returns>
            internal static LicenseTerms FromString(String licenseTerms)
            {

                using (var ms = new MemoryStream(Convert.FromBase64String(licenseTerms)))
                {
                    // create a binary formatter:
                    var bnfmt = new BinaryFormatter();

                    // serialize the data to the memory-steam;
                    var value = bnfmt.Deserialize(ms);

                    if (value is LicenseTerms)
                        return (LicenseTerms)value;
                    else
                        throw new ApplicationException("Invalid Type!");

                }
            }

        }

        /// <summary>
        /// builds a user-license pack. This includes the public-key that must be embedded in the application,
        /// and the private key (which must be kept secure) and a license-file for each user, specific to the 
        /// currently executing assembly, with the specified end date. Start date for the user-license file is
        /// current date.
        /// </summary>
        /// <param name="outputFolder"></param>
        /// <param name="userNames"></param>
        /// <param name="endDates"></param>
        public static void GenerateLicensePack(String outputFolder, String[] userNames, DateTime[] endDates)
        {
            // if the key files don't exist..create them:
            if (!File.Exists(outputFolder + "\\privateKey.xml"))
                GenerateLicenseResources(outputFolder);

            // generate each user-license for the current assembly:
            var i = 0;
            foreach (var userName in userNames)
            {
                // generate each license file:
                GenerateUserLicenseFile(outputFolder, userName, endDates[i++]);
            }
        }

        /// <summary>
        /// generate the public and private key files in the specified folder.
        /// </summary>
        /// <param name="outputFolder"></param>
        public static void GenerateLicenseResources(String outputFolder)
        {
            // create the directory if it doesn't exist:
            if (!Directory.Exists(outputFolder))
            {
                Directory.CreateDirectory(outputFolder);
            }

            // generate the required key files:
            var publicKeyFile = outputFolder + "\\publicKey.xml";
            var privateKeyFile = outputFolder + "\\privateKey.xml";

            // create a new private key:
            var privateKey = GeneratePrivateKey();

            // extract the public part of the key:
            var publicKey = GetPublicKey(privateKey);

            // save them:
            File.WriteAllText(publicKeyFile, publicKey);
            File.WriteAllText(privateKeyFile, privateKey);

        }

        /// <summary>
        /// generate a user-license file.
        /// </summary>
        /// <param name="licenseResourceFolder"></param>
        /// <param name="orgName"></param>
        /// <param name="endDate"></param>
        public static void GenerateUserLicenseFile(String licenseResourceFolder, String orgName, DateTime endDate)
        {

            // find and load the private key:
            var privateKeyFile = licenseResourceFolder + "\\privateKey.xml";

            // check the key file exists:
            if (File.Exists(privateKeyFile))
            {
                // load the private key:
                var privateKey = File.ReadAllText(privateKeyFile);
                // generate the license file;
                var license = CreateLicense(
                    DateTime.Now, endDate,
                    Assembly.GetExecutingAssembly().FullName,
                    orgName,
                    privateKey
                    );

                // save the license file:
                license.Save(licenseResourceFolder + "\\" + orgName + ".lic");
            }
            else
            {
                //MessageBox.Show("Can't find private-key file: " + privateKeyFile);
            }
        }

        /// <summary>
        /// generate a new, private key. this will be the master key for generating license files.
        /// </summary>
        /// <returns></returns>
        public static String GeneratePrivateKey()
        {
            var dsa = new RSACryptoServiceProvider();
            return dsa.ToXmlString(true);
        }

        /// <summary>
        /// get the public key from a private key. this key must be distributed with the application.
        /// </summary>
        /// <param name="privateKey"></param>
        /// <returns></returns>
        public static String GetPublicKey(String privateKey)
        {
            var dsa = new RSACryptoServiceProvider();
            dsa.FromXmlString(privateKey);
            return dsa.ToXmlString(false);
        }

        /// <summary>
        /// use a private key to generate a secure license file. the private key must match the public key accessible to
        /// the system validating the license.
        /// </summary>
        /// <param name="start">applicable start date for the license file.</param>
        /// <param name="end">applicable end date for the license file</param>
        /// <param name="productName">applicable product name</param>
        /// <param name="userName">user-name</param>
        /// <param name="privateKey">the private key (in XML form)</param>
        /// <returns>secure, public license, validated with the public part of the key</returns>
        public static License CreateLicense(DateTime start, DateTime end, String productId, String userName, String privateKey)
        {
            // create the licence terms:
            var terms = new LicenseTerms()
            {
                StartDate = start,
                ExpiryDate = end,
                ProductId = productId,
                OrganisationName = userName
            };

            var rsa = new RSACryptoServiceProvider();
            rsa.FromXmlString(privateKey);
            var license = terms.GetLicenseData();
            var signature = rsa.SignData(license, null);
            return new License()
            {
                LicenseTerms = Convert.ToBase64String(license),
                Signature = Convert.ToBase64String(signature)
            };
        }

        /// <summary>
        /// validates the license and if the app should run; if the license is valid the 
        /// method will complete, if not it will throw a security exception.
        /// </summary>
        /// <param name="license">
        /// the license object.
        /// </param>
        /// <exception cref="SecurityException">thrown if the license is invalid or expired</exception>
        /// <returns></returns>
        public static void ValidateLicense(License license, String publicKey)
        {
            // get the valid terms for the license: (this checks the digital signature on the license file)
            var terms = GetValidTerms(license, publicKey);

            // ensure a valid license-terms object was returned:
            if (terms != null)
            {
                // validate the date-range of the license terms:
                if (DateTime.Now.CompareTo(terms.ExpiryDate) <= 0)
                {
                    if (DateTime.Now.CompareTo(terms.StartDate) >= 0)
                    {
                        // date range is valid... check the product name against the current assembly
                        if (Assembly.GetExecutingAssembly().FullName == terms.ProductId)
                        {
                            return;
                        }
                        else
                        {
                            // product name doesn't match.
                            throw new SecurityException("Invalid Product Name: " + terms.ProductId);
                        }
                    }
                    // license terms not valid yet.
                    throw new SecurityException("License Terms Not Valid Until: " + terms.StartDate.ToShortDateString());
                }
                else
                {
                    // license terms have expired.
                    throw new SecurityException("License Terms Expired On: " + terms.ExpiryDate.ToShortDateString());
                }
            }
            else
            {
                // the license file was not valid.
                throw new SecurityException("Invalid License File!");
            }
        }

        /// <summary>
        /// validate license file and return the license terms.
        /// </summary>
        /// <param name="license"></param>
        /// <param name="publicKey"></param>
        /// <returns></returns>
        internal static LicenseTerms GetValidTerms(License license, String publicKey)
        {
            // create the crypto-service provider:
            var dsa = new RSACryptoServiceProvider();

            // setup the provider from the public key:
            dsa.FromXmlString(publicKey);

            // get the license terms data:
            var terms = Convert.FromBase64String(license.LicenseTerms);

            // get the signature data:
            var signature = Convert.FromBase64String(license.Signature);

            // verify that the license-terms match the signature data
            if (dsa.VerifyData(terms, null, signature))
                return LicenseTerms.FromString(license.LicenseTerms);
            else
                throw new SecurityException("Signature Not Verified!");
        }

        /// <summary>
        /// runs a test of the licensing system from:
        /// C:\temp\user2.lic and C:\temp\user2_publicKey.xml
        /// </summary>
        public static void TestTestLicense()
        {
            var l = License.Load(@"c:\temp\user2.lic");
            var pkey = File.ReadAllText(@"c:\temp\user2_publicKey.xml");

            if (l != null)
            {
                try
                {
                    ValidateLicense(l, pkey);
                    Console.WriteLine("License is Valid");
                }
                catch (SecurityException se)
                {
                    Console.WriteLine("License INVALID: " + se.Message);
                }
            }
        }

        /// <summary>
        /// generate the required files for TestTestLicense()
        /// C:\temp\user2.lic and C:\temp\user2_publicKey.xml
        /// </summary>
        public static void SaveTestLicense()
        {
            var privateKey = GeneratePrivateKey();

            var l = CreateLicense(DateTime.MinValue, DateTime.MaxValue, Assembly.GetExecutingAssembly().FullName, "Simon", privateKey);
            l.Save(@"C:\temp\user2.lic");
            File.WriteAllText(@"C:\temp\user2_publicKey.xml", GetPublicKey(privateKey));
            File.WriteAllText(@"C:\temp\system_privateKey.xml", privateKey);
        }
    }
}
