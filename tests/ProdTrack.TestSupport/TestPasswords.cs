namespace ProdTrack.TestSupport;

/// <summary>
/// Generates throwaway passwords at run time so that no password-like literals live in source control. Each value
/// satisfies the Identity rules (12+ characters, upper, lower, digit and non-alphanumeric).
/// </summary>
public static class TestPasswords
{
    public static string Generate() => $"Tp1-{Guid.NewGuid():N}";
}
