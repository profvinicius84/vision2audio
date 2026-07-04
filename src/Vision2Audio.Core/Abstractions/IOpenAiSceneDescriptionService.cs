using Vision2Audio.Core.Models;

namespace Vision2Audio.Core.Abstractions;

/// <summary>
/// Sends a scene capture directly to OpenAI and returns a Portuguese description.
/// </summary>
public interface IOpenAiSceneDescriptionService
{
    /// <summary>Describes a captured scene.</summary>
    Task<Result<SceneDescription>> DescribeAsync(SceneCapture capture, GeoCoordinate? location, CancellationToken cancellationToken);
}
