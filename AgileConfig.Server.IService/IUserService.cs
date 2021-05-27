using AgileConfig.Server.Data.Entity;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace AgileConfig.Server.IService
{
    public interface IUserService: IDisposable
    {
        Task<List<User>> GetAll();
        Task<User> GetUserAsync(string userId);

        Task<User> GetUserByNameAsync(string userName);
        Task<List<Role>> GetUserRolesAsync(string userId);

        Task<bool> AddAsync(User user);

        Task<bool> DeleteAsync(User user);

        Task<bool> UpdateAsync(User user);

        Task<List<User>> GetUserByTeamAsync(string teamId);

        Task<List<Team>> GetUserTeamsAsync(string userId);

        Task<bool> AddUserToTeam(string userId, string teamId);

        Task<bool> UpdateUserAppRoles(string userId, string appId, List<Role> roles);

        Task<bool> RemoveUserFromTeam(string userId, string teamId);
    }
}
