namespace Vision2Audio.Core.Diagnostics;

/// <summary>
/// Sanitized exception details safe for device-visible diagnostic logs.
/// </summary>
public sealed record SanitizedExceptionDiagnostic(string Operation, string ExceptionType, string Message, string StackTrace);
