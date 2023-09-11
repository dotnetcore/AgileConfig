namespace AgileConfig.Server.IService;

public interface IJwtService
{
    string Issuer { get; }
    string Audience { get; }
    int ExpireSeconds { get; }
    string GetSecurityKey();
    string GetToken(string userId, string userName, bool isAdmin);
}