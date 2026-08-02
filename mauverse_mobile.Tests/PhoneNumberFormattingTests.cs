using mau.Utils;
using Xunit;

namespace Mauverse.Mobile.Tests;

public sealed class PhoneNumberFormattingTests
{
    [Theory]
    [InlineData("21-38-81 (3045)", "+78152213881;ext=3045")]
    [InlineData("40-33-39", "+78152403339")]
    [InlineData("8 8152 21-38-72", "+78152213872")]
    [InlineData("+7 (8152) 21-38-01", "+78152213801")]
    [InlineData("8 800 350-12-21", "+78003501221")]
    [InlineData("21-38-81 доб. 3045", "+78152213881;ext=3045")]
    public void FormatsMurmanskAndRussianNumbers(string raw, string expected)
    {
        Assert.Equal(expected, PhoneNumberFormatting.ToDialString(raw));
    }

    [Fact]
    public void RejectsEmptyInput()
    {
        Assert.Null(PhoneNumberFormatting.ToDialString(" "));
        Assert.Null(PhoneNumberFormatting.ToDialString(null));
    }
}
