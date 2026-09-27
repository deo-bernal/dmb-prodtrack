namespace ProdTrack.Domain.Quality;

public enum QcItemKind
{
    PassFail,
    Measured,
}

public enum QcResult
{
    Passed,
    Failed,
}

public enum QcDisposition
{
    None,
    Rework,
    Hold,
}
