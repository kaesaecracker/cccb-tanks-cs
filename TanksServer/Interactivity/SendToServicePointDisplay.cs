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
    private const int ScoresPlayerRows = ScoresHeight - 5;

    private readonly IDisplayConnection _displayConnection;
    private readonly ILogger<SendToServicePointDisplay> _logger;
    private readonly PlayerServer _players;
    private readonly Cp437Grid _scoresBuffer;

    private PixelGrid? _lastSentFrame;
    private DateTime _nextFailLog = DateTime.Now;

    public SendToServicePointDisplay(
        PlayerServer players,
        ILogger<SendToServicePointDisplay> logger,
        IDisplayConnection displayConnection
    )
    {
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

    public async Task OnFrameDoneAsync(GamePixelGrid gamePixelGrid, PixelGrid observerPixels)
    {
        RefreshScores();
        try
        {
            await _displayConnection.SendCp437DataAsync(MapService.TilesPerRow, 0, _scoresBuffer);

            if (_lastSentFrame == observerPixels)
                return;
            _lastSentFrame = observerPixels;
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

        for (; row < 17; row++)
            _scoresBuffer[row] = string.Empty;
    }
}
