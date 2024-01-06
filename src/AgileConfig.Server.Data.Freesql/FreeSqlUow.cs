using FreeSql;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AgileConfig.Server.Data.Freesql
{
    public class FreeSqlUow : Abstraction.IUow
    {
        private readonly IFreeSql _freeSql;
        private IUnitOfWork _unitOfWork;


        public FreeSqlUow(IFreeSql freeSql)
        {
            this._freeSql = freeSql;
            _unitOfWork = _freeSql.CreateUnitOfWork();
        }

        public IUnitOfWork GetFreesqlUnitOfWork()
        {
            return _unitOfWork;
        }   

        public Task<bool> SaveChangesAsync()
        {
            _unitOfWork.Commit();

            return Task.FromResult(true);
        }

        public void Dispose()
        {
            _unitOfWork?.Dispose();
        }
    }
}
