using AgileConfig.Server.Data.Abstraction;
using MongoDB.Driver;
using MongoDB.Driver.Core.Clusters;

namespace AgileConfig.Server.Data.Mongodb;

/// <summary>
///     This is a empty implementation of IUow for mongodb.
/// </summary>
public class MongodbUow : IUow
{
    private bool _disposed;
    public IClientSessionHandle? Session { get; private set; }

    public async Task<bool> SaveChangesAsync()
    {
        if (Session == null)
        {
        }
        else
        {
            await Session.CommitTransactionAsync();
        }

        return true;
    }

    public void Rollback()
    {
        Session?.AbortTransaction();
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    public void Begin()
    {
        if (Session?.IsInTransaction != true) Session?.StartTransaction();
    }

    private void Dispose(bool disposing)
    {
        if (!_disposed)
        {
            if (disposing) Session?.Dispose();

            _disposed = true;
        }
    }

    internal void SetSession(IClientSessionHandle session)
    {
        if (session.Client.Cluster.Description.Type == ClusterType.Standalone)
        {
            // standalone mode is not support transaction.
        }
        else
        {
            Session = session;
        }
    }
}