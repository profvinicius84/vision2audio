using System.Net.Http.Headers;
using System.Net.Http.Json;
using Vision2Audio.Core;
using Vision2Audio.Core.Abstractions;
using Vision2Audio.Core.Models;
using Vision2Audio.Core.Options;

namespace Vision2Audio.App.Services;

/// <summary>
/// Sends the scene directly to OpenAI using the chat-completions API.
/// </summary>
public sealed class OpenAiSceneDescriptionService : IOpenAiSceneDescriptionService
{
    private readonly HttpClient _httpClient;
    private readonly OpenAiOptions _options;
    private readonly IOpenAiSecretsProvider _secretsProvider;

    public OpenAiSceneDescriptionService(HttpClient httpClient, OpenAiOptions options, IOpenAiSecretsProvider secretsProvider)
    {
        _httpClient = httpClient;
        _options = options;
        _secretsProvider = secretsProvider;
    }

    /// <inheritdoc />
    public async Task<Result<SceneDescription>> DescribeAsync(SceneCapture capture, GeoCoordinate? location, CancellationToken cancellationToken)
    {
        var apiKey = await _secretsProvider.GetApiKeyAsync(cancellationToken);
        if (string.IsNullOrWhiteSpace(apiKey))
        {
            return Result<SceneDescription>.Failure("Chave da OpenAI não encontrada em secrets.local.json.");
        }

        var prompt = BuildPrompt(location);
        var request = new
        {
            model = _options.Model,
            temperature = 0.2,
            messages = new object[]
            {
                new
                {
                    role = "system",
                    content = "Você descreve cenas com clareza, em português do Brasil, de forma curta, útil e acessível para pessoas com deficiência visual."
                },
                new
                {
                    role = "user",
                    content = new object[]
                    {
                        new { type = "text", text = prompt },
                        new { type = "image_url", image_url = new { url = $"data:{capture.MimeType};base64,{Convert.ToBase64String(capture.Data)}" } }
                    }
                }
            }
        };

        using var httpRequest = new HttpRequestMessage(HttpMethod.Post, _options.Endpoint);
        httpRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);
        httpRequest.Content = JsonContent.Create(request);

        using var response = await _httpClient.SendAsync(httpRequest, cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            return Result<SceneDescription>.Failure(BuildSafeErrorMessage(response.StatusCode));
        }

        var payload = await response.Content.ReadFromJsonAsync<OpenAiChatResponse>(cancellationToken: cancellationToken);
        var text = payload?.Choices?.FirstOrDefault()?.Message?.Content?.Trim();
        if (string.IsNullOrWhiteSpace(text))
        {
            return Result<SceneDescription>.Failure("OpenAI não retornou texto.");
        }

        return Result<SceneDescription>.Success(new SceneDescription(text, _options.Model, DateTimeOffset.UtcNow));
    }

    private static string BuildPrompt(GeoCoordinate? location)
    {
        var locationText = location is null
            ? "Localização indisponível."
            : $"Localização atual: latitude {location.Latitude:F6}, longitude {location.Longitude:F6}.";

        return $"Descreva a cena com foco em orientação imediata, segurança e pontos úteis para uma pessoa com deficiência visual. Responda em português do Brasil. Seja objetivo. {locationText}";
    }

    private static string BuildSafeErrorMessage(System.Net.HttpStatusCode statusCode)
    {
        var category = (int)statusCode switch
        {
            401 or 403 => "autorização",
            408 => "tempo limite",
            429 => "limite de uso",
            >= 500 => "indisponibilidade do serviço",
            _ => "falha na solicitação"
        };

        return $"Não foi possível obter a descrição da cena. OpenAI retornou erro {(int)statusCode} ({category}).";
    }

    private sealed record OpenAiChatResponse(IReadOnlyList<OpenAiChatResponse.Choice>? Choices)
    {
        public sealed record Choice(Message? Message);
        public sealed record Message(string? Content);
    }
}
