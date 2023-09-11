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
        User GetUser(string userId);

        Task<List<User>> GetUsersByNameAsync(string userName);
        Task<List<Role>> GetUserRolesAsync(string userId);

        Task<bool> AddAsync(User user);

        Task<bool> DeleteAsync(User user);

        Task<bool> UpdateAsync(User user);


        Task<bool> UpdateUserRolesAsync(string userId, List<Role> roles);


        Task<bool> ValidateUserPassword(string userName, string password);

        Task<List<User>> GetUsersByRoleAsync(Role role);

    }
}
