using AgileConfig.Server.Data.Entity;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AgileConfig.Server.IService
{
    public interface IConfigService: IDisposable
    {
        /// <summary>
        /// 发布当前待发布的配置项
        /// </summary>
        /// <param name="appId"></param>
        /// <param name="log"></param>
        /// <param name="operatorr"></param>
        /// <returns></returns>
        (bool result, string publishTimelineId) Publish(string appId, string log, string operatorr);

        Task<Config> GetAsync(string id);

        Task<Config> GetByAppIdKey(string appId, string group, string key);
        /// <summary>
        /// 根据appId,group,key查询配置，其中group，key使用like查询
        /// </summary>
        /// <param name="appId"></param>
        /// <param name="group"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        Task<List<Config>> Search(string appId, string group, string key);
        Task<List<Config>> GetByAppIdAsync(string appId);

        /// <summary>
        /// 获取app相关的已发布的配置继承的app的配置一并查出
        /// </summary>
        /// <param name="appId"></param>
        /// <returns></returns>
        Task<List<Config>> GetPublishedConfigsByAppIdWithInheritanced(string appId);
        /// <summary>
        /// 获取app的配置项继承的app配置合并进来转换为字典
        /// </summary>
        /// <param name="appId"></param>
        /// <returns></returns>
        Task<Dictionary<string, Config>> GetPublishedConfigsByAppIdWithInheritanced_Dictionary(string appId);
        Task<bool> AddAsync(Config config);

        Task<bool> AddRangeAsync(List<Config> configs);

        Task<bool> DeleteAsync(Config config);

        Task<bool> DeleteAsync(string configId);

        Task<bool> UpdateAsync(Config config);

        Task<bool> UpdateAsync(List<Config> configs);

        /// <summary>
        /// 撤销编辑状态
        /// </summary>
        /// <param name="ids"></param>
        /// <returns></returns>
        Task<bool> CancelEdit(List<string> ids);

        Task<List<Config>> GetAllConfigsAsync();

        Task<int> CountEnabledConfigsAsync();

        /// <summary>
        /// 计算已发布配置项的MD5
        /// </summary>
        /// <param name="appId"></param>
        /// <returns></returns>
        Task<string> AppPublishedConfigsMd5(string appId);
        /// <summary>
        /// 计算已发布配置项的MD5 合并继承app的配置
        /// </summary>
        /// <param name="appId"></param>
        /// <returns></returns>
        Task<string> AppPublishedConfigsMd5WithInheritanced(string appId);
        
        /// <summary>
        /// 计算已发布配置项的MD5进行缓存
        /// </summary>
        /// <param name="appId"></param>
        /// <returns></returns>
        Task<string> AppPublishedConfigsMd5Cache(string appId);

        /// <summary>
        /// 计算已发布配置项的MD5进行缓存 合并继承app的配置
        /// </summary>
        /// <param name="appId"></param>
        /// <returns></returns>
        Task<string> AppPublishedConfigsMd5CacheWithInheritanced(string appId);

        /// <summary>
        /// 构造key
        /// </summary>
        /// <param name="config"></param>
        /// <returns></returns>
        string GenerateKey(Config config);

        /// <summary>
        /// 判断是否已经发布
        /// </summary>
        /// <param name="configId"></param>
        /// <returns></returns>
        Task<bool> IsPublishedAsync(string configId);

        /// <summary>
        /// 根据发布时间点获取发布的详细信息
        /// </summary>
        /// <param name="publishTimelineId"></param>
        /// <returns></returns>
        Task<List<PublishDetail>> GetPublishDetailByPublishTimelineIdAsync(string publishTimelineId);

        /// <summary>
        /// 查询发布时间节点
        /// </summary>
        /// <param name="publishTimelineId"></param>
        /// <returns></returns>
        Task<PublishTimeline> GetPublishTimeLineNodeAsync(string publishTimelineId);

        /// <summary>
        /// 获取发布历史
        /// </summary>
        /// <param name="appId"></param>
        /// <returns></returns>
        Task<List<PublishTimeline>> GetPublishTimelineHistoryAsync(string appId);

        /// <summary>
        /// 获取发布详情列表
        /// </summary>
        /// <param name="appId"></param>
        /// <returns></returns>
        Task<List<PublishDetail>> GetPublishDetailListAsync(string appId);

        /// <summary>
        /// 获取某个配置的发布历史
        /// </summary>
        /// <param name="configId"></param>
        /// <returns></returns>
        Task<List<PublishDetail>> GetConfigPublishedHistory(string configId);

        /// <summary>
        /// 获取当前发布的配置
        /// </summary>
        /// <param name="appId"></param>
        /// <returns></returns>
        Task<List<ConfigPublished>> GetPublishedConfigsAsync(string appId);

        /// <summary>
        /// 获取单个发布的配置
        /// </summary>
        /// <param name="configId"></param>
        /// <returns></returns>
        Task<ConfigPublished> GetPublishedConfigAsync(string configId);

        /// <summary>
        /// 回滚至某个时刻的发布版本
        /// </summary>
        /// <param name="publishTimelineId"></param>
        /// <returns></returns>
        Task<bool> RollbackAsync(string publishTimelineId);
    }
}
