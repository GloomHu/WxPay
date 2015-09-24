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
    public partial class wapPay : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            Log.Info(this.GetType().ToString(), "WapPay page load");
            if (!IsPostBack)
            {
                JsApiPay jsApiPay = new JsApiPay(this);
                string orderNo = Request.QueryString["orderNo"];//订单号
                string payTradeNo = Request.QueryString["payTradeNo"];//订单支付号
                string total_fee = Request.QueryString["total_fee"];
                //检测是否给当前页面传递了相关参数以及检测网页授权是否回传给当前页面传递了相关参数
                if (string.IsNullOrEmpty(orderNo) || string.IsNullOrEmpty(payTradeNo) || string.IsNullOrEmpty(total_fee))
                {
                    ////new WxPayLog().InsertLog(string.Format("{0}:{1}", "WapPay page load 页面传参出错,请返回重试 ", " orderNo:" + orderNo + " payTradeNo:" + payTradeNo + " total_fee:" + total_fee), PayStatus.Intit.GetHashCode());
                    Response.Write("<span style='color:#FF0000;font-size:20px'>" + "页面传参出错,请返回重试" + "</span>");
                    Log.Error(this.GetType().ToString(), "This page have not get params, cannot be inited, exit...");
                    return;
                }
                ////new WxPayLog().InsertLog(string.Format("{0}:{1}", "WapPay page load  Request", " orderNo:" + orderNo + " payTradeNo:" + payTradeNo + " total_fee:" + total_fee), PayStatus.Intit.GetHashCode(), payTradeNo);
            
                try
                {
                    //调用【网页授权获取用户信息】接口获取用户的openid和access_token
                    jsApiPay.GetOpenidAndAccessToken();
                    string openid = jsApiPay.openid;
                    Log.Debug(this.GetType().ToString(), "WapPay page load openid: " + jsApiPay.openid + " orderNo:" + orderNo + " payTradeNo:" + payTradeNo + " total_fee:" + total_fee);
                    //检测网页授权是否回传给当前页面传递了相关参数
                    if (!string.IsNullOrEmpty(openid))
                    {
                        //跳转支付JsApiPayPage.aspx页面
                        string url = "JsApiPayPage.aspx?openid=" + openid + "&orderNo=" + orderNo + "&payTradeNo=" + payTradeNo + "&total_fee=" + total_fee;
                        ////new WxPayLog().InsertLog(string.Format("{0}:{1}", "WapPay Redirect url ", url), PayStatus.GetOpenidSucces.GetHashCode(), payTradeNo, openid);
                        Log.Info(this.GetType().ToString(), "WapPay page url:" + url);
                        Response.Redirect(url, false);
                    }
                }
                catch (Exception ex)
                {
                    ////new WxPayLog().InsertLog(string.Format("{0}:{1}", "WapPay 页面加载出错，请返回重试 ", " orderNo:" + orderNo + " payTradeNo:" + payTradeNo + " total_fee:" + total_fee), PayStatus.Intit.GetHashCode(),payTradeNo);
                    Response.Write("<span style='color:#FF0000;font-size:20px'>" + "页面加载出错，请重试" + "</span>");
                }
            }
        }
    }
}