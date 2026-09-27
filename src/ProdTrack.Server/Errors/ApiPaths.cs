namespace ProdTrack.Server.Errors;

internal static class ApiPaths
{
    public static bool IsApi(PathString path) =>
        path.StartsWithSegments("/api") || path.StartsWithSegments("/hubs") || path.StartsWithSegments("/health");
}
