using Anniversary.IRepository;
using Anniversary.IServices;
using Anniversary.Model.Models;
using Anniversary.Services.Base;
using System;
using System.Collections.Generic;
using System.Text;

namespace Anniversary.Services
{
    public class InfoServices : BaseServices<Info>, IInfoServices
    {
        IInfoRepository _dal;

        public InfoServices(IInfoRepository dal)
        {
            this._dal = dal;
            base.BaseDal = dal;
        }
    }
}
