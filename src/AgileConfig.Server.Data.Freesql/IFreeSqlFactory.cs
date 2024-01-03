namespace AgileConfig.Server.Data.Freesql
{
    public interface IFreeSqlFactory
    {
        IFreeSql Create(string env);
    }
}