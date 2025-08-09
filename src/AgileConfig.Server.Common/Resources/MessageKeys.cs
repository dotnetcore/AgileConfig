using Microsoft.Extensions.Localization;
using System.Diagnostics.CodeAnalysis;

namespace AgileConfig.Server.Common.Resources
{
    [ExcludeFromCodeCoverage]
    public static class MessageKeys
    {
        // Validation Messages
        public const string AppIdRequired = nameof(AppIdRequired);
        public const string AppIdMaxLength = nameof(AppIdMaxLength);
        public const string ConfigGroupMaxLength = nameof(ConfigGroupMaxLength);
        public const string ConfigKeyRequired = nameof(ConfigKeyRequired);
        public const string ConfigKeyMaxLength = nameof(ConfigKeyMaxLength);
        public const string ConfigValueMaxLength = nameof(ConfigValueMaxLength);
        public const string DescriptionMaxLength = nameof(DescriptionMaxLength);
        public const string UserIdRequired = nameof(UserIdRequired);
        public const string UserIdMaxLength = nameof(UserIdMaxLength);
        public const string UserNameRequired = nameof(UserNameRequired);
        public const string UserNameMaxLength = nameof(UserNameMaxLength);
        public const string PasswordRequired = nameof(PasswordRequired);
        public const string PasswordMaxLength = nameof(PasswordMaxLength);
        public const string TeamMaxLength = nameof(TeamMaxLength);
        public const string NodeAddressRequired = nameof(NodeAddressRequired);
        public const string NodeAddressMaxLength = nameof(NodeAddressMaxLength);
        public const string RemarkMaxLength = nameof(RemarkMaxLength);
        public const string ServiceIdRequired = nameof(ServiceIdRequired);
        public const string ServiceIdMaxLength = nameof(ServiceIdMaxLength);
        public const string ServiceNameRequired = nameof(ServiceNameRequired);
        public const string ServiceNameMaxLength = nameof(ServiceNameMaxLength);
        public const string IpMaxLength = nameof(IpMaxLength);
        public const string HealthCheckModeRequired = nameof(HealthCheckModeRequired);
        public const string HealthCheckModeMaxLength = nameof(HealthCheckModeMaxLength);
        public const string CheckUrlMaxLength = nameof(CheckUrlMaxLength);
        public const string AlarmUrlMaxLength = nameof(AlarmUrlMaxLength);

        // Error Messages
        public const string IdCannotBeEmpty = nameof(IdCannotBeEmpty);
        public const string NameCannotBeEmpty = nameof(NameCannotBeEmpty);
        public const string AddressCannotBeEmpty = nameof(AddressCannotBeEmpty);
        public const string KeyCannotBeEmpty = nameof(KeyCannotBeEmpty);
        public const string AppIdCannotBeEmpty = nameof(AppIdCannotBeEmpty);

        // Config Import Messages
        public const string ConfigImportMissingEqualSign = nameof(ConfigImportMissingEqualSign);
        public const string ConfigImportDuplicateKey = nameof(ConfigImportDuplicateKey);

        // Log Messages
        public const string RegistrationCenter = nameof(RegistrationCenter);
        public const string ConfigurationCenter = nameof(ConfigurationCenter);
        public const string NotifyNodeAllClients = nameof(NotifyNodeAllClients);
        public const string NotifyNodeAppClients = nameof(NotifyNodeAppClients);
        public const string NotifyNodeSpecificClient = nameof(NotifyNodeSpecificClient);
        public const string Success = nameof(Success);
        public const string Failed = nameof(Failed);

        // Event Log Messages
        public const string LoginSuccess = nameof(LoginSuccess);
        public const string AdminPasswordInitialized = nameof(AdminPasswordInitialized);
        public const string PasswordReset = nameof(PasswordReset);
        public const string PasswordChangeSuccess = nameof(PasswordChangeSuccess);
        public const string AppAdded = nameof(AppAdded);
        public const string AppUpdated = nameof(AppUpdated);
        public const string AppStatusChanged = nameof(AppStatusChanged);
        public const string AppDeleted = nameof(AppDeleted);
        public const string ConfigAdded = nameof(ConfigAdded);
        public const string ConfigUpdated = nameof(ConfigUpdated);
        public const string ConfigDeleted = nameof(ConfigDeleted);
        public const string ConfigBatchDeleted = nameof(ConfigBatchDeleted);
        public const string ConfigPublished = nameof(ConfigPublished);
        public const string ConfigRolledBack = nameof(ConfigRolledBack);
        public const string ConfigCancelled = nameof(ConfigCancelled);
        public const string ConfigBatchCancelled = nameof(ConfigBatchCancelled);
        public const string NodeAdded = nameof(NodeAdded);
        public const string NodeDeleted = nameof(NodeDeleted);
        public const string UserAdded = nameof(UserAdded);
        public const string UserUpdated = nameof(UserUpdated);
        public const string Enabled = nameof(Enabled);
        public const string Disabled = nameof(Disabled);

        // Exception Messages
        public const string MongodbConnectionStringNotConfigured = nameof(MongodbConnectionStringNotConfigured);

        // Login Messages
        public const string PasswordCannotBeEmpty = nameof(PasswordCannotBeEmpty);
        public const string PasswordError = nameof(PasswordError);
        public const string UserDeleted = nameof(UserDeleted);
        public const string PasswordMaxLength50 = nameof(PasswordMaxLength50);
        public const string PasswordMismatch = nameof(PasswordMismatch);
        public const string PasswordAlreadySet = nameof(PasswordAlreadySet);
        public const string InitPasswordFailed = nameof(InitPasswordFailed);
        public const string DemoModeNoPasswordChange = nameof(DemoModeNoPasswordChange);
        public const string OriginalPasswordCannotBeEmpty = nameof(OriginalPasswordCannotBeEmpty);
        public const string OriginalPasswordError = nameof(OriginalPasswordError);
        public const string NewPasswordCannotBeEmpty = nameof(NewPasswordCannotBeEmpty);
        public const string NewPasswordMaxLength50 = nameof(NewPasswordMaxLength50);
        public const string NewPasswordMismatch = nameof(NewPasswordMismatch);
        public const string UserNotFound = nameof(UserNotFound);
        public const string ChangePasswordFailed = nameof(ChangePasswordFailed);

        // Config Messages
        public const string AppNotExists = nameof(AppNotExists);
        public const string ConfigExists = nameof(ConfigExists);
        public const string CreateConfigFailed = nameof(CreateConfigFailed);
        public const string DuplicateConfig = nameof(DuplicateConfig);
        public const string BatchCreateConfigFailed = nameof(BatchCreateConfigFailed);
        public const string ConfigNotFound = nameof(ConfigNotFound);
        public const string UpdateConfigFailed = nameof(UpdateConfigFailed);
        public const string DeleteConfigFailed = nameof(DeleteConfigFailed);
        public const string BatchDeleteConfigFailed = nameof(BatchDeleteConfigFailed);

        // App Messages
        public const string AppIdExists = nameof(AppIdExists);
        public const string CreateAppFailed = nameof(CreateAppFailed);
        public const string AppNotFound = nameof(AppNotFound);
        public const string DemoModeNoTestAppEdit = nameof(DemoModeNoTestAppEdit);
        public const string UpdateAppFailed = nameof(UpdateAppFailed);
        public const string ConfigKeyExists = nameof(ConfigKeyExists);

        // App Controller specific messages
        public const string CurrentCannotBeLessThanOne = nameof(CurrentCannotBeLessThanOne);
        public const string PageSizeCannotBeLessThanOne = nameof(PageSizeCannotBeLessThanOne);
        public const string DemoModeNoClientDisconnect = nameof(DemoModeNoClientDisconnect);

        // Server Node Messages
        public const string NodeAlreadyExists = nameof(NodeAlreadyExists);
        public const string AddNodeFailed = nameof(AddNodeFailed);
        public const string DemoModeNoNodeDelete = nameof(DemoModeNoNodeDelete);
        public const string NodeNotFound = nameof(NodeNotFound);
        public const string DeleteNodeFailed = nameof(DeleteNodeFailed);

        // Service Messages
        public const string ServiceAlreadyExists = nameof(ServiceAlreadyExists);
        public const string ServiceNotFound = nameof(ServiceNotFound);
        public const string CurrentCannotBeLessThanOneService = nameof(CurrentCannotBeLessThanOneService);
        public const string PageSizeCannotBeLessThanOneService = nameof(PageSizeCannotBeLessThanOneService);

        // User Messages
        public const string CurrentCannotBeLessThanOneUser = nameof(CurrentCannotBeLessThanOneUser);
        public const string PageSizeCannotBeLessThanOneUser = nameof(PageSizeCannotBeLessThanOneUser);
        public const string UserAlreadyExists = nameof(UserAlreadyExists);
        public const string AddUserFailed = nameof(AddUserFailed);
        public const string UserNotFoundForOperation = nameof(UserNotFoundForOperation);
        public const string UpdateUserFailed = nameof(UpdateUserFailed);
        public const string ResetUserPasswordFailed = nameof(ResetUserPasswordFailed);
        public const string DeleteUserFailed = nameof(DeleteUserFailed);
    }

    [ExcludeFromCodeCoverage]
    public static class MessageService
    {
        private static IStringLocalizer _localizer;

        public static void Initialize(IStringLocalizer localizer)
        {
            _localizer = localizer;
        }

        public static string GetMessage(string key, params object[] args)
        {
            if (_localizer == null)
            {
                // Fallback to resource files if localizer is not initialized
                return Messages.GetString(key) ?? key;
            }

            if (args != null && args.Length > 0)
            {
                return string.Format(_localizer[key].Value, args);
            }
            return _localizer[key].Value;
        }
    }
}
