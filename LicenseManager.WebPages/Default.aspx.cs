using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace LicenseManager.WebPages
{
    public partial class Default : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!Page.IsPostBack)
            {
                LicenseAuthorization.CheckLicense2("A25EB515-C0FA-474C-941E-1684BB84F147");
            }
        }
    }
}