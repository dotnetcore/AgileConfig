using AgileConfig.Server.Common;

namespace AgileConfig.Server.Data.Freesql
{
    public class EnvFreeSqlFactory : IFreeSqlFactory
    {
        private readonly string _env;
        public EnvFreeSqlFactory(IEnvAccessor envAccessor)
        {
            _env = envAccessor.Env;
        }

        public IFreeSql Create()
        {
            return FreeSQL.GetInstanceByEnv(_env);
        }
    }
}
