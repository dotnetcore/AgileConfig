using AgileConfig.Server.Data.Entity;
using AgileConfig.Server.Data.Freesql;
using AgileConfig.Server.IService;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;
using AgileConfig.Server.Common;

namespace AgileConfig.Server.Service
{
    public class UserService : IUserService
    {
        private FreeSqlContext _dbContext;

        public UserService(FreeSqlContext context)
        {
            _dbContext = context;
        }

        public async Task<bool> AddAsync(User user)
        {
            var old = await _dbContext.Users.Where(u => u.UserName == user.UserName && u.Status == UserStatus.Normal).FirstAsync();
            if (old != null)
            {
                return false;
            }

            await _dbContext.Users.AddAsync(user);
            var result = await _dbContext.SaveChangesAsync();

            return result > 0;
        }

        public async Task<bool> DeleteAsync(User user)
        {
            _dbContext.Users.Remove(user);
            var result = await _dbContext.SaveChangesAsync();

            return result > 0;
        }

        public async Task<List<User>> GetUsersByNameAsync(string userName)
        {
            return await _dbContext.Users.Where(u => u.UserName == userName).ToListAsync();
        }

        public async Task<User> GetUserAsync(string id)
        {
            return await _dbContext.Users.Where(u => u.Id == id).ToOneAsync();
        }


        public async Task<List<Role>> GetUserRolesAsync(string userId)
        {
            var userRoles = await _dbContext.UserRoles.Where(x => x.UserId == userId).ToListAsync();

            return userRoles.Select(x => x.Role).ToList();
        }


        public async Task<bool> UpdateAsync(User user)
        {
            await _dbContext.Users.UpdateAsync(user);
            var result = await _dbContext.SaveChangesAsync();

            return result > 0;
        }

        public async Task<bool> UpdateUserRolesAsync(string userId, List<Role> roles)
        {
            await _dbContext.UserRoles.RemoveAsync(x => x.UserId == userId);
            var userRoles = new List<UserRole>();
            roles.ForEach(x => {
                userRoles.Add(new UserRole
                {
                    Id = Guid.NewGuid().ToString("N"),
                    UserId = userId,
                    Role = x
                });
            });

            await _dbContext.UserRoles.AddRangeAsync(userRoles);
            var result = await _dbContext.SaveChangesAsync();

            return result > 0;
        }

        public void Dispose()
        {
            _dbContext?.Dispose();
        }

        public async Task<List<User>> GetAll()
        {
            return await _dbContext.Users.Where(x => 1 == 1).ToListAsync();
        }

        public async Task<bool> ValidateUserPassword(string userName, string password)
        {
            var user = await _dbContext.Users.Where(u => u.Status == UserStatus.Normal && u.UserName == userName).FirstAsync();
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
            var users = await FreeSQL.Instance.Select<User, UserRole>()
                .InnerJoin((a, b) => a.Id == b.UserId)
                .Where((a, b) => b.Role == role)
                .ToListAsync((a, b) => a);

            return users;
        }

        public User GetUser(string userId)
        {
            return _dbContext.Users.Where(u => u.Id == userId).ToOne();
        }
    }
}
