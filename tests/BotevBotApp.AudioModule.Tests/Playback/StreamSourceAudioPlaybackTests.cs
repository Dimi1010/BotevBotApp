using Xunit;
using FluentAssertions;
using BotevBotApp.AudioModule.Playback;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace BotevBotApp.AudioModule.Playback.Tests
{
    public class StreamSourceAudioPlaybackTests
    {
        [Fact()]
        public async Task GetAudioStreamAsyncTest()
        {
            // Arrange 
            byte[] mockData = { 4, 5, 78, 88, 44 };
            var givenStream = new MemoryStream(mockData);

            // Act
            using var sourcePlayback = new StreamSourceAudioPlayback(givenStream);

            var receivedStream = await sourcePlayback.GetAudioStreamAsync().ConfigureAwait(false);

            // Assert
            receivedStream.Should().Be(givenStream);
        }
    }
}