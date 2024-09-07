using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using ServicePoint;
using TanksServer.GameLogic;
using TanksServer.Graphics;
using CompressionCode = ServicePoint.BindGen.CompressionCode;

namespace TanksServer.Interactivity;

internal sealed class SendToServicePointDisplay : IFrameConsumer
{
    private const int ScoresWidth = 12;
    private const int ScoresHeight = 20;
    private const int ScoresPlayerRows = ScoresHeight - 6;

    private readonly Connection _displayConnection;
    private readonly MapService _mapService;
    private readonly ILogger<SendToServicePointDisplay> _logger;
    private readonly PlayerServer _players;
    private readonly Cp437Grid _scoresBuffer;
    private readonly TimeSpan _minFrameTime;
    private readonly IOptionsMonitor<HostConfiguration> _options;

    private DateTime _nextFailLogAfter = DateTime.Now;
    private DateTime _nextFrameAfter = DateTime.Now;

    public SendToServicePointDisplay(
        PlayerServer players,
        ILogger<SendToServicePointDisplay> logger,
        Connection displayConnection,
        IOptions<HostConfiguration> hostOptions,
        MapService mapService,
        IOptionsMonitor<HostConfiguration> options,
        IOptions<DisplayConfiguration> displayConfig)
    {
        _players = players;
        _logger = logger;
        _displayConnection = displayConnection;
        _mapService = mapService;
        _minFrameTime = TimeSpan.FromMilliseconds(hostOptions.Value.ServicePointDisplayMinFrameTimeMs);
        _options = options;

        var localIp = GetLocalIPv4(displayConfig.Value).Split('.');
        Debug.Assert(localIp.Length == 4);
        _scoresBuffer = Cp437Grid.New(12, 20);

        _scoresBuffer[00] = "== TANKS! ==";
        _scoresBuffer[01] = "-- scores --";
        _scoresBuffer[17] = "--  join  --";
        _scoresBuffer[18] = string.Join('.', localIp[..2]);
        _scoresBuffer[19] = string.Join('.', localIp[2..]);
    }

    public async Task OnFrameDoneAsync(GamePixelGrid gamePixelGrid, PixelGrid observerPixels)
    {
        if (!_options.CurrentValue.EnableServicePointDisplay)
            return;

        if (DateTime.Now < _nextFrameAfter)
            return;

        _nextFrameAfter = DateTime.Now + _minFrameTime;
        await Task.Yield();

        RefreshScores();

        try
        {
            _displayConnection.Send(Command.BitmapLinearWin(0, 0, observerPixels.Clone(), CompressionCode.Lzma)
                .IntoPacket());
            _displayConnection.Send(Command.Cp437Data(MapService.TilesPerRow, 0, _scoresBuffer.Clone()).IntoPacket());
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
        var playersToDisplay = _players.Players
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

    private static string GetLocalIPv4(DisplayConfiguration configuration)
    {
        using var socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, 0);
        socket.Connect(configuration.Hostname, configuration.Port);
        var endPoint = socket.LocalEndPoint as IPEndPoint ?? throw new NotSupportedException();
        return endPoint.Address.ToString();
    }
}
