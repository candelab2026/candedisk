using CandeDisk;
using Xunit;

namespace CandeDisk.Tests;

public class UpdateCheckerTests
{
    [Theory]
    [InlineData("v1.3.0", "1.2.0", true)]
    [InlineData("v1.2.1", "1.2.0", true)]
    [InlineData("v1.2.0", "1.2.0", false)]
    [InlineData("v1.1.0", "1.2.0", false)]
    [InlineData("v2.0.0", "1.9.9", true)]
    [InlineData("nightly", "1.2.0", false)]
    [InlineData("", "1.2.0", false)]
    public void Newer_version_is_recognised(string latest, string current, bool expected) =>
        Assert.Equal(expected, UpdateChecker.IsNewer(latest, current));
}
