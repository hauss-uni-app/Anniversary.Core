using Anniversary.IRepository;
using Anniversary.IServices;
using Anniversary.Model.Models;
using Anniversary.Services.Base;
using System;
using System.Collections.Generic;
using System.Text;

namespace Anniversary.Services
{
    public class InfoDetailServices : BaseServices<InfoDetail>, IInfoDetailServices
    {
        IInfoDetailRepository _dal;

        public InfoDetailServices(IInfoDetailRepository dal)
        {
            this._dal = dal;
            base.BaseDal = dal;
        }
    }
}
