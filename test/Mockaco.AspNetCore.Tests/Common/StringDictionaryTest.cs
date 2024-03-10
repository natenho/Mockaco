using Xunit;
using Mockaco;
using FluentAssertions;

public class StringDictionaryTests
{
    [Fact]
    public void Add_WhenKeyNotExists_AddsKeyValue()
    {
        var dictionary = new StringDictionary();
        var key = "testKey";
        var value = "testValue";
        dictionary.Add(key, value);
        dictionary.ContainsKey(key).Should().BeTrue();
        dictionary[key].Should().Be(value);
    }

    [Fact]
    public void Add_WhenKeyExists_ReplacesValue()
    {
        var dictionary = new StringDictionary();
        var key = "testKey";
        var initialValue = "initialValue";
        var newValue = "newValue";
        dictionary.Add(key, initialValue);
        dictionary.Add(key, newValue);
        dictionary.ContainsKey(key).Should().BeTrue();
        dictionary[key].Should().Be(newValue);
    }

    [Fact]
    public void Replace_WhenKeyNotExists_AddsKeyValue()
    {
        var dictionary = new StringDictionary();
        var key = "testKey";
        var value = "testValue";
        dictionary.Replace(key, value);
        dictionary.ContainsKey(key).Should().BeTrue();
        dictionary[key].Should().Be(value);
    }

    [Fact]
    public void Replace_WhenKeyExists_ReplacesValue()
    {
        var dictionary = new StringDictionary();
        var key = "testKey";
        var initialValue = "initialValue";
        var newValue = "newValue";
        dictionary.Add(key, initialValue);
        dictionary.Replace(key, newValue);
        dictionary.ContainsKey(key).Should().BeTrue();
        dictionary[key].Should().Be(newValue);
    }

    [Fact]
    public void Indexer_Get_WhenKeyNotExists_ReturnsEmptyString()
    {
        var dictionary = new StringDictionary();
        var key = "testKey";
        var value = dictionary[key];
        value.Should().BeEmpty();
    }

    [Fact]
    public void Indexer_Set_UpdatesValue()
    {
        var dictionary = new StringDictionary();
        var key = "testKey";
        var value = "testValue";
        dictionary[key] = value;
        dictionary[key].Should().Be(value);
    }
}
