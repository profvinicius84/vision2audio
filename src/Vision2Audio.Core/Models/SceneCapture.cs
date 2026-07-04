namespace Vision2Audio.Core.Models;

/// <summary>
/// Represents captured scene image data.
/// </summary>
public sealed record SceneCapture(byte[] Data, string FileName, string MimeType, DateTimeOffset CapturedAtUtc);
