namespace ProdTrack.Application.Messaging;

/// <summary>A state-changing use case returning <typeparamref name="TResult"/> on success.</summary>
#pragma warning disable CA1040 // Marker interface is intentional (typed dispatch).
public interface ICommand<TResult>;

/// <summary>A read-only use case returning <typeparamref name="TResult"/> on success.</summary>
public interface IQuery<TResult>;
#pragma warning restore CA1040
