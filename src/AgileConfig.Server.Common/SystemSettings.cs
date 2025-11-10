namespace AgileConfig.Server.Common;

public static class SystemSettings
{
    public const string SuperAdminId = "super_admin";
    public const string SuperAdminUserName = "admin";

    public const string DefaultEnvironment = "DEV,TEST,STAGING,PROD";
    public const string DefaultEnvironmentKey = "environment";
    public const string DefaultJwtSecretKey = "jwtsecret";
}