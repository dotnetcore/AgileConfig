namespace AgileConfig.Server.Data.Freesql
{
    public class DefaultFreeSqlFactory : IFreeSqlFactory
    {
        public IFreeSql Create()
        {
            return FreeSQL.Instance;
        }
    }
}
