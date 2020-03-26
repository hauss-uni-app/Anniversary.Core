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

        public InfoDetailController(IInfoServices infoServices, IUserServices userServices)
        {
            _userServices = userServices;
            _infoServices = infoServices;
        }

        //[HttpPost]
        //[AllowAnonymous]
        //public async Task<MessageModel<string>> Post(string infoId, DateTime date, int days)
        //{
        //    var data = new MessageModel<string>();

        //    User user = (await _userServices.Query(q => q.OpenId == openId)).FirstOrDefault();
                       
        //    if (user != null)
        //    {
        //        user.Version += 1;

        //        data.success = await _userServices.Update(user);

        //        if (data.success)
        //        {
        //            Info info = new Info();

        //            info.OpenId = openId;

        //            info.Name = name;

        //            var id = (await _infoServices.Add(info));

        //            data.success = id > 0;

        //            if (data.success)
        //            {
        //                data.response = id.ObjToString();
        //                data.msg = "添加成功";
        //            }
        //        }
        //    }
            
        //    return data;
        //}
    }
}
