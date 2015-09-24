<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="JsApiPayPage.aspx.cs" Inherits="Pay.JsApiPayPage" %>

<!DOCTYPE html>

<html lang="en">
<head>
    <meta charset="UTF-8">
    <title>菜嘟美微信支付-JSAPI支付</title>
    <meta http-equiv="Cache-Control" content="no-cache, no-store, must-revalidate" />
    <meta http-equiv="Pragma" content="no-cache" />
    <meta http-equiv="Expires" content="0" />

    <meta name="viewport" content="width=device-width, initial-scale=1.0, maximum-scale=1.0, user-scalable=0">
    <meta name="apple-mobile-web-app-capable" content="yes">
    <meta name="apple-mobile-web-app-status-bar-style" content="black">
    <meta name="format-detection" content="telephone=no">

    <script type="text/javascript">

        //调用微信JS api 支付
        function jsApiCall() {
            WeixinJSBridge.invoke(
            'getBrandWCPayRequest',
            <%=wxJsApiParam%>,//josn串
            function(res) {
                // 使用以上方式判断前端返回,微信团队郑重提示：res.err_msg将在用户支付成功后返回    ok，但并不保证它绝对可靠。 
                if (res.err_msg == "get_brand_wcpay_request:ok") {
                    window.location.href = "PaySUCCESS.html";
                } else if (res.err_msg == "get_brand_wcpay_request:cancel") {
                    //window.location.href = "WapPay.aspx";
                } else {
                    window.location.href = "PayFAIL.html";
                }
                //WeixinJSBridge.log(res.err_msg);
                //alert(res.err_code + res.err_desc + res.err_msg);
            });
        }
        function callpay() {
            if (typeof WeixinJSBridge == "undefined") {
                if (document.addEventListener) {
                    document.addEventListener('WeixinJSBridgeReady', jsApiCall, false);
                } else if (document.attachEvent) {
                    document.attachEvent('WeixinJSBridgeReady', jsApiCall);
                    document.attachEvent('onWeixinJSBridgeReady', jsApiCall);
                }
                alert("请用微信浏览器打开!")
            } else {
                jsApiCall();
            }
        }

    </script>
    <link href="css/reset.css" rel="stylesheet" />
    <link href="css/css.css" rel="stylesheet" />
</head>
<body>
    <form runat="server">
        <header>
            <div class="header-wrap">
                <a href="javascript:history.back();" class="header-back"><span>返回</span></a><h2>核对支付信息</h2>
                <a href="javascript:void(0)" id="btn-opera" class="i-main-opera"><span></span></a>
            </div>
        </header>
        <div>
            <div class="contxt">
                <ul>
                    <li class="clearfix">
                        <span class="fleft">订单号：</span>
                        <asp:Label ID="lblorderNo" runat="server" Text="0" CssClass="fleft"></asp:Label>
                    </li>
                    <li class="clearfix">
                        <span class="fleft">总金额：</span><asp:Label ID="lblTotalFee" runat="server" Text="0" CssClass="fleft" Style="color: red"></asp:Label>元
                    </li>
                </ul>
            </div>
        </div>
        <a href="javascript:void(0);" onclick="callpay()" class="post-pay">确认支付</a>
        <footer id="footer">
            <div class="footer">
                <div class="footer-content">
                    <p class="copyright">版权所有 2007-2015 © 蜀海供应链</p>
                </div>
            </div>
        </footer>
    </form>
</body>
</html>
