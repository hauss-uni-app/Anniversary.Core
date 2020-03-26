using System;
using System.Collections.Generic;
using System.Text;
using SqlSugar;

namespace Anniversary.IRepository.IUnitOfWork
{
    public interface IUnitOfWork
    {
        SqlSugarClient GetDbClient();

        void BeginTran();

        void CommitTran();
        void RollbackTran();
    }
}
