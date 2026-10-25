using System.Text.RegularExpressions;
using CandeDisk;
using Xunit;

namespace CandeDisk.Tests;

/// <summary>Controlla che ogni lingua sia completa e coerente con l'inglese: nessuna chiave mancante, segnaposto uguali.</summary>
public class LocTests
{
    private static readonly Regex Placeholder = new(@"\{\d+\}", RegexOptions.Compiled);

    public static IEnumerable<object[]> Codes() => Loc.Languages.Select(l => new object[] { l.Code });

    private static string[] Keys(string code) => Loc.Table(code).Keys.Where(k => !k.StartsWith('@')).ToArray();

    [Fact]
    public void English_is_the_base_language_and_there_are_several_languages()
    {
        Assert.Contains(Loc.Languages, l => l.Code == Loc.Fallback);
        Assert.True(Loc.Languages.Count >= 5);
    }

    [Theory]
    [MemberData(nameof(Codes))]
    public void Every_language_has_exactly_the_keys_of_the_base_language(string code)
    {
        var baseKeys = Keys(Loc.Fallback).ToHashSet();
        var keys = Keys(code).ToHashSet();

        Assert.Empty(baseKeys.Except(keys));
        Assert.Empty(keys.Except(baseKeys));
    }

    [Theory]
    [MemberData(nameof(Codes))]
    public void Placeholders_match_the_base_language(string code)
    {
        foreach (string key in Keys(Loc.Fallback))
        {
            var expected = Placeholder.Matches(Loc.Table(Loc.Fallback)[key]).Select(m => m.Value).OrderBy(v => v);
            var actual = Placeholder.Matches(Loc.Table(code)[key]).Select(m => m.Value).OrderBy(v => v);

            Assert.True(expected.SequenceEqual(actual), $"{code}: segnaposto diversi in '{key}'");
        }
    }

    [Theory]
    [MemberData(nameof(Codes))]
    public void No_value_is_empty_and_each_language_names_itself(string code)
    {
        Assert.All(Keys(code), key => Assert.False(string.IsNullOrWhiteSpace(Loc.Table(code)[key]), $"{code}: '{key}' è vuoto"));
        Assert.False(string.IsNullOrWhiteSpace(Loc.Table(code)["@Name"]));
        Assert.False(string.IsNullOrWhiteSpace(Loc.Table(code)["@Culture"]));
    }

    [Theory]
    [MemberData(nameof(Codes))]
    public void Every_language_has_a_guide(string code) => Assert.True(Loc.HasGuide(code), $"manca docs/guide/{code}.md");

    [Theory]
    [MemberData(nameof(Codes))]
    public void Menu_labels_survive_the_removal_of_access_keys(string code)
    {
        foreach (string key in new[] { "MenuCreate", "MenuFormat", "MenuResize", "MenuDelete" })
        {
            string plain = Loc.Plain(Loc.Table(code)[key]);
            Assert.DoesNotContain("_", plain);
            Assert.False(string.IsNullOrWhiteSpace(plain));
        }
    }

    [Fact]
    public void Parse_reads_keys_comments_and_newlines()
    {
        var table = Loc.Parse("# comment\r\nA = one\r\n\r\nB = two\\nlines\r\nC = x = y\r\n");

        Assert.Equal("one", table["A"]);
        Assert.Equal("two\nlines", table["B"]);
        Assert.Equal("x = y", table["C"]);
        Assert.False(table.ContainsKey("# comment"));
    }
}
