namespace Mockaco.AspNetCore.Tests.Extensions;

public class StringExtensionsTests
{
    [Fact]
    public void Returns_False_For_Null_String()
    {
        string? stringValue = null;
        Assert.False(stringValue.IsRemoteAbsolutePath());
    }

    [Fact]
    public void Returns_False_For_Empty_String()
    {
        string? stringValue = "";
        Assert.False(stringValue.IsRemoteAbsolutePath());
    }

    [Fact]
    public void Returns_False_For_Relative_Path()
    {
        string? stringValue = ".";
        Assert.False(stringValue.IsRemoteAbsolutePath());
    }

    [Fact]
    public void Returns_True_For_Absolute_Path()
    {
        string? stringValue = "http://www.github.com";
        Assert.True(stringValue.IsRemoteAbsolutePath());
    }
}