using Game.Shared.Art.Json;

namespace Game.Shared.Tests.Art;

/// <summary>
/// <see cref="JsonValue"/> is the only JSON layer this project has (no System.Text.Json - see
/// ADR 0006), so it needs its own coverage of the document shapes genomes actually use.
/// </summary>
public class JsonValueTests
{
    [Fact]
    public void RoundTrips_AllPrimitiveKinds()
    {
        var root = JsonValue.Object()
            .Set("nullValue", JsonValue.Null)
            .Set("boolValue", JsonValue.Of(true))
            .Set("intValue", JsonValue.Of(42))
            .Set("doubleValue", JsonValue.Of(3.5))
            .Set("stringValue", JsonValue.Of("hello \"world\"\n"))
            .Set("arrayValue", JsonValue.Array().Add(JsonValue.Of(1)).Add(JsonValue.Of(2)).Add(JsonValue.Of(3)));

        var json = root.ToJsonString();
        var parsed = JsonValue.Parse(json);

        Assert.Equal(JsonKind.Null, parsed.Get("nullValue").Kind);
        Assert.True(parsed.Get("boolValue").AsBool());
        Assert.Equal(42, parsed.Get("intValue").AsInt());
        Assert.Equal(3.5, parsed.Get("doubleValue").AsDouble());
        Assert.Equal("hello \"world\"\n", parsed.Get("stringValue").AsString());
        Assert.Equal(3, parsed.Get("arrayValue").AsArray().Count);
        Assert.Equal(2, parsed.Get("arrayValue").AsArray()[1].AsInt());
    }

    [Fact]
    public void Parse_HandlesNestedObjectsAndArrays()
    {
        const string json = """{"outer":{"inner":[1,2,{"deep":true}]}}""";

        var parsed = JsonValue.Parse(json);

        var deep = parsed.Get("outer").Get("inner").AsArray()[2];
        Assert.True(deep.Get("deep").AsBool());
    }

    [Fact]
    public void Parse_HandlesUnicodeEscapes()
    {
        const string json = """{"value":"\u00e9"}""";

        var parsed = JsonValue.Parse(json);

        Assert.Equal("\u00e9", parsed.Get("value").AsString());
    }

    [Theory]
    [InlineData("{")]
    [InlineData("[1, 2")]
    [InlineData("{\"a\": }")]
    [InlineData("nul")]
    public void Parse_RejectsMalformedInput(string json)
    {
        Assert.ThrowsAny<FormatException>(() => JsonValue.Parse(json));
    }

    [Fact]
    public void Get_ThrowsForMissingKey()
    {
        var root = JsonValue.Object();

        Assert.Throws<KeyNotFoundException>(() => root.Get("missing"));
    }

    [Fact]
    public void TryGet_ReturnsFalseForMissingKey()
    {
        var root = JsonValue.Object();

        Assert.False(root.TryGet("missing", out _));
    }
}
