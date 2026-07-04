namespace Vision2Audio.Core.Options;

/// <summary>
/// Configuration for the direct OpenAI client.
/// </summary>
public sealed class OpenAiOptions
{
    /// <summary>OpenAI chat model to use.</summary>
    public string Model { get; set; } = "gpt-4.1-mini";

    /// <summary>Base API endpoint.</summary>
    public string Endpoint { get; set; } = "https://api.openai.com/v1/chat/completions";
}
