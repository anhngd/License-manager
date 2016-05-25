using System;
using System.CodeDom;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Remoting.Channels;
using System.Runtime.Serialization.Formatters.Binary;
using System.Security;
using System.Security.Cryptography;
using System.Text;
using System.Web;

namespace LicenseManager
{
    /// <summary>
    /// handles license authorization.
    /// </summary>
    public abstract class LicenseAuthorization
    {
        const string PubKey = "<RSAKeyValue><Modulus>4E8nZs+eqArDA8NSI7unxoLNLDFVgKCiftQAIpo6XbsbJ0b5kIgJFMglYtejdtWu77/3EkxP3SV6Qk19X5DLzfUyHM7V9gvvKl/qKoMjLgRcU0SGO0EdltAsJeXHTnebwY9fARbsOulTipsY9hXSyIktuljWOWFNLkwrxL1zPFOKRjzrqBL4d3GcPIc2OpwpkUDvKnsa8WdiLEuQCjRdPMZj7TYXnUdgqMP6d4YI2EXTanfa51DxTppSHztAM2+PiUpyRshikT5Bcs+gCpufvU73ZbMWJpiU41ybWnKFO+hYFlMmylDmuzdQbMP5ToL8AJjaKkTX1KhFfG+kaKtGTZX39Rx53f8Db3QG9OGTYXsDEK3Q52OfVm4ZQdot1Eh4wGkHJFp8uCQ2MVfCXlQf7MS9XQ0BATASWNLzsmxQkjQn/FOWSYp3zC/AchSYUHITDC79HI7plW/cCA40P57yX+aFZfh0YDlY2KtcaF9tYHT+RCzob673IVOnlsnk0nvvzzYlxvolcK1pUPO12hERe2DIl+a6uN/lGxJAWeqplNwFQ+bNLnn5394og0IvKY+vMSBZXSe66vP1RWuEw9OsXf+1Kip1lhtfQbW9XvjUNk6f/xT1aZChrAYhR47op257DbCHCveSCx0AP6wdmjxGx0GL2jn9b0MjHM0PWtiLtXshnA/K7hJxKnSpbG/MDT4WZS+ovDgKoDrSMyL54rtMXAHNkxGWxB+vcSsD9pc4tv5YX+Ui5wrf/4QazfUx0seSla1enExruAZ40yKjXfpXZ/RE+kGkhMiwe3m4k9FbyjplNClaDqel116Lm4Ona8NNXe0oPx54QyeaeYhScW2+RioPhO1ilVMd8GjtTVr3CUmREmjykcvpWxwSTc4e8mk+nXxCIqMyRHn7EXEhOgFGAHYUixWMuwnv2eKdStkWK/Am4fRWmZKHqHCBWnNMKAiBGklH7X3ArRKLvBKDgtsNN22ypt46wnz4K1TH/E1UMdvPhiVbxK/s6mCPQMsOcJ1r1ByrIcZ55yKJI1RG40rmmmCJbM17INFv8ytX/y6z6sOZvcgjh22mnUK9j8eqlnSEDkaI+/9gdxtelbdT6zeTifBUVpZWdBzAMgZId+a7hbm6++F8rRpRDtaJT8VxgDxsqOvPm1WoBGn/Y4v3J4NRSJALg30gIad67drT+Yrn3k+UI7MM68bD2aOsRREqs7OmWozn5sJ0q+q2qpVd9r4oqKKiI37nDMC36489oZXpwXIyqzjDkYT4jPzee8EG39pmkCkViSRt2IlI/ILhzeE4h7KpHtIZ0M7l4kyWlAwNSy60m+wagBfou1LoyDVPJWVJyK7vPk9FJ3JR0ovGcmJ4Yw==</Modulus><Exponent>AQAB</Exponent></RSAKeyValue>";
        const string PrvKey = "<RSAKeyValue><Modulus>4E8nZs+eqArDA8NSI7unxoLNLDFVgKCiftQAIpo6XbsbJ0b5kIgJFMglYtejdtWu77/3EkxP3SV6Qk19X5DLzfUyHM7V9gvvKl/qKoMjLgRcU0SGO0EdltAsJeXHTnebwY9fARbsOulTipsY9hXSyIktuljWOWFNLkwrxL1zPFOKRjzrqBL4d3GcPIc2OpwpkUDvKnsa8WdiLEuQCjRdPMZj7TYXnUdgqMP6d4YI2EXTanfa51DxTppSHztAM2+PiUpyRshikT5Bcs+gCpufvU73ZbMWJpiU41ybWnKFO+hYFlMmylDmuzdQbMP5ToL8AJjaKkTX1KhFfG+kaKtGTZX39Rx53f8Db3QG9OGTYXsDEK3Q52OfVm4ZQdot1Eh4wGkHJFp8uCQ2MVfCXlQf7MS9XQ0BATASWNLzsmxQkjQn/FOWSYp3zC/AchSYUHITDC79HI7plW/cCA40P57yX+aFZfh0YDlY2KtcaF9tYHT+RCzob673IVOnlsnk0nvvzzYlxvolcK1pUPO12hERe2DIl+a6uN/lGxJAWeqplNwFQ+bNLnn5394og0IvKY+vMSBZXSe66vP1RWuEw9OsXf+1Kip1lhtfQbW9XvjUNk6f/xT1aZChrAYhR47op257DbCHCveSCx0AP6wdmjxGx0GL2jn9b0MjHM0PWtiLtXshnA/K7hJxKnSpbG/MDT4WZS+ovDgKoDrSMyL54rtMXAHNkxGWxB+vcSsD9pc4tv5YX+Ui5wrf/4QazfUx0seSla1enExruAZ40yKjXfpXZ/RE+kGkhMiwe3m4k9FbyjplNClaDqel116Lm4Ona8NNXe0oPx54QyeaeYhScW2+RioPhO1ilVMd8GjtTVr3CUmREmjykcvpWxwSTc4e8mk+nXxCIqMyRHn7EXEhOgFGAHYUixWMuwnv2eKdStkWK/Am4fRWmZKHqHCBWnNMKAiBGklH7X3ArRKLvBKDgtsNN22ypt46wnz4K1TH/E1UMdvPhiVbxK/s6mCPQMsOcJ1r1ByrIcZ55yKJI1RG40rmmmCJbM17INFv8ytX/y6z6sOZvcgjh22mnUK9j8eqlnSEDkaI+/9gdxtelbdT6zeTifBUVpZWdBzAMgZId+a7hbm6++F8rRpRDtaJT8VxgDxsqOvPm1WoBGn/Y4v3J4NRSJALg30gIad67drT+Yrn3k+UI7MM68bD2aOsRREqs7OmWozn5sJ0q+q2qpVd9r4oqKKiI37nDMC36489oZXpwXIyqzjDkYT4jPzee8EG39pmkCkViSRt2IlI/ILhzeE4h7KpHtIZ0M7l4kyWlAwNSy60m+wagBfou1LoyDVPJWVJyK7vPk9FJ3JR0ovGcmJ4Yw==</Modulus><Exponent>AQAB</Exponent><P>4KATaFO2J0Zmd1QTFuiMQUgYtpNrKmCBJXsXweZqgOaeaQk15EVeaXLcZpB2nhxFaVpdrMEFgnTTijbout2j9wO0Xu7GEJRzEESoqyVGkTsZ3GR72AVeySiKn30B8xGw6YDsvHm3X8vm/rUWDyQ0ESTxJIokL/Dv9UP96qMDUBXiq7JYCalS4DMYfZazqVq4vFFRlbY6sZVi6L890+kfJnUxWvwWr59DZZXZfCwgGELy/EV/VlRVNUN6332u0TnApnCVnFBfhd/XSD90uawGP7xYHck9SlBoKjTSBPnCOvCwrV1Zjsva0RFTBzuodTuNVVRT7jHbwByIXFHZ7K59LYuabKeDekifX9btFlK/70OZGnc0j5FDE+Bh2woLQIftCWGktQMzsUd8DYQP9bQRS5+L0VvpJATFaScLCwd/rdwPd8k2OfWC/KGSauQ7vv0FYwBII0flNiER/tiWuflLuCWzbFkysylpm7wfR7bHINXYPd14EhaHF7y5J2mOCVHVQ+VF4o9Ysl6eIBAaL+9wDsihRnGaZ+w2Jo4ijTie3o6eC+lSxj1zro/7p3q/CGnInAIDPVE+IVHw4kfdeEjLaLPbDyzMbFD1UmCQhMKyiBMiCiJw6MBwhJGOMOrr05JMjyDW4HH6QSOGgJgrritDG4Sd1995sn2iAVHpHGZXOz0=</P><Q>/6PGeH47pje2gi8DeqMi/KBXmoqEHU1NodaXdWS4s/aHuDt5W5pni3VLPWyLZPuaBcwYsfcgk4Of009rl4VSf6iue+knt90gzwX22Q0iGrtLOt9hMnbPkr6p3EVc9p+P4gNK+bFn+1qmzwnyWFmhhoTclBL8O+7BoYC+T7UMoNgN41B+SZBi6Rp32lNADc7QkO1FdM0Lf3gXU+tOXGhOhGde3UKWTqFCsmwS8VgJW52HyN9p28NShjXhTzcu/N9lr3B/N4EFjFoi8ogQCQ6ziJs11jNXc3MVAp1Cqyv0buitGWFXgPyF9SgHRDyMSlNyXeVD/wcLIGyo5LCRuFouAnSF+prFppKjR+3FXg6TR56KYkgOcU2AvzwrzBuUfxGuzdIXaNFWqxS/pVEuNdEEFi4DUw8q4Ysz4CdMO4ohni5y0lQ6jvIDH7l/+dlbKdplkLqorKIZ1CKEVOf5fj6yWdUMfVKeeTsV+7Mf/BgQvKch/LdAf5oWoT86vbSDTEYJSYSAi34fgW9tz/jUmdWT2hC62iX18BwVTlq63S4LaTviH5dV6iOfsPWpkHSYXrZYFMG04Zm8nYQ812tqk9hpVNEYuUR7AbhsZhfaCAz24HDcz0lgA7BJx6beM1ddMkfPTIKCUWel2fL1seygtNT5uE+puiAdsbJtsaGEWoqwPB8=</Q><DP>JbH3T8yshs7Dp0bOpUuFLfdhXlJC66uiQJ31kK6QNO1+q/XFu3cArwV9gyC/Jy1rYJCvo6wItqnQTiOEzscfGvbeMAg16NZgsNw0yfPJcdnPLzMVrVzJWUyaZsVdrGLNo4HlOIPciBD5xMiy2PukWTQv24frJrhyLGCqndOZXLkvfszBBW4K4GDyvTCrGcKFueSTomIgW2jMXm6pe4Rlm9iS3cfsLrC1aLjK9JGbGTcy+67V/bDNU9DtL9AxpB7i+4ttL/w064xy3kb32ajKq6t5xhXdWTlBIcQXcopPVADMRWui+dmv8OHNYytZy8Kgpy8tno1zaTvgjk2WGqf1jc0drxo1cDH7vOOeid59IzPe35gdk/I2pYFEKrP2y2+EEcz3DQFqjVeHs+TzoLIFLQGrPwkDiO3cCxdoPQOXI6AIYHu9w+A1iRu8LH/8nsJrQv/rGzCXgIvwLyo9sRvq3w5Em+wkG/jZJi9duHUt2Y7JaShfW/m31f3vvHN0SJAZD/nLzodrxRMnyg2pt12hGnoExoON+96h/7pehgB4eLTKRQl0wJvTa/nxq1tn74uBBN05mfKmUIUlvXN98Q+WQw3btyI3c/3qDVbzKbRarYc4cSbyd7zDpRugFwl1Zd3pRyOlq6i07HV/vtjKi8Hq0xVgP7fa/l8i7sHGMuPhw6k=</DP><DQ>iQ4FGhSTZkn4T1NlShqsZ0SPswsQSaVoOZwTF50oaVOC8ALyzWm6mcoIzKWlFj7OFM2Xb9JOZMfsqB2sUU/pKwgEg+o8oRaswFEyRqwD3NENREL+mU+ZedM+vhlx/ccOHN4nFDdIyDmRtO5gP33OW5BfsgN7t6YJqcycO5+VZaZvy68cBdSjMtY96482iDCZIRoUYsqLn4uC9jrxiBlj/xhsg7NvYjiSyZWnkjzgx31tjwQ4fNn7JHk+knnDd09N9QEbM2a40saAdgb8kd6P4m1UL8LW386TNROa6qpmIhAcvb6yazT3nLgZLTxI+pBCgJSqH9Bm5rta0UDY3z157VcLfY/855E2eRnA9uxZicw15Ps8wwIKRHy9/m/K7VVNootAgbG6BX7aQIeFftTQnBOPNLitBL7c0RBAjqs/2s6MQLdnrbuScS/CrPEbLxUn3rA0Plmp/1GCIQM9f7GdbQwL5iSlUdmbr5YZT86XKo1wa0zUB5VVDJ4ZoBtnST1DPWcMg7zap0pWVWnSkacv3Xmas9omQNTNl3SNOoe1FnZEhStdpGn086rBffEEv8KC9p3jnBAQT2LWO6gbO6iK0C5KUwmj6Qg3FYU0H5zTvwDDYV61W0ToSWHtPxlXnd1iBmLx+zLWeEMJ9pomjsygFXCgTmlpghtnNBSkIOa6FYE=</DQ><InverseQ>jn5fh/RW91R1IH1GIfr9GC7IKTrUKX55n4PVeSP/038srBg5O09cuiWwM3aQFx87XaHXW/rnYlbcyvW5X3msDbIRagYnP3xIbJzkDeJwbfZOkfYjQBOf3tpHtzFhrDjd8vAKYwZE+7orScl02tP1kgmORxv9QdF4xzwaCBhLMGz9a94x0bkN8Ij/YivkNax8ZWpsedEja90xaEQsTf7OfmBMQmBUJRa+1UOMSvwgpvqFttfEvO92FToAX1H6BRKxEs/OHtf7cKlWSJEtRfMrk5qjbWYWJU4Cg1tn4USaTzx1ZU4bUk015gAT+bGdSI+hhG11kiVZPn+7bN8Vf9lREMrp0ENjoDaGyEySduv6QNQ0orUEWwmb2z3/AGP4nO4WwVGaLmD8CrqbWp0lz0fTDvAx77jfw6C5P+TCAEJdlN6AthFOPbQQsy2JqfCBRyisui8fc8WGXvslEvNlLga+syArm67F5bTK/hkNjyIIawouYedVb+aQpRKF7OQDpth5HW3iBZf3oRT2OiQgTz9vKOqIsHpAlKf7q3zqgLB/DTYOMbavEqePfOM4my8DUCRhZG0t55wYrjSsnuOV/t9RmAJUYjNgMI4KQicp9ENq76Ldz2rudr1ptarvkqOL7HdSw3+Ys743AGFa3IsUy+5jlkw6hMxDvytTQ0M0FHhlWA8=</InverseQ><D>LScx5svHg8s6y4CfgcruMJM3CvzZWd28KCyz5ENImYKnKiZq6XHA5jqN8BI5eRv1rZRmf3v2Ha3EKSGhKcIjjI+dqVfnkTKAz/Zj1G/2fnZpQWfLX5UxiCDaqjxRfRWtuG6McNM09XzvkLi88xiciDq5BkiIwx83oqi5rM54Kn4kmRP9WiWLd7vaeAK941l5MqCwJEyItWyawhDp62v0Et5a0i7v3Py30/Ezc6j7EdIoh0eePK86DBkbi+bo+xGrgrcGPwJqeRNfqGVsMYZ3Ruvk+EdvH5Dac9Q5/6CK+m7AgKn3ZoWXlLtDSMnAX1IVuWcLj7OxAMFoz25H2bwUmFymf+ReEdaaUe640cWw3dAd41ch9NpEgmkCU5PAYvm9uPnLcsF2VrkPYappWYS0snw+JEOQXrVXFCNO5C1SoMO4At6uo5oomGVKZ4yGxrbX571zvTJ1idvtHoABe4cbAjwMwWScxJv2qxL/tJD1GxQh9QZXOdguLjrBQyTcF5TURtF/FElZjQ+TYjTU+BM0gjIO3ykWkE2duG/ZnWAUB/+sXalRx1fNeYemfxmtFcAzQ3/gDVUIlvBKANHudV8gVQG67cSDTBaxe7BCcSJE0bPNGFOXPUtciBzpyRZW9hFS2wQcgxQoBW2ulaKniNs2YjjrFqBSyHlq0Yrj2uU5ZmwEv+WGfWgCmphCNIZ+NQNUx/PahuXdvGAMZqrfUpXOsP42dsbr9+aqGpef08V5+FCIIUQPYOM0WezANXGIWOFurW0PDlIAqxc5mdXPSf12VhM83/jrWuz6NqijYAQaPLXN/rF1TMGsS2CRv5bd3R7bU2l6ltDlnIXupA/rj/umcOqOWGGc+VO37FoN5Gct3/Q7kyGvhXDck2AO+eC1URmjehPpv8ZZXSxUmlSbunC5/y8fIhGuozILGh0lmLJvJT0UAgl8MaCZAYX0mfJDnsnYfnaoJJg7OlsTsqWiTR8ck5w/4EprWtKfKTdDpnGuRgRp++0CgkgT4mJb7UlWlhKYgYlH/m6kpLPKEbsgfYInEL7VVP/JsY888aTnqzvQKaAH1LMr2IFRerqYqckw14XfhsiECXofH+ToBKist0p9lQTJMQzCrDjMvWH2BxS3c9gu2v83v8S3YQXJYk0T/Vtz9Jionij8m8nbaD2iYqiM1EARwWa2x8FvlarnjS2xSNXKxVWA+340F+lQ/ofrHFErJBz2Ghab1kIiQgUuFeQygXTNjeTyN2fbIhTUVbeooyGdtUTboBy2FnsnS+Wyj/sYBsYRgUq/AevUdwrQ3SXODVi+aCnGAQ/UiPI1HulUD6p8G7my9IOsVVDVesddtfMpn2vJRO1N726eTYqz9XWkxQ==</D></RSAKeyValue>";
        const string LicenseResourceFolder = "C:\\";
        private const string RedirectPage = "/_tandan/license.aspx";

        /// <summary>
        /// terms of the license agreement: it's not encrypted (but is obscured)
        /// </summary>
        [Serializable]
        public class LicenseTerms
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
            /// Limit number of groups for the license agreement
            /// </summary>
            public int NumberOfGroups { get; set; }


            /// <summary>
            /// Registered Organisation name for the license agreement.
            /// </summary>
            public String OrganizationName { get; set; }

            /// <summary>
            /// ID of product that need to get license
            /// </summary>
            public string ProductId { get; set; }
            
            public string ProductKey { get; set; }

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
                File.ReadAllText(privateKeyFile);
                // generate the license file;
                var license = CreateLicense(
                    DateTime.Now, endDate,
                    Assembly.GetExecutingAssembly().FullName,
                    orgName);

                // save the license file:
                license.Save(licenseResourceFolder + "\\" + orgName + ".lic");
            }
            else
            {
                //MessageBox.Show("Can't find private-key file: " + privateKeyFile);
            }
        }


        private void GenerateLicense()
        {
            var assemblyName = Assembly.GetExecutingAssembly().GetHashCode();
            // check the key file exists:
            //license.Save(licenseResourceFolder + "\\" + orgName + ".lic");
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

       
        public static License CreateLicense(DateTime start, DateTime end, String productId, String organisationName)
        {
            // create the licence terms:
            var terms = new LicenseTerms()
            {
                StartDate = start,
                ExpiryDate = end,
                ProductId = productId,
                OrganizationName = organisationName
            };

            var rsa = new RSACryptoServiceProvider();
            rsa.FromXmlString(PrvKey);
            var license = terms.GetLicenseData();
            var signature = rsa.SignData(license, null);
            return new License()
            {
                LicenseTerms = Convert.ToBase64String(license),
                Signature = Convert.ToBase64String(signature)
            };
        }

        public static License CreateLicense(LicenseTerms terms)
        {
            var rsa = new RSACryptoServiceProvider();
            rsa.FromXmlString(PrvKey);
            var hash = new SHA512Managed();
            var license = terms.GetLicenseData();
            var hashedData = hash.ComputeHash(license);
            var hashedSign = rsa.SignHash(hashedData, CryptoConfig.MapNameToOID("SHA512"));
           
            var signature = rsa.SignData(license, "SHA512");
            return new License()
            {
                LicenseTerms = Convert.ToBase64String(license),
                Signature = Convert.ToBase64String(signature),
                Hashed = Convert.ToBase64String(hashedSign)
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

        public static LicenseTerms GetValidTerms(License lic)
        {
            return GetValidTerms(lic, PubKey);
        }

        public static int CheckLicense(string appId)
        {
            var serverId = Core.GetServerId();
            var productId = Core.GetProductId(serverId, appId);
            License lic = null;
            LicenseTerms term = null;
            try
            {
                lic = License.Load(productId + ".lic");
            }
            catch (Exception ex)
            {
                return 2;}
            try
            {
                term = LicenseAuthorization.GetValidTerms(lic);
                if (term.ProductId.ToUpper() != productId)
                    return 3;
            }
            catch (Exception)
            {
                return 2;
                throw;
            }
            return DateTime.Compare(term.ExpiryDate.Date, DateTime.Today.Date) < 0 ? 0 : 1;

        }

        public static void CheckLicense2(string appId)
        {
            var flag = CheckLicense(appId);
            var url = string.Empty;
            switch (flag)
            {
                case 0:
                    url = string.Format("{0}?s=0", RedirectPage);
                    break;
                case 1:
                    return;
                case 2:
                    url = string.Format("{0}?s=2", RedirectPage);
                    break;
            }
            HttpContext.Current.Response.Redirect(url);
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
            if (dsa.VerifyData(terms, "SHA512", signature))
            {
                var term = LicenseTerms.FromString(license.LicenseTerms);
                try
                {
                    var k = Library.ProductKey.Parse(term.ProductKey);
                    if (DateTime.Compare(term.ExpiryDate.Date, k.ExpireDate.Date) == 0)
                    {
                        return term;
                    }
                    else throw  new SecurityException("The license key is not invalid");
                }
                catch
                {
                    throw new SecurityException("The license key is not invalid");
                }
            }
            else
                throw new SecurityException("Signature Not Verified!");
        }
    }
}
