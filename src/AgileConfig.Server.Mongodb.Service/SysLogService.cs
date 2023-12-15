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
        var insertLogs = logs.ToList();
        var newId = await GenerateIdAsync();
        foreach (var item in insertLogs)
        {
            item.Id = newId;
            newId++;
        }
        await repository.InsertAsync(insertLogs);
        return true;
    }

    public async Task<bool> AddSysLogAsync(SysLog log)
    {
        if (log.Id <= 0)
        {
            log.Id = await GenerateIdAsync();
        }
        await repository.InsertAsync(log);
        return true;
    }

    private async Task<int> GenerateIdAsync()
    {
        var count = await repository.MongodbQueryable.CountAsync();
        if (count == 0)
        {
            return 1;
        }
        var log = await repository.MongodbQueryable.OrderByDescending(x => x.Id).FirstAsync();
        return log.Id + 1;
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

        if (pageIndex <= 0)
        {
            pageIndex = 1;
        }

        if (pageSize <= 0)
        {
            pageSize = 1;
        }

        query = query.OrderByDescending(x => x.Id).Skip((pageIndex - 1) * pageSize).Take(pageSize);

        return query.ToListAsync();
    }
}