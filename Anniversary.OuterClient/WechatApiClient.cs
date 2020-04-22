using Anniversary.Model;
using Anniversary.Model.Models;
using Anniversary.OuterClient.Extensions;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;

namespace Anniversary.OuterClient
{
    public class WechatApiClient: IWechatApiClient
    {
        private readonly ILogger<WechatApiClient> _logger;

        public HttpClient _client { get; private set; }

        public WechatApiClient(ILogger<WechatApiClient> logger, HttpClient httpClient)
        {
            _logger = logger;
            _client = httpClient;
        }

        public async Task<MessageModel<WeChatApi>> GetOpenIDAsync(string appid, string secret, string code)
        {
            MessageModel<WeChatApi> result = new MessageModel<WeChatApi>();

            string funName = "Get Wechat Data By Code";

            WeChatApiPara weChatApiPara = new WeChatApiPara() { 
                
                appid = appid,
                secret = secret, 
                js_code = code
            };

            try
            {
               var wechatResponse = await HttpClientExtensions.PostData<WeChatApi>(_client, _logger, "/sns/jscode2session", weChatApiPara);

                if (!string.IsNullOrEmpty(wechatResponse.openid ))
                {
                    _logger.LogInformation($"{funName},获取open - {wechatResponse.openid}");
                    result.success = true;
                    result.response = wechatResponse;
                }
                else
                {
                    result.success = false;
                    result.msg = "获取OpenId失败";
                    _logger.LogError($"{funName}：获取OpenId失败");
                }

            }
            catch (Exception ex)
            {
                result.success = false;
                result.msg = funName + "调用外部接口异常：。" + ex.Message;
                _logger.LogError(ex, (string)$"{funName}调用外部接口异常：{ex.Message}");
            }

            return result;
        }
    }
}