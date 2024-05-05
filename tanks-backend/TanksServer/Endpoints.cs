using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using TanksServer.GameLogic;
using TanksServer.Interactivity;

namespace TanksServer;

internal sealed class Endpoints(
    ClientScreenServer clientScreenServer,
    PlayerServer playerService,
    ControlsServer controlsServer,
    MapService mapService,
    ChangeToRequestedMap changeToRequestedMap
)
{
    public void Map(WebApplication app)
    {
        app.MapPost("/player", PostPlayer);
        app.MapGet("/player", GetPlayerAsync);
        app.MapGet("/scores", () => playerService.Players);
        app.Map("/screen", ConnectScreenAsync);
        app.Map("/controls", ConnectControlsAsync);
        app.MapGet("/map", () => mapService.MapNames);
        app.MapPost("/map", PostMap);
        app.MapGet("/map/{name}", GetMapByName);
    }

    private Results<BadRequest<string>, NotFound<string>, Ok> PostMap([FromQuery] string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            return TypedResults.BadRequest("invalid map name");

        name = name.Trim().ToUpperInvariant();
        if (!mapService.TryGetPrototype(name, out var map))
            return TypedResults.NotFound("map with name not found");

        changeToRequestedMap.Request(map);
        return TypedResults.Ok();
    }

    private async Task<Results<BadRequest, NotFound, EmptyHttpResult>> ConnectControlsAsync(HttpContext context,
        [FromQuery] string playerName)
    {
        if (!context.WebSockets.IsWebSocketRequest)
            return TypedResults.BadRequest();

        if (!playerService.TryGet(playerName, out var player))
            return TypedResults.NotFound();

        using var ws = await context.WebSockets.AcceptWebSocketAsync();
        await controlsServer.HandleClientAsync(ws, player);
        return TypedResults.Empty;
    }

    private async Task<Results<BadRequest, EmptyHttpResult, NotFound>> ConnectScreenAsync(HttpContext context,
        [FromQuery] string? playerName)
    {
        if (!context.WebSockets.IsWebSocketRequest)
            return TypedResults.BadRequest();

        Player? player = null;
        if (!string.IsNullOrWhiteSpace(playerName) && !playerService.TryGet(playerName, out player))
            return TypedResults.NotFound();

        using var ws = await context.WebSockets.AcceptWebSocketAsync();
        await clientScreenServer.HandleClientAsync(ws, player);
        return TypedResults.Empty;
    }

    private async Task<Results<NotFound, Ok<Player>, EmptyHttpResult>> GetPlayerAsync(HttpContext context,
        [FromQuery] string name)
    {
        if (!playerService.TryGet(name, out var foundPlayer))
            return TypedResults.NotFound();

        if (!context.WebSockets.IsWebSocketRequest)
            return TypedResults.Ok(foundPlayer);

        using var ws = await context.WebSockets.AcceptWebSocketAsync();
        await playerService.HandleClientAsync(ws, foundPlayer);
        return TypedResults.Empty;
    }

    private Results<BadRequest<string>, Ok<string>, UnauthorizedHttpResult> PostPlayer([FromQuery] string name)
    {
        name = name.Trim().ToUpperInvariant();
        if (name == string.Empty) return TypedResults.BadRequest("name cannot be blank");
        if (name.Length > 12) return TypedResults.BadRequest("name too long");

        var player = playerService.GetOrAdd(name);
        return TypedResults.Ok(player.Name);
    }

    private Results<Ok<MapInfo>, NotFound, BadRequest<string>> GetMapByName(string name)
    {
        name = name.Trim().ToUpperInvariant();
        if (string.IsNullOrEmpty(name))
            return TypedResults.BadRequest("map name cannot be empty");

        if (!mapService.TryGetPrototype(name, out var prototype))
            return TypedResults.NotFound();
        if (!mapService.TryGetPreview(name, out var preview))
            return TypedResults.NotFound();

        var mapInfo = new MapInfo(prototype.Name, prototype.GetType().Name, preview.Data);
        return TypedResults.Ok(mapInfo);
    }
}
