using System.Text.RegularExpressions;

namespace Vision2Audio.Core.Diagnostics;

/// <summary>
/// Builds diagnostic exception text while redacting sensitive values from logs and status messages.
/// </summary>
public static class SanitizedExceptionDiagnostics
{
    private const int MaxOperationLength = 80;
    private const int MaxMessageLength = 512;
    private const int MaxStackTraceLength = 2048;

    private static readonly Regex WindowsPathRegex = new(@"[A-Za-z]:\\(?:[^\\\s:]+\\)*[^\\\s:]+", RegexOptions.Compiled);
    private static readonly Regex UnixHomePathRegex = new(@"/(?:Users|home)/[^\s:]+", RegexOptions.Compiled | RegexOptions.IgnoreCase);
    private static readonly Regex UsbBusPathRegex = new(@"/dev/bus/usb/\d{3}/\d{3}", RegexOptions.Compiled | RegexOptions.IgnoreCase);
    private static readonly Regex KeyValueSecretRegex = new(@"""?\b(api[_-]?key|apiKey|password|secret|token|serial|authorization)\b""?\s*[:=]\s*""?[^""\s;,}]+""?", RegexOptions.Compiled | RegexOptions.IgnoreCase);
    private static readonly Regex BearerTokenRegex = new(@"\bBearer\s+[^\s;,}]+", RegexOptions.Compiled | RegexOptions.IgnoreCase);
    private static readonly Regex LongTokenRegex = new(@"\b[A-Za-z0-9+/=_-]{32,}\b", RegexOptions.Compiled);

    public static SanitizedExceptionDiagnostic Create(string operation, Exception exception)
        => new(
            Sanitize(operation, MaxOperationLength),
            exception.GetType().Name,
            Sanitize(exception.Message, MaxMessageLength),
            Sanitize(exception.StackTrace ?? string.Empty, MaxStackTraceLength));

    public static string SanitizeForStatus(string? value)
        => Sanitize(value, MaxMessageLength);

    private static string Sanitize(string? value, int maxLength)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return string.Empty;
        }

        var sanitized = value.ReplaceLineEndings(" ");
        sanitized = UsbBusPathRegex.Replace(sanitized, "<usb-device-path>");
        sanitized = WindowsPathRegex.Replace(sanitized, "<path>");
        sanitized = UnixHomePathRegex.Replace(sanitized, "<path>");
        sanitized = BearerTokenRegex.Replace(sanitized, "Bearer <redacted>");
        sanitized = KeyValueSecretRegex.Replace(sanitized, match => $"{match.Groups[1].Value}=<redacted>");
        sanitized = LongTokenRegex.Replace(sanitized, "<redacted>");

        return sanitized.Length <= maxLength ? sanitized : sanitized[..maxLength] + "…";
    }
}
