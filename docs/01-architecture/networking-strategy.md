# Networking Strategy: SignalR + REST

A hybrid approach, strictly divided by who initiates the action. See
[ADR 0003](../02-decisions/0003-signalr-plus-rest-hybrid-networking.md) for why.

## SignalR — server-initiated, real-time

Used to push actual data payloads, avoiding a "thundering herd" where a bare notification
("something changed, go fetch it") causes every connected client to hammer the REST API at
once.

Use cases:
- Train movement updates.
- Economy ticks / price changes.
- Player chat.

`Game.Shared` DTOs are (de)serialized directly over the SignalR connection — the official
Microsoft `Microsoft.AspNetCore.SignalR.Client` package works in Unity, so the same C# types
flow end to end with no duplicate client-side models.

## REST — client-initiated, heavy lifting

Reserved for explicit actions where a short loading delay is acceptable:
- Downloading initial game state on login.
- Player transactions (e.g., "Buy Train").
- Fetching historical logs / leaderboards.

## Rule of thumb

If the server decides something happened and needs to tell clients → **SignalR**.
If the client decides to do something and needs a response → **REST**.

Don't blur the two: pushing large state dumps over SignalR or polling REST for real-time
updates both reintroduce the problems this split exists to avoid.
