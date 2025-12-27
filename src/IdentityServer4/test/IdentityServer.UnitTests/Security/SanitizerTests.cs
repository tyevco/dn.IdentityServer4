using IdentityServer4.Security;
using Xunit;

namespace IdentityServer.UnitTests.Security
{
    public class SanitizerTests
    {
        [Fact]
        public void LogSanitizer_RemovesNewlines()
        {
            // Arrange
            var input = "user\r\ntest\nvalue";

            // Act
            var result = Sanitizer.Log.Sanitize(input);

            // Assert
            Assert.DoesNotContain("\r", result);
            Assert.DoesNotContain("\n", result);
            Assert.Contains("user", result);
            Assert.Contains("test", result);
        }

        [Fact]
        public void LogSanitizer_HandlesLongStrings()
        {
            // Arrange
            var input = new string('a', 2000);

            // Act
            var result = Sanitizer.Log.Sanitize(input);

            // Assert
            // LogSanitizer doesn't truncate - it processes the full string
            Assert.NotNull(result);
            Assert.True(result.Length > 0);
        }

        [Fact]
        public void HtmlSanitizer_EncodesSpecialCharacters()
        {
            // Arrange
            var input = "<script>alert('xss')</script>";

            // Act
            var result = Sanitizer.Html.Sanitize(input, SanitizerMode.Clean);

            // Assert
            // HtmlSanitizer HTML-encodes special characters
            Assert.DoesNotContain("<script>", result);
            Assert.DoesNotContain("</script>", result);
            Assert.Contains("&lt;", result);
            Assert.Contains("&gt;", result);
        }

        [Fact]
        public void HtmlSanitizer_PreventsXSS()
        {
            // Arrange
            var input = "test<img src=x onerror=alert(1)>";

            // Act
            var result = Sanitizer.Html.Sanitize(input, SanitizerMode.Clean);

            // Assert
            // HtmlSanitizer prevents XSS by encoding the tags
            Assert.DoesNotContain("<img", result);
            Assert.Contains("&lt;img", result);
        }

        [Fact]
        public void UrlSanitizer_EncodesUrls()
        {
            // Arrange
            var url = "https://example.com/path";
            var maliciousUrl = "javascript:alert(1)";

            // Act
            var result1 = Sanitizer.Url.Sanitize(url, SanitizerMode.Clean);
            var result2 = Sanitizer.Url.Sanitize(maliciousUrl, SanitizerMode.Clean);

            // Assert
            // UrlSanitizer URL-encodes strings for safe embedding
            Assert.Contains("%3A%2F%2F", result1); // :// encoded
            Assert.DoesNotContain("javascript:", result2); // : encoded to %3A
        }

        [Fact]
        public void Sanitizer_MaskMode_PreservesLastCharacters()
        {
            // Arrange
            var input = "username123";

            // Act
            var result = Sanitizer.Log.Sanitize(input, SanitizerMode.Mask);

            // Assert
            Assert.Contains("*", result); // Should have masking
            Assert.EndsWith("e123", result); // Should preserve last 4 chars by default
            Assert.Equal(11, result.Length); // Same length as input
        }

        [Fact]
        public void LogSanitizer_WithNullInput_ReturnsNull()
        {
            // Arrange
            string input = null;

            // Act
            var result = Sanitizer.Log.Sanitize(input);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public void LogSanitizer_WithEmptyInput_ReturnsEmpty()
        {
            // Arrange
            var input = string.Empty;

            // Act
            var result = Sanitizer.Log.Sanitize(input);

            // Assert
            Assert.Equal(string.Empty, result);
        }

        [Theory]
        [InlineData("normal text")]
        [InlineData("text with spaces")]
        [InlineData("text-with-dashes")]
        [InlineData("text_with_underscores")]
        public void LogSanitizer_WithSafeInput_ReturnsUnchanged(string input)
        {
            // Act
            var result = Sanitizer.Log.Sanitize(input);

            // Assert
            Assert.Equal(input, result);
        }

        [Fact]
        public void HtmlSanitizer_WithNullInput_ReturnsNull()
        {
            // Arrange
            string input = null;

            // Act
            var result = Sanitizer.Html.Sanitize(input, SanitizerMode.Clean);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public void UrlSanitizer_WithNullInput_ReturnsNull()
        {
            // Arrange
            string input = null;

            // Act
            var result = Sanitizer.Url.Sanitize(input, SanitizerMode.Clean);

            // Assert
            Assert.Null(result);
        }
    }
}
