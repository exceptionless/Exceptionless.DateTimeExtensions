﻿using System;
using Exceptionless.DateTimeExtensions.FormatParsers;
using Foundatio.Xunit;
using Microsoft.Extensions.Logging;
using Xunit;
using Xunit.Abstractions;

namespace Exceptionless.DateTimeExtensions.Tests.FormatParsers;

public abstract class FormatParserTestsBase : TestWithLoggingBase
{
    protected static readonly DateTime _now;

    protected FormatParserTestsBase(ITestOutputHelper output) : base(output) { }

    static FormatParserTestsBase()
    {
        _now = RandomHelper.GetRandomDate(new DateTime(2014, 11, 1), new DateTime(2014, 12, 1).SubtractMilliseconds(1));
    }

    public void ValidateInput(IFormatParser parser, string input, DateTime? start, DateTime? end)
    {
        _logger.LogInformation("Input: {Input}, Now: {Now}, Start: {Start}, End: {End}", input, _now, start, end);

        var range = parser.Parse(input, _now);
        _logger.LogInformation("Parsed range: Start: {Start}, End: {End}", range?.Start, range?.End);

        if (range == null)
        {
            Assert.Null(start);
            Assert.Null(end);
            return;
        }

        Assert.Equal(start, range.Start);
        Assert.Equal(end, range.End);
    }
}
