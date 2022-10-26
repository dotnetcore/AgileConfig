using System;
using System.Collections.Generic;
using NuGet.Common;

namespace AgileConfig.Server.Apisite.UIExtension;

internal class UIFileBag
{
    private static Dictionary<string, string> _contentTypesMap = new()
    {
        {".html", "text/html; charset=utf-8"},
        {".css", "text/css; charset=utf-8"},
        {".js", "application/javascript"},
        {".png", "image/png"},
        {".svg", "image/svg+xml"},
        {".json","application/json;charset=utf-8"},
        {".ico","image/x-icon"},
        {".ttf","application/octet-stream"},
        {".otf","font/otf"},
        {".woff","font/x-woff"},
        {".woff2","application/octet-stream"},
    };
    public string FilePath { get; init; }
    public byte[] Data { get; init; }
    
    public string ExtType { get; init; }

    public string ContentType => _contentTypesMap[ExtType];

    public DateTime LastModified { get; init; }
}