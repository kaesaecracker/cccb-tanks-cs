using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using TanksServer.GameLogic;
using TanksServer.Interactivity;

namespace TanksServer;

internal static class Endpoints
{
    public static void MapEndpoints(WebApplication app)
    {
        var clientScreenServer = app.Services.GetRequiredService<ClientScreenServer>();
        var playerService = app.Services.GetRequiredService<PlayerServer>();
        var controlsServer = app.Services.GetRequiredService<ControlsServer>();
        var mapService = app.Services.GetRequiredService<MapService>();

        app.MapPost("/player", (string name, Guid? id) =>
        {
            name = name.Trim().ToUpperInvariant();
            if (name == string.Empty)
                return Results.BadRequest("name cannot be blank");
            if (name.Length > 12)
                return Results.BadRequest("name too long");

            if (!id.HasValue || id.Value == Guid.Empty)
                id = Guid.NewGuid();

            var player = playerService.GetOrAdd(name, id.Value);
            return player != null
                ? Results.Ok(new NameId(player.Name, player.Id))
                : Results.Unauthorized();
        });

        app.MapGet("/player", ([FromQuery] Guid id) =>
            playerService.TryGet(id, out var foundPlayer)
                ? Results.Ok((object?)foundPlayer)
                : Results.NotFound()
        );

        app.MapGet("/scores", () => playerService.GetAll());

        app.Map("/screen", async (HttpContext context, [FromQuery] Guid? player) =>
        {
            if (!context.WebSockets.IsWebSocketRequest)
                return Results.BadRequest();

            using var ws = await context.WebSockets.AcceptWebSocketAsync();
            await clientScreenServer.HandleClient(ws, player);
            return Results.Empty;
        });

        app.Map("/controls", async (HttpContext context, [FromQuery] Guid playerId) =>
        {
            if (!context.WebSockets.IsWebSocketRequest)
                return Results.BadRequest();

            if (!playerService.TryGet(playerId, out var player))
                return Results.NotFound();

            using var ws = await context.WebSockets.AcceptWebSocketAsync();
            await controlsServer.HandleClient(ws, player);
            return Results.Empty;
        });

        app.MapGet("/map", () => mapService.MapNames);

        app.MapPost("/map", ([FromQuery] string name) =>
        {
            if (string.IsNullOrWhiteSpace(name))
                return Results.BadRequest("invalid map name");
            if (!mapService.TrySwitchTo(name))
                return Results.NotFound("map with name not found");
            return Results.Ok();
        });
    }

}
