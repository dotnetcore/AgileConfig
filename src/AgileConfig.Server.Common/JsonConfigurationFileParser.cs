using System;
using System.Buffers;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Configuration;

namespace AgileConfig.Server.Common;

/// <summary>
///     Result of parsing a jsonc document: the flattened values plus the comments attached to them.
/// </summary>
public class JsonParseResult
{
    public IDictionary<string, string> Data { get; init; }

    public IDictionary<string, string> Comments { get; init; }
}

/// <summary>
///     Adaptation of the JSON configuration parser implementation provided by Microsoft.
///     Supports jsonc: comments are kept and associated with the configuration item they describe.
/// </summary>
public class JsonConfigurationFileParser
{
    private readonly IDictionary<string, string> _comments =
        new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

    private readonly Stack<ContainerState> _containers = new();

    private readonly Stack<string> _context = new();

    private readonly IDictionary<string, string> _data =
        new SortedDictionary<string, string>(StringComparer.OrdinalIgnoreCase);

    private readonly List<string> _pendingComments = new();

    private string _currentPath;

    private JsonConfigurationFileParser()
    {
    }

    public static IDictionary<string, string> Parse(Stream input)
    {
        return new JsonConfigurationFileParser().ParseStream(input).Data;
    }

    /// <summary>
    ///     Parse a jsonc document and keep the comments that describe each leaf value.
    /// </summary>
    public static JsonParseResult ParseWithComments(Stream input)
    {
        return new JsonConfigurationFileParser().ParseStream(input);
    }

    private JsonParseResult ParseStream(Stream input)
    {
        var bytes = ReadAllBytes(input);

        var reader = new Utf8JsonReader(bytes, new JsonReaderOptions
        {
            CommentHandling = JsonCommentHandling.Allow,
            AllowTrailingCommas = true
        });

        // Key of the value that was just read, used to attach same line trailing comments.
        string lastValueKey = null;
        long previousTokenEnd = 0;
        var rootChecked = false;

        while (reader.Read())
        {
            if (reader.TokenType == JsonTokenType.Comment)
            {
                var text = reader.GetComment().Trim();
                if (text.Length > 0)
                {
                    if (lastValueKey != null && !HasLineBreak(bytes, previousTokenEnd, reader.TokenStartIndex))
                        AppendComment(lastValueKey, text);
                    else
                        _pendingComments.AddRange(SplitCommentLines(text));
                }

                previousTokenEnd = reader.BytesConsumed;
                continue;
            }

            if (!rootChecked)
            {
                if (reader.TokenType != JsonTokenType.StartObject)
                    throw new FormatException("Error_UnsupportedJSONToken");

                rootChecked = true;
            }

            switch (reader.TokenType)
            {
                case JsonTokenType.PropertyName:
                    EnterContext(reader.GetString());
                    break;

                case JsonTokenType.StartObject:
                case JsonTokenType.StartArray:
                    if (_containers.Count > 0 && _containers.Peek().IsArray)
                        EnterContext(_containers.Peek().NextIndex().ToString());

                    _containers.Push(new ContainerState(reader.TokenType == JsonTokenType.StartArray));
                    // A comment describing a whole section has no configuration item to live on.
                    _pendingComments.Clear();
                    break;

                case JsonTokenType.EndObject:
                case JsonTokenType.EndArray:
                    _containers.Pop();
                    if (_containers.Count > 0) ExitContext();

                    _pendingComments.Clear();
                    break;

                case JsonTokenType.String:
                case JsonTokenType.Number:
                case JsonTokenType.True:
                case JsonTokenType.False:
                case JsonTokenType.Null:
                    if (_containers.Peek().IsArray)
                        EnterContext(_containers.Peek().NextIndex().ToString());

                    lastValueKey = AddValue(ref reader);
                    ExitContext();
                    previousTokenEnd = reader.BytesConsumed;
                    continue;

                default:
                    throw new FormatException("Error_UnsupportedJSONToken");
            }

            lastValueKey = null;
            previousTokenEnd = reader.BytesConsumed;
        }

        return new JsonParseResult { Data = _data, Comments = _comments };
    }

    private string AddValue(ref Utf8JsonReader reader)
    {
        var key = _currentPath;
        if (_data.ContainsKey(key)) throw new FormatException("Error_KeyIsDuplicated");

        _data[key] = ReadScalar(ref reader);

        if (_pendingComments.Count > 0)
        {
            _comments[key] = string.Join("\n", _pendingComments);
            _pendingComments.Clear();
        }

        return key;
    }

    private void AppendComment(string key, string text)
    {
        var lines = SplitCommentLines(text);
        if (lines.Count == 0) return;

        var joined = string.Join("\n", lines);
        _comments[key] = _comments.TryGetValue(key, out var existing) && existing.Length > 0
            ? existing + "\n" + joined
            : joined;
    }

    private static List<string> SplitCommentLines(string text)
    {
        return text.Replace("\r\n", "\n").Split('\n')
            .Select(x => x.Trim())
            .Where(x => x.Length > 0)
            .ToList();
    }

    private static string ReadScalar(ref Utf8JsonReader reader)
    {
        switch (reader.TokenType)
        {
            case JsonTokenType.String:
                return reader.GetString();
            case JsonTokenType.True:
                return bool.TrueString;
            case JsonTokenType.False:
                return bool.FalseString;
            case JsonTokenType.Null:
                return string.Empty;
            default:
                return reader.HasValueSequence
                    ? Encoding.UTF8.GetString(reader.ValueSequence.ToArray())
                    : Encoding.UTF8.GetString(reader.ValueSpan);
        }
    }

    private static byte[] ReadAllBytes(Stream input)
    {
        using var ms = new MemoryStream();
        input.CopyTo(ms);
        var bytes = ms.ToArray();

        // Utf8JsonReader does not accept a UTF-8 BOM.
        if (bytes.Length >= 3 && bytes[0] == 0xEF && bytes[1] == 0xBB && bytes[2] == 0xBF)
            return bytes.AsSpan(3).ToArray();

        return bytes;
    }

    private static bool HasLineBreak(byte[] bytes, long start, long end)
    {
        for (var i = start; i < end && i < bytes.Length; i++)
            if (bytes[i] == (byte)'\n')
                return true;

        return false;
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

    private sealed class ContainerState
    {
        private int _index;

        public ContainerState(bool isArray)
        {
            IsArray = isArray;
        }

        public bool IsArray { get; }

        public int NextIndex()
        {
            return _index++;
        }
    }
}