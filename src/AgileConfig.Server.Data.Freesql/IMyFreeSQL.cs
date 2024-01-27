namespace AgileConfig.Server.Data.Freesql
{
    public interface IMyFreeSQL
    {
        IFreeSql GetInstanceByEnv(string env);
    }
}