using AgileConfig.Server.Data.Entity;
using AgileConfig.Server.Data.Mongodb;
using AgileConfig.Server.IService;
using MongoDB.Driver;
using MongoDB.Driver.Linq;

namespace AgileConfig.Server.Mongodb.Service;

public class SysLogService(IRepository<SysLog> repository) : ISysLogService
{
    public async Task<bool> AddRangeAsync(IEnumerable<SysLog> logs)
    {
        await repository.InsertAsync(logs.ToList());
        return true;
    }

    public async Task<bool> AddSysLogAsync(SysLog log)
    {
        await repository.InsertAsync(log);
        return true;
    }

    public async Task<long> Count(string appId, SysLogType? logType, DateTime? startTime, DateTime? endTime)
    {
        var query = repository.SearchFor(x => true);
        if (!string.IsNullOrEmpty(appId))
        {
            query = query.Where(x => x.AppId == appId);
        }

        if (startTime.HasValue)
        {
            query = query.Where(x => x.LogTime >= startTime);
        }

        if (endTime.HasValue)
        {
            query = query.Where(x => x.LogTime < endTime);
        }

        if (logType.HasValue)
        {
            query = query.Where(x => x.LogType == logType);
        }

        var count = await query.CountAsync();

        return count;
    }

    public void Dispose()
    {
        GC.SuppressFinalize(this);
    }

    public Task<List<SysLog>> SearchPage(string appId, SysLogType? logType, DateTime? startTime, DateTime? endTime,
        int pageSize, int pageIndex)
    {
        var query = repository.SearchFor(x => true);
        if (!string.IsNullOrEmpty(appId))
        {
            query = query.Where(x => x.AppId == appId);
        }

        if (startTime.HasValue)
        {
            query = query.Where(x => x.LogTime >= startTime);
        }

        if (endTime.HasValue)
        {
            query = query.Where(x => x.LogTime < endTime);
        }

        if (logType.HasValue)
        {
            query = query.Where(x => x.LogType == logType);
        }

        query = query.OrderByDescending(x => x.Id).Skip((pageIndex - 1) * pageSize).Take(pageSize);

        return query.ToListAsync();
    }
}