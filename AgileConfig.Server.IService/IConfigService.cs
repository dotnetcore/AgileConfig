using AgileConfig.Server.Data.Entity;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AgileConfig.Server.IService
{
    public interface IConfigService: IDisposable
    {
        /// <summary>
        /// 如果环境为空，返回默认环境，默认环境为数据库设置的环境列表的第一个
        /// </summary>
        /// <param name="env"></param>
        /// <returns></returns>
        Task<string> IfEnvEmptySetDefaultAsync(string env);

        /// <summary>
        /// 发布当前待发布的配置项
        /// </summary>
        /// <param name="appId"></param>
        /// <param name="log"></param>
        /// <param name="operatorr"></param>
        /// <returns></returns>
        (bool result, string publishTimelineId) Publish(string appId, string log, string operatorr, string env);

        Task<Config> GetAsync(string id, string env);

        Task<Config> GetByAppIdKeyEnv(string appId, string group, string key, string env);
        /// <summary>
        /// 根据appId,group,key查询配置，其中group，key使用like查询
        /// </summary>
        /// <param name="appId"></param>
        /// <param name="group"></param>
        /// <param name="key"></param>
        /// <param name="env">环境</param>
        /// <returns></returns>
        Task<List<Config>> Search(string appId, string group, string key, string env);
        Task<List<Config>> GetByAppIdAsync(string appId, string env);

        /// <summary>
        /// 获取app相关的已发布的配置继承的app的配置一并查出
        /// </summary>
        /// <param name="appId"></param>
        /// <returns></returns>
        Task<List<Config>> GetPublishedConfigsByAppIdWithInheritanced(string appId, string env);
        /// <summary>
        /// 获取app的配置项继承的app配置合并进来转换为字典
        /// </summary>
        /// <param name="appId"></param>
        /// <returns></returns>
        Task<Dictionary<string, Config>> GetPublishedConfigsByAppIdWithInheritanced_Dictionary(string appId, string env);
        Task<bool> AddAsync(Config config, string env);

        Task<bool> AddRangeAsync(List<Config> configs, string env);

        Task<bool> DeleteAsync(Config config, string env);

        Task<bool> DeleteAsync(string configId, string env);

        Task<bool> UpdateAsync(Config config, string env);

        Task<bool> UpdateAsync(List<Config> configs, string env);

        /// <summary>
        /// 撤销编辑状态
        /// </summary>
        /// <param name="ids"></param>
        /// <returns></returns>
        Task<bool> CancelEdit(List<string> ids, string env);

        Task<List<Config>> GetAllConfigsAsync(string env);

        Task<int> CountEnabledConfigsAsync();

        /// <summary>
        /// 计算已发布配置项的MD5
        /// </summary>
        /// <param name="appId"></param>
        /// <returns></returns>
        Task<string> AppPublishedConfigsMd5(string appId, string env);
        /// <summary>
        /// 计算已发布配置项的MD5 合并继承app的配置
        /// </summary>
        /// <param name="appId"></param>
        /// <returns></returns>
        Task<string> AppPublishedConfigsMd5WithInheritanced(string appId, string env);
        
        /// <summary>
        /// 计算已发布配置项的MD5进行缓存
        /// </summary>
        /// <param name="appId"></param>
        /// <returns></returns>
        Task<string> AppPublishedConfigsMd5Cache(string appId, string env);

        /// <summary>
        /// 计算已发布配置项的MD5进行缓存 合并继承app的配置
        /// </summary>
        /// <param name="appId"></param>
        /// <returns></returns>
        Task<string> AppPublishedConfigsMd5CacheWithInheritanced(string appId, string env);

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
        Task<bool> IsPublishedAsync(string configId, string env);

        /// <summary>
        /// 根据发布时间点获取发布的详细信息
        /// </summary>
        /// <param name="publishTimelineId"></param>
        /// <returns></returns>
        Task<List<PublishDetail>> GetPublishDetailByPublishTimelineIdAsync(string publishTimelineId, string env);

        /// <summary>
        /// 查询发布时间节点
        /// </summary>
        /// <param name="publishTimelineId"></param>
        /// <returns></returns>
        Task<PublishTimeline> GetPublishTimeLineNodeAsync(string publishTimelineId, string env);

        /// <summary>
        /// 获取发布历史
        /// </summary>
        /// <param name="appId"></param>
        /// <returns></returns>
        Task<List<PublishTimeline>> GetPublishTimelineHistoryAsync(string appId, string env);

        /// <summary>
        /// 获取发布详情列表
        /// </summary>
        /// <param name="appId"></param>
        /// <param name="env"></param>
        /// <returns></returns>
        Task<List<PublishDetail>> GetPublishDetailListAsync(string appId, string env);

        /// <summary>
        /// 获取某个配置的发布历史
        /// </summary>
        /// <param name="configId"></param>
        /// <returns></returns>
        Task<List<PublishDetail>> GetConfigPublishedHistory(string configId, string env);

        /// <summary>
        /// 获取当前发布的配置
        /// </summary>
        /// <param name="appId"></param>
        /// <returns></returns>
        Task<List<ConfigPublished>> GetPublishedConfigsAsync(string appId, string env);

        /// <summary>
        /// 获取单个发布的配置
        /// </summary>
        /// <param name="configId"></param>
        /// <returns></returns>
        Task<ConfigPublished> GetPublishedConfigAsync(string configId, string env);

        /// <summary>
        /// 回滚至某个时刻的发布版本
        /// </summary>
        /// <param name="publishTimelineId"></param>
        /// <returns></returns>
        Task<bool> RollbackAsync(string publishTimelineId, string env);

        /// <summary>
        /// 同步到环境
        /// </summary>
        /// <param name="appId"></param>
        /// <param name="currentEnv"></param>
        /// <param name="toEnvs"></param>
        /// <returns></returns>
        Task<bool> EnvSync(string appId, string currentEnv, List<string> toEnvs);

        /// <summary>
        /// 获取配置的 键值对 列表
        /// </summary>
        /// <param name="appId"></param>
        /// <param name="env"></param>
        /// <returns></returns>
        Task<List<KeyValuePair<string, string>>> GetKvListAsync(string appId, string env);
        
        /// <summary>
        /// 把json字符串格式的配置转换为标准的config然后保存到数据库。
        /// 跟原来的配置比较，标记出新增，删除，更新
        /// </summary>
        /// <param name="json"></param>
        /// <param name="appId"></param>
        /// <param name="env"></param>
        /// <returns></returns>
        Task<bool> SaveJsonAsync(string json, string appId, string env);
        
        /// <summary>
        /// 把键值对方式的配置列表保存到数据库
        /// </summary>
        /// <param name="kvString"></param>
        /// <param name="appId"></param>
        /// <param name="env"></param>
        /// <returns></returns>
        Task<bool> SaveKvListAsync(string kvString, string appId, string env);

        /// <summary>
        /// 校验键值对的文本是否合法
        /// </summary>
        /// <param name="kvStr"></param>
        /// <returns></returns>
        (bool, string) ValidateKvString(string kvStr);

        /// <summary>
        /// clear all cache
        /// </summary>
        void ClearCache();
    }
}
