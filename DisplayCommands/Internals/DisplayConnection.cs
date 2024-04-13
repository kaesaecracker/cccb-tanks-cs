using System.Buffers;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using Microsoft.Extensions.Options;

namespace DisplayCommands.Internals;

internal sealed class DisplayConnection(IOptions<DisplayConfiguration> options) : IDisplayConnection, IDisposable
{
    private readonly ArrayPool<byte> _arrayPool = ArrayPool<byte>.Shared;
    private readonly UdpClient _udpClient = new(options.Value.Hostname, options.Value.Port);

    public ValueTask SendClearAsync()
    {
        var header = new HeaderWindow { Command = DisplayCommand.Clear };
        return SendAsync(header, Memory<byte>.Empty);
    }

    public ValueTask SendCp437DataAsync(ushort x, ushort y, Cp437Grid grid)
    {
        var header = new HeaderWindow
        {
            Command = DisplayCommand.Cp437Data,
            Height = grid.Height,
            Width = grid.Width,
            PosX = x,
            PosY = y
        };
        return SendAsync(header, grid.Data);
    }

    public ValueTask SendCharBrightnessAsync(ushort x, ushort y, ByteGrid luma)
    {
        var header = new HeaderWindow
        {
            Command = DisplayCommand.CharBrightness,
            PosX = x,
            PosY = y,
            Height = luma.Height,
            Width = luma.Width
        };

        return SendAsync(header, luma.Data);
    }

    public async ValueTask SendBrightnessAsync(byte brightness)
    {
        var header = new HeaderWindow { Command = DisplayCommand.Brightness };

        var payloadBuffer = _arrayPool.Rent(1);
        var payload = payloadBuffer.AsMemory(0, 1);
        payload.Span[0] = brightness;

        await SendAsync(header, payload);
        _arrayPool.Return(payloadBuffer);
    }

    public ValueTask SendHardResetAsync()
    {
        var header = new HeaderWindow { Command = DisplayCommand.HardReset };
        return SendAsync(header, Memory<byte>.Empty);
    }

    public async ValueTask SendFadeOutAsync(byte loops)
    {
        var header = new HeaderWindow { Command = DisplayCommand.FadeOut };

        var payloadBuffer = _arrayPool.Rent(1);
        var payload = payloadBuffer.AsMemory(0, 1);
        payload.Span[0] = loops;

        await SendAsync(header, payload);
        _arrayPool.Return(payloadBuffer);
    }

    public ValueTask SendBitmapLinearWindowAsync(ushort x, ushort y, PixelGrid pixels)
    {
        var header = new HeaderWindow
        {
            Command = DisplayCommand.BitmapLinearWin,
            PosX = x, PosY = y,
            Width = pixels.Width,
            Height = pixels.Height
        };

        return SendAsync(header, pixels.Data);
    }

    public string GetLocalIPv4()
    {
        using var socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, 0);
        socket.Connect(options.Value.Hostname, options.Value.Port);
        var endPoint = socket.LocalEndPoint as IPEndPoint ?? throw new NotSupportedException();
        return endPoint.Address.ToString();
    }

    private async ValueTask SendAsync(HeaderWindow header, Memory<byte> payload)
    {
        int headerSize;
        unsafe
        {
            // because we specified the struct layout, no platform-specific padding will be added and this is be safe.
            headerSize = sizeof(HeaderWindow);
        }

        Debug.Assert(headerSize == 10);
        var messageSize = headerSize + payload.Length;

        var buffer = _arrayPool.Rent(messageSize);
        var message = buffer.AsMemory(0, messageSize);

        MemoryMarshal.Write(message.Span, header);
        payload.CopyTo(message[headerSize..]);

        await _udpClient.SendAsync(message);

        _arrayPool.Return(buffer);
    }

    public void Dispose() => _udpClient.Dispose();
}
