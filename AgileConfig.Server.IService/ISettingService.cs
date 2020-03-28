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

        Task<List<Setting>> GetAllSettingsAsync();

        string AdminPasswordSettingKey { get; }

        Task<bool> HasAdminPassword();

        Task<bool> SetAdminPassword(string password);

        Task<bool> ValidateAdminPassword(string password);
    }
}
