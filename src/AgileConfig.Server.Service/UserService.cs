using AgileConfig.Server.Data.Entity;
using AgileConfig.Server.IService;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;
using AgileConfig.Server.Common;
using AgileConfig.Server.Data.Abstraction;

namespace AgileConfig.Server.Service
{
    public class UserService : IUserService
    {
        private readonly IUserRepository _userRepository;
        private readonly IUserRoleRepository _userRoleRepository;


        public UserService(IUserRepository userRepository, IUserRoleRepository userRoleRepository)
        {
            _userRepository = userRepository;
            _userRoleRepository = userRoleRepository;
        }

        public async Task<bool> AddAsync(User user)
        {
            var old = (await _userRepository.QueryAsync(u => u.UserName == user.UserName && u.Status == UserStatus.Normal)).FirstOrDefault();
            if (old != null)
            {
                return false;
            }

            await _userRepository.InsertAsync(user);

            return true;
        }

        public async Task<bool> DeleteAsync(User user)
        {
            await _userRepository.DeleteAsync(user);
            return true;
        }

        public Task<List<User>> GetUsersByNameAsync(string userName)
        {
            return _userRepository.QueryAsync(u => u.UserName == userName);
        }

        public Task<User> GetUserAsync(string id)
        {
            return _userRepository.GetAsync(id);
        }


        public async Task<List<Role>> GetUserRolesAsync(string userId)
        {
            var userRoles = await _userRoleRepository.QueryAsync(x => x.UserId == userId);

            return userRoles.Select(x => x.Role).ToList();
        }


        public async Task<bool> UpdateAsync(User user)
        {
            await _userRepository.UpdateAsync(user);
            return true;
        }

        public async Task<bool> UpdateUserRolesAsync(string userId, List<Role> roles)
        {
            var dbUserRoles = await _userRoleRepository.QueryAsync(x => x.UserId == userId);
            await _userRoleRepository.DeleteAsync(dbUserRoles);
            var userRoles = new List<UserRole>();
            roles.ForEach(x =>
            {
                userRoles.Add(new UserRole
                {
                    Id = Guid.NewGuid().ToString("N"),
                    UserId = userId,
                    Role = x
                });
            });

            await _userRoleRepository.InsertAsync(userRoles);
            return true;
        }

        public void Dispose()
        {
            _userRepository.Dispose();
            _userRoleRepository.Dispose();
        }

        public Task<List<User>> GetAll()
        {
            return _userRepository.AllAsync();
        }

        public async Task<bool> ValidateUserPassword(string userName, string password)
        {
            var user = (await _userRepository.QueryAsync(u => u.Status == UserStatus.Normal && u.UserName == userName)).FirstOrDefault();
            if (user == null)
            {
                return false;
            }

            if (user.Password == Encrypt.Md5(password + user.Salt))
            {
                return true;
            }

            return false;
        }

        public async Task<List<User>> GetUsersByRoleAsync(Role role)
        {
            var userRoles = await _userRoleRepository.QueryAsync(x => x.Role == role);
            var userIds = userRoles.Select(x => x.UserId).Distinct().ToList();
            return await _userRepository.QueryAsync(x => userIds.Contains(x.Id));
        }
    }
}
