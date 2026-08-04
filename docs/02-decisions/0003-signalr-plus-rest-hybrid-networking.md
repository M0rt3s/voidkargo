# ADR 0003 — SignalR + REST Hybrid Networking

- **Status**: Accepted
- **Date**: 2026-07-30

## Context

The game needs to push frequent, real-time updates (ship movement, economy ticks, chat) to
many connected clients, as well as handle explicit client actions (buying a ship, loading
initial state, fetching history/leaderboards). A single networking approach for both would
be a poor fit for at least one of the two.

In particular, a naive "poke" model — where the server tells clients "something changed,
go fetch it via REST" — creates a **thundering herd** problem: a single server-side event can
cause thousands of clients to simultaneously hit the REST API, risking a crash under load.

## Decision

Use a **hybrid** approach, strictly divided by who initiates the action:

- **SignalR** for server-initiated, real-time pushes: ship movement, economy ticks/price
  changes, chat. The server pushes the actual data payload directly — not just a "go fetch"
  notification.
- **REST API** for client-initiated, state-heavy actions: initial game-state download on
  login, transactions (e.g., "Buy Ship"), historical logs, leaderboards.

The official Microsoft `Microsoft.AspNetCore.SignalR.Client` package is used in Unity, so
`Game.Shared` DTOs deserialize incoming WebSocket messages directly — no separate client-side
wire format.

## Consequences

- **Easier**: avoids thundering-herd load spikes; clear rule of thumb for where new
  functionality belongs (server-initiated vs client-initiated); one shared DTO layer for both
  channels.
- **Harder**: two networking code paths to maintain and reason about; need discipline to keep
  the split clean (see [networking strategy](../01-architecture/networking-strategy.md)) so
  it doesn't devolve into ad hoc REST polling or oversized SignalR payloads.

## Alternatives considered

- **Pure REST polling**: simplest to build, but reintroduces thundering-herd risk and adds
  latency for real-time updates like ship movement.
- **Pure SignalR for everything**: would force state-heavy, client-initiated actions (like
  loading full initial game state) through a persistent-connection model that's a worse fit
  than a plain request/response call, and complicates caching/retries for those actions.
