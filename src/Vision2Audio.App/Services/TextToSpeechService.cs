using Vision2Audio.Core.Abstractions;
using Vision2Audio.Core.Models;
using Microsoft.Maui.Media;

namespace Vision2Audio.App.Services;

/// <summary>
/// Uses MAUI text-to-speech.
/// </summary>
public sealed class TextToSpeechService : ITextToSpeechService
{
    /// <inheritdoc />
    public Task SpeakAsync(SceneDescription description, CancellationToken cancellationToken)
        => TextToSpeech.Default.SpeakAsync(description.Text, cancelToken: cancellationToken);
}
