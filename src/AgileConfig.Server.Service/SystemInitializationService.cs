using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AgileConfig.Server.Common;
using AgileConfig.Server.Data.Abstraction;
using AgileConfig.Server.Data.Entity;
using AgileConfig.Server.IService;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace AgileConfig.Server.Service;

public class SystemInitializationService(
    ISysInitRepository sysInitRepository,
    IConfiguration configuration,
    ILogger<SystemInitializationService> logger,
    IRoleDefinitionRepository roleDefinitionRepository,
    IFunctionRepository functionRepository,
    IRoleFunctionRepository roleFunctionRepository) : ISystemInitializationService
{
    /// <summary>
    ///     Initialize the JWT secret if it is not configured via file or environment variables.
    /// </summary>
    /// <returns></returns>
    public bool TryInitJwtSecret()
    {
        var jwtSecretFromConfig = configuration["JwtSetting:SecurityKey"];
        if (string.IsNullOrEmpty(jwtSecretFromConfig))
        {
            var jwtSecretSetting = sysInitRepository.GetJwtTokenSecret();
            if (jwtSecretSetting == null)
            {
                var setting = new Setting
                {
                    Id = SystemSettings.DefaultJwtSecretKey,
                    Value = GenerateJwtSecretKey(),
                    CreateTime = DateTime.Now
                };

                try
                {
                    sysInitRepository.SaveInitSetting(setting);
                    return true;
                }
                catch (Exception e)
                {
                    // Handle concurrent initialization across multiple instances.
                    Console.WriteLine(e);
                }

                return false;
            }
        }

        return true;
    }

    public bool TryInitDefaultEnvironment()
    {
        var envArrayString = sysInitRepository.GetDefaultEnvironmentFromDb();
        if (envArrayString == null)
        {
            envArrayString = SystemSettings.DefaultEnvironment;
            var setting = new Setting
            {
                Id = SystemSettings.DefaultEnvironmentKey,
                Value = envArrayString,
                CreateTime = DateTime.Now
            };
            try
            {
                sysInitRepository.SaveInitSetting(setting);
            }
            catch (Exception e)
            {
                logger.LogError(
                    "TryInitDefaultEnvironment error, maybe exec this saveing action in parallel on another node.");
            }
        }

        ISettingService.EnvironmentList = envArrayString.Split(new[] { "," }, StringSplitOptions.RemoveEmptyEntries);

        return true;
    }

    /// <summary>
    ///     Initialize the super administrator password, optionally reading from configuration when no value is provided.
    /// </summary>
    /// <param name="password">Plain text password to set for the super administrator, or empty to read from configuration.</param>
    /// <returns>True if the password is already set or initialization completed successfully; otherwise false.</returns>
    public bool TryInitSaPassword(string password = "")
    {
        if (string.IsNullOrEmpty(password)) password = configuration["saPassword"];
        if (!string.IsNullOrEmpty(password) && !sysInitRepository.HasSa())
            try
            {
                sysInitRepository.InitSa(password);
                logger.LogInformation("Init super admin password successful.");
            }
            catch (Exception e)
            {
                logger.LogError(e, "Init super admin password occur error.");
                return false;
            }

        return true;
    }

    public bool HasSa()
    {
        return sysInitRepository.HasSa();
    }

    public bool TryInitDefaultApp(string appName = "")
    {
        if (string.IsNullOrEmpty(appName)) appName = configuration["defaultApp"];

        if (!string.IsNullOrEmpty(appName))
            try
            {
                sysInitRepository.InitDefaultApp(appName);
                logger.LogInformation("Init default app {appName} successful.", appName);
            }
            catch (Exception e)
            {
                logger.LogError(e, "Init default app {appName} error.", appName);
                return false;
            }

        return true;
    }

    public async Task<bool> TryInitSuperAdminRole()
    {
        try
        {
            // Check if SuperAdministrator role already exists
            var superAdminRole = await roleDefinitionRepository.GetAsync(SystemRoleConstants.SuperAdminId);
            if (superAdminRole != null)
            {
                logger.LogInformation("SuperAdministrator role already exists.");
                return true;
            }

            // Create SuperAdministrator role
            superAdminRole = new Role
            {
                Id = SystemRoleConstants.SuperAdminId,
                Name = "SuperAdministrator",
                Description = "System super administrator with all permissions",
                IsSystem = true,
                CreateTime = DateTime.Now
            };

            await roleDefinitionRepository.InsertAsync(superAdminRole);
            logger.LogInformation("SuperAdministrator role created successfully.");

            // Get all existing functions
            var allFunctions = await functionRepository.AllAsync();

            // If no functions exist yet, create them from the Functions class constants
            if (allFunctions.Count == 0) allFunctions = await InitializeFunctions();

// Bind all functions to SuperAdministrator role
            var roleFunctions = new List<RoleFunction>();
            foreach (var function in allFunctions)
                roleFunctions.Add(new RoleFunction
                {
                    Id = Guid.NewGuid().ToString("N"),
                    RoleId = SystemRoleConstants.SuperAdminId,
                    FunctionId = function.Id,
                    CreateTime = DateTime.Now
                });

            if (roleFunctions.Count > 0)
            {
                await roleFunctionRepository.InsertAsync(roleFunctions);
                logger.LogInformation("Bound {count} functions to SuperAdministrator role.", roleFunctions.Count);
            }

            return true;
        }
        catch (Exception e)
        {
            logger.LogError(e, "Failed to initialize SuperAdministrator role.");
            return false;
        }
    }

    /// <summary>
    ///     Generate a 38-character JWT secret key.
    /// </summary>
    /// <returns></returns>
    private static string GenerateJwtSecretKey()
    {
        var guid1 = Guid.NewGuid().ToString("n");
        var guid2 = Guid.NewGuid().ToString("n");

        return guid1[..19] + guid2[..19];
    }

    private async Task<List<Function>> InitializeFunctions()
    {
        var functions = new List<Function>
        {
            // Application permissions
            new()
            {
                Id = Functions.App_Read,
                Code = Functions.App_Read,
                Name = "Read Application",
                Description = "Permission to read applications",
                Category = "Application",
                SortIndex = 1,
                CreateTime = DateTime.Now
            },
            new()
            {
                Id = Functions.App_Add,
                Code = Functions.App_Add,
                Name = "Add Application",
                Description = "Permission to add new applications",
                Category = "Application",
                SortIndex = 2,
                CreateTime = DateTime.Now
            },
            new()
            {
                Id = Functions.App_Edit,
                Code = Functions.App_Edit,
                Name = "Edit Application",
                Description = "Permission to edit applications",
                Category = "Application",
                SortIndex = 3,
                CreateTime = DateTime.Now
            },
            new()
            {
                Id = Functions.App_Delete,
                Code = Functions.App_Delete,
                Name = "Delete Application",
                Description = "Permission to delete applications",
                Category = "Application",
                SortIndex = 4,
                CreateTime = DateTime.Now
            },
            new()
            {
                Id = Functions.App_Auth,
                Code = Functions.App_Auth,
                Name = "Authorize Application",
                Description = "Permission to authorize applications",
                Category = "Application",
                SortIndex = 5,
                CreateTime = DateTime.Now
            },
            // Configuration permissions
            new()
            {
                Id = Functions.Confing_Read,
                Code = Functions.Confing_Read,
                Name = "Read Configuration",
                Description = "Permission to read configurations",
                Category = "Configuration",
                SortIndex = 6,
                CreateTime = DateTime.Now
            },
            new()
            {
                Id = Functions.Config_Add,
                Code = Functions.Config_Add,
                Name = "Add Configuration",
                Description = "Permission to add configurations",
                Category = "Configuration",
                SortIndex = 7,
                CreateTime = DateTime.Now
            },
            new()
            {
                Id = Functions.Config_Edit,
                Code = Functions.Config_Edit,
                Name = "Edit Configuration",
                Description = "Permission to edit configurations",
                Category = "Configuration",
                SortIndex = 8,
                CreateTime = DateTime.Now
            },
            new()
            {
                Id = Functions.Config_Delete,
                Code = Functions.Config_Delete,
                Name = "Delete Configuration",
                Description = "Permission to delete configurations",
                Category = "Configuration",
                SortIndex = 9,
                CreateTime = DateTime.Now
            },
            new()
            {
                Id = Functions.Config_Publish,
                Code = Functions.Config_Publish,
                Name = "Publish Configuration",
                Description = "Permission to publish configurations",
                Category = "Configuration",
                SortIndex = 10,
                CreateTime = DateTime.Now
            },
            new()
            {
                Id = Functions.Config_Offline,
                Code = Functions.Config_Offline,
                Name = "Offline Configuration",
                Description = "Permission to offline configurations",
                Category = "Configuration",
                SortIndex = 11,
                CreateTime = DateTime.Now
            },
            // Node permissions
            new()
            {
                Id = Functions.Node_Read,
                Code = Functions.Node_Read,
                Name = "Read Node",
                Description = "Permission to read nodes",
                Category = "Node",
                SortIndex = 12,
                CreateTime = DateTime.Now
            },
            new()
            {
                Id = Functions.Node_Add,
                Code = Functions.Node_Add,
                Name = "Add Node",
                Description = "Permission to add nodes",
                Category = "Node",
                SortIndex = 13,
                CreateTime = DateTime.Now
            },
            new()
            {
                Id = Functions.Node_Delete,
                Code = Functions.Node_Delete,
                Name = "Delete Node",
                Description = "Permission to delete nodes",
                Category = "Node",
                SortIndex = 14,
                CreateTime = DateTime.Now
            },
            // Client permissions
            new()
            {
                Id = Functions.Client_Refresh,
                Code = Functions.Client_Refresh,
                Name = "Refresh Client",
                Description = "Permission to refresh clients",
                Category = "Client",
                SortIndex = 15,
                CreateTime = DateTime.Now
            },
            new()
            {
                Id = Functions.Client_Disconnect,
                Code = Functions.Client_Disconnect,
                Name = "Disconnect Client",
                Description = "Permission to disconnect clients",
                Category = "Client",
                SortIndex = 16,
                CreateTime = DateTime.Now
            },
            // User permissions
            new()
            {
                Id = Functions.User_Read,
                Code = Functions.User_Read,
                Name = "Read User",
                Description = "Permission to read users",
                Category = "User",
                SortIndex = 17,
                CreateTime = DateTime.Now
            },
            new()
            {
                Id = Functions.User_Add,
                Code = Functions.User_Add,
                Name = "Add User",
                Description = "Permission to add users",
                Category = "User",
                SortIndex = 18,
                CreateTime = DateTime.Now
            },
            new()
            {
                Id = Functions.User_Edit,
                Code = Functions.User_Edit,
                Name = "Edit User",
                Description = "Permission to edit users",
                Category = "User",
                SortIndex = 19,
                CreateTime = DateTime.Now
            },
            new()
            {
                Id = Functions.User_Delete,
                Code = Functions.User_Delete,
                Name = "Delete User",
                Description = "Permission to delete users",
                Category = "User",
                SortIndex = 20,
                CreateTime = DateTime.Now
            },
            // Role permissions
            new()
            {
                Id = Functions.Role_Read,
                Code = Functions.Role_Read,
                Name = "Read Role",
                Description = "Permission to read roles",
                Category = "Role",
                SortIndex = 21,
                CreateTime = DateTime.Now
            },
            new()
            {
                Id = Functions.Role_Add,
                Code = Functions.Role_Add,
                Name = "Add Role",
                Description = "Permission to add roles",
                Category = "Role",
                SortIndex = 22,
                CreateTime = DateTime.Now
            },
            new()
            {
                Id = Functions.Role_Edit,
                Code = Functions.Role_Edit,
                Name = "Edit Role",
                Description = "Permission to edit roles",
                Category = "Role",
                SortIndex = 23,
                CreateTime = DateTime.Now
            },
            new()
            {
                Id = Functions.Role_Delete,
                Code = Functions.Role_Delete,
                Name = "Delete Role",
                Description = "Permission to delete roles",
                Category = "Role",
                SortIndex = 24,
                CreateTime = DateTime.Now
            },
            // Service permissions
            new()
            {
                Id = Functions.Service_Read,
                Code = Functions.Service_Read,
                Name = "Read Service",
                Description = "Permission to read services",
                Category = "Service",
                SortIndex = 25,
                CreateTime = DateTime.Now
            },
            new()
            {
                Id = Functions.Service_Add,
                Code = Functions.Service_Add,
                Name = "Add Service",
                Description = "Permission to add services",
                Category = "Service",
                SortIndex = 26,
                CreateTime = DateTime.Now
            },
            new()
            {
                Id = Functions.Service_Delete,
                Code = Functions.Service_Delete,
                Name = "Delete Service",
                Description = "Permission to delete services",
                Category = "Service",
                SortIndex = 27,
                CreateTime = DateTime.Now
            },
            // System permissions
            new()
            {
                Id = Functions.Log_Read,
                Code = Functions.Log_Read,
                Name = "Read Log",
                Description = "Permission to read system logs",
                Category = "Log",
                SortIndex = 28,
                CreateTime = DateTime.Now
            }
        };

        await functionRepository.InsertAsync(functions);
        logger.LogInformation("Initialized {count} system functions.", functions.Count);

        return functions;
    }
}