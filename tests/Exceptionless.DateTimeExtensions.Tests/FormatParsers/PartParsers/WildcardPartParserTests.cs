using System;
using System.Collections.Generic;
using Exceptionless.DateTimeExtensions.FormatParsers.PartParsers;
using Microsoft.Extensions.Logging;
using Xunit;

namespace Exceptionless.DateTimeExtensions.Tests.FormatParsers.PartParsers;

public class WildcardPartParserTests : PartParserTestsBase
{
    public WildcardPartParserTests(ITestOutputHelper output) : base(output) { }

    [Theory]
    [MemberData(nameof(ParseInputs))]
    public void ParseInput(string input, bool isUpperLimit, DateTimeOffset? expected)
    {
        var parser = new WildcardPartParser();
        _logger.LogInformation("Testing input: '{Input}', IsUpperLimit: {IsUpperLimit}, Expected: {Expected}", input, isUpperLimit, expected);

        var match = parser.Regex.Match(input);
        _logger.LogInformation("Regex match success: {Success}, Value: '{Value}', Index: {Index}, Length: {Length}", match.Success, match.Value, match.Index, match.Length);

        var result = parser.Parse(match, _now, isUpperLimit);
        _logger.LogInformation("Parse result: {Result}", result);

        if (expected == null)
        {
            Assert.Null(result);
        }
        else
        {
            Assert.NotNull(result);
            Assert.Equal(expected.Value.DateTime, result.Value.DateTime);
        }
    }

    public static IEnumerable<object[]> ParseInputs
    {
        get
        {
            return
            [
                // Valid wildcard inputs
                ["*", false, DateTimeOffset.MinValue],
                ["*", true, DateTimeOffset.MaxValue],
                [" * ", false, DateTimeOffset.MinValue],
                [" * ", true, DateTimeOffset.MaxValue],
                ["  *  ", false, DateTimeOffset.MinValue],
                ["  *  ", true, DateTimeOffset.MaxValue],

                // Invalid inputs (patterns that should not match a complete wildcard)
                ["blah", false, null],
                ["blah", true, null],
                ["2012", false, null],
                ["2012", true, null],
                ["**", false, null],

                // This should match the first * in a two-part context like "* *"
                ["* *", false, DateTimeOffset.MinValue]
            ];
        }
    }

    [Fact]
    public void RegexPatternTest()
    {
        var parser = new WildcardPartParser();
        var regex = parser.Regex;

        _logger.LogInformation("Regex pattern: {Pattern}", regex);

        // Test various inputs
        string[] testInputs = ["*", " * ", "  *  ", "blah", "2012", "**", "* *", ""];

        foreach (string input in testInputs)
        {
            var match = regex.Match(input);
            _logger.LogInformation("Input: '{Input}' -> Success: {Success}, Value: '{Value}', Index: {Index}, Length: {Length}", input, match.Success, match.Value, match.Index, match.Length);
        }
    }

    [Fact]
    public void TestInTwoPartContext()
    {
        var parser = new WildcardPartParser();

        // Test how it behaves in a two-part parsing context
        string[] inputs = ["* TO 2013", "2012 TO *", "[* TO 2013]", "{2012 TO *}"];

        foreach (string input in inputs)
        {
            _logger.LogInformation("Testing two-part context for: '{Input}'", input);

            // Test parsing at the beginning
            var match = parser.Regex.Match(input, 0);
            _logger.LogInformation("  At position 0: Success: {Success}, Value: '{Value}', Index: {Index}, Length: {Length}", match.Success, match.Value, match.Index, match.Length);

            // Test parsing after bracket
            if (input.StartsWith("[") || input.StartsWith("{"))
            {
                match = parser.Regex.Match(input, 1);
                _logger.LogInformation("  At position 1: Success: {Success}, Value: '{Value}', Index: {Index}, Length: {Length}", match.Success, match.Value, match.Index, match.Length);
            }

            // Find TO and test parsing after it
            int toIndex = input.IndexOf(" TO ", StringComparison.OrdinalIgnoreCase);
            if (toIndex >= 0)
            {
                int afterTo = toIndex + 4;
                match = parser.Regex.Match(input, afterTo);
                _logger.LogInformation("  After TO at position {Position}: Success: {Success}, Value: '{Value}', Index: {Index}, Length: {Length}", afterTo, match.Success, match.Value, match.Index, match.Length);
            }
        }
    }
}
