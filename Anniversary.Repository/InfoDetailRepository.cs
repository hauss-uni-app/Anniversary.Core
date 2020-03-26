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
    public class InfoDetailRepository : BaseRepository<InfoDetail>, IInfoDetailRepository
    {
        public InfoDetailRepository(IUnitOfWork unitOfWork) : base(unitOfWork)
        {
        }
    }
}
