using System.Collections.Generic;
using AgileConfig.Server.Apisite.Models;
using AgileConfig.Server.Data.Entity;

namespace AgileConfig.Server.Apisite.Controllers.api.Models
{
    public class ApiConfigVM : IAppIdModel
    {
        /// <summary>
        /// id
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// Application ID.
        /// </summary>
        public string AppId { get; set; }

        /// <summary>
        /// Configuration group name.
        /// </summary>
        public string Group { get; set; }
        /// <summary>
        /// Configuration key.
        /// </summary>
        public string Key { get; set; }
        /// <summary>
        /// Configuration value.
        /// </summary>
        public string Value { get; set; }

        /// <summary>
        /// Configuration status.
        /// </summary>
        public ConfigStatus Status { get; set; }

        /// <summary>
        /// Online status for the configuration.
        /// </summary>
        public OnlineStatus OnlineStatus { get; set; }

        /// <summary>
        /// Editing status for the configuration.
        /// </summary>
        public EditStatus EditStatus { get; set; }

        /// <summary>
        /// Description of the configuration.
        /// </summary>
        public string Description { get; set; }
    }


    public class AppConfigsCache
    {
        public string Key { get; set; }

        public string VirtualId { get; set; }

        public List<ApiConfigVM> Configs { get; set; }
    }

    public static class ApiConfigVMExtension
    {
        public static ConfigVM ToConfigVM(this ApiConfigVM model)
        {
            if (model is null)
            {
                return null;
            }

            return new ConfigVM()
            {
                Id = model.Id,
                AppId = model.AppId,
                Group = model.Group,
                Key = model.Key,
                Value = model.Value,
                Description = model.Description
            };
        }
    }
}
