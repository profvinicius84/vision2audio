using Vision2Audio.Core.Models;

namespace Vision2Audio.Core.Abstractions;

/// <summary>
/// Speaks the approved Brazilian Portuguese response.
/// </summary>
public interface ITextToSpeechService
{
    /// <summary>Speaks the provided text.</summary>
    Task SpeakAsync(SceneDescription description, CancellationToken cancellationToken);
}
