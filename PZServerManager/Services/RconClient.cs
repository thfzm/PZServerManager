using System.Net.Sockets;
using System.Text;

namespace PZServerManager.Services;

/// Source RCON protocol client.
/// Detects end-of-response for multi-packet output by sending an empty SERVERDATA_RESPONSE_VALUE
/// after each EXECCOMMAND — the server echoes that empty packet back, marking the boundary
/// (Valve's documented trick).
public sealed class RconClient : IDisposable
{
    private const int SERVERDATA_AUTH = 3;
    private const int SERVERDATA_AUTH_RESPONSE = 2;
    private const int SERVERDATA_EXECCOMMAND = 2;
    private const int SERVERDATA_RESPONSE_VALUE = 0;

    private TcpClient? _tcp;
    private NetworkStream? _stream;
    private int _nextId = 1;
    private readonly SemaphoreSlim _gate = new(1, 1);

    public bool IsConnected => _tcp?.Connected == true;
    public string? Host { get; private set; }
    public int Port { get; private set; }

    public event Action<bool>? ConnectionChanged;

    public async Task ConnectAsync(string host, int port, string password, TimeSpan timeout, CancellationToken ct)
    {
        Disconnect();
        var tcp = new TcpClient();
        try
        {
            using var linked = CancellationTokenSource.CreateLinkedTokenSource(ct);
            linked.CancelAfter(timeout);
            await tcp.ConnectAsync(host, port, linked.Token);
        }
        catch
        {
            tcp.Dispose();
            throw;
        }

        _tcp = tcp;
        _stream = tcp.GetStream();
        Host = host;
        Port = port;

        var authId = NextId();
        await SendPacketAsync(authId, SERVERDATA_AUTH, password, ct);

        // Server may send a stray empty RESPONSE_VALUE before the AUTH_RESPONSE.
        while (true)
        {
            var (id, type, _) = await ReadPacketAsync(ct);
            if (type == SERVERDATA_AUTH_RESPONSE)
            {
                if (id == -1)
                {
                    Disconnect();
                    throw new UnauthorizedAccessException("RCON 인증 실패 (비밀번호 틀림).");
                }
                ConnectionChanged?.Invoke(true);
                return;
            }
        }
    }

    public async Task<string> ExecuteAsync(string command, CancellationToken ct)
    {
        if (!IsConnected) throw new InvalidOperationException("연결되지 않았습니다.");

        await _gate.WaitAsync(ct);
        try
        {
            var cmdId = NextId();
            await SendPacketAsync(cmdId, SERVERDATA_EXECCOMMAND, command, ct);

            var sentinelId = NextId();
            await SendPacketAsync(sentinelId, SERVERDATA_RESPONSE_VALUE, "", ct);

            var sb = new StringBuilder();
            while (true)
            {
                var (id, _, body) = await ReadPacketAsync(ct);
                if (id == sentinelId) return sb.ToString();
                if (id == cmdId) sb.Append(body);
            }
        }
        finally
        {
            _gate.Release();
        }
    }

    private async Task SendPacketAsync(int id, int type, string body, CancellationToken ct)
    {
        if (_stream is null) throw new InvalidOperationException("연결되지 않았습니다.");
        var bodyBytes = Encoding.ASCII.GetBytes(body);
        var size = 4 + 4 + bodyBytes.Length + 2;
        var buf = new byte[4 + size];
        BitConverter.TryWriteBytes(buf.AsSpan(0, 4), size);
        BitConverter.TryWriteBytes(buf.AsSpan(4, 4), id);
        BitConverter.TryWriteBytes(buf.AsSpan(8, 4), type);
        Array.Copy(bodyBytes, 0, buf, 12, bodyBytes.Length);
        buf[12 + bodyBytes.Length] = 0;
        buf[12 + bodyBytes.Length + 1] = 0;
        await _stream.WriteAsync(buf.AsMemory(), ct);
        await _stream.FlushAsync(ct);
    }

    private async Task<(int id, int type, string body)> ReadPacketAsync(CancellationToken ct)
    {
        if (_stream is null) throw new InvalidOperationException("연결되지 않았습니다.");
        var sizeBuf = new byte[4];
        await _stream.ReadExactlyAsync(sizeBuf, ct);
        var size = BitConverter.ToInt32(sizeBuf, 0);
        if (size < 10 || size > 4 * 4096)
            throw new InvalidDataException($"Invalid RCON packet size: {size}");
        var buf = new byte[size];
        await _stream.ReadExactlyAsync(buf, ct);
        var id = BitConverter.ToInt32(buf, 0);
        var type = BitConverter.ToInt32(buf, 4);
        var bodyEnd = Array.IndexOf(buf, (byte)0, 8);
        if (bodyEnd < 0) bodyEnd = size - 2;
        var body = Encoding.UTF8.GetString(buf, 8, bodyEnd - 8);
        return (id, type, body);
    }

    private int NextId()
    {
        if (_nextId == int.MaxValue) _nextId = 1;
        return _nextId++;
    }

    public void Disconnect()
    {
        var was = IsConnected;
        try { _stream?.Dispose(); } catch { }
        try { _tcp?.Dispose(); } catch { }
        _stream = null;
        _tcp = null;
        if (was) ConnectionChanged?.Invoke(false);
    }

    public void Dispose() => Disconnect();
}
