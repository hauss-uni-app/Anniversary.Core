using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Anniversary.Common.Helper;
using Anniversary.IServices;
using Anniversary.Model;
using Anniversary.Model.Models;
using DnsClient;
using Microsoft.AspNetCore.Authorization;
//using Anniversary.OuterClient;
//using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Resilience;
using SqlSugar;

namespace Anniversary.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UserController : ControllerBase
    {
        readonly IUserServices _userServices;
        readonly IInfoServices _infoServices;
        readonly IInfoDetailServices _infoDetailServices;
        //readonly IWechatApiClient _wechatApiClient;
        private ILogger<UserController> _logger;
        private IHttpClient _httpClient;
        private string _wechatApiServiceUrl;

        public UserController(IUserServices userServices,
            IInfoServices infoServices,
            IInfoDetailServices infoDetailServices,
            IHttpClient httpClient = null,
            IDnsQuery dnsQuery = null,
            IOptions<Options.ServiceDiscoveryOptions> serviceDiscoveryOptions = null,
            ILogger<UserController> logger = null/*, IWechatApiClient wechatApiClient = null*/)
        {
            _userServices = userServices;
            _infoServices = infoServices;
            _infoDetailServices = infoDetailServices;
            _logger = logger;
            _httpClient = httpClient;
            //_wechatApiClient = wechatApiClient;


            var address = dnsQuery.ResolveService("service.consul",
                serviceDiscoveryOptions.Value.OuterClientServiceName);
            var addressList = address.First().AddressList;

            var host = addressList.Any() ? addressList.First().ToString() : address.First().HostName;
            var port = address.First().Port;

            _wechatApiServiceUrl = $"http://{host}:{port}";
        }

        /// <returns></returns>
        [HttpGet]
        [AllowAnonymous]
        public async Task<MessageModel<List<AllData>>> Get(string openId = "123456", DateTime? date = null)
        {
            List<AllData> allDatas = new List<AllData>();

            if (!string.IsNullOrEmpty(openId))
            {
                User user = (await _userServices.Query(q => q.OpenId == openId)).FirstOrDefault();

                if (user != null)
                {
                    List<Info> infos = (await _infoServices.Query(q => q.OpenId == openId)).ToList();

                    foreach (Info info in infos)
                    {
                        List<InfoDetail> infoDetails = new List<InfoDetail>();

                        DateTime? lastMonth = null;

                        DateTime? nextMonth = null;

                        if (date != null)
                        {
                            lastMonth = new DateTime(date.Value.Year, date.Value.Month, 1).AddMonths(-1);

                            nextMonth = lastMonth.Value.AddMonths(3).AddDays(-1);
                        }

                        if (lastMonth != null && nextMonth != null)
                        {
                            infoDetails = (await _infoDetailServices.Query(q =>
                            q.InfoId == info.InfoId
                            && SqlFunc.Between(q.Date.Value, lastMonth.Value, nextMonth.Value)
                            )).ToList();
                        }
                        else
                        {
                            infoDetails = (await _infoDetailServices.Query(q =>
                            q.InfoId == info.InfoId
                            )).ToList();
                        }

                        allDatas.Add(new AllData()
                        {
                            user = user,
                            info = info,
                            infoDetail = infoDetails
                        }); ;
                    }
                }
                else
                {
                    allDatas = await UserInitialAsync(openId);
                }
            }

            return new MessageModel<List<AllData>>()
            {
                msg = "获取成功",
                success = allDatas.Count > 0,
                response = allDatas
            };
        }

        [HttpGet]
        [AllowAnonymous]
        [Route("Version")]
        public async Task<MessageModel<int>> GetVersion(string openId = "123456")
        {
            int responseVersion = -1;

            if (!string.IsNullOrEmpty(openId))
            {
                User user = (await _userServices.Query(q => q.OpenId == openId)).FirstOrDefault();
                if (user != null)
                    responseVersion = user.Version;
            }

            return new MessageModel<int>()
            {
                msg = "获取成功",
                success = responseVersion >= 0,
                response = responseVersion
            };
        }

        [HttpPost]
        [AllowAnonymous]
        [Route("GetOpenId")]
        public async Task<MessageModel<WeChatApi>> GetOpenId(string code)
        {
            //MessageModel<WeChatApi> result = new MessageModel<WeChatApi>();

            //result = await _wechatApiClient.GetOpenIDAsync(Appsettings.app(new string[] { "Wechat", "AppID" }), Appsettings.app(new string[] { "Wechat", "AppSecret" }), code);

            //return result;

            _logger.LogTrace($"Enter into GetOpenId:{code}");
            var form = new Dictionary<string, string> {
                {"appid", Appsettings.app(new string[] { "Wechat", "AppID" })},
                {"secret", Appsettings.app(new string[] { "Wechat", "AppSecret" })},
                {"code", code},
            };

            try
            {
                var response = await _httpClient.PostAsync(string.Concat(_wechatApiServiceUrl, "/api/WechatApi/getwechatopenid"), form);
                if (response.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    var result = await response.Content.ReadAsStringAsync();
                    var weChatApiInfo = JsonConvert.DeserializeObject<MessageModel<WeChatApi>>(result);
                    _logger.LogTrace($"Completed GetOpenId with openid:{weChatApiInfo.response?.openid}");
                    return weChatApiInfo;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(" GetOpenId 在重试之后失败", ex.Message + ex.StackTrace);
                throw ex;
            }
            return null;

        }

        private async Task<List<AllData>> UserInitialAsync(string openId)
        {
            try
            {
                _userServices.GetAdo().BeginTran();

                List<AllData> allDatas = new List<AllData>();

                User newUser = new User
                {
                    OpenId = openId,
                    Version = 0
                };

                var userId = await _userServices.Add(newUser, false);
                if (userId > 0)
                {

                    Info newInfo = new Info()
                    {
                        OpenId = openId,
                        Name = "初体验"
                    };

                    var infoId = (await _infoServices.Add(newInfo));

                    if (infoId > 0)
                    {
                        InfoDetail infoDetail0day = new InfoDetail()
                        {
                            InfoId = infoId,
                            Date = DateTime.Now,
                            Count = 0,
                            Type = "日"
                        };

                        List<InfoDetail> newInfoDetails = new List<InfoDetail> {
                            new InfoDetail()
                            {
                                InfoId = infoId,
                                Date = DateTime.Now.Date,
                                Count = 0,
                                Type = "日"
                            },
                            new InfoDetail()
                            {
                                InfoId = infoId,
                                Date = DateTime.Now.Date.AddDays(100),
                                Count = 100,
                                Type = "日"
                            },
                            new InfoDetail()
                            {
                                InfoId = infoId,
                                Date = DateTime.Now.Date.AddMonths(100),
                                Count = 100,
                                Type = "月"
                            },
                            new InfoDetail()
                            {
                                InfoId = infoId,
                                Date = DateTime.Now.Date.AddYears(100),
                                Count = 100,
                                Type = "年"
                            }
                        };

                        bool addSuccess = true;

                        foreach (var infoDetail in newInfoDetails)
                        {
                            var infoDetailId = await _infoDetailServices.Add(infoDetail);

                            if (infoDetailId <= 0)
                            {
                                addSuccess = false;
                                break;
                            }
                        }

                        if (addSuccess)
                        {
                            allDatas.Add(new AllData()
                            {
                                user = newUser,
                                info = newInfo,
                                infoDetail = newInfoDetails
                            });
                        }
                    }
                }

                if (allDatas.Count > 0)
                {
                    _userServices.GetAdo().CommitTran();
                }
                else
                {
                    _userServices.GetAdo().RollbackTran();
                }

                return allDatas;
            }
            catch (Exception ex)
            {
                _userServices.GetAdo().RollbackTran();
                throw ex;
            }
        }
    }
}
