using System.Diagnostics.CodeAnalysis;

namespace AgileConfig.Server.Apisite.Models
{
    [ExcludeFromCodeCoverage]
    public class PublishLogVM: IAppIdModel
    {
        public string AppId { get; set; }
        public string Log { get; set; }

        public string[] Ids { get; set; }
    }
}
