#if ANDROID
using Android.Util;
using Vision2Audio.Core.Diagnostics;

namespace Vision2Audio.App;

internal static class AndroidDiagnosticLog
{
    private const string Tag = "Vision2Audio";

    public static void Exception(string operation, Exception exception)
    {
        try
        {
            var diagnostic = SanitizedExceptionDiagnostics.Create(operation, exception);
            Log.Debug(Tag, $"[Diagnostics] Operation={diagnostic.Operation}; Exception={diagnostic.ExceptionType}; Message={diagnostic.Message}");
            if (!string.IsNullOrWhiteSpace(diagnostic.StackTrace))
            {
                Log.Debug(Tag, $"[Diagnostics] Operation={diagnostic.Operation}; Stack={diagnostic.StackTrace}");
            }
        }
        catch
        {
            // Diagnostic logging must never mask the original camera failure.
        }
    }
}
#endif
