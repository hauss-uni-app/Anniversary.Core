﻿using Anniversary.IServices;
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
    public class InfoController : ControllerBase
    {
        readonly IUserServices _userServices;
        readonly IInfoServices _infoServices;
        readonly IInfoDetailServices _infoDetailServices;

        UserController userController;

        public InfoController(IInfoServices infoServices, IInfoDetailServices infoDetailServices, IUserServices userServices)
        {
            _userServices = userServices;
            _infoServices = infoServices;
            _infoDetailServices = infoDetailServices;
            userController = new UserController(userServices, infoServices, infoDetailServices);
        }

        [HttpPost]
        [AllowAnonymous]
        public async Task<MessageModel<List<AllData>>> Post(string openId, string name, DateTime date)
        {
            try
            {
                _userServices.GetAdo().BeginTran();

                var data = new MessageModel<List<AllData>>();

                User user = (await _userServices.Query(q => q.OpenId == openId)).FirstOrDefault();

                if (user != null)
                {
                    user.Version += 1;

                    Info info = new Info()
                    {
                        OpenId = openId,
                        Name = name
                    };

                    var infoId = (await _infoServices.Add(info));

                    if (infoId > 0)
                    {
                        InfoDetail infoDetail = new InfoDetail()
                        {
                            InfoId = infoId,
                            Date = date,
                            Count = 0,
                            Type = "日"
                        };

                        var infoDetailId = (await _infoDetailServices.Add(infoDetail));

                        if (await _userServices.Update(user) && infoDetailId > 0)
                            data.success = true;
                    }

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
        public async Task<MessageModel<List<AllData>>> Put(string openId, string infoId, string name, DateTime? date)
        {
            try
            {
                _userServices.GetAdo().BeginTran();

                var data = new MessageModel<List<AllData>>();

                if (!string.IsNullOrEmpty(name) || date != null)
                {
                    User user = (await _userServices.Query(q => q.OpenId == openId)).FirstOrDefault();

                    if (user != null)
                    {
                        user.Version += 1;

                        Info info = (await _infoServices.QueryById(infoId));

                        if (info != null)
                        {
                            if (!string.IsNullOrEmpty(name))
                            {
                                info.Name = name;
                            }
                            if (await _userServices.Update(user) && await _infoServices.Update(info))
                            {
                                if (date != null)
                                {
                                    InfoDetail infoDetail = (await _infoDetailServices.Query(q => q.InfoId.ToString() == infoId && q.Count == 0 && q.Type == "日")).FirstOrDefault();

                                    if (infoDetail != null)
                                    {
                                        infoDetail.Date = date;

                                        if (await _infoDetailServices.Update(infoDetail))
                                        {
                                            List<InfoDetail> infoDetails = (await _infoDetailServices.Query(q => q.InfoId.ToString() == infoId && q.Count != 0 && q.Type != "日")).ToList();

                                            if (infoDetails.Count > 0)
                                            {
                                                foreach (InfoDetail infoDetail1 in infoDetails)
                                                {
                                                    infoDetail1.Date = date.Value.AddDays(infoDetail1.Count);
                                                }

                                                if (await _infoDetailServices.Update(infoDetails))
                                                    data.success = true;
                                            }
                                            else { 
                                                    data.success = true;
                                            }
                                        }
                                    }
                                }
                                else
                                {
                                    data.success = true;
                                }
                            }
                        }

                        if (data.success)
                        {
                            data.response = (await userController.Get(openId)).response;
                            data.msg = "更新成功";
                            _userServices.GetAdo().CommitTran();
                        }
                        else
                        {
                            _userServices.GetAdo().RollbackTran();
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
        public async Task<MessageModel<List<AllData>>> Delete(string openId, int infoId)
        {
            try
            {
                _userServices.GetAdo().BeginTran();

                var data = new MessageModel<List<AllData>>();

                User user = (await _userServices.Query(q => q.OpenId == openId)).FirstOrDefault();

                if (user != null)
                {
                    user.Version += 1;

                    if (await _userServices.Update(user))
                        if (await _infoServices.DeleteById(infoId))
                        {
                            List<InfoDetail> infoDetails = (await _infoDetailServices.Query(q => q.InfoId == infoId)).ToList();

                            bool deletedSuccess = await _infoDetailServices.DeleteByIds(infoDetails.Select(s => s.InfoDetailId.ToString()).ToArray());

                            if (deletedSuccess)
                                data.success = true;
                        }

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
    }

}