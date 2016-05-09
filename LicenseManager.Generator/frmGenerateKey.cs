using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Windows.Forms;

namespace LicenseManager.Generator
{
    public partial class frmGenerateKey : Form
    {
        public frmGenerateKey()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            RSACryptoServiceProvider rsa = new RSACryptoServiceProvider(4096);
            //Pair of public and private key as XML string.
            //Do not share this to other party
            string publicPrivateKeyXML = rsa.ToXmlString(true);
            //Private key in xml file, this string should be share to other parties
            string publicOnlyKeyXML = rsa.ToXmlString(false);

            txtPubKey.Text = publicOnlyKeyXML;
            txtPrvKey.Text = publicPrivateKeyXML;
            var fileName = Guid.NewGuid().ToString();
            var fs = new FileStream(fileName+ ".prv", FileMode.Create, FileAccess.Write);
            var sw = new StreamWriter(fs);
            sw.Write(publicPrivateKeyXML);
            sw.Flush();
            fs = new FileStream(fileName + ".pub", FileMode.Create, FileAccess.Write);
            sw = new StreamWriter(fs);
            sw.Write(publicOnlyKeyXML);
            sw.Flush();
        }
    }
}
