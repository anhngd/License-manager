using LicenseManager.Library;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
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

        private void button1_Click(object sender, EventArgs e){
            frmGenerateKey frm = new frmGenerateKey();
            frm.Show();
        }

        private void frmMain_Load(object sender, EventArgs e){
            txtProductID.Text = "";
            txtProductWin.Text = Core.GetServerProductId();
            txtHDDID.Text = Core.GetHddSerialNo();
            
            txtAppIDs.Text = Guid.NewGuid().ToString().ToUpper();
            var serverID = Core.GetServerId();
            txtProductID.Text = Core.GetProductId(serverID, txtAppIDs.Text);

        }

        private void button2_Click(object sender, EventArgs e)
        {
            Random rd = new Random((int)DateTime.Now.Ticks);
            var key = ProductKey.GenKey(dtpExpiryDate.Value, txtProductID.Text, uint.Parse(txtNumberOfUser.Text), null, rd);
            txtLicensKey.Text = key.ToString();
        }
    }
}
