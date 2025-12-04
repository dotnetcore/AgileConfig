using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using Microsoft.Extensions.Configuration;

namespace AgileConfig.Server.Common;

/// <summary>
///     Adaptation of the JSON configuration parser implementation provided by Microsoft.
/// </summary>
public class JsonConfigurationFileParser
{
    private readonly Stack<string> _context = new();

    private readonly IDictionary<string, string> _data =
        new SortedDictionary<string, string>(StringComparer.OrdinalIgnoreCase);

    private string _currentPath;

    private JsonConfigurationFileParser()
    {
    }

    public static IDictionary<string, string> Parse(Stream input)
    {
        return new JsonConfigurationFileParser().ParseStream(input);
    }

    private IDictionary<string, string> ParseStream(Stream input)
    {
        _data.Clear();

        var jsonDocumentOptions = new JsonDocumentOptions
        {
            CommentHandling = JsonCommentHandling.Skip,
            AllowTrailingCommas = true
        };

        using (var reader = new StreamReader(input))
        using (var doc = JsonDocument.Parse(reader.ReadToEnd(), jsonDocumentOptions))
        {
            if (doc.RootElement.ValueKind != JsonValueKind.Object)
                throw new FormatException("Error_UnsupportedJSONToken");
            VisitElement(doc.RootElement);
        }

        return _data;
    }

    private void VisitElement(JsonElement element)
    {
        foreach (var property in element.EnumerateObject())
        {
            EnterContext(property.Name);
            VisitValue(property.Value);
            ExitContext();
        }
    }

    private void VisitValue(JsonElement value)
    {
        switch (value.ValueKind)
        {
            case JsonValueKind.Object:
                VisitElement(value);
                break;

            case JsonValueKind.Array:
                var index = 0;
                foreach (var arrayElement in value.EnumerateArray())
                {
                    EnterContext(index.ToString());
                    VisitValue(arrayElement);
                    ExitContext();
                    index++;
                }

                break;

            case JsonValueKind.Number:
            case JsonValueKind.String:
            case JsonValueKind.True:
            case JsonValueKind.False:
            case JsonValueKind.Null:
                var key = _currentPath;
                if (_data.ContainsKey(key)) throw new FormatException("Error_KeyIsDuplicated");
                _data[key] = value.ToString();
                break;

            default:
                throw new FormatException("Error_UnsupportedJSONToken");
        }
    }

    private void EnterContext(string context)
    {
        _context.Push(context);
        _currentPath = ConfigurationPath.Combine(_context.Reverse());
    }

    private void ExitContext()
    {
        _context.Pop();
        _currentPath = ConfigurationPath.Combine(_context.Reverse());
    }
}