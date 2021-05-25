using AgileConfig.Server.Data.Entity;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace AgileConfig.Server.IService
{
    public interface ITeamService: IDisposable
    {
        Task<bool> AddAsync(Team team);
        Task<Team> GetAsync(string teamId);
        Task<bool> RemoveAsync(Team team);
        Task<bool> UpdateAsync(Team team);
    }
}
