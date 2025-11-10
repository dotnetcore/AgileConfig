namespace AgileConfig.Server.Data.Abstraction;

public interface IUow : IDisposable
{
    void Begin();

    Task<bool> SaveChangesAsync();

    void Rollback();
}