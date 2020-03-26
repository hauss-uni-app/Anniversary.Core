using Anniversary.IRepository;
using Anniversary.IRepository.IUnitOfWork;
using Anniversary.Model.Models;
using Anniversary.Repository.Base;
using Anniversary.Repository.UnitOfWork;
using System;
using System.Collections.Generic;
using System.Text;

namespace Anniversary.Repository
{
    public class UserRepository : BaseRepository<User>, IUserRepository
    {
        public UserRepository(IUnitOfWork unitOfWork) : base(unitOfWork)
        {
        }
    }
}
