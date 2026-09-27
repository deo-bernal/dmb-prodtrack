# ADR-0006: Minimal APIs with OpenAPI; SignalR for push only

- Status: Accepted
- Date: 2026-09-27

## Context

Clients (Blazor WASM, future MAUI, integrations) need a documented REST API and live updates.

## Decision

REST endpoints under `/api/v1` built with Minimal APIs and grouped per feature, described with the built-in OpenAPI document (`/openapi/v1.json`, Swagger UI at `/swagger` in Development). Errors are RFC 9457 ProblemDetails with a stable `code`. SignalR hub `/hubs/production` only pushes notifications after commits; writes always go through REST or in-process use cases.

## Consequences

+ Small, fast, testable. - Endpoint metadata is hand-written.

## Alternatives considered

MVC controllers; gRPC.
