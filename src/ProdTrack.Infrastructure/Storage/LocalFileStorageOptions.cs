namespace ProdTrack.Infrastructure.Storage;

public sealed class LocalFileStorageOptions
{
    public const string SectionName = "Storage:Local";

    /// <summary>Root folder; relative paths are resolved against the content root (App_Data/files on the host).</summary>
    public string RootPath { get; set; } = "App_Data/files";
}
