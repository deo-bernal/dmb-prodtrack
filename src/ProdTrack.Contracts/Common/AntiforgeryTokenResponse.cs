namespace ProdTrack.Contracts.Common;

/// <summary>Token for cookie-authenticated unsafe API calls, sent in the <see cref="HeaderName"/> header.</summary>
public sealed record AntiforgeryTokenResponse(string Token, string HeaderName);
