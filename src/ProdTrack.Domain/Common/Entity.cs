namespace ProdTrack.Domain.Common;

/// <summary>Base class for entities with an integer identity key.</summary>
public abstract class Entity
{
    public int Id { get; protected set; }
}
