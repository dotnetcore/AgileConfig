using AgileConfig.Server.Common;

namespace AgileConfig.Server.Data.Freesql
{
    public class EnvFreeSqlFactory : IFreeSqlFactory
    {
        private readonly IMyFreeSQL _freeSQL;

        public EnvFreeSqlFactory(IMyFreeSQL freeSQL)
        {
            this._freeSQL = freeSQL;
        }

        public IFreeSql Create(string env)
        {
            return _freeSQL.GetInstanceByEnv(env);
        }
    }
}
