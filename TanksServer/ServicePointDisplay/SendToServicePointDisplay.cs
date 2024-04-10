using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using TanksServer.GameLogic;
using TanksServer.Graphics;
using TanksServer.Interactivity;

namespace TanksServer.ServicePointDisplay;

internal sealed class SendToServicePointDisplay : ITickStep, IDisposable
{
    private readonly UdpClient? _udpClient;
    private readonly LastFinishedFrameProvider _lastFinishedFrameProvider;
    private readonly TextDisplayBuffer _scoresBuffer;
    private readonly PlayerServer _players;
    private readonly ILogger<SendToServicePointDisplay> _logger;
    private DateTime _nextFailLog = DateTime.Now;

    private const int ScoresWidth = 12;
    private const int ScoresHeight = 20;
    private const int ScoresPlayerRows = ScoresHeight - 5;

    public SendToServicePointDisplay(
        IOptions<ServicePointDisplayConfiguration> options,
        LastFinishedFrameProvider lastFinishedFrameProvider,
        PlayerServer players,
        ILogger<SendToServicePointDisplay> logger
    )
    {
        _lastFinishedFrameProvider = lastFinishedFrameProvider;
        _players = players;
        _logger = logger;
        _udpClient = options.Value.Enable
            ? new UdpClient(options.Value.Hostname, options.Value.Port)
            : null;

        var localIp = GetLocalIp(options.Value.Hostname, options.Value.Port).Split('.');
        Debug.Assert(localIp.Length == 4); // were talking legacy ip
        _scoresBuffer = new TextDisplayBuffer(new TilePosition(MapService.TilesPerRow, 0), 12, 20)
        {
            Rows =
            {
                [0] = "== TANKS! ==",
                [1] = "-- scores --",
                [17] = "--  join  --",
                [18] = string.Join('.', localIp[..2]),
                [19] = string.Join('.', localIp[2..])
            }
        };
    }

    private static string GetLocalIp(string host, int port)
    {
        using var socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, 0);
        socket.Connect(host, port);
        var endPoint = socket.LocalEndPoint as IPEndPoint ?? throw new NotSupportedException();
        return endPoint.Address.ToString();
    }

    public Task TickAsync()
    {
        return _udpClient == null ? Task.CompletedTask : Core();

        async Task Core()
        {
            RefreshScores();
            try
            {
                await _udpClient.SendAsync(_scoresBuffer.Data);
                await _udpClient.SendAsync(_lastFinishedFrameProvider.LastFrame.Data);
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
    }

    private void RefreshScores()
    {
        var playersToDisplay = _players.GetAll()
            .OrderByDescending(p => p.Kills)
            .Take(ScoresPlayerRows);

        var row = 2;
        foreach (var p in playersToDisplay)
        {
            var score = p.Kills.ToString();
            var nameLength = ScoresWidth - score.Length;

            var name = p.Name[..nameLength];
            var spaces = new string(' ', nameLength - name.Length + 1);

            _scoresBuffer.Rows[row] = name + spaces + score;
            row++;
        }

        for (; row < 17; row++)
            _scoresBuffer.Rows[row] = string.Empty;
    }

    public void Dispose()
    {
        _udpClient?.Dispose();
    }
}