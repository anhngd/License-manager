<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="License.aspx.cs" Inherits="LicenseManager.WebPages.License" %>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title></title>
    <link type="text/css" rel="stylesheet" href="https://maxcdn.bootstrapcdn.com/bootstrap/3.3.6/css/bootstrap.min.css"/>
    <script type="text/javascript" src="https://maxcdn.bootstrapcdn.com/bootstrap/3.3.6/js/bootstrap.min.js"></script>
    <style type="text/css">
        #form1 .col-md-12 .col-md-4{ font-family: Segoe UI;color: rgb(32, 29, 29);font-weight: 500;font-size: 16px;}
        #form1 .col-md-12 .col-md-8{ font-family: Segoe UI;color: rgb(13, 134, 13);font-weight: 600;font-size: 16px;}
    </style>
</head>
<body>
    <form id="form1" runat="server">
        <div class="col-md-12">
            <div class="col-md-12">
                <div class="col-md-4"></div>
                <div class="col-md-8"><%=Lic.OrganizationName %></div>
            </div>
            <div class="col-md-12">
                <div class="col-md-4">Mã sản phẩm</div>
                <div class="col-md-8"><%=Lic.ProductId %></div><br/>
            </div>
            <div class="col-md-12">
                <div class="col-md-4">Mã kích hoạt</div>
                <div class="col-md-8"><%=Lic.ProductKey %></div>
            </div>
            <div class="col-md-12">
                <div class="col-md-4">Số đơn vị</div>
                <div class="col-md-8"><%=Lic.NumberOfGroups %></div>
            </div>
            <div class="col-md-12">
                <div class="col-md-4">Số người dùng</div>
                <div class="col-md-8"><%=Lic.NumberOfUsers %></div>
            </div>
            <div class="col-md-12">
                <div class="col-md-4">Thời hạn</div>
                <div class="col-md-8"><%=Lic.ExpiryDate.Date.ToString("dd/MM/yyyy") %> <span>(<%=(Lic.ExpiryDate.Date - DateTime.Today.Date).Days %> ngày)</span></div>
            </div>
            <div class="col-md-12">
                <div class="col-md-4">Nhập mã kích hoạt</div>
                <div class="col-md-8">
                    <asp:TextBox ID="txtLicenseTerm" runat="server" Rows="5" TextMode="MultiLine" Width="100%"></asp:TextBox>
                    <asp:Button ID="Button1" runat="server" Text="Cập nhật" OnClick="Button1_Click" />
                </div>
            </div>
            <div class="col-md-12">
                <div class="col-md-4">hoặc tải tệp lên</div>
                <div class="col-md-8">
                    <asp:FileUpload ID="FileUpload1" runat="server"/>
                    <asp:RegularExpressionValidator ID="uplValidator" runat="server" ControlToValidate="FileUpload1"
                            ValidationExpression="^(([a-zA-Z]:)|(\\{2}\w+)\$?)(\\(\w[\w].*))(.LIC|.lic)$"></asp:RegularExpressionValidator>
                </div>
            </div>

            <div class="col-md-12"><%=TotalTime %></div>
        </div>

    </form>
</body>
</html>
