using System.Globalization;
using System.Resources;

namespace AgileConfig.Server.Common.Resources
{
    /// <summary>
    /// 资源文件访问器，用于获取本地化消息
    /// </summary>
    public static class Messages
    {
        private static ResourceManager _resourceManager;
        private static ResourceManager ResourceManager
        {
            get
            {
                if (_resourceManager == null)
                {
                    _resourceManager = new ResourceManager("AgileConfig.Server.Common.Resources.Messages", typeof(Messages).Assembly);
                }
                return _resourceManager;
            }
        }

        /// <summary>
        /// 获取本地化字符串
        /// </summary>
        /// <param name="name">资源名称</param>
        /// <param name="culture">文化信息，如果为null则使用当前线程的文化信息</param>
        /// <returns>本地化字符串</returns>
        public static string GetString(string name, CultureInfo culture = null)
        {
            return ResourceManager.GetString(name, culture ?? CultureInfo.CurrentUICulture);
        }

        /// <summary>
        /// 获取格式化的本地化字符串
        /// </summary>
        /// <param name="name">资源名称</param>
        /// <param name="args">格式化参数</param>
        /// <returns>格式化的本地化字符串</returns>
        public static string GetString(string name, params object[] args)
        {
            var format = GetString(name);
            return string.Format(format, args);
        }

        /// <summary>
        /// 获取格式化的本地化字符串
        /// </summary>
        /// <param name="name">资源名称</param>
        /// <param name="culture">文化信息</param>
        /// <param name="args">格式化参数</param>
        /// <returns>格式化的本地化字符串</returns>
        public static string GetString(string name, CultureInfo culture, params object[] args)
        {
            var format = GetString(name, culture);
            return string.Format(format, args);
        }

        // 验证消息
        public static string AppIdRequired => GetString(nameof(AppIdRequired));
        public static string AppIdMaxLength => GetString(nameof(AppIdMaxLength));
        public static string ConfigGroupMaxLength => GetString(nameof(ConfigGroupMaxLength));
        public static string ConfigKeyRequired => GetString(nameof(ConfigKeyRequired));
        public static string ConfigKeyMaxLength => GetString(nameof(ConfigKeyMaxLength));
        public static string ConfigValueMaxLength => GetString(nameof(ConfigValueMaxLength));
        public static string DescriptionMaxLength => GetString(nameof(DescriptionMaxLength));
        public static string UserIdRequired => GetString(nameof(UserIdRequired));
        public static string UserIdMaxLength => GetString(nameof(UserIdMaxLength));
        public static string UserNameRequired => GetString(nameof(UserNameRequired));
        public static string UserNameMaxLength => GetString(nameof(UserNameMaxLength));
        public static string PasswordRequired => GetString(nameof(PasswordRequired));
        public static string PasswordMaxLength => GetString(nameof(PasswordMaxLength));
        public static string TeamMaxLength => GetString(nameof(TeamMaxLength));
        public static string NodeAddressRequired => GetString(nameof(NodeAddressRequired));
        public static string NodeAddressMaxLength => GetString(nameof(NodeAddressMaxLength));
        public static string RemarkMaxLength => GetString(nameof(RemarkMaxLength));
        public static string ServiceIdRequired => GetString(nameof(ServiceIdRequired));
        public static string ServiceIdMaxLength => GetString(nameof(ServiceIdMaxLength));
        public static string ServiceNameRequired => GetString(nameof(ServiceNameRequired));
        public static string ServiceNameMaxLength => GetString(nameof(ServiceNameMaxLength));
        public static string IpMaxLength => GetString(nameof(IpMaxLength));
        public static string HealthCheckModeRequired => GetString(nameof(HealthCheckModeRequired));
        public static string HealthCheckModeMaxLength => GetString(nameof(HealthCheckModeMaxLength));
        public static string CheckUrlMaxLength => GetString(nameof(CheckUrlMaxLength));
        public static string AlarmUrlMaxLength => GetString(nameof(AlarmUrlMaxLength));

        // 错误消息
        public static string IdCannotBeEmpty => GetString(nameof(IdCannotBeEmpty));
        public static string NameCannotBeEmpty => GetString(nameof(NameCannotBeEmpty));
        public static string AddressCannotBeEmpty => GetString(nameof(AddressCannotBeEmpty));
        public static string KeyCannotBeEmpty => GetString(nameof(KeyCannotBeEmpty));
        public static string AppIdCannotBeEmpty => GetString(nameof(AppIdCannotBeEmpty));

        // 配置导入消息
        public static string ConfigImportMissingEqualSign(int lineNumber) => GetString(nameof(ConfigImportMissingEqualSign), lineNumber);
        public static string ConfigImportDuplicateKey(string key) => GetString(nameof(ConfigImportDuplicateKey), key);

        // 日志消息
        public static string RegistrationCenter => GetString(nameof(RegistrationCenter));
        public static string ConfigurationCenter => GetString(nameof(ConfigurationCenter));
        public static string NotifyNodeAllClients(string node, string action1, string action2, string response) => GetString(nameof(NotifyNodeAllClients), node, action1, action2, response);
        public static string NotifyNodeAppClients(string node, string app, string action1, string action2, string response) => GetString(nameof(NotifyNodeAppClients), node, app, action1, action2, response);
        public static string NotifyNodeSpecificClient(string node, string client, string action1, string action2, string response) => GetString(nameof(NotifyNodeSpecificClient), node, client, action1, action2, response);
        public static string Success => GetString(nameof(Success));
        public static string Failed => GetString(nameof(Failed));

        // 事件日志消息
        public static string LoginSuccess(string user) => GetString(nameof(LoginSuccess), user);
        public static string AdminPasswordInitialized => GetString(nameof(AdminPasswordInitialized));
        public static string PasswordReset(string admin, string user) => GetString(nameof(PasswordReset), admin, user);
        public static string PasswordChangeSuccess(string user) => GetString(nameof(PasswordChangeSuccess), user);
        public static string AppAdded(string user, string appId, string appName) => GetString(nameof(AppAdded), user, appId, appName);
        public static string AppUpdated(string user, string appId, string appName) => GetString(nameof(AppUpdated), user, appId, appName);
        public static string AppStatusChanged(string user, string status, string appId) => GetString(nameof(AppStatusChanged), user, status, appId);
        public static string AppDeleted(string user, string appId) => GetString(nameof(AppDeleted), user, appId);
        public static string ConfigAdded(string user, string group, string key, string appId, string env) => GetString(nameof(ConfigAdded), user, group, key, appId, env);
        public static string ConfigUpdated(string user, string group, string key, string appId, string env) => GetString(nameof(ConfigUpdated), user, group, key, appId, env);
        public static string ConfigDeleted(string user, string group, string key, string appId, string env) => GetString(nameof(ConfigDeleted), user, group, key, appId, env);
        public static string ConfigBatchDeleted(string user, string env) => GetString(nameof(ConfigBatchDeleted), user, env);
        public static string ConfigPublished(string user, string appId, string env, string version) => GetString(nameof(ConfigPublished), user, appId, env, version);
        public static string ConfigRolledBack(string user, string appId, string env, string version) => GetString(nameof(ConfigRolledBack), user, appId, env, version);
        public static string ConfigCancelled(string user, string group, string key, string appId, string env) => GetString(nameof(ConfigCancelled), user, group, key, appId, env);
        public static string ConfigBatchCancelled(string user, string env) => GetString(nameof(ConfigBatchCancelled), user, env);
        public static string NodeAdded(string user, string node) => GetString(nameof(NodeAdded), user, node);
        public static string NodeDeleted(string user, string node) => GetString(nameof(NodeDeleted), user, node);
        public static string UserAdded(string admin, string user) => GetString(nameof(UserAdded), admin, user);
        public static string UserUpdated(string admin, string user) => GetString(nameof(UserUpdated), admin, user);
        public static string Enabled => GetString(nameof(Enabled));
        public static string Disabled => GetString(nameof(Disabled));

        // 异常消息
        public static string MongodbConnectionStringNotConfigured => GetString(nameof(MongodbConnectionStringNotConfigured));

        // 登录消息
        public static string PasswordCannotBeEmpty => GetString(nameof(PasswordCannotBeEmpty));
        public static string PasswordError => GetString(nameof(PasswordError));
        public static string UserDeleted => GetString(nameof(UserDeleted));
        public static string PasswordMaxLength50 => GetString(nameof(PasswordMaxLength50));
        public static string PasswordMismatch => GetString(nameof(PasswordMismatch));
        public static string PasswordAlreadySet => GetString(nameof(PasswordAlreadySet));
        public static string InitPasswordFailed => GetString(nameof(InitPasswordFailed));
        public static string DemoModeNoPasswordChange => GetString(nameof(DemoModeNoPasswordChange));
        public static string OriginalPasswordCannotBeEmpty => GetString(nameof(OriginalPasswordCannotBeEmpty));
        public static string OriginalPasswordError => GetString(nameof(OriginalPasswordError));
        public static string NewPasswordCannotBeEmpty => GetString(nameof(NewPasswordCannotBeEmpty));
        public static string NewPasswordMaxLength50 => GetString(nameof(NewPasswordMaxLength50));
        public static string NewPasswordMismatch => GetString(nameof(NewPasswordMismatch));
        public static string UserNotFound => GetString(nameof(UserNotFound));
        public static string ChangePasswordFailed => GetString(nameof(ChangePasswordFailed));

        // 配置消息
        public static string AppNotExists(string appId) => GetString(nameof(AppNotExists), appId);
        public static string ConfigExists => GetString(nameof(ConfigExists));
        public static string CreateConfigFailed => GetString(nameof(CreateConfigFailed));
        public static string DuplicateConfig(string key) => GetString(nameof(DuplicateConfig), key);
        public static string BatchCreateConfigFailed => GetString(nameof(BatchCreateConfigFailed));
        public static string ConfigNotFound => GetString(nameof(ConfigNotFound));
        public static string UpdateConfigFailed => GetString(nameof(UpdateConfigFailed));
        public static string DeleteConfigFailed => GetString(nameof(DeleteConfigFailed));
        public static string BatchDeleteConfigFailed => GetString(nameof(BatchDeleteConfigFailed));

        // 应用消息
        public static string AppIdExists => GetString(nameof(AppIdExists));
        public static string CreateAppFailed => GetString(nameof(CreateAppFailed));
        public static string AppNotFound => GetString(nameof(AppNotFound));
        public static string DemoModeNoTestAppEdit => GetString(nameof(DemoModeNoTestAppEdit));
        public static string UpdateAppFailed => GetString(nameof(UpdateAppFailed));
        public static string ConfigKeyExists => GetString(nameof(ConfigKeyExists));

        // App Controller specific messages
        public static string CurrentCannotBeLessThanOne => GetString(nameof(CurrentCannotBeLessThanOne));
        public static string PageSizeCannotBeLessThanOne => GetString(nameof(PageSizeCannotBeLessThanOne));
        public static string DemoModeNoClientDisconnect => GetString(nameof(DemoModeNoClientDisconnect));

        // Server Node Messages
        public static string NodeAlreadyExists => GetString(nameof(NodeAlreadyExists));
        public static string AddNodeFailed => GetString(nameof(AddNodeFailed));
        public static string DemoModeNoNodeDelete => GetString(nameof(DemoModeNoNodeDelete));
        public static string NodeNotFound => GetString(nameof(NodeNotFound));
        public static string DeleteNodeFailed => GetString(nameof(DeleteNodeFailed));

        // Service Messages
        public static string ServiceAlreadyExists => GetString(nameof(ServiceAlreadyExists));
        public static string ServiceNotFound => GetString(nameof(ServiceNotFound));
        public static string CurrentCannotBeLessThanOneService => GetString(nameof(CurrentCannotBeLessThanOneService));
        public static string PageSizeCannotBeLessThanOneService => GetString(nameof(PageSizeCannotBeLessThanOneService));

        // User Messages
        public static string CurrentCannotBeLessThanOneUser => GetString(nameof(CurrentCannotBeLessThanOneUser));
        public static string PageSizeCannotBeLessThanOneUser => GetString(nameof(PageSizeCannotBeLessThanOneUser));
        public static string UserAlreadyExists(string userName) => GetString(nameof(UserAlreadyExists), userName);
        public static string AddUserFailed => GetString(nameof(AddUserFailed));
        public static string UserNotFoundForOperation => GetString(nameof(UserNotFoundForOperation));
        public static string UpdateUserFailed => GetString(nameof(UpdateUserFailed));
        public static string ResetUserPasswordFailed => GetString(nameof(ResetUserPasswordFailed));
        public static string DeleteUserFailed => GetString(nameof(DeleteUserFailed));
    }
}
