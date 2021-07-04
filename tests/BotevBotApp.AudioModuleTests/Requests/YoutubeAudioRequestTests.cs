using System;
using System.Threading.Tasks;
using Xunit;
using BotevBotApp.AudioModule.Requests;

namespace BotevBotApp.AudioModuleTests.Requests
{
    public class YoutubeAudioRequestTests
    {
        [Fact]
        public async Task TestToAudioItemDto()
        {
            // Arrange
            string expectedSource = "Youtube";

            string requester = "TestMethod";
            
            string request = "https://www.youtube.com/watch?v=fJg-Wnne7Pk";
            string expectedName = "Playing a Socialist Paladin";

            Uri requestUri = new(request);

            YoutubeAudioRequest ytRequest = new(requester, requestUri);

            // Act
            var audioItemDto = await ytRequest.ToAudioItemAsync();

            // Assert
            Assert.Equal(requester, audioItemDto.Requester);
            Assert.Equal(expectedName, audioItemDto.Name);
            Assert.Equal(expectedSource, audioItemDto.Source, ignoreCase: true);
        }

        [Fact]
        public async Task TestToAudioPlayback()
        {
            // Arrange
            string requester = "TestMethod";

            string request = "https://www.youtube.com/watch?v=d1YBv2mWll0";
            Uri requestUri = new(request);

            YoutubeAudioRequest ytRequest = new(requester, requestUri);

            // Act
            var playback = await ytRequest.GetAudioPlaybackAsync();

            // Assert
            // TODO: How do I test this?
        }
    }
}
