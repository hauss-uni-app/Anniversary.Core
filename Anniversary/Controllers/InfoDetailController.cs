using Anniversary.IServices;
using Anniversary.Model;
using Anniversary.Model.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Anniversary.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class InfoDetailController : ControllerBase
    {
        readonly IUserServices _userServices;
        readonly IInfoServices _infoServices;
        readonly IInfoDetailServices _infoDetailServices;

        UserController userController;

        public InfoDetailController(IInfoServices infoServices, IUserServices userServices, IInfoDetailServices infoDetailServices)
        {
            _userServices = userServices;
            _infoServices = infoServices;
            _infoDetailServices = infoDetailServices;
            userController = new UserController(userServices, infoServices, infoDetailServices);
        }


        [HttpPost]
        [AllowAnonymous]
        public async Task<MessageModel<List<AllData>>> Post(string openId, int infoId, int addCount, string type)
        {
            try
            {
                _userServices.GetAdo().BeginTran();

                var data = new MessageModel<List<AllData>>();

                if (addCount != 0)
                {
                    User user = (await _userServices.Query(q => q.OpenId == openId)).FirstOrDefault();

                    if (user != null)
                    {
                        user.Version += 1;

                        InfoDetail infoDetailInitail = (await _infoDetailServices.Query(q => q.InfoId == infoId && q.Count == 0 && q.Type == "日")).FirstOrDefault();

                        if (infoDetailInitail != null)
                        {
                            InfoDetail infoDetail = new InfoDetail()
                            {
                                InfoId = infoId,
                                Type = type,
                                Count = addCount,
                                Date = GetDate(infoDetailInitail.Date.Value, addCount, type)
                            };

                            if (await _userServices.Update(user) && await _infoDetailServices.Add(infoDetail) > 0)
                                data.success = true;

                            if (data.success)
                            {
                                data.response = (await userController.Get(openId)).response;
                                data.msg = "添加成功";
                                _userServices.GetAdo().CommitTran();
                            }
                            else
                            {
                                _userServices.GetAdo().RollbackTran();
                            }
                        }
                    }
                }
                return data;
            }
            catch (Exception ex)
            {
                _userServices.GetAdo().RollbackTran();
                throw ex;
            }
        }


        [HttpPut]
        [AllowAnonymous]
        //[Route("Update")]
        public async Task<MessageModel<List<AllData>>> Put(string openId, int infoId, string infoDetailId, int addCount, string type)
        {
            try
            {
                _userServices.GetAdo().BeginTran();

                var data = new MessageModel<List<AllData>>();

                if (addCount != 0)
                {
                    User user = (await _userServices.Query(q => q.OpenId == openId)).FirstOrDefault();

                    if (user != null)
                    {
                        user.Version += 1;

                        InfoDetail infoDetail = (await _infoDetailServices.QueryById(infoDetailId));

                        InfoDetail infoDetailInitail = (await _infoDetailServices.Query(q => q.InfoId == infoId && q.Count == 0 && q.Type == "日")).FirstOrDefault();

                        if (infoDetail != null && infoDetailInitail != null)
                        {
                            infoDetail.Count = addCount;
                            infoDetail.Type = type;
                            infoDetail.Date = GetDate(infoDetailInitail.Date.Value, addCount, type);

                            if (await _userServices.Update(user) && await _infoDetailServices.Update(infoDetail))
                                data.success = true;

                            if (data.success)
                            {
                                data.response = (await userController.Get(openId)).response;
                                data.msg = "添加成功";
                                _userServices.GetAdo().CommitTran();
                            }
                            else
                            {
                                _userServices.GetAdo().RollbackTran();
                            }
                        }
                    }

                }
                return data;
            }
            catch (Exception ex)
            {
                _userServices.GetAdo().RollbackTran();
                throw ex;
            }
        }


        [HttpDelete]
        [AllowAnonymous]
        public async Task<MessageModel<List<AllData>>> Delete(string openId, int infoDetailId)
        {
            try
            {
                _userServices.GetAdo().BeginTran();

                var data = new MessageModel<List<AllData>>();

                User user = (await _userServices.Query(q => q.OpenId == openId)).FirstOrDefault();

                if (user != null)
                {
                    user.Version += 1;

                    if (await _userServices.Update(user) && await _infoServices.DeleteById(infoDetailId))
                        data.success = true;

                    if (data.success)
                    {
                        data.response = (await userController.Get(openId)).response;
                        data.msg = "删除成功";
                        _userServices.GetAdo().CommitTran();
                    }
                    else
                    {
                        _userServices.GetAdo().RollbackTran();
                    }
                }

                return data;
            }
            catch (Exception ex)
            {
                _userServices.GetAdo().RollbackTran();
                throw ex;
            }
        }

        private DateTime? GetDate(DateTime innitailDate, int addCount, string type)
        {
            switch (type)
            {
                case "年":
                    return innitailDate.AddYears(addCount);
                case "月":
                    return innitailDate.AddMonths(addCount);
                case "日":
                    return innitailDate.AddDays(addCount);
                default:
                    return null;
            }
        }

    }
}
