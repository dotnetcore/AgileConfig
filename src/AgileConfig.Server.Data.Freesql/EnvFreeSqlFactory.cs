using AgileConfig.Server.Common;

namespace AgileConfig.Server.Data.Freesql
{
    public class EnvFreeSqlFactory : IFreeSqlFactory
    {
        public IFreeSql Create(string env)
        {
            return FreeSQL.GetInstanceByEnv(env);
        }
    }
}
