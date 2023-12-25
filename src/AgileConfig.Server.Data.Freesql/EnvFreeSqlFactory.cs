using AgileConfig.Server.Common;

namespace AgileConfig.Server.Data.Freesql
{
    public class EnvFreeSqlFactory : IFreeSqlFactory
    {
        private readonly IEnvAccessor _envAccessor;
        public EnvFreeSqlFactory(IEnvAccessor envAccessor)
        {
            _envAccessor = envAccessor;
        }
        public IFreeSql Create()
        {
            return FreeSQL.GetInstanceByEnv(_envAccessor.Env);
        }
    }
}
