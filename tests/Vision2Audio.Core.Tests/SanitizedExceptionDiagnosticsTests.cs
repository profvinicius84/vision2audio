using Vision2Audio.Core.Diagnostics;

namespace Vision2Audio.Core.Tests;

public sealed class SanitizedExceptionDiagnosticsTests
{
    [Fact]
    public void Create_RemovesSensitiveDeviceAndPathDetails()
    {
        var exception = new InvalidOperationException(
            "Open failed for /dev/bus/usb/001/002 at C:\\Users\\person\\project\\file.cs with api_key=secret-token and serial=ABC123");

        var diagnostic = SanitizedExceptionDiagnostics.Create("otg-preview", exception);

        Assert.Equal("otg-preview", diagnostic.Operation);
        Assert.Equal(nameof(InvalidOperationException), diagnostic.ExceptionType);
        Assert.DoesNotContain("/dev/bus/usb", diagnostic.Message);
        Assert.DoesNotContain("C:\\Users", diagnostic.Message);
        Assert.DoesNotContain("secret-token", diagnostic.Message);
        Assert.DoesNotContain("ABC123", diagnostic.Message);
    }

    [Fact]
    public void Create_RemovesColonJsonAndBearerSecretDetails()
    {
        var exception = new InvalidOperationException(
            "Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.secretsecretsecret | {\"apiKey\":\"sk-test-secret\",\"serial\":\"USB123456\"} | token: plain-secret");

        var diagnostic = SanitizedExceptionDiagnostics.Create("otg-preview", exception);

        Assert.DoesNotContain("eyJhbGci", diagnostic.Message);
        Assert.DoesNotContain("sk-test-secret", diagnostic.Message);
        Assert.DoesNotContain("USB123456", diagnostic.Message);
        Assert.DoesNotContain("plain-secret", diagnostic.Message);
    }

    [Fact]
    public void Create_SanitizesStackTraceSourcePaths()
    {
        var exception = CreateExceptionWithStackTrace();

        var diagnostic = SanitizedExceptionDiagnostics.Create("otg-capture", exception);

        Assert.Equal(nameof(InvalidOperationException), diagnostic.ExceptionType);
        Assert.DoesNotContain("C:\\", diagnostic.StackTrace);
        Assert.DoesNotContain("/Users/", diagnostic.StackTrace);
    }

    private static Exception CreateExceptionWithStackTrace()
    {
        try
        {
            throw new InvalidOperationException("camera failed");
        }
        catch (Exception ex)
        {
            return ex;
        }
    }
}
