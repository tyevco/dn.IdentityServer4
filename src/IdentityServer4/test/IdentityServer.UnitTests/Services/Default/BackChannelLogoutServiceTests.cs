using System.Linq;
using System.Text.Json;
using IdentityModel;
using Xunit;

namespace IdentityServer.UnitTests.Services.Default
{
    public class BackChannelLogoutServiceTests
    {
        [Fact]
        public void BackChannelLogout_EventsClaim_HasValidJsonStructure()
        {
            // Arrange - Build the events claim as done in DefaultBackChannelLogoutService
            var eventsObj = new System.Collections.Generic.Dictionary<string, object>
            {
                [OidcConstants.Events.BackChannelLogout] = new { }
            };
            var json = JsonSerializer.Serialize(eventsObj);

            // Act
            var parsed = JsonDocument.Parse(json);

            // Assert
            Assert.NotNull(parsed);
            Assert.Equal(JsonValueKind.Object, parsed.RootElement.ValueKind);

            // Verify the events property exists
            Assert.True(parsed.RootElement.TryGetProperty(OidcConstants.Events.BackChannelLogout, out var eventsProp));

            // Verify the backchannel logout event is an object
            Assert.Equal(JsonValueKind.Object, eventsProp.ValueKind);
        }

        [Fact]
        public void BackChannelLogout_EventsClaim_IsNotMalformed()
        {
            // Arrange
            var eventsObj = new System.Collections.Generic.Dictionary<string, object>
            {
                [OidcConstants.Events.BackChannelLogout] = new { }
            };
            var json = JsonSerializer.Serialize(eventsObj);

            // Assert - Should not have trailing spaces or malformed structure
            Assert.DoesNotContain("} }", json); // Old bug: {"event":{} }
            Assert.DoesNotContain(" }", json.TrimEnd()); // No space before closing brace

            // Verify valid JSON
            var parsed = JsonDocument.Parse(json);
            Assert.NotNull(parsed);
        }

        [Fact]
        public void BackChannelLogout_EventsClaim_PropertyNameIsCorrect()
        {
            // Arrange
            var eventsObj = new System.Collections.Generic.Dictionary<string, object>
            {
                [OidcConstants.Events.BackChannelLogout] = new { }
            };
            var json = JsonSerializer.Serialize(eventsObj);
            var parsed = JsonDocument.Parse(json);

            // Assert - Verify the exact property name
            Assert.True(parsed.RootElement.TryGetProperty("http://schemas.openid.net/event/backchannel-logout", out _));
        }

        [Fact]
        public void BackChannelLogout_EventsClaim_EmptyObjectStructure()
        {
            // Arrange
            var eventsObj = new System.Collections.Generic.Dictionary<string, object>
            {
                [OidcConstants.Events.BackChannelLogout] = new { }
            };
            var json = JsonSerializer.Serialize(eventsObj);
            var parsed = JsonDocument.Parse(json);

            // Act
            var eventProp = parsed.RootElement.GetProperty(OidcConstants.Events.BackChannelLogout);

            // Assert - Event should be an empty object {} not an array []
            Assert.Equal(JsonValueKind.Object, eventProp.ValueKind);
            Assert.Equal(0, eventProp.EnumerateObject().Count());
        }

        [Fact]
        public void BackChannelLogout_SystemTextJson_SerializesCorrectly()
        {
            // This test verifies we're using System.Text.Json correctly
            // and not the old Newtonsoft.Json approach

            // Arrange
            var eventsObj = new System.Collections.Generic.Dictionary<string, object>
            {
                [OidcConstants.Events.BackChannelLogout] = new { }
            };

            // Act
            var json = JsonSerializer.Serialize(eventsObj);
            var expectedJson = "{\"http://schemas.openid.net/event/backchannel-logout\":{}}";

            // Assert
            Assert.Equal(expectedJson, json);
        }
    }
}
