using LicenseManager.Library;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace LicenseManager.Generator
{
    public partial class frmMain : Form
    {
        public frmMain()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            var frm = new frmGenerateKey();
            frm.Show();
        }

        private void frmMain_Load(object sender, EventArgs e)
        {
            txtProductID.Text = "";
            txtProductWin.Text = Core.GetServerProductId();
            txtHDDID.Text = Core.GetHddSerialNo();

            txtAppIDs.Text = "A25EB515-C0FA-474C-941E-1684BB84F147";
            txtNumberOfUser.Text = "10";
            var serverID = Core.GetServerId();
            txtProductID.Text = Core.GetProductId(serverID, txtAppIDs.Text);

        }

        private void button2_Click(object sender, EventArgs e)
        {
            Random rd = new Random((int)DateTime.Now.Ticks);
            var key = ProductKey.GenKey(dtpExpiryDate.Value, txtProductID.Text, uint.Parse(txtNumberOfUser.Text), null, rd);
            txtLicensKey.Text = key.ToString();
        }

        private void button3_Click(object sender, EventArgs e)
        {
            try
            {
                var k = ProductKey.Parse(txtLicensKey.Text);
                MessageBox.Show(string.Format("{0} - {1} - {2}", k.ExpireDate.ToString(CultureInfo.InvariantCulture), k.NumberOfUser.ToString(), k.Verify(DateTime.Today, txtProductID.Text).ToString()));
            }
            catch
            {
                MessageBox.Show("The entered key is not invalid!");
            }
        }

        private void button4_Click(object sender, EventArgs e)
        {
            var terms = new LicenseAuthorization.LicenseTerms();
            terms.StartDate = DateTime.Today;
            terms.ExpiryDate = dtpExpiryDate.Value;
            terms.NumberOfGroups = 100;
            terms.NumberOfUsers = 1000;
            terms.OrganizationName = txtOrigazation.Text;
            terms.ProductId = txtProductID.Text;
            terms.ProductKey = txtLicensKey.Text;
            var lic = LicenseAuthorization.CreateLicense(terms);
            lic.Save(terms.ProductId + ".lic");

        }

        private void button5_Click(object sender, EventArgs e)
        {
            LicenseAuthorization.CheckLicense2(txtAppIDs.Text);
            return;
            var st =new Stopwatch();
            st.Start();
            var lic = License.Load(txtProductID.Text + ".lic");
            var term = LicenseAuthorization.GetValidTerms(lic);
            st.Stop();
            MessageBox.Show(st.Elapsed.ToString());
        }
    }
}
