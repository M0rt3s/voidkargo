using Game.Shared.Dtos;
using Microsoft.AspNetCore.SignalR;

namespace Game.Backend.Hubs;

/// <summary>
/// Pushes server-initiated, real-time updates (train movement, economy ticks,
/// chat) to connected clients. See docs/01-architecture/networking-strategy.md
/// and ADR 0003. Client-initiated actions belong in REST endpoints instead.
/// </summary>
public sealed class GameHub : Hub
{
    /// <summary>Broadcasts an economy tick to all connected clients.</summary>
    public async Task BroadcastEconomyTick(EconomyTickDto tick)
    {
        await Clients.All.SendAsync("EconomyTick", tick);
    }
}
