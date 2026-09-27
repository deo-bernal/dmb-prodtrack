namespace ProdTrack.Application.Messaging;

/// <summary>Declares the authorization policy a command or query requires (checked by the authorization decorator).</summary>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
public sealed class RequiresPolicyAttribute(string policy) : Attribute
{
    public string Policy { get; } = policy;
}
