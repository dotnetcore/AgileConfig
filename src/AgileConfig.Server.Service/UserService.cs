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
        private readonly IRoleDefinitionRepository _roleDefinitionRepository;


        public UserService(IUserRepository userRepository, IUserRoleRepository userRoleRepository, IRoleDefinitionRepository roleDefinitionRepository)
        {
            _userRepository = userRepository;
            _userRoleRepository = userRoleRepository;
            _roleDefinitionRepository = roleDefinitionRepository;
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
            var migratedRoles = new List<UserRole>();
            foreach (var userRole in userRoles)
            {
                if (string.IsNullOrEmpty(userRole.RoleId) && userRole.LegacyRoleValue.HasValue)
                {
                    var mappedRoleId = MapLegacyRole(userRole.LegacyRoleValue.Value);
                    if (!string.IsNullOrEmpty(mappedRoleId))
                    {
                        userRole.RoleId = mappedRoleId;
                        userRole.LegacyRoleValue = null;
                        if (userRole.CreateTime == default)
                        {
                            userRole.CreateTime = DateTime.Now;
                        }
                        migratedRoles.Add(userRole);
                    }
                }
            }

            if (migratedRoles.Any())
            {
                await _userRoleRepository.UpdateAsync(migratedRoles);
            }

            var roleIds = userRoles.Select(x => x.RoleId).Distinct().ToList();
            if (!roleIds.Any())
            {
                return new List<Role>();
            }

            var roles = await _roleDefinitionRepository.QueryAsync(x => roleIds.Contains(x.Id));
            return roles.OrderBy(r => roleIds.IndexOf(r.Id)).ToList();
        }


        public async Task<bool> UpdateAsync(User user)
        {
            await _userRepository.UpdateAsync(user);
            return true;
        }

        public async Task<bool> UpdateUserRolesAsync(string userId, List<string> roleIds)
        {
            var dbUserRoles = await _userRoleRepository.QueryAsync(x => x.UserId == userId);
            await _userRoleRepository.DeleteAsync(dbUserRoles);
            var userRoles = new List<UserRole>();
            var now = DateTime.Now;
            roleIds.Distinct().ToList().ForEach(x =>
            {
                userRoles.Add(new UserRole
                {
                    Id = Guid.NewGuid().ToString("N"),
                    UserId = userId,
                    RoleId = x,
                    LegacyRoleValue = null,
                    CreateTime = now
                });
            });

            await _userRoleRepository.InsertAsync(userRoles);
            return true;
        }

        public void Dispose()
        {
            _userRepository.Dispose();
            _userRoleRepository.Dispose();
            _roleDefinitionRepository.Dispose();
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

        public async Task<List<User>> GetUsersByRoleAsync(string roleId)
        {
            var userRoles = await _userRoleRepository.QueryAsync(x => x.RoleId == roleId);
            var userIds = userRoles.Select(x => x.UserId).Distinct().ToList();
            return await _userRepository.QueryAsync(x => userIds.Contains(x.Id));
        }

        private static string MapLegacyRole(int legacyRole)
        {
            return legacyRole switch
            {
                0 => SystemRoleConstants.SuperAdminId,
                1 => SystemRoleConstants.AdminId,
                2 => SystemRoleConstants.OperatorId,
                _ => string.Empty
            };
        }
    }
}
