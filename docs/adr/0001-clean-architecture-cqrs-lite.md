# ADR-0001: Clean architecture with CQRS-lite handlers, no MediatR

- Status: Accepted
- Date: 2026-09-27

## Context

The app needs clear boundaries (Domain, Application, Infrastructure, Server) that a single developer can keep consistent, and use cases must be reachable from REST and Blazor Server with the same authorization.

## Decision

Four layers as in docs/02 section 3. Use cases are `ICommand<T>`/`IQuery<T>` records with internal handlers resolved by a small `IDispatcher` (Scrutor scan + decorators for authorization and FluentValidation). Every request type carries `[RequiresPolicy]`; an architecture test enforces it. Expected failures are `Result`/`Error` values mapped to ProblemDetails.

## Consequences

+ No MediatR licence/runtime dependency; explicit, testable pipeline. + Same rules for REST and Blazor. - A little plumbing code to own (Dispatcher, decorators).

## Alternatives considered

MediatR (commercial licence since v13); controllers calling services directly (authorization spread across layers).
