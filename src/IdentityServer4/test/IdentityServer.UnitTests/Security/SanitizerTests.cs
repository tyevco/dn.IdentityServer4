using IdentityServer4.Security;
using Xunit;

namespace IdentityServer.UnitTests.Security
{
    public class SanitizerTests
    {
        [Fact]
        public void LogSanitizer_RemovesControlCharacters()
        {
            // Arrange
            var input = "user\u0000name\u0001test\u001F";

            // Act
            var result = Sanitizer.Log.Sanitize(input);

            // Assert
            Assert.DoesNotContain("\u0000", result);
            Assert.DoesNotContain("\u0001", result);
            Assert.DoesNotContain("\u001F", result);
        }

        [Fact]
        public void LogSanitizer_TruncatesLongStrings()
        {
            // Arrange
            var input = new string('a', 2000);

            // Act
            var result = Sanitizer.Log.Sanitize(input);

            // Assert
            Assert.True(result.Length <= 1000); // Default max length
        }

        [Fact]
        public void HtmlSanitizer_EncodesSpecialCharacters()
        {
            // Arrange
            var input = "<script>alert('xss')</script>";

            // Act
            var result = Sanitizer.Html.Sanitize(input, SanitizerMode.Clean);

            // Assert
            Assert.DoesNotContain("<script>", result);
            Assert.DoesNotContain("</script>", result);
        }

        [Fact]
        public void HtmlSanitizer_PreventsXSS()
        {
            // Arrange
            var input = "test<img src=x onerror=alert(1)>";

            // Act
            var result = Sanitizer.Html.Sanitize(input, SanitizerMode.Clean);

            // Assert
            Assert.DoesNotContain("<img", result);
            Assert.DoesNotContain("onerror", result);
        }

        [Fact]
        public void UrlSanitizer_ValidatesUrls()
        {
            // Arrange
            var validUrl = "https://example.com/path";
            var invalidUrl = "javascript:alert(1)";

            // Act
            var result1 = Sanitizer.Url.Sanitize(validUrl, SanitizerMode.Clean);
            var result2 = Sanitizer.Url.Sanitize(invalidUrl, SanitizerMode.Clean);

            // Assert
            Assert.Contains("https://example.com", result1);
            Assert.DoesNotContain("javascript:", result2);
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
            Assert.EndsWith("23", result); // Should preserve last 2 chars by default
        }

        [Fact]
        public void LogSanitizer_WithNullInput_ReturnsEmpty()
        {
            // Arrange
            string input = null;

            // Act
            var result = Sanitizer.Log.Sanitize(input);

            // Assert
            Assert.Equal(string.Empty, result);
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
        public void HtmlSanitizer_WithNullInput_ReturnsEmpty()
        {
            // Arrange
            string input = null;

            // Act
            var result = Sanitizer.Html.Sanitize(input, SanitizerMode.Clean);

            // Assert
            Assert.Equal(string.Empty, result);
        }

        [Fact]
        public void UrlSanitizer_WithNullInput_ReturnsEmpty()
        {
            // Arrange
            string input = null;

            // Act
            var result = Sanitizer.Url.Sanitize(input, SanitizerMode.Clean);

            // Assert
            Assert.Equal(string.Empty, result);
        }
    }
}
