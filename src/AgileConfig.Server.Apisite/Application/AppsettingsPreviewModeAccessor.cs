using AgileConfig.Server.Application;
using AgileConfig.Server.Common;

namespace AgileConfig.Server.Apisite.Application;

public sealed class AppsettingsPreviewModeAccessor : IPreviewModeAccessor
{
    public bool IsPreviewMode => Appsettings.IsPreviewMode;
}
