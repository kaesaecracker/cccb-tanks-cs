using System.Diagnostics;
using System.Net.Sockets;
using DisplayCommands;
using TanksServer.GameLogic;
using TanksServer.Graphics;

namespace TanksServer.Interactivity;

internal sealed class SendToServicePointDisplay : IFrameConsumer
{
    private const int ScoresWidth = 12;
    private const int ScoresHeight = 20;
    private const int ScoresPlayerRows = ScoresHeight - 6;

    private readonly IDisplayConnection _displayConnection;
    private readonly MapService _mapService;
    private readonly ILogger<SendToServicePointDisplay> _logger;
    private readonly PlayerServer _players;
    private readonly Cp437Grid _scoresBuffer;
    private readonly TimeSpan _minFrameTime;

    private DateTime _nextFailLogAfter = DateTime.Now;
    private DateTime _nextFrameAfter = DateTime.Now;

    public SendToServicePointDisplay(
        PlayerServer players,
        ILogger<SendToServicePointDisplay> logger,
        IDisplayConnection displayConnection,
        IOptions<HostConfiguration> hostOptions,
        MapService mapService
    )
    {
        _players = players;
        _logger = logger;
        _displayConnection = displayConnection;
        _mapService = mapService;
        _minFrameTime = TimeSpan.FromMilliseconds(hostOptions.Value.ServicePointDisplayMinFrameTimeMs);

        var localIp = _displayConnection.GetLocalIPv4().Split('.');
        Debug.Assert(localIp.Length == 4);
        _scoresBuffer = new Cp437Grid(12, 20)
        {
            [00] = "== TANKS! ==",
            [01] = "-- scores --",
            [17] = "--  join  --",
            [18] = string.Join('.', localIp[..2]),
            [19] = string.Join('.', localIp[2..])
        };
    }

    public async Task OnFrameDoneAsync(GamePixelGrid gamePixelGrid, PixelGrid observerPixels)
    {
        if (DateTime.Now < _nextFrameAfter)
            return;

        _nextFrameAfter = DateTime.Now + _minFrameTime;
        await Task.Yield();

        RefreshScores();

        try
        {
            await _displayConnection.SendBitmapLinearWindowAsync(0, 0, observerPixels);
            await _displayConnection.SendCp437DataAsync(MapService.TilesPerRow, 0, _scoresBuffer);
        }
        catch (SocketException ex)
        {
            if (DateTime.Now > _nextFailLogAfter)
            {
                _logger.LogWarning("could not send data to service point display: {}", ex.Message);
                _nextFailLogAfter = DateTime.Now + TimeSpan.FromSeconds(5);
            }
        }
    }

    private void RefreshScores()
    {
        var playersToDisplay = _players.GetAll()
            .OrderByDescending(p => p.Scores.Kills)
            .Take(ScoresPlayerRows);

        ushort row = 2;
        foreach (var p in playersToDisplay)
        {
            var score = p.Scores.Kills.ToString();
            var nameLength = Math.Min(p.Name.Length, ScoresWidth - score.Length - 1);

            var name = p.Name[..nameLength];
            var spaces = new string(' ', ScoresWidth - score.Length - nameLength);

            _scoresBuffer[row] = name + spaces + score;
            row++;
        }

        for (; row < 16; row++)
            _scoresBuffer[row] = string.Empty;

        _scoresBuffer[16] = _mapService.Current.Name[..(Math.Min(ScoresWidth, _mapService.Current.Name.Length) - 1)];
    }
}
