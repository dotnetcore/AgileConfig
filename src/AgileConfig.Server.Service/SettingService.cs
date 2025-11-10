using System.Collections.Generic;
using System.Threading.Tasks;
using AgileConfig.Server.Common;
using AgileConfig.Server.Data.Abstraction;
using AgileConfig.Server.Data.Entity;
using AgileConfig.Server.IService;

namespace AgileConfig.Server.Service;

public class SettingService : ISettingService
{
    private readonly ISettingRepository _settingRepository;
    private readonly IUserRepository _userRepository;
    private readonly IUserRoleRepository _userRoleRepository;

    public SettingService(
        ISettingRepository settingRepository,
        IUserRepository userRepository,
        IUserRoleRepository userRoleRepository)
    {
        _settingRepository = settingRepository;
        _userRepository = userRepository;
        _userRoleRepository = userRoleRepository;
    }

    public async Task<bool> AddAsync(Setting setting)
    {
        await _settingRepository.InsertAsync(setting);
        return true;
    }

    public async Task<bool> DeleteAsync(Setting setting)
    {
        var setting2 = await _settingRepository.GetAsync(setting.Id);
        if (setting2 != null) await _settingRepository.DeleteAsync(setting);
        return true;
    }

    public async Task<bool> DeleteAsync(string settingId)
    {
        var setting = await _settingRepository.GetAsync(settingId);
        if (setting != null) await _settingRepository.DeleteAsync(setting);
        return true;
    }

    public Task<Setting> GetAsync(string id)
    {
        return _settingRepository.GetAsync(id);
    }

    public Task<List<Setting>> GetAllSettingsAsync()
    {
        return _settingRepository.AllAsync();
    }

    public async Task<bool> UpdateAsync(Setting setting)
    {
        await _settingRepository.UpdateAsync(setting);
        return true;
    }

    public void Dispose()
    {
        _settingRepository.Dispose();
        _userRepository.Dispose();
        _userRoleRepository.Dispose();
    }

    public async Task<string[]> GetEnvironmentList()
    {
        if (ISettingService.EnvironmentList != null) return ISettingService.EnvironmentList;

        var environments = await _settingRepository.GetAsync(SystemSettings.DefaultEnvironmentKey);

        ISettingService.EnvironmentList = environments?.Value?.ToUpper().Split(',') ?? [];

        return ISettingService.EnvironmentList;
    }
}