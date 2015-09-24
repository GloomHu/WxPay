using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Pay.lib;
using System.Web.UI;

using System.Text;
using System.Net;

namespace Pay.business
{
    public class NotifyResult
    {
        /// <summary>
        /// 保存页面对象，因为要在类的方法中使用Page的Request对象
        /// </summary>
        private Page page { get; set; }

        public NotifyResult(Page page)
        {
            this.page = page;
        }

        public void WapPayProcess(WxPayData notifyData, int timeOut = 6)
        {
            string url = "http://123.57.211.198/api/pay/NotifyResultOp.ashx";
            WxPayData inputObj = new WxPayData();
            inputObj.SetValue("attach",notifyData.GetValue("attach"));//业务订单号
            inputObj.SetValue("out_trade_no",notifyData.GetValue("out_trade_no"));//业务支付订单号
            inputObj.SetValue("total_fee",notifyData.GetValue("total_fee"));//支付金额
            inputObj.SetValue("transaction_id",notifyData.GetValue("transaction_id"));//微信支付订单号
            inputObj.SetValue("sign", inputObj.MakeSign());
            string xml = inputObj.ToXml();
            if (!WxPayConfig.CURL_TIMEOUT.Equals(0))
            {
                timeOut = WxPayConfig.CURL_TIMEOUT;
            }
            Log.Debug("NotifyResult", "WapPayProcess request : " + xml);
            try
            {
                string response = HttpService.Post(xml, url, false, timeOut); //调用HTTP通信接口提交数据

                Log.Debug("NotifyResult", "WapPayProcess response : " + response);

                ////new WxPayLog().InsertLog(string.Format("{0}:{1}", "NotifyResult WapPayProcess response : ", response), PayStatus.GetOpenidSucces.GetHashCode());

                WxPayData res = new WxPayData();
                res.FromXml(response);
                page.Response.Write(res.ToXml());
                page.Response.End();
            }
            catch (WxPayException ex)
            {
                WxPayData res = new WxPayData();
                res.SetValue("return_code", "FAIL");
                res.SetValue("return_msg", "业务处理发生错误");
                ////new WxPayLog().InsertLog(string.Format("{0}:{1}", "NotifyResult WapPayProcess处理发生错误", res.ToJson()), PayStatus.PayResultNotifyFail.GetHashCode());
                
                Log.Error(this.GetType().ToString(), "NotifyResult WapPayProcess failure : " + res.ToXml());
                page.Response.Write(res.ToXml());
                page.Response.End();
            }

        }
    }
}