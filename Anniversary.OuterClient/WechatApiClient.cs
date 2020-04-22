using Anniversary.Model;
using Anniversary.Model.Models;
using Anniversary.OuterClient.Extensions;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;

namespace Anniversary.OuterClient
{
    public class WechatApiClient: IWechatApiClient
    {
        private ILogger _logger;

        public HttpClient _client { get; private set; }

        public WechatApiClient(HttpClient httpClient)
        {
            _client = httpClient;
        }

        public async Task<MessageModel<WeChatApi>> GetOpenIDAsync(string appid, string secret, string code)
        {
            MessageModel<WeChatApi> result = new MessageModel<WeChatApi>();

            string funName = "Get Wechat Data By Code";

            var msgReq = new List<string>{
                appid,
                secret,
                code
            };

            try
            {
                result = await HttpClientExtensions.PostData<MessageModel<WeChatApi>>(_client, _logger, "/sns/jscode2session", msgReq);

                if (sendRet != null)
                {
                    _logger.LogInformation($"{funName},{hrcodeStr}推送消息成功");
                    result.state = true;
                    result.data = sendRet.Return_data.MessageId;
                }
                else
                {
                    result.state = false;
                    result.error_msg = sendRet.Return_msg;
                    _logger.LogError($"{funName}：{hrcodeStr}推送消息失败：{sendRet.Return_msg}");
                }

            }
            catch (Exception ex)
            {
                result.state = false;
                result.error_code = ErrorCode.OuterApiError;
                result.error_msg = funName + "调用外部接口异常：。" + ex.Message;
                _logger.LogError(ex, (string)$"{funName}向{hrcodeStr}推送消息处理异常：{ex.Message}");
            }


            return result;
        }

        Task<MessageModel<string>> IWechatApiClient.GetOpenIDAsync(string appid, string secret, string code)
        {
            throw new System.NotImplementedException();
        }
    }
}