namespace Vision2Audio.App.Services;

/// <summary>
/// Loads local development secrets for the app.
/// </summary>
public interface IOpenAiSecretsProvider
{
    /// <summary>Gets the OpenAI API key from the local secrets file.</summary>
    Task<string?> GetApiKeyAsync(CancellationToken cancellationToken);
}
