using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using Pay.business;
using Pay.lib;


namespace Pay
{
    public partial class ResultNotifyPage : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            Log.Info(this.GetType().ToString(), "Pay Result Page ");
            JsApiPay jsApiPay = new JsApiPay(this);
            WxPayData notifyData = jsApiPay.GetNotifyData();
            
            //检查支付结果中transaction_id是否存在
            if (!notifyData.IsSet("transaction_id"))
            {
                //若transaction_id不存在，则立即返回结果给微信支付后台
                WxPayData res = new WxPayData();
                res.SetValue("return_code", "FAIL");
                res.SetValue("return_msg", "支付结果中微信订单号不存在");
                ////new WxPayLog().InsertLog(string.Format("{0}:{1}", "ResultNotifyPage  cheack transaction_id 支付结果中微信订单号不存在", res.ToJson()), PayStatus.IntitPayResultNotify.GetHashCode());
                Log.Error(this.GetType().ToString(), "The Pay result is error : " + res.ToXml());
                this.Response.Write(res.ToXml());
                this.Response.End();
            }

            string transaction_id = notifyData.GetValue("transaction_id").ToString();

            //查询订单，判断订单真实性
            if (!QueryOrder(transaction_id, jsApiPay))
            {
                //若订单查询失败，则立即返回结果给微信支付后台
                WxPayData res = new WxPayData();
                res.SetValue("return_code", "FAIL");
                res.SetValue("return_msg", "订单查询失败");
                ////new WxPayLog().InsertLog(string.Format("{0}:{1}", "ResultNotifyPage 判断transaction_id订单真实性 订单查询失败", res.ToJson()), PayStatus.PayResultNotifyFail.GetHashCode());
              
                Log.Error(this.GetType().ToString(), "Order query failure : " + res.ToXml());
                this.Response.Write(res.ToXml());
                this.Response.End();
            }
            //查询订单成功
            else
            {
                //将处理结果发送给微信Wap
                new NotifyResult(this).WapPayProcess(notifyData);

                //WxPayData res = new WxPayData();
                //res.SetValue("return_code", "SUCCESS");
                //res.SetValue("return_msg", "OK");
                //Log.Info(this.GetType().ToString(), "order query success : " + res.ToXml());
                ////new WxPayLog().InsertLog(string.Format("{0}:{1}", "ResultNotifyPage 查询订单成功", res.ToJson()), PayStatus.PayResultNotifySucces.GetHashCode(), notifyData.GetValue("out_trade_no").ToString(), notifyData.GetValue("openid").ToString(), notifyData.GetValue("transaction_id").ToString());
                //this.Response.Write(res.ToXml());
                //this.Response.End();
            }
        }
        //查询订单
        private bool QueryOrder(string transaction_id,JsApiPay jsApiPay)
        {
            WxPayData req = new WxPayData();
            req.SetValue("transaction_id", transaction_id);
            WxPayData res = jsApiPay.OrderQuery(req);
            if (res.GetValue("return_code").ToString() == "SUCCESS" &&
                res.GetValue("result_code").ToString() == "SUCCESS")
            {
                return true;
            }
            else
            {
                return false;
            }
        }
    }
}