using AgileConfig.Server.Data.Entity;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AgileConfig.Server.IService
{
    public interface ISettingService: IDisposable
    {
        Task<Setting> GetAsync(string id);
        Task<bool> AddAsync(Setting setting);

        Task<bool> DeleteAsync(Setting setting);

        Task<bool> DeleteAsync(string settingId);

        Task<bool> UpdateAsync(Setting setting);

        /// <summary>
        /// 获取所有的设定
        /// </summary>
        /// <returns></returns>
        Task<List<Setting>> GetAllSettingsAsync();

        /// <summary>
        /// 是否已经设置密码
        /// </summary>
        /// <returns></returns>
        Task<bool> HasSuperAdmin();

        /// <summary>
        /// 设置管理员密码
        /// </summary>
        /// <param name="password"></param>
        /// <returns></returns>
        Task<bool> SetSuperAdminPassword(string password);

        /// <summary>
        /// 初始化环境列表
        /// </summary>
        /// <returns></returns>
        Task<bool> InitDefaultEnvironment();

        /// <summary>
        /// 获取环境列表
        /// </summary>
        /// <returns></returns>
        Task<string[]> GetEnvironmentList();
    }
}
