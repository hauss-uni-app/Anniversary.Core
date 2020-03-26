using Anniversary.IRepository;
using Anniversary.IServices;
using Anniversary.Model.Models;
using Anniversary.Services.Base;
using System;
using System.Collections.Generic;
using System.Text;

namespace Anniversary.Services
{
    public class UserServices : BaseServices<User>, IUserServices
    {
        IUserRepository _dal;

        public UserServices(IUserRepository dal)
        {
            this._dal = dal;
            base.BaseDal = dal;
        }
    }
}
