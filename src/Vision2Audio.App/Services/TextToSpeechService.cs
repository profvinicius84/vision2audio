using Vision2Audio.Core.Abstractions;
using Vision2Audio.Core.Models;
using Microsoft.Maui.Media;

namespace Vision2Audio.App.Services;

/// <summary>
/// Uses MAUI text-to-speech.
/// </summary>
public sealed class TextToSpeechService : ITextToSpeechService
{
    private readonly object _gate = new();
    private CancellationTokenSource? _activeSpeechCancellation;

    /// <inheritdoc />
    public async Task SpeakAsync(SceneDescription description, CancellationToken cancellationToken)
    {
        using var linkedCancellation = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        lock (_gate)
        {
            _activeSpeechCancellation?.Cancel();
            _activeSpeechCancellation = linkedCancellation;
        }

        try
        {
            await TextToSpeech.Default.SpeakAsync(description.Text, cancelToken: linkedCancellation.Token);
        }
        finally
        {
            lock (_gate)
            {
                if (ReferenceEquals(_activeSpeechCancellation, linkedCancellation))
                {
                    _activeSpeechCancellation = null;
                }
            }
        }
    }

    /// <inheritdoc />
    public Task StopAsync()
    {
        lock (_gate)
        {
            _activeSpeechCancellation?.Cancel();
            _activeSpeechCancellation = null;
        }

        return Task.CompletedTask;
    }
}
