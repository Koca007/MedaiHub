using MediaHub.Application.Scanning;

namespace MediaHub.Application.Tests.Scanning;

public sealed class SupportedMediaFilesTests
{
    [Theory]
    [InlineData("movie.MKV")]
    [InlineData("movie.mp4")]
    [InlineData("episode.m2ts")]
    public void RecognizesSupportedVideoExtensions(string path)
    {
        Assert.True(SupportedMediaFiles.IsVideo(path));
    }

    [Theory]
    [InlineData("notes.txt")]
    [InlineData("download.mkv.part")]
    [InlineData("cover.jpg")]
    public void RejectsUnsupportedExtensions(string path)
    {
        Assert.False(SupportedMediaFiles.IsVideo(path));
    }
}
