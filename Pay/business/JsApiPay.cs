using System.Text;
using Pay.lib;
using LitJson;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Security;
using System.Web.UI;


namespace Pay.business
{
    public class JsApiPay
    {
        /// <summary>
        /// 保存页面对象，因为要在类的方法中使用Page的Request对象
        /// </summary>
        private Page page { get; set; }

        /// <summary>
        /// access_token用于获取收货地址js函数入口参数
        /// </summary>
        public string access_token { get; set; }

        /// <summary>
        /// openid用于调用统一下单接口
        /// </summary>
        public string openid { get; set; }

        /// <summary>
        /// 统一下单接口返回结果
        /// </summary>
        public WxPayData unifiedOrderResult { get; set; }

        /// <summary>
        /// 订单号
        /// </summary>
        public string orderNo { get; set; }
        /// <summary>
        /// 支付订单号
        /// </summary>
        public string payTradeNo { get; set; }
        /// <summary>
        /// 商品金额，用于统一下单
        /// </summary>
        public int total_fee { get; set; }


        public JsApiPay(Page page)
        {
            this.page = page;
        }

        /**
       * 
       * 网页授权获取用户基本信息的全部过程
       * 详情请参看网页授权获取用户基本信息：http://mp.weixin.qq.com/wiki/17/c0f37d5704f0b64713d5d2c37b468d75.html
       * 第一步：利用url跳转获取code
       * 第二步：利用code去获取openid和access_token
       * 
       */

        public void GetOpenidAndAccessToken()
        {
            string r_payTradeNo = page.Request.QueryString["payTradeNo"];

            if (!string.IsNullOrEmpty(page.Request.QueryString["code"]))
            {
                //获取code码，以获取openid和access_token
                string code = page.Request.QueryString["code"];
                Log.Debug(this.GetType().ToString(), "Get code : " + code);
                Log.Debug(this.GetType().ToString(), "oauth2 return  Request: " + page.Request.Url.Query);
                ////new WxPayLog().Insert(string.Format("{0}:{1}", "GetOpenidAndAccessToken code", code),);
                GetOpenidAndAccessTokenFromCode(code, r_payTradeNo);
            }
            else
            {
                //构造网页授权获取code的URL
                string host = page.Request.Url.Host;
                string path = page.Request.Path;
                string queryString = page.Request.Url.Query;
                string redirect_uri = HttpUtility.UrlEncode("http://" + host + path + queryString);
                WxPayData data = new WxPayData();
                data.SetValue("appid", WxPayConfig.APPID);
                data.SetValue("redirect_uri", redirect_uri);
                data.SetValue("response_type", "code");
                data.SetValue("scope", "snsapi_base");
                data.SetValue("state", "STATE" + "#wechat_redirect");
                string url = "https://open.weixin.qq.com/connect/oauth2/authorize?" + data.ToUrl();
                Log.Debug(this.GetType().ToString(), "Will Redirect to URL : " + url);
                try
                {
                    //new WxPayLog().InsertLog(string.Format("{0}:{1}", "GetOpenidAndAccessToken url", url), PayStatus.Intit.GetHashCode(), r_payTradeNo);
                    //触发微信返回code码         
                    page.Response.Redirect(url,false); //Redirect函数会抛出ThreadAbortException异常，不用处理这个异常
                }
                catch (System.Threading.ThreadAbortException ex)
                {
                }
            }
        }


        /**
        * 
        * 通过code换取网页授权access_token和openid的返回数据，正确时返回的JSON数据包如下：
        * {
        *  "access_token":"ACCESS_TOKEN",
        *  "expires_in":7200,
        *  "refresh_token":"REFRESH_TOKEN",
        *  "openid":"OPENID",
        *  "scope":"SCOPE",
        *  "unionid": "o6_bmasdasdsad6_2sgVt7hMZOPfL"
        * }
        * 其中access_token可用于获取共享收货地址
        * openid是微信支付jsapi支付接口统一下单时必须的参数
        * 更详细的说明请参考网页授权获取用户基本信息：http://mp.weixin.qq.com/wiki/17/c0f37d5704f0b64713d5d2c37b468d75.html
        * @失败时抛异常WxPayException
        */
        public void GetOpenidAndAccessTokenFromCode(string code, string r_payTradeNo)
        {
            string url = string.Empty;
            try
            {
                //构造获取openid及access_token的url
                WxPayData data = new WxPayData();
                data.SetValue("appid", WxPayConfig.APPID);
                data.SetValue("secret", WxPayConfig.APPSECRET);
                data.SetValue("code", code);
                data.SetValue("grant_type", "authorization_code");
                url = "https://api.weixin.qq.com/sns/oauth2/access_token?" + data.ToUrl();

                Log.Debug(this.GetType().ToString(), "Get access_token begin :" + url);
                //请求url以获取数据 
                //string result = string.Empty;
                string result = HttpService.Get(url);

                Log.Debug(this.GetType().ToString(), "GetOpenidAndAccessTokenFromCode response : " + result);

                //保存access_token，用于收货地址获取
                JsonData jd = JsonMapper.ToObject(result);
                access_token = (string)jd["access_token"];

                //获取用户openid
                openid = (string)jd["openid"];

                //new WxPayLog().InsertLog(string.Format("{0}:{1}", "GetOpenidAndAccessToken url", url), PayStatus.GetOpenidSucces.GetHashCode(), r_payTradeNo, openid);

                Log.Debug(this.GetType().ToString(), "Get openid : " + openid);
                Log.Debug(this.GetType().ToString(), "Get access_token : " + access_token);
            }
            catch (Exception ex)
            {
                //new WxPayLog().InsertLog(string.Format("{0}:{1}", "GetOpenidAndAccessToken url", url), PayStatus.GetOpenidFail.GetHashCode(), r_payTradeNo);
                Log.Error(this.GetType().ToString(), ex.ToString());
                throw new WxPayException(ex.ToString());
            }
        }
        /**
       * 
       * 获取收货地址js函数入口参数,详情请参考收货地址共享接口：http://pay.weixin.qq.com/wiki/doc/api/jsapi.php?chapter=7_9
       * @return string 共享收货地址js函数需要的参数，json格式可以直接做参数使用
       */
        public string GetEditAddressParameters()
        {
            string parameter = string.Empty;
            try
            {
                string host = page.Request.Url.Host;
                string path = page.Request.Path;
                string queryString = page.Request.Url.Query;
                //这个地方要注意，参与签名的是网页授权获取用户信息时微信后台回传的完整url
                string url = "http://" + host + path + queryString;

                //构造需要用SHA1算法加密的数据
                WxPayData signData = new WxPayData();
                signData.SetValue("appid", WxPayConfig.APPID);
                signData.SetValue("url", url);
                signData.SetValue("timestamp", signData.GenerateTimeStamp());
                signData.SetValue("noncestr", signData.GenerateNonceStr());
                signData.SetValue("accesstoken", access_token);
                string param = signData.ToUrl();

                Log.Debug(this.GetType().ToString(), "SHA1 encrypt param : " + param);
                //SHA1加密
                string addrSign = FormsAuthentication.HashPasswordForStoringInConfigFile(param, "SHA1");
                Log.Debug(this.GetType().ToString(), "SHA1 encrypt result : " + addrSign);

                //获取收货地址js函数入口参数
                WxPayData afterData = new WxPayData();
                afterData.SetValue("appId", WxPayConfig.APPID);
                afterData.SetValue("scope", "jsapi_address");
                afterData.SetValue("signType", "sha1");
                afterData.SetValue("addrSign", addrSign);
                afterData.SetValue("timeStamp", signData.GetValue("timestamp"));
                afterData.SetValue("nonceStr", signData.GetValue("noncestr"));

                //转为json格式
                parameter = afterData.ToJson();
                Log.Debug(this.GetType().ToString(), "Get EditAddressParam : " + parameter);
            }
            catch (Exception ex)
            {
                Log.Error(this.GetType().ToString(), ex.ToString());
                throw new WxPayException(ex.ToString());
            }

            return parameter;
        }

        /**
        * 调用统一下单，获得下单结果
        * @return 统一下单结果
        * @失败时抛异常WxPayException
        */
        public WxPayData GetUnifiedOrderResult()
        {
            //统一下单
            WxPayData data = new WxPayData();
            //data.SetValue("body", "WxPay");
            data.SetValue("body", orderNo);
            //data.SetValue("attach", "WxPay");
            data.SetValue("attach", orderNo);
            //data.SetValue("out_trade_no", data.GenerateOutTradeNo());
            data.SetValue("out_trade_no", payTradeNo);
            data.SetValue("total_fee", total_fee);
            data.SetValue("time_start", DateTime.Now.ToString("yyyyMMddHHmmss"));
            data.SetValue("time_expire", DateTime.Now.AddMinutes(10).ToString("yyyyMMddHHmmss"));
            //data.SetValue("goods_tag", "WxPay");
            data.SetValue("goods_tag", orderNo);
            data.SetValue("trade_type", "JSAPI");
            data.SetValue("openid", openid);
            
            //new WxPayLog().InsertLog(string.Format("{0}:{1}", "GetUnifiedOrderResult  WxPayData ", data.ToJson()), PayStatus.IntitUnifiedOrder.GetHashCode(),payTradeNo,openid );
             
            WxPayData result = UnifiedOrder(data);
            if (!result.IsSet("appid") || !result.IsSet("prepay_id") || result.GetValue("prepay_id").ToString() == "")
            {
                //new WxPayLog().InsertLog(string.Format("{0}:{1}", "GetUnifiedOrderResult  WxPayData ", result.ToJson()), PayStatus.UnifiedOrderFail.GetHashCode(), payTradeNo, openid);
         
                Log.Error(this.GetType().ToString(), "UnifiedOrder response error!");
                throw new WxPayException("UnifiedOrder response error!");
            }

            unifiedOrderResult = result;
            return result;
        }

        /**
        * 
        * 统一下单
        * @param WxPaydata inputObj 提交给统一下单API的参数
        * @param int timeOut 超时时间
        * @throws WxPayException
        * @return 成功时返回，其他抛异常
        */

        public static WxPayData UnifiedOrder(WxPayData inputObj, int timeOut = 6)
        {
            string url = "https://api.mch.weixin.qq.com/pay/unifiedorder";
            //检测必填参数
            if (!inputObj.IsSet("out_trade_no"))
            {
                throw new WxPayException("缺少统一支付接口必填参数out_trade_no！");
            }
            else if (!inputObj.IsSet("body"))
            {
                throw new WxPayException("缺少统一支付接口必填参数body！");
            }
            else if (!inputObj.IsSet("total_fee"))
            {
                throw new WxPayException("缺少统一支付接口必填参数total_fee！");
            }
            else if (!inputObj.IsSet("trade_type"))
            {
                throw new WxPayException("缺少统一支付接口必填参数trade_type！");
            }

            //关联参数
            if (inputObj.GetValue("trade_type").ToString() == "JSAPI" && !inputObj.IsSet("openid"))
            {
                throw new WxPayException("统一支付接口中，缺少必填参数openid！trade_type为JSAPI时，openid为必填参数！");
            }
            if (inputObj.GetValue("trade_type").ToString() == "NATIVE" && !inputObj.IsSet("product_id"))
            {
                throw new WxPayException("统一支付接口中，缺少必填参数product_id！trade_type为JSAPI时，product_id为必填参数！");
            }

            //异步通知url未设置，则使用配置文件中的url
            if (!inputObj.IsSet("notify_url"))
            {
                inputObj.SetValue("notify_url", WxPayConfig.NOTIFY_URL); //异步通知url
            }

            inputObj.SetValue("appid", WxPayConfig.APPID); //公众账号ID
            inputObj.SetValue("mch_id", WxPayConfig.MCHID); //商户号
            inputObj.SetValue("spbill_create_ip", WxPayConfig.IP); //终端ip	  	    
            inputObj.SetValue("nonce_str", inputObj.GenerateNonceStr()); //随机字符串

            //签名
            inputObj.SetValue("sign", inputObj.MakeSign());
            string xml = inputObj.ToXml();

            var start = DateTime.Now;

            Log.Debug("WxPayApi", "UnfiedOrder request : " + xml);
            Log.Debug("WxPayApi", "UnfiedOrder request json : " + inputObj.ToJson());

            //new WxPayLog().InsertLog(string.Format("{0}:{1}", "UnifiedOrder  request json ", inputObj.ToJson()),
                //PayStatus.IntitUnifiedOrder.GetHashCode(),
                // inputObj.GetValue("out_trade_no").ToString(), inputObj.GetValue("openid").ToString());

            if (!WxPayConfig.CURL_TIMEOUT.Equals(0))
            {
                timeOut = WxPayConfig.CURL_TIMEOUT;
            }
            string response = HttpService.Post(xml, url, false, timeOut);
            //string response = string.Empty;
            Log.Debug("WxPayApi", "UnfiedOrder response : " + response);
            var end = DateTime.Now;
            int timeCost = (int) ((end - start).TotalMilliseconds);

            WxPayData result = new WxPayData();
            result.FromXml(response);

            //new WxPayLog().InsertLog(string.Format("{0}:{1}", "UnifiedOrder  response ", result.ToJson()),
                //PayStatus.UnifiedOrderSucces.GetHashCode(),
                //inputObj.GetValue("out_trade_no").ToString(), inputObj.GetValue("openid").ToString());

            //ReportCostTime(url, timeCost, result);//测速上报

            return result;
        }

        /**
       *  
       * 从统一下单成功返回的数据中获取微信浏览器调起jsapi支付所需的参数，
       * 微信浏览器调起JSAPI时的输入参数格式如下：
       * {
       *   "appId" : "wx2421b1c4370ec43b",     //公众号名称，由商户传入     
       *   "timeStamp":" 1395712654",         //时间戳，自1970年以来的秒数     
       *   "nonceStr" : "e61463f8efa94090b1f366cccfbbb444", //随机串     
       *   "package" : "prepay_id=u802345jgfjsdfgsdg888",     
       *   "signType" : "MD5",         //微信签名方式:    
       *   "paySign" : "70EA570631E4BB79628FBCA90534C63FF7FADD89" //微信签名 
       * }
       * @return string 微信浏览器调起JSAPI时的输入参数，json格式可以直接做参数用
       * 更详细的说明请参考网页端调起支付API：http://pay.weixin.qq.com/wiki/doc/api/jsapi.php?chapter=7_7
       * 
       */
        public string GetJsApiParameters()
        {
            Log.Debug(this.GetType().ToString(), "JsApiPay::GetJsApiParam is processing...");

            WxPayData jsApiParam = new WxPayData();
            jsApiParam.SetValue("appId", unifiedOrderResult.GetValue("appid"));
            jsApiParam.SetValue("timeStamp", jsApiParam.GenerateTimeStamp());
            jsApiParam.SetValue("nonceStr", jsApiParam.GenerateNonceStr());
            jsApiParam.SetValue("package", "prepay_id=" + unifiedOrderResult.GetValue("prepay_id"));
            jsApiParam.SetValue("signType", "MD5");
            jsApiParam.SetValue("paySign", jsApiParam.MakeSign());

            string parameters = jsApiParam.ToJson();

            Log.Debug(this.GetType().ToString(), "Get jsApiParam : " + parameters);

            //new WxPayLog().InsertLog(string.Format("{0}:{1}", "GetJsApiParameters  jsApiParam ", parameters),
                //PayStatus.UnifiedOrderSucces.GetHashCode(),
                //payTradeNo, openid);
         
            return parameters;
        }

        /// <summary>
        /// 接收从微信支付后台发送过来的数据并验证签名
        /// </summary>
        /// <returns>微信支付后台返回的数据</returns>
        public WxPayData GetNotifyData()
        {
            //接收从微信后台POST过来的数据
            System.IO.Stream s = page.Request.InputStream;
            int count = 0;
            byte[] buffer = new byte[1024];
            StringBuilder builder = new StringBuilder();
            while ((count = s.Read(buffer, 0, 1024)) > 0)
            {
                builder.Append(Encoding.UTF8.GetString(buffer, 0, count));
            }
            s.Flush();
            s.Close();
            s.Dispose();
            Log.Info(this.GetType().ToString(), "Receive data from WeChat : " + builder.ToString());
            //转换数据格式并验证签名
            WxPayData data = new WxPayData();
            try
            {
                data.FromXml(builder.ToString());
            }
            catch (WxPayException ex)
            {
                //若签名错误，则立即返回结果给微信支付后台
                WxPayData res = new WxPayData();
                res.SetValue("return_code", "FAIL");
                res.SetValue("return_msg", ex.Message);
                Log.Error(this.GetType().ToString(), "Sign check error : " + res.ToXml());
                //new WxPayLog().InsertLog(string.Format("{0}:{1}", "GetNotifyData  Sign check error", data.ToJson()),
                    //PayStatus.IntitPayResultNotify.GetHashCode());
                page.Response.Write(res.ToXml());
                page.Response.End();
            }
            //new WxPayLog().InsertLog(string.Format("{0}:{1}", "GetNotifyData  Receive data from WeChat and Check sign success", data.ToJson()), PayStatus.IntitPayResultNotify.GetHashCode());
            Log.Info(this.GetType().ToString(), "Check sign success");
            return data;
        }
        /**
        *    
        * 查询订单
        * @param WxPayData inputObj 提交给查询订单API的参数
        * @param int timeOut 超时时间
        * @throws WxPayException
        * @return 成功时返回订单查询结果，其他抛异常
        */
        public WxPayData OrderQuery(WxPayData inputObj, int timeOut = 6)
        {
            string url = "https://api.mch.weixin.qq.com/pay/orderquery";
            //检测必填参数
            if (!inputObj.IsSet("out_trade_no") && !inputObj.IsSet("transaction_id"))
            {
                throw new WxPayException("订单查询接口中，out_trade_no、transaction_id至少填一个！");
            }

            inputObj.SetValue("appid", WxPayConfig.APPID);//公众账号ID
            inputObj.SetValue("mch_id", WxPayConfig.MCHID);//商户号
            inputObj.SetValue("nonce_str", inputObj.GenerateNonceStr());//随机字符串
            inputObj.SetValue("sign", inputObj.MakeSign());//签名

            string xml = inputObj.ToXml();

            var start = DateTime.Now;

            Log.Debug("WxPayApi", "OrderQuery request : " + xml);
            if (!WxPayConfig.CURL_TIMEOUT.Equals(0))
            {
                timeOut = WxPayConfig.CURL_TIMEOUT;
            }
            string response = HttpService.Post(xml, url, false, timeOut);//调用HTTP通信接口提交数据
            Log.Debug("WxPayApi", "OrderQuery response : " + response);

            var end = DateTime.Now;
            int timeCost = (int)((end - start).TotalMilliseconds);//获得接口耗时

            //将xml格式的数据转化为对象以返回
            WxPayData result = new WxPayData();
            result.FromXml(response);

            //ReportCostTime(url, timeCost, result);//测速上报

            return result;
        }
    }
}