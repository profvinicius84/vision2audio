using System.Text.Json;

namespace Vision2Audio.App.Services;

/// <summary>
/// Reads secrets.local.json from the app package.
/// </summary>
public sealed class AppPackageOpenAiSecretsProvider : IOpenAiSecretsProvider
{
    /// <inheritdoc />
    public async Task<string?> GetApiKeyAsync(CancellationToken cancellationToken)
    {
        try
        {
            await using var stream = await FileSystem.OpenAppPackageFileAsync("secrets.local.json");
            var document = await JsonSerializer.DeserializeAsync<SecretsFile>(stream, cancellationToken: cancellationToken);
            return string.IsNullOrWhiteSpace(document?.OpenAiApiKey) ? null : document.OpenAiApiKey;
        }
        catch (FileNotFoundException)
        {
            return null;
        }
    }

    private sealed record SecretsFile(string? OpenAiApiKey);
}
