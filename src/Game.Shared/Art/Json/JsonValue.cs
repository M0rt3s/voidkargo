// Explicit usings and a block-scoped namespace are used here (instead of relying on
// ImplicitUsings/file-scoped namespaces) because Unity compiles this file directly as part of
// the com.voidkargo.shared local package, ignoring this project's .csproj SDK settings, and
// Unity's compiler is pinned to C# 9.0 (file-scoped namespaces need C# 10+).
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace Game.Shared.Art.Json
{
    /// <summary>Discriminates the kind of value a <see cref="JsonValue"/> holds.</summary>
    public enum JsonKind
    {
        Null,
        Bool,
        Number,
        String,
        Array,
        Object,
    }

    /// <summary>
    /// A minimal, dependency-free JSON document model, parser, and writer. Used instead of
    /// <c>System.Text.Json</c> because that package isn't part of the netstandard2.1 base class
    /// library and Unity compiles <c>Game.Shared</c> directly from source rather than via its
    /// .csproj/NuGet references (see client/Game.Client/README.md) - pulling it in would mean
    /// also installing it as a Unity package. This model only needs to support the ship genome
    /// schema (see ADR 0006): objects, arrays, strings, numbers, and booleans.
    /// </summary>
    public sealed class JsonValue
    {
        private readonly bool _boolValue;
        private readonly double _numberValue;
        private readonly string? _stringValue;
        private readonly List<JsonValue>? _arrayValue;
        private readonly Dictionary<string, JsonValue>? _objectValue;

        public JsonKind Kind { get; }

        private JsonValue(JsonKind kind, bool boolValue = false, double numberValue = 0, string? stringValue = null,
            List<JsonValue>? arrayValue = null, Dictionary<string, JsonValue>? objectValue = null)
        {
            Kind = kind;
            _boolValue = boolValue;
            _numberValue = numberValue;
            _stringValue = stringValue;
            _arrayValue = arrayValue;
            _objectValue = objectValue;
        }

        public static readonly JsonValue Null = new JsonValue(JsonKind.Null);

        public static JsonValue Of(bool value) => new JsonValue(JsonKind.Bool, boolValue: value);
        public static JsonValue Of(double value) => new JsonValue(JsonKind.Number, numberValue: value);
        public static JsonValue Of(int value) => new JsonValue(JsonKind.Number, numberValue: value);
        public static JsonValue Of(ulong value) => new JsonValue(JsonKind.Number, numberValue: value);
        public static JsonValue Of(string value) => new JsonValue(JsonKind.String, stringValue: value);
        public static JsonValue Array() => new JsonValue(JsonKind.Array, arrayValue: new List<JsonValue>());
        public static JsonValue Object() => new JsonValue(JsonKind.Object, objectValue: new Dictionary<string, JsonValue>());

        public JsonValue Add(JsonValue item)
        {
            if (Kind != JsonKind.Array || _arrayValue is null)
            {
                throw new InvalidOperationException("Add is only valid on an array JsonValue.");
            }

            _arrayValue.Add(item);
            return this;
        }

        public JsonValue Set(string key, JsonValue value)
        {
            if (Kind != JsonKind.Object || _objectValue is null)
            {
                throw new InvalidOperationException("Set is only valid on an object JsonValue.");
            }

            _objectValue[key] = value;
            return this;
        }

        public bool AsBool() => Kind == JsonKind.Bool
            ? _boolValue
            : throw new InvalidOperationException($"Expected bool, was {Kind}.");

        public double AsDouble() => Kind == JsonKind.Number
            ? _numberValue
            : throw new InvalidOperationException($"Expected number, was {Kind}.");

        public int AsInt() => (int)AsDouble();

        public ulong AsUInt64() => (ulong)AsDouble();

        public string AsString() => Kind == JsonKind.String && _stringValue is not null
            ? _stringValue
            : throw new InvalidOperationException($"Expected string, was {Kind}.");

        public IReadOnlyList<JsonValue> AsArray() => Kind == JsonKind.Array && _arrayValue is not null
            ? _arrayValue
            : throw new InvalidOperationException($"Expected array, was {Kind}.");

        public IReadOnlyDictionary<string, JsonValue> AsObject() => Kind == JsonKind.Object && _objectValue is not null
            ? _objectValue
            : throw new InvalidOperationException($"Expected object, was {Kind}.");

        public bool TryGet(string key, out JsonValue value)
        {
            if (Kind == JsonKind.Object && _objectValue is not null && _objectValue.TryGetValue(key, out var found))
            {
                value = found;
                return true;
            }

            value = Null;
            return false;
        }

        public JsonValue Get(string key)
        {
            if (TryGet(key, out var value))
            {
                return value;
            }

            throw new KeyNotFoundException($"JSON object has no property '{key}'.");
        }

        public JsonValue? GetOrNull(string key) => TryGet(key, out var value) ? value : null;

        /// <summary>Parses a JSON document from text. Throws <see cref="FormatException"/> on invalid input.</summary>
        public static JsonValue Parse(string json)
        {
            var index = 0;
            var result = ParseValue(json, ref index);
            SkipWhitespace(json, ref index);
            if (index != json.Length)
            {
                throw new FormatException($"Unexpected trailing content at position {index}.");
            }

            return result;
        }

        private static JsonValue ParseValue(string json, ref int index)
        {
            SkipWhitespace(json, ref index);
            if (index >= json.Length)
            {
                throw new FormatException("Unexpected end of JSON input.");
            }

            var c = json[index];
            switch (c)
            {
                case '{': return ParseObject(json, ref index);
                case '[': return ParseArray(json, ref index);
                case '"': return Of(ParseString(json, ref index));
                case 't':
                    Expect(json, ref index, "true");
                    return Of(true);
                case 'f':
                    Expect(json, ref index, "false");
                    return Of(false);
                case 'n':
                    Expect(json, ref index, "null");
                    return Null;
                default:
                    return ParseNumber(json, ref index);
            }
        }

        private static JsonValue ParseObject(string json, ref int index)
        {
            var result = Object();
            index++; // consume '{'
            SkipWhitespace(json, ref index);
            if (Peek(json, index) == '}')
            {
                index++;
                return result;
            }

            while (true)
            {
                SkipWhitespace(json, ref index);
                var key = ParseString(json, ref index);
                SkipWhitespace(json, ref index);
                if (Peek(json, index) != ':')
                {
                    throw new FormatException($"Expected ':' at position {index}.");
                }

                index++; // consume ':'
                var value = ParseValue(json, ref index);
                result.Set(key, value);

                SkipWhitespace(json, ref index);
                var next = Peek(json, index);
                if (next == ',')
                {
                    index++;
                    continue;
                }

                if (next == '}')
                {
                    index++;
                    break;
                }

                throw new FormatException($"Expected ',' or '}}' at position {index}.");
            }

            return result;
        }

        private static JsonValue ParseArray(string json, ref int index)
        {
            var result = Array();
            index++; // consume '['
            SkipWhitespace(json, ref index);
            if (Peek(json, index) == ']')
            {
                index++;
                return result;
            }

            while (true)
            {
                var value = ParseValue(json, ref index);
                result.Add(value);

                SkipWhitespace(json, ref index);
                var next = Peek(json, index);
                if (next == ',')
                {
                    index++;
                    continue;
                }

                if (next == ']')
                {
                    index++;
                    break;
                }

                throw new FormatException($"Expected ',' or ']' at position {index}.");
            }

            return result;
        }

        private static string ParseString(string json, ref int index)
        {
            if (Peek(json, index) != '"')
            {
                throw new FormatException($"Expected string at position {index}.");
            }

            index++; // consume opening quote
            var sb = new StringBuilder();
            while (true)
            {
                if (index >= json.Length)
                {
                    throw new FormatException("Unterminated string literal.");
                }

                var c = json[index++];
                if (c == '"')
                {
                    break;
                }

                if (c == '\\')
                {
                    if (index >= json.Length)
                    {
                        throw new FormatException("Unterminated escape sequence.");
                    }

                    var escape = json[index++];
                    switch (escape)
                    {
                        case '"': sb.Append('"'); break;
                        case '\\': sb.Append('\\'); break;
                        case '/': sb.Append('/'); break;
                        case 'b': sb.Append('\b'); break;
                        case 'f': sb.Append('\f'); break;
                        case 'n': sb.Append('\n'); break;
                        case 'r': sb.Append('\r'); break;
                        case 't': sb.Append('\t'); break;
                        case 'u':
                            if (index + 4 > json.Length)
                            {
                                throw new FormatException("Truncated unicode escape.");
                            }

                            var hex = json.Substring(index, 4);
                            sb.Append((char)ushort.Parse(hex, NumberStyles.HexNumber, CultureInfo.InvariantCulture));
                            index += 4;
                            break;
                        default:
                            throw new FormatException($"Unknown escape sequence '\\{escape}'.");
                    }
                }
                else
                {
                    sb.Append(c);
                }
            }

            return sb.ToString();
        }

        private static JsonValue ParseNumber(string json, ref int index)
        {
            var start = index;
            if (Peek(json, index) == '-')
            {
                index++;
            }

            while (index < json.Length && (char.IsDigit(json[index]) || json[index] is '.' or 'e' or 'E' or '+' or '-'))
            {
                index++;
            }

            var slice = json.Substring(start, index - start);
            if (slice.Length == 0)
            {
                throw new FormatException($"Expected number at position {start}.");
            }

            return Of(double.Parse(slice, CultureInfo.InvariantCulture));
        }

        private static void Expect(string json, ref int index, string literal)
        {
            if (index + literal.Length > json.Length || json.Substring(index, literal.Length) != literal)
            {
                throw new FormatException($"Expected literal '{literal}' at position {index}.");
            }

            index += literal.Length;
        }

        private static char Peek(string json, int index) => index < json.Length ? json[index] : '\0';

        private static void SkipWhitespace(string json, ref int index)
        {
            while (index < json.Length && char.IsWhiteSpace(json[index]))
            {
                index++;
            }
        }

        /// <summary>Serializes this value to a compact JSON string. Object keys preserve insertion order.</summary>
        public string ToJsonString()
        {
            var sb = new StringBuilder();
            Write(sb);
            return sb.ToString();
        }

        private void Write(StringBuilder sb)
        {
            switch (Kind)
            {
                case JsonKind.Null:
                    sb.Append("null");
                    break;
                case JsonKind.Bool:
                    sb.Append(_boolValue ? "true" : "false");
                    break;
                case JsonKind.Number:
                    sb.Append(_numberValue.ToString("R", CultureInfo.InvariantCulture));
                    break;
                case JsonKind.String:
                    WriteString(sb, _stringValue ?? string.Empty);
                    break;
                case JsonKind.Array:
                    sb.Append('[');
                    for (var i = 0; i < _arrayValue!.Count; i++)
                    {
                        if (i > 0)
                        {
                            sb.Append(',');
                        }

                        _arrayValue[i].Write(sb);
                    }

                    sb.Append(']');
                    break;
                case JsonKind.Object:
                    sb.Append('{');
                    var first = true;
                    foreach (var kvp in _objectValue!)
                    {
                        if (!first)
                        {
                            sb.Append(',');
                        }

                        first = false;
                        WriteString(sb, kvp.Key);
                        sb.Append(':');
                        kvp.Value.Write(sb);
                    }

                    sb.Append('}');
                    break;
            }
        }

        private static void WriteString(StringBuilder sb, string value)
        {
            sb.Append('"');
            foreach (var c in value)
            {
                switch (c)
                {
                    case '"': sb.Append("\\\""); break;
                    case '\\': sb.Append("\\\\"); break;
                    case '\n': sb.Append("\\n"); break;
                    case '\r': sb.Append("\\r"); break;
                    case '\t': sb.Append("\\t"); break;
                    default:
                        if (c < 0x20)
                        {
                            sb.Append("\\u").Append(((int)c).ToString("x4", CultureInfo.InvariantCulture));
                        }
                        else
                        {
                            sb.Append(c);
                        }

                        break;
                }
            }

            sb.Append('"');
        }
    }
}
