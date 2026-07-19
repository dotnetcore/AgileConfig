using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json;

namespace AgileConfig.Server.Common;

/// <summary>
///     Parse JSONC (JSON with Comments) format.
///     Extracts both values and inline comments, returning them as two separate dictionaries
///     keyed by the flattened configuration path (e.g., "group:key").
///     Uses Newtonsoft.Json's JsonTextReader to walk tokens and capture comment tokens.
/// </summary>
public class JsonCConfigurationFileParser
{
    private readonly Stack<string> _path = new();

    private JsonCConfigurationFileParser()
    {
    }

    /// <summary>
    ///     Parse a JSONC string and return the flattened key-value pairs along with any inline comments.
    /// </summary>
    /// <param name="json">JSONC string to parse. Supports // and /**/ comments.</param>
    /// <returns>
    ///     A tuple containing:
    ///     - values: flattened key-value dictionary
    ///     - descriptions: flattened key-description dictionary (extracted from comments)
    /// </returns>
    public static (Dictionary<string, string> values, Dictionary<string, string> descriptions) Parse(string json)
    {
        return new JsonCConfigurationFileParser().ParseString(json);
    }

    /// <summary>
    ///     Parse a JSONC stream and return the flattened key-value pairs along with any inline comments.
    /// </summary>
    /// <param name="input">Stream containing JSONC data.</param>
    /// <returns>
    ///     A tuple containing:
    ///     - values: flattened key-value dictionary
    ///     - descriptions: flattened key-description dictionary (extracted from comments)
    /// </returns>
    public static (Dictionary<string, string> values, Dictionary<string, string> descriptions) Parse(Stream input)
    {
        using var reader = new StreamReader(input);
        var json = reader.ReadToEnd();
        return Parse(json);
    }

    private (Dictionary<string, string> values, Dictionary<string, string> descriptions) ParseString(string json)
    {
        var values = new Dictionary<string, string>();
        var descriptions = new Dictionary<string, string>();
        string lastValueKey = null;

        using var reader = new JsonTextReader(new StringReader(json))
        {
            DateParseHandling = DateParseHandling.None
        };

        while (reader.Read())
        {
            switch (reader.TokenType)
            {
                case JsonToken.Comment:
                {
                    // Comments in JSONC typically appear after a value on the same line,
                    // so associate the comment with the last value we encountered.
                    var comment = ((string)reader.Value)?.Trim();
                    if (!string.IsNullOrEmpty(comment) && lastValueKey != null)
                    {
                        // Only set if not already set (first comment wins for a given key)
                        if (!descriptions.ContainsKey(lastValueKey))
                            descriptions[lastValueKey] = comment;
                    }

                    break;
                }
                case JsonToken.PropertyName:
                {
                    _path.Push((string)reader.Value);
                    break;
                }
                case JsonToken.String:
                case JsonToken.Integer:
                case JsonToken.Float:
                case JsonToken.Boolean:
                case JsonToken.Null:
                {
                    var key = string.Join(":", _path.Reverse());
                    values[key] = reader.Value?.ToString() ?? "";
                    lastValueKey = key;
                    break;
                }
                case JsonToken.EndObject:
                {
                    if (_path.Count > 0) _path.Pop();
                    break;
                }
            }
        }

        return (values, descriptions);
    }
}
