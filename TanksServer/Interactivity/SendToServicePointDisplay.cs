using System.Diagnostics;
using System.Net.Sockets;
using DisplayCommands;
using TanksServer.GameLogic;
using TanksServer.Graphics;

namespace TanksServer.Interactivity;

internal sealed class SendToServicePointDisplay : ITickStep
{
    private readonly LastFinishedFrameProvider _lastFinishedFrameProvider;
    private readonly Cp437Grid _scoresBuffer;
    private readonly PlayerServer _players;
    private readonly ILogger<SendToServicePointDisplay> _logger;
    private readonly IDisplayConnection _displayConnection;
    private PixelGrid? _lastSentFrame;

    private DateTime _nextFailLog = DateTime.Now;

    private const int ScoresWidth = 12;
    private const int ScoresHeight = 20;
    private const int ScoresPlayerRows = ScoresHeight - 5;

    public SendToServicePointDisplay(
        LastFinishedFrameProvider lastFinishedFrameProvider,
        PlayerServer players,
        ILogger<SendToServicePointDisplay> logger,
        IDisplayConnection displayConnection
    )
    {
        _lastFinishedFrameProvider = lastFinishedFrameProvider;
        _players = players;
        _logger = logger;
        _displayConnection = displayConnection;

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

    public async Task TickAsync()
    {
        RefreshScores();
        try
        {
            await _displayConnection.SendCp437DataAsync(MapService.TilesPerRow, 0, _scoresBuffer);

            var currentFrame = _lastFinishedFrameProvider.LastFrame;
            if (_lastSentFrame == currentFrame)
                return;
            _lastSentFrame = currentFrame;
            await _displayConnection.SendBitmapLinearWindowAsync(0, 0, _lastSentFrame);
        }
        catch (SocketException ex)
        {
            if (DateTime.Now > _nextFailLog)
            {
                _logger.LogWarning("could not send data to service point display: {}", ex.Message);
                _nextFailLog = DateTime.Now + TimeSpan.FromSeconds(5);
            }
        }
    }

    private void RefreshScores()
    {
        var playersToDisplay = _players.GetAll()
            .OrderByDescending(p => p.Kills)
            .Take(ScoresPlayerRows);

        ushort row = 2;
        foreach (var p in playersToDisplay)
        {
            var score = p.Kills.ToString();
            var nameLength = Math.Min(p.Name.Length, ScoresWidth - score.Length - 1);

            var name = p.Name[..nameLength];
            var spaces = new string(' ', ScoresWidth - score.Length - nameLength);

            _scoresBuffer[row] = name + spaces + score;
            row++;
        }

        for (; row < 17; row++)
            _scoresBuffer[row] = string.Empty;
    }
}