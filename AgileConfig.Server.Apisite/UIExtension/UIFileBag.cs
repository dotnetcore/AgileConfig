using System;
using System.Collections.Generic;
using NuGet.Common;

namespace AgileConfig.Server.Apisite.UIExtension;

internal class UIFileBag
{
    private static Dictionary<string, string> _contentTypesMap = new Dictionary<string, string>
    {
        {".html", "text/html; charset=utf-8"},
        {".css", "text/css; charset=utf-8"},
        {".js", "application/javascript"},
        {".png", "image/png"},
        {".svg", "image/svg+xml"},
        { ".json","application/json;charset=utf-8"},
        { ".ico","image/x-icon"}
    };
    public string FilePath { get; set; }
    public byte[] Data { get; set; }
    
    public string ExtType { get; set; }

    public string ContentType => _contentTypesMap[ExtType];

    public DateTime LastModified { get; set; }
}