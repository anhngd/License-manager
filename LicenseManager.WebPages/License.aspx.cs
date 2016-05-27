using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace LicenseManager.WebPages
{
    public partial class License : System.Web.UI.Page
    {
        public static LicenseAuthorization.LicenseTerms Lic = new LicenseAuthorization.LicenseTerms();
        public static string TotalTime = string.Empty;
        public static string ProductId = "A25EB515-C0FA-474C-941E-1684BB84F147";
        protected void Page_Load(object sender, EventArgs e)
        {
            if (Page.IsPostBack) return;
            var stopwatch = new Stopwatch();
            stopwatch.Start();
            try { Lic = LicenseAuthorization.GetLicense(ProductId); } catch { Lic= new LicenseAuthorization.LicenseTerms();}
            stopwatch.Stop();
            TotalTime = stopwatch.Elapsed.TotalMilliseconds.ToString(CultureInfo.InvariantCulture);
        }

        protected void Button1_Click(object sender, EventArgs e)
        {
            var val = txtLicenseTerm.Text;
            if (string.IsNullOrEmpty(val))
                throw new Exception("Chưa nhập dữ liệu kích hoạt");
            else
            {
                using (var writer = new StreamWriter("C:\\LLL\\13910-71822-91421-48541-17200.lic")){
                    writer.WriteLine(val);
                }
            }
            if (!FileUpload1.HasFile) return;
            var fileName = FileUpload1.FileName;
            FileUpload1.SaveAs(fileName);
        }
    }
}