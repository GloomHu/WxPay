<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="Default.aspx.cs" Inherits="Pay.Default" %>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <meta http-equiv="Content-Type" content="text/html; charset=utf-8" />
    <title>支付测试</title>
    <script type="text/javascript">
        //获取共享地址
        function editAddress()
        {
            WeixinJSBridge.invoke(
                'editAddress',
                <%=wxEditAddrParam%>,//josn串
                   function (res)
                   {
                       var addr1 = res.proviceFirstStageName;
                       var addr2 = res.addressCitySecondStageName;
                       var addr3 = res.addressCountiesThirdStageName;
                       var addr4 = res.addressDetailInfo;
                       var tel = res.telNumber;
                       var addr = addr1 + addr2 + addr3 + addr4;
                       //alert(addr + ":" + tel);
                       document.getElementById("sp_tel").innerText  = tel;
                       document.getElementById("sp_addr").innerText = addr;
                   }
               );
        }

        window.onload = function ()
        {
            if (typeof WeixinJSBridge == "undefined")
            {
                if (document.addEventListener)
                {
                    document.addEventListener('WeixinJSBridgeReady', editAddress, false);
                }
                else if (document.attachEvent)
                {
                    document.attachEvent('WeixinJSBridgeReady', editAddress);
                    document.attachEvent('onWeixinJSBridgeReady', editAddress);
                }
            }
            else
            {
                editAddress();
            }
        };
    </script>
</head>
<body>
    <form runat="server">
        <div style="border: 1px #FE6714 solid;">
            <h2>微信支付-JSAPI</h2>
            <div>
                <h5>获取的用户信息如下：</h5>
                <p>电话:<span id="sp_tel"></span></p>
                <p>地址:<span id="sp_addr"></span></p>
            </div>
            <div>
                <asp:Label ID="Label1" runat="server" Style="color: #00CD00;"><b>商品一：价格为<span style="color:#f00;font-size:50px">1分</span>钱</b></asp:Label><br />
                <br />
            </div>
            <div align="center">
                <asp:Button ID="Button1" runat="server" Text="立即购买" Style="width: 210px; height: 50px; border-radius: 15px; background-color: #00CD00; border: 0px #FE6714 solid; cursor: pointer; color: white; font-size: 16px;" OnClick="Button1_Click" />
            </div>
            <br />
            <br />
            <br />
            <div>
                <asp:Label ID="Label2" runat="server" Style="color: #00CD00;"><b>商品二：价格为<span style="color:#f00;font-size:50px">2分</span>钱</b></asp:Label><br />
                <br />
            </div>
            <div align="center">
                <asp:Button ID="Button2" runat="server" Text="立即购买" Style="width: 210px; height: 50px; border-radius: 15px; background-color: #00CD00; border: 0px #FE6714 solid; cursor: pointer; color: white; font-size: 16px;" OnClick="Button2_Click" />
            </div>
        </div>
    </form>
</body>
</html>
