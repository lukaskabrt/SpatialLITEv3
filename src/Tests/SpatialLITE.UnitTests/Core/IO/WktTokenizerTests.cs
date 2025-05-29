using SpatialLite.Core.IO;

namespace SpatialLITE.UnitTests.Core.IO;

public class WktTokenizerTests
{
    [Fact]
    public void Tokenize_String_ReturnsEmptyTokenForEmptyString()
    {
        var data = string.Empty;
        var tokens = WktTokenizer.Tokenize(data);

        Assert.Empty(tokens);
    }

    [Theory]
    [InlineData("stringTOOKEN", TokenType.STRING)]
    [InlineData(" ", TokenType.WHITESPACE)]
    [InlineData("\t", TokenType.WHITESPACE)]
    [InlineData("\n", TokenType.WHITESPACE)]
    [InlineData("\r", TokenType.WHITESPACE)]
    [InlineData("(", TokenType.LEFT_PARENTHESIS)]
    [InlineData(")", TokenType.RIGHT_PARENTHESIS)]
    [InlineData(",", TokenType.COMMA)]
    [InlineData("-123456780.9", TokenType.NUMBER)]
    internal void Tokenize_String_CorrectlyRecognizesTokenTypes(string str, TokenType expectedType)
    {
        var tokens = WktTokenizer.Tokenize(str).ToArray();

        Assert.Single(tokens);

        var t = tokens.First();
        Assert.Equal(expectedType, t.Type);
        Assert.Equal(str, t.Value);
    }

    [Fact]
    public void Tokenize_String_ProcessesComplexText()
    {
        var data = "point z (-10 -15 -100.1)";
        var tokens = WktTokenizer.Tokenize(data).ToArray();

        Assert.Equal(11, tokens.Length);

        var t = tokens[0];
        Assert.Equal(TokenType.STRING, t.Type);
        Assert.Equal("point", t.Value);

        t = tokens[1];
        Assert.Equal(TokenType.WHITESPACE, t.Type);

        t = tokens[2];
        Assert.Equal(TokenType.STRING, t.Type);
        Assert.Equal("z", t.Value);

        t = tokens[3];
        Assert.Equal(TokenType.WHITESPACE, t.Type);

        t = tokens[4];
        Assert.Equal(TokenType.LEFT_PARENTHESIS, t.Type);

        t = tokens[5];
        Assert.Equal(TokenType.NUMBER, t.Type);
        Assert.Equal("-10", t.Value);

        t = tokens[6];
        Assert.Equal(TokenType.WHITESPACE, t.Type);

        t = tokens[7];
        Assert.Equal(TokenType.NUMBER, t.Type);
        Assert.Equal("-15", t.Value);

        t = tokens[8];
        Assert.Equal(TokenType.WHITESPACE, t.Type);

        t = tokens[9];
        Assert.Equal(TokenType.NUMBER, t.Type);
        Assert.Equal("-100.1", t.Value);

        t = tokens[10];
        Assert.Equal(TokenType.RIGHT_PARENTHESIS, t.Type);
    }

    [Fact]
    public void Tokenize_TextReader_ProcessesComplexText()
    {
        var reader = new StringReader("point z (-10 -15 -100.1)");

        var tokens = WktTokenizer.Tokenize(reader).ToArray();

        Assert.Equal(11, tokens.Length);

        var t = tokens[0];
        Assert.Equal(TokenType.STRING, t.Type);
        Assert.Equal("point", t.Value);

        t = tokens[1];
        Assert.Equal(TokenType.WHITESPACE, t.Type);

        t = tokens[2];
        Assert.Equal(TokenType.STRING, t.Type);
        Assert.Equal("z", t.Value);

        t = tokens[3];
        Assert.Equal(TokenType.WHITESPACE, t.Type);

        t = tokens[4];
        Assert.Equal(TokenType.LEFT_PARENTHESIS, t.Type);

        t = tokens[5];
        Assert.Equal(TokenType.NUMBER, t.Type);
        Assert.Equal("-10", t.Value);

        t = tokens[6];
        Assert.Equal(TokenType.WHITESPACE, t.Type);

        t = tokens[7];
        Assert.Equal(TokenType.NUMBER, t.Type);
        Assert.Equal("-15", t.Value);

        t = tokens[8];
        Assert.Equal(TokenType.WHITESPACE, t.Type);

        t = tokens[9];
        Assert.Equal(TokenType.NUMBER, t.Type);
        Assert.Equal("-100.1", t.Value);

        t = tokens[10];
        Assert.Equal(TokenType.RIGHT_PARENTHESIS, t.Type);
    }
}
