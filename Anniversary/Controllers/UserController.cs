using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Anniversary.IServices;
using Anniversary.Model;
using Anniversary.Model.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
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

        public UserController(IUserServices userServices, IInfoServices infoServices, IInfoDetailServices infoDetailServices)
        {
            _userServices = userServices;
            _infoServices = infoServices;
            _infoDetailServices = infoDetailServices;
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

            return new MessageModel<List<AllData>>()
            {
                msg = "获取成功",
                success = allDatas.Count > 0,
                response = allDatas
            };
        }


    }
}
