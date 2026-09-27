namespace ProdTrack.Server.Security;

/// <summary>Auth:Mode (docs/02 section 7.4). Dev and Test are refused outside Development/Testing.</summary>
public enum AuthMode
{
    Identity,
    Dev,
    Test,
}
