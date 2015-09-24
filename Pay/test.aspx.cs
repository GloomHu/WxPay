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
    public partial class test : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
        }
        protected void Button3_Click(object sender, EventArgs e)
        {
            string total_fee = "0.01";
            string url = "WapPay.aspx?total_fee=" + total_fee + "&orderNo=100115060115090001&payTradeNo=" + new WxPayData().GenerateOutTradeNo();
            //string url = "WapPay.aspx?total_fee=" + total_fee +
            //             "&orderNo=100115060115090001&payTradeNo=10011506011509000120150601150947111";
            Log.Info(this.GetType().ToString(), "test page url:" + url);
            Response.Redirect(url);
        }

        protected void Button4_Click(object sender, EventArgs e)
        {
            WxPayData notifyData = new WxPayData();
            notifyData.SetValue("attach", "100115060115500001");
            notifyData.SetValue("out_trade_no", "120150601155014540");
            notifyData.SetValue("total_fee", 150);
            notifyData.SetValue("transaction_id", "1001600183201505280174966533");

            new NotifyResult(this).WapPayProcess(notifyData);
        }
    }
}