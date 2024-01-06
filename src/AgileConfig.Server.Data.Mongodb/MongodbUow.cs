namespace AgileConfig.Server.Data.Mongodb
{
    /// <summary>
    /// This is a empty implementation of IUow for mongodb.
    /// </summary>
    public class MongodbUow : Abstraction.IUow
    {
        public Task<bool> SaveChangesAsync()
        {
            return Task.FromResult(true);
        }

        public void Dispose()
        {
        }

        public void Begin()
        {
        }
    }
}
