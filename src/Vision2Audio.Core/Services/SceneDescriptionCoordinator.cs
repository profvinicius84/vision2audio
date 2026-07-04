using Vision2Audio.Core.Abstractions;
using Vision2Audio.Core.Models;

namespace Vision2Audio.Core.Services;

/// <summary>
/// Coordinates capture, location, OpenAI and history persistence.
/// </summary>
public sealed class SceneDescriptionCoordinator(
    IConnectivityService connectivityService,
    ICaptureService captureService,
    ILocationService locationService,
    IOpenAiSceneDescriptionService openAiService,
    IHistoryRepository historyRepository)
{
    /// <summary>Captures and describes the current scene.</summary>
    public async Task<Result<SceneDescription>> CaptureAndDescribeAsync(CancellationToken cancellationToken)
    {
        if (!connectivityService.IsOnline())
        {
            return Result<SceneDescription>.Failure("Sem conexão com a internet.");
        }

        var captureResult = await captureService.CaptureAsync(cancellationToken);
        if (!captureResult.IsSuccess || captureResult.Value is null)
        {
            return Result<SceneDescription>.Failure(captureResult.Error ?? "Não foi possível capturar a cena.");
        }

        var locationResult = await locationService.GetCurrentLocationAsync(cancellationToken);
        if (!locationResult.IsSuccess || locationResult.Value is null)
        {
            return Result<SceneDescription>.Failure(locationResult.Error ?? "Não foi possível obter a localização.");
        }

        GeoCoordinate? location = locationResult.Value;

        var descriptionResult = await openAiService.DescribeAsync(captureResult.Value, location, cancellationToken);
        if (!descriptionResult.IsSuccess || descriptionResult.Value is null)
        {
            return Result<SceneDescription>.Failure(descriptionResult.Error ?? "Não foi possível obter a descrição.");
        }

        await historyRepository.AddAsync(descriptionResult.Value, location, cancellationToken);
        return descriptionResult;
    }
}
