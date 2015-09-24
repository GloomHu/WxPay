using Pay.business;
using Pay.lib;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace Pay
{
    public partial class JsApiPayPage : System.Web.UI.Page
    {
        public static string wxJsApiParam { get; set; } //H5调起JS API参数

        protected void Page_Load(object sender, EventArgs e)
        {

            Log.Info(this.GetType().ToString(), "JsApiPayPage page load");
            if (!IsPostBack)
            {
                string openid = Request.QueryString["openid"];
                string orderNo = Request.QueryString["orderNo"];
                string payTradeNo = Request.QueryString["payTradeNo"];
                string total_fee = Request.QueryString["total_fee"];
                Log.Info(this.GetType().ToString(), "JsApiPayPage page openid:" + openid + " orderNo:" + orderNo + " payTradeNo:" + payTradeNo + " total_fee:" + total_fee);
                //检测是否给当前页面传递了相关参数
                if (string.IsNullOrEmpty(openid) || string.IsNullOrEmpty(orderNo) || string.IsNullOrEmpty(payTradeNo) || string.IsNullOrEmpty(total_fee))
                {
                    ////new WxPayLog().InsertLog(string.Format("{0}:{1}", "JsApiPayPage page load 页面传参出错,请返回重试", " orderNo:" + orderNo + " payTradeNo:" + payTradeNo + " total_fee:" + total_fee + " openid:" + openid), PayStatus.IntitUnifiedOrder.GetHashCode());
                    Response.Write("<span style='color:#FF0000;font-size:20px'>" + "页面传参出错,请返回重试" + "</span>");
                    Response.End();
                    Log.Error(this.GetType().ToString(), "This page have not get params, cannot be inited, exit...");
                    return;
                }
                ////new WxPayLog().InsertLog(string.Format("{0}:{1}", "JsApiPayPage page Request ", " orderNo:" + orderNo + " payTradeNo:" + payTradeNo + " total_fee:" + total_fee + " openid:" + openid), PayStatus.IntitUnifiedOrder.GetHashCode(),payTradeNo, openid );
                //在页面显示传递参数
                this.lblorderNo.Text = orderNo;
                this.lblTotalFee.Text = total_fee;
                //若传递了相关参数，则调统一下单接口，获得后续相关接口的入口参数
                JsApiPay jsApiPay = new JsApiPay(this);
                jsApiPay.openid = openid;
                jsApiPay.orderNo = orderNo;
                jsApiPay.payTradeNo = payTradeNo;
                jsApiPay.total_fee = Convert.ToInt32(Convert.ToDecimal(total_fee)*100);//转换为分
                //JSAPI支付预处理
                try
                {
                    WxPayData unifiedOrderResult = jsApiPay.GetUnifiedOrderResult();
                    wxJsApiParam = jsApiPay.GetJsApiParameters(); //获取H5调起JS API参数      
                    Log.Debug(this.GetType().ToString(), "wxJsApiParam : " + wxJsApiParam);
                    //在页面上显示订单信息
                    //Response.Write("<span style='color:#00CD00;font-size:20px'>订单详情：</span><br/>");
                    //Response.Write("<span style='color:#00CD00;font-size:20px'>" + unifiedOrderResult.ToPrintStr() +
                    //               "</span>");
                    //Response.End();
                }
                catch (Exception ex)
                {
                    ////new WxPayLog().InsertLog(string.Format("{0}:{1}", "JsApiPayPage page load 下单失败，请返回重试", " orderNo:" + orderNo + " payTradeNo:" + payTradeNo + " total_fee:" + total_fee + " openid:" + openid), PayStatus.UnifiedOrderFail.GetHashCode(),payTradeNo, openid );
                    Response.Write("<span style='color:#FF0000;font-size:20px'>" + "下单失败，请返回重试"+"</span>");
                    Response.End();
                }
            }
        }
    }
}