using System.Net.WebSockets;
using System.Text.Json;
using TanksServer.GameLogic;

namespace TanksServer.Interactivity;

internal sealed class PlayerInfoConnection(
    Player player,
    ILogger logger,
    WebSocket rawSocket,
    MapEntityManager entityManager
) : WebsocketServerConnection(logger, new ByteChannelWebSocket(rawSocket, logger, 0)), IDisposable
{
    private readonly SemaphoreSlim _wantedFrames = new(1);
    private readonly AppSerializerContext _context = new(new JsonSerializerOptions(JsonSerializerDefaults.Web));
    private byte[] _lastMessage = [];

    protected override ValueTask HandleMessageAsync(Memory<byte> buffer)
    {
        var response = GetMessageToSend();
        if (response == null)
        {
            Logger.LogTrace("cannot respond directly, increasing wanted frames");
            _wantedFrames.Release();
            return ValueTask.CompletedTask;
        }

        Logger.LogTrace("responding directly");
        return Socket.SendTextAsync(response);
    }

    public async Task OnGameTickAsync()
    {
        if (!await _wantedFrames.WaitAsync(TimeSpan.Zero))
            return;

        var response = GetMessageToSend();
        if (response == null)
        {
            _wantedFrames.Release();
            return;
        }

        Logger.LogTrace("responding indirectly");
        await Socket.SendTextAsync(response);
    }

    private byte[]? GetMessageToSend()
    {
        var tank = entityManager.GetCurrentTankOfPlayer(player);
        var tankInfo = tank != null
            ? new TankInfo(tank.Orientation, tank.ExplosiveBullets, tank.Position.ToPixelPosition(), tank.Moving)
            : null;
        var info = new PlayerInfo(player.Name, player.Scores, player.Controls, tankInfo);
        var response = JsonSerializer.SerializeToUtf8Bytes(info, _context.PlayerInfo);

        if (response.SequenceEqual(_lastMessage))
            return null;

        return _lastMessage = response;
    }

    public void Dispose() => _wantedFrames.Dispose();
}
