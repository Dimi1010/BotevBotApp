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
    public class DecodingAudioPlaybackTests
    {
        [Fact()]
        public async Task GetAudioStreamAsyncTest()
        {
            // Arrange
            var mockWavStr = "RIFF$\x00\x00\x00WAVEfmt \x10\x00\x00\x00\x01\x00\x01\x00\x00\x04\x00\x00\x00\x04\x00\x00\x01\x00\x08\x00data\x00\x00\x00\x00";
            var expectedPcmStr = "";

            byte[] mockData = mockWavStr.Select(x => Convert.ToByte(x)).ToArray();
            byte[] expectedPcm = expectedPcmStr.Select(x => Convert.ToByte(x)).ToArray();


            var mockInnerPlayback = new Moq.Mock<AudioPlayback>();
            mockInnerPlayback.Setup(ap => ap.GetAudioStreamAsync(default)).Returns(Task.FromResult<Stream>(new MemoryStream(mockData, false)));

            var decodingPlayback = new DecodingAudioPlayback(mockInnerPlayback.Object);

            // Act
            using var decodedStream = await decodingPlayback.GetAudioStreamAsync().ConfigureAwait(false);

            byte[] decodedData = new byte[decodedStream.Length];
            decodedStream.Read(decodedData, 0, (int)decodedStream.Length);

            decodedData.Should().BeEquivalentTo(expectedPcm);

            // Assert
            Assert.True(false, "This test needs an implementation");
        }
    }
}