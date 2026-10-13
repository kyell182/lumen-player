namespace Lumen.Features.Playback.Subtitles.Parsing;

public interface ISubtitleParser
{
    IReadOnlyList<SubtitleCue> Parse(string content);
}
