using AgileConfig.Server.Data.Entity;
using AgileConfig.Server.Data.Freesql;
using AgileConfig.Server.IService;
using System.Threading.Tasks;

namespace AgileConfig.Server.Service
{
    public class TeamService : ITeamService
    {
        private FreeSqlContext _dbContext;

        public TeamService(FreeSqlContext context)
        {
            _dbContext = context;
        }

        public async Task<Team> GetAsync(string teamId)
        {
            var team = await _dbContext.Teams.Where(x => x.Id == teamId).ToOneAsync();

            return team;
        }

        public async Task<bool> AddAsync(Team team)
        {
            _dbContext.Teams.Add(team);
            var result = await _dbContext.SaveChangesAsync();

            return result > 0;
        }

        public async Task<bool> UpdateAsync(Team team)
        {
            _dbContext.Teams.Update(team);
            var result = await _dbContext.SaveChangesAsync();

            return result > 0;
        }

        public async Task<bool> RemoveAsync(Team team)
        {
            _dbContext.Teams.Remove(team);
            var result = await _dbContext.SaveChangesAsync();

            return result > 0;
        }

        public void Dispose()
        {
            _dbContext?.Dispose();
        }
    }
}
