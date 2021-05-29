using AgileConfig.Server.Data.Entity;
using AgileConfig.Server.Data.Freesql;
using AgileConfig.Server.IService;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;

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

        public async Task<User> GetUserByNameAsync(string userName)
        {
            return await _dbContext.Users.Where(u => u.UserName == userName).ToOneAsync();
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
    }
}
