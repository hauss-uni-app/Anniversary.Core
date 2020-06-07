using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Anniversary.Model;
using Anniversary.Model.Models;
using Anniversary.OuterClient.Extensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace Anniversary.OuterClient.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class WechatApiController : ControllerBase
    {
        private readonly ILogger<WechatApiController> _logger;

        private readonly IHttpClientFactory _clientFactory;

        private HttpClient _httpClient;

        public WechatApiController(ILogger<WechatApiController> logger, IHttpClientFactory clientFactory)
        {
            _logger = logger;
            _clientFactory = clientFactory;
            _httpClient = _clientFactory.CreateClient("Wechat");
        }

        [HttpGet]
        public IActionResult Ping()
        {
            return Ok();
        }

        [Route("getwechatopenid")]
        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> GetOpenIDAsync(string appid, string secret, string code)
        {
            MessageModel<WeChatApi> result = new MessageModel<WeChatApi>();
            //throw new HttpRequestException();
            string funName = "Get Wechat Data By Code";

            //try
            //{
            var url = $"/sns/jscode2session?appid={appid}&secret={secret}&js_code={code}&grant_type=authorization_code";

            var wechatResponse = await HttpClientExtensions.GetData<WeChatApi>(_httpClient, _logger, url);

            if (!string.IsNullOrEmpty(wechatResponse.openid))
            {
                _logger.LogInformation($"{funName},获取open - {wechatResponse.openid}");
                result.success = true;
                result.msg = "获取OpenId成功";
                result.response = wechatResponse;
            }
            else
            {
                result.success = false;
                result.msg = "获取OpenId失败";
                _logger.LogError($"{funName}：获取OpenId失败");
            }

            //}
            //catch (Exception ex)
            //{
            //    result.success = false;
            //    result.msg = funName + "调用外部接口异常：。" + ex.Message;
            //    _logger.LogError(ex, (string)$"{funName}调用外部接口异常：{ex.Message}");
            //}

            return Ok(result);
        }

    }
}