using SpatialLite.Core.IO;

namespace SpatialLITE.UnitTests.Core.IO;

public class WktTokensBufferTests
{
    private readonly WktToken[] _testData = new WktToken[] {new WktToken() {Type = TokenType.STRING, Value = "point"}, new WktToken() {Type = TokenType.WHITESPACE, Value=" "},
        new WktToken() {Type = TokenType.LEFT_PARENTHESIS, Value = "("}};

    [Fact]
    public void Constructor__CreatesEmptyBuffer()
    {
        var target = new WktTokensBuffer();

        Assert.Empty(target);
    }

    [Fact]
    public void Constructor_TextReader_CreatesBufferWithSpecificTokens()
    {
        var target = new WktTokensBuffer(_testData);

        Assert.Equal(_testData.Length, target.Count());
        for (var i = 0; i < _testData.Length; i++)
        {
            Assert.Equal(_testData[i], target.ToArray()[i]);
        }
    }

    [Fact]
    public void Count_GetsNumberOfItemsInBuffer()
    {
        var target = new WktTokensBuffer();
        target.Add(_testData);

        Assert.Equal(_testData.Length, target.Count);
    }

    [Fact]
    public void Add_WktToken_AddsItemToTheCollection()
    {
        var target = new WktTokensBuffer();
        target.Add(_testData[0]);

        Assert.Single(target);
        Assert.Contains(_testData[0], target);
    }

    [Fact]
    public void Add_IEnumerable_AddsItemsToTheCollection()
    {
        var target = new WktTokensBuffer();
        target.Add(_testData);

        Assert.Equal(_testData.Length, target.Count());
        for (var i = 0; i < _testData.Length; i++)
        {
            Assert.Equal(_testData[i], target.ToArray()[i]);
        }
    }

    [Fact]
    public void Clear_RemovesAllItemsFromCollection()
    {
        var target = new WktTokensBuffer();
        target.Add(_testData);

        target.Clear();

        Assert.Empty(target);
    }

    [Fact]
    public void Peek_IgnoreWhitespace_GetsNextTokenFromBufferAndLeavesItThere()
    {
        var target = new WktTokensBuffer();
        target.Add(_testData[0]);

        var result = target.Peek(false);

        Assert.Equal(_testData[0], result);
        Assert.Contains(_testData[0], target);
    }

    [Fact]
    public void Peek_IgnoreWhitespace_IgnoresWhitespacesBeforeTokenIfIgnoreTokenIsTrue()
    {
        var target = new WktTokensBuffer();
        target.Add(new WktToken() { Type = TokenType.WHITESPACE, Value = " " });
        target.Add(new WktToken() { Type = TokenType.WHITESPACE, Value = " " });
        target.Add(_testData[0]);

        var result = target.Peek(true);

        Assert.Equal(_testData[0], result);
        Assert.Equal(3, target.Count);
    }

    [Fact]
    public void Peek_IgnoreWhitespace_ReturnsWhitespaceIfIgnoreWhitespaceIsFalseAndNextTokenIsWhitespace()
    {
        var whitespaceToken = new WktToken() { Type = TokenType.WHITESPACE, Value = " " };
        var target = new WktTokensBuffer();
        target.Add(whitespaceToken);
        target.Add(_testData[0]);

        var result = target.Peek(false);

        Assert.Equal(whitespaceToken, result);
        Assert.Equal(2, target.Count);
    }

    [Fact]
    public void Peek_IgnoreWhitespace_ReturnsEndOfDataTokenIfNoMoreTokensAreAvailable()
    {
        var target = new WktTokensBuffer();

        var result = target.Peek(false);

        Assert.Equal(WktToken.EndOfDataToken, result);
    }

    [Fact]
    public void Peek_IgnoreWhitespace_ReturnsEndOfDataTokenIfOnlyWhitespaceTokensAreAvailableAndIgnoreWhitespaceIsTrue()
    {
        var whitespaceToken = new WktToken() { Type = TokenType.WHITESPACE, Value = " " };
        var target = new WktTokensBuffer();
        target.Add(whitespaceToken);

        var result = target.Peek(true);

        Assert.Equal(WktToken.EndOfDataToken, result);
    }

    [Fact]
    public void GetToken_IgnoreWhitespace_GetsNextTokenFromBufferAndRemoveIt()
    {
        var target = new WktTokensBuffer();
        target.Add(_testData[0]);

        var result = target.GetToken(false);

        Assert.Equal(_testData[0], result);
        Assert.DoesNotContain(_testData[0], target);
    }

    [Fact]
    public void GetToken_IgnoreWhitespace_IgnoresWhitespacesBeforeTokenIfIgnoreTokenIsTrue()
    {
        var target = new WktTokensBuffer();
        target.Add(new WktToken() { Type = TokenType.WHITESPACE, Value = " " });
        target.Add(new WktToken() { Type = TokenType.WHITESPACE, Value = " " });
        target.Add(_testData[0]);

        var result = target.GetToken(true);

        Assert.Equal(_testData[0], result);
        Assert.Equal(0, target.Count);
    }

    [Fact]
    public void GetToken_IgnoreWhitespace_ReturnsWhitespaceIfIgnoreWhitespaceIsFalseAndNextTokenIsWhitespace()
    {
        var whitespaceToken = new WktToken() { Type = TokenType.WHITESPACE, Value = " " };
        var target = new WktTokensBuffer();
        target.Add(whitespaceToken);
        target.Add(_testData[0]);

        var result = target.GetToken(false);

        Assert.Equal(whitespaceToken, result);
        Assert.Equal(1, target.Count);
    }

    [Fact]
    public void GetToken_IgnoreWhitespace_ReturnsEndOfDataTokenIfNoMoreTokensAreAvailable()
    {
        var target = new WktTokensBuffer();

        var result = target.GetToken(false);

        Assert.Equal(WktToken.EndOfDataToken, result);
    }

    [Fact]
    public void GetToken_IgnoreWhitespace_ReturnsEndOfDataTokenIfOnlyWhitespaceTokensAreAvailalbleAndIgnoreWhitespaceIsTrue()
    {
        var whitespaceToken = new WktToken() { Type = TokenType.WHITESPACE, Value = " " };
        var target = new WktTokensBuffer();
        target.Add(whitespaceToken);

        var result = target.GetToken(true);

        Assert.Equal(WktToken.EndOfDataToken, result);
        Assert.Empty(target);
    }
}
