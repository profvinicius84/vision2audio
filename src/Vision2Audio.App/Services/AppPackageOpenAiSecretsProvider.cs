using System.Text.Json;

namespace Vision2Audio.App.Services;

/// <summary>
/// Reads local OpenAI secrets from environment variables or explicitly packaged development secret files.
/// </summary>
public sealed class AppPackageOpenAiSecretsProvider : IOpenAiSecretsProvider
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web)
    {
        PropertyNameCaseInsensitive = true
    };

    private static readonly string[] AppPackageSecretFileNames = ["secrets.local.json", "secrets.local"];

    /// <inheritdoc />
    public async Task<string?> GetApiKeyAsync(CancellationToken cancellationToken)
    {
        var environmentApiKey = Environment.GetEnvironmentVariable("OPENAI_API_KEY");
        if (!string.IsNullOrWhiteSpace(environmentApiKey))
        {
            return environmentApiKey;
        }

        foreach (var fileName in AppPackageSecretFileNames)
        {
            var apiKey = await TryReadAppPackageSecretAsync(fileName, cancellationToken);
            if (!string.IsNullOrWhiteSpace(apiKey))
            {
                return apiKey;
            }
        }

        return null;
    }

    private static async Task<string?> TryReadAppPackageSecretAsync(string fileName, CancellationToken cancellationToken)
    {
        try
        {
            await using var stream = await FileSystem.OpenAppPackageFileAsync(fileName);
            var document = await JsonSerializer.DeserializeAsync<SecretsFile>(stream, JsonOptions, cancellationToken);
            return string.IsNullOrWhiteSpace(document?.OpenAiApiKey) ? null : document.OpenAiApiKey;
        }
        catch (FileNotFoundException)
        {
            return null;
        }
        catch (JsonException)
        {
            return null;
        }
    }

    private sealed record SecretsFile(string? OpenAiApiKey);
}
