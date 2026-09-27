# ADR-0002: Blazor Web App (Interactive Server) back office + Blazor WASM PWA shop floor, single host

- Status: Accepted
- Date: 2026-09-27

## Context

Back-office users are on the plant LAN/desktop; tablets need an installable, offline-tolerant UI. The free host allows one site.

## Decision

`ProdTrack.Server` hosts the REST API, the SignalR hub, the Blazor back office (static SSR for account pages, Interactive Server for feature pages) and serves the `ProdTrack.ShopFloor` WebAssembly PWA under `/floor` (same origin, same Identity cookie).

## Consequences

+ One deployable, one cookie, no CORS. - Interactive Server circuits cost memory (circuit retention reduced for the 256 MB plan).

## Alternatives considered

Separate SPA + API sites (needs a second site/CORS); MAUI only (Phase 2).
