using mau.Utils;
using Xunit;

namespace Mauverse.Mobile.Tests;

public sealed class UserGreetingTests
{
    [Fact]
    public void UsesFirstNameInsteadOfLogin()
    {
        Assert.Equal("Иван", UserGreeting.ResolveFirstName("Иван", "Иванов Иван Иванович", "student01"));
    }

    [Fact]
    public void IgnoresLoginAccidentallyReturnedAsFirstName()
    {
        Assert.Equal("Иван", UserGreeting.ResolveFirstName("student01", "Иванов Иван Иванович", "student01"));
    }

    [Fact]
    public void UsesNeutralFallbackWhenOnlyLoginIsAvailable()
    {
        Assert.Equal("Студент", UserGreeting.ResolveFirstName("student01", "", "student01"));
    }

    [Fact]
    public void UsesFirstPartForMoodleTwoPartDisplayName()
    {
        Assert.Equal("Иван", UserGreeting.ResolveFirstName("", "Иван Иванов", "student01"));
    }
}
