using AgileConfig.Server.Data.Entity;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AgileConfig.Server.IService
{
    public interface ISettingService
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
        /// 管理员密码存储键
        /// </summary>
        string AdminPasswordSettingKey { get; }

        /// <summary>
        /// 是否已经设置密码
        /// </summary>
        /// <returns></returns>
        Task<bool> HasAdminPassword();

        /// <summary>
        /// 设置管理员密码
        /// </summary>
        /// <param name="password"></param>
        /// <returns></returns>
        Task<bool> SetAdminPassword(string password);

        /// <summary>
        /// 校验管理员密码
        /// </summary>
        /// <param name="password"></param>
        /// <returns></returns>
        Task<bool> ValidateAdminPassword(string password);
    }
}
