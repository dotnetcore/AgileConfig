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

        public async Task<List<User>> GetUserByTeamAsync(string teamId)
        {
            var userTeams = await _dbContext.UserTeams.Where(x => x.TeamId == teamId).ToListAsync();
            var users = new List<User>();

            userTeams.ForEach(async x =>
            {
                var user = await _dbContext.Users.Where(u => u.Id == x.UserId).ToOneAsync();
                users.Add(user);
            });

            return users;
        }

        public async Task<List<Role>> GetUserRolesAsync(string userId)
        {
            var userRoles = await _dbContext.UserRoles.Where(x => x.UserId == userId).ToListAsync();

            return userRoles.Select(x => x.Role).ToList();
        }


        public async Task<List<Team>> GetUserTeamsAsync(string userId)
        {
            var userTeams = await _dbContext.UserTeams.Where(x => x.UserId == userId ).ToListAsync();

            var teams = new List<Team>();
            userTeams.ForEach(async x=> {
                var team = await _dbContext.Teams.Where(t=>t.Id == x.TeamId).ToOneAsync();
                teams.Add(team);
            });

            return teams;
        }

        public async Task<bool> UpdateAsync(User user)
        {
            await _dbContext.Users.UpdateAsync(user);
            var result = await _dbContext.SaveChangesAsync();

            return result > 0;
        }

        public async Task<bool> AddUserToTeam(string userId, string teamId)
        {
            var userTeam = new UserTeam()
            {
                Id = Guid.NewGuid().ToString("N"),
                UserId = userId,
                TeamId = teamId
            };

            await _dbContext.UserTeams.AddAsync(userTeam);
            var result = await _dbContext.SaveChangesAsync();

            return result > 0;
        }

        public async Task<bool> RemoveUserFromTeam(string userId, string teamId)
        {
            await _dbContext.UserTeams.RemoveAsync(x => x.UserId == userId && x.TeamId == teamId);
            var result = await _dbContext.SaveChangesAsync();

            return result > 0;
        }

        public async Task<bool> UpdateUserAppRoles(string userId, string appId, List<Role> roles)
        {
            await _dbContext.UserRoles.RemoveAsync(x => x.UserId == userId && x.AppId == appId);
            var userRoles = new List<UserRole>();
            roles.ForEach(x=> {
                userRoles.Add(new UserRole { 
                    Id = Guid.NewGuid().ToString("N"),
                    AppId = appId,
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
    }
}
