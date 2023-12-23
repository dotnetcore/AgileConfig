using AgileConfig.Server.Common;
using AgileConfig.Server.Data.Entity;
using AgileConfig.Server.Data.Mongodb;
using AgileConfig.Server.IService;
using MongoDB.Driver;
using MongoDB.Driver.Linq;

namespace AgileConfig.Server.Mongodb.Service;

public class UserService(IRepository<User> repository, IRepository<UserRole> userRoleRepository) : IUserService
{
    public void Dispose()
    {
        GC.SuppressFinalize(this);
    }

    public Task<List<User>> GetAll()
    {
        return repository.SearchFor(x => true).ToListAsync();
    }

    public Task<User?> GetUserAsync(string userId)
    {
        return repository.FindAsync(userId);
    }

    public User? GetUser(string userId)
    {
        return repository.Find(userId);
    }

    public Task<List<User>> GetUsersByNameAsync(string userName)
    {
        return repository.SearchFor(x => x.UserName == userName).ToListAsync();
    }

    public async Task<List<Role>> GetUserRolesAsync(string userId)
    {
        var userRoles = await userRoleRepository.SearchFor(x => x.UserId == userId).ToListAsync();
        return userRoles.Select(x => x.Role).ToList();
    }

    public async Task<bool> AddAsync(User user)
    {
        var old = repository.SearchFor(x => x.UserName == user.UserName && x.Status == UserStatus.Normal)
            .FirstOrDefault();
        if (old != null)
            return false;

        await repository.InsertAsync(user);
        return true;
    }

    public async Task<bool> DeleteAsync(User user)
    {
        var result = await repository.DeleteAsync(user.Id.ToString());
        return result.DeletedCount > 0;
    }

    public async Task<bool> UpdateAsync(User user)
    {
        var result = await repository.UpdateAsync(user);
        return result.ModifiedCount > 0;
    }

    public async Task<bool> UpdateUserRolesAsync(string userId, List<Role> roles)
    {
        await userRoleRepository.DeleteAsync(x => x.UserId == userId);
        var userRoles = roles.Select(x => new UserRole
            {
                Id = Guid.NewGuid().ToString("N"),
                UserId = userId,
                Role = x
            })
            .ToList();

        await userRoleRepository.InsertAsync(userRoles);
        return true;
    }

    public async Task<bool> ValidateUserPassword(string userName, string password)
    {
        var user = await repository.SearchFor(x => x.Status == UserStatus.Normal && x.UserName == userName)
            .FirstOrDefaultAsync();
        if (user == null)
            return false;

        if (user.Password == Encrypt.Md5(password + user.Salt))
        {
            return true;
        }

        return false;
    }

    public Task<List<User>> GetUsersByRoleAsync(Role role)
    {
        return (from user in repository.MongodbQueryable
            join userRole in userRoleRepository.MongodbQueryable
                on user.Id equals userRole.UserId
            where userRole.Role == role
            select user).ToListAsync();
    }
}