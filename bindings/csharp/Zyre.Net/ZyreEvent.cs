using System.Collections.ObjectModel;
using Zyre.Net.Native;

namespace Zyre.Net;

/// <summary>
/// Represents an event received from the Zyre network
/// </summary>
public sealed class ZyreEvent : IDisposable
{
    private IntPtr _handle;
    private bool _disposed;
    private ZyreEventType? _eventType;
    private string? _peerUuid;
    private string? _peerName;
    private string? _peerAddress;
    private string? _group;
    private ZyreMessage? _message;
    private IReadOnlyDictionary<string, string>? _headers;

    internal ZyreEvent(IntPtr handle)
    {
        _handle = handle;
    }

    /// <summary>
    /// Gets the type of this event
    /// </summary>
    public ZyreEventType EventType
    {
        get
        {
            ThrowIfDisposed();
            if (_eventType.HasValue)
                return _eventType.Value;

            string typeStr = ZyreNative.zyre_event_type(_handle);
            _eventType = typeStr switch
            {
                "ENTER" => ZyreEventType.Enter,
                "EXIT" => ZyreEventType.Exit,
                "JOIN" => ZyreEventType.Join,
                "LEAVE" => ZyreEventType.Leave,
                "EVASIVE" => ZyreEventType.Evasive,
                "WHISPER" => ZyreEventType.Whisper,
                "SHOUT" => ZyreEventType.Shout,
                "STOP" => ZyreEventType.Stop,
                _ => throw new InvalidOperationException($"Unknown event type: {typeStr}")
            };

            return _eventType.Value;
        }
    }

    /// <summary>
    /// Gets the UUID of the peer that generated this event
    /// </summary>
    public string PeerUuid
    {
        get
        {
            ThrowIfDisposed();
            return _peerUuid ??= ZyreNative.zyre_event_peer_uuid(_handle) ?? string.Empty;
        }
    }

    /// <summary>
    /// Gets the name of the peer that generated this event
    /// </summary>
    public string PeerName
    {
        get
        {
            ThrowIfDisposed();
            return _peerName ??= ZyreNative.zyre_event_peer_name(_handle) ?? string.Empty;
        }
    }

    /// <summary>
    /// Gets the IP address of the peer that generated this event
    /// </summary>
    public string PeerAddress
    {
        get
        {
            ThrowIfDisposed();
            return _peerAddress ??= ZyreNative.zyre_event_peer_addr(_handle) ?? string.Empty;
        }
    }

    /// <summary>
    /// Gets the group name for JOIN, LEAVE, and SHOUT events
    /// </summary>
    public string? Group
    {
        get
        {
            ThrowIfDisposed();
            if (_group == null && (EventType == ZyreEventType.Join || 
                                   EventType == ZyreEventType.Leave || 
                                   EventType == ZyreEventType.Shout))
            {
                _group = ZyreNative.zyre_event_group(_handle);
            }
            return _group;
        }
    }

    /// <summary>
    /// Gets the message payload for WHISPER and SHOUT events
    /// </summary>
    public ZyreMessage? Message
    {
        get
        {
            ThrowIfDisposed();
            if (_message == null && (EventType == ZyreEventType.Whisper || EventType == ZyreEventType.Shout))
            {
                var msgHandle = ZyreNative.zyre_event_get_msg(_handle);
                if (msgHandle != IntPtr.Zero)
                {
                    _message = new ZyreMessage(msgHandle);
                }
            }
            return _message;
        }
    }

    /// <summary>
    /// Gets a header value by name for ENTER events
    /// </summary>
    /// <param name="name">The header name</param>
    /// <returns>The header value, or null if not found</returns>
    public string? GetHeader(string name)
    {
        ThrowIfDisposed();
        ArgumentNullException.ThrowIfNull(name);
        
        return ZyreNative.zyre_event_header(_handle, name);
    }

    /// <summary>
    /// Gets all headers for ENTER events
    /// </summary>
    public IReadOnlyDictionary<string, string> Headers
    {
        get
        {
            ThrowIfDisposed();
            if (_headers == null)
            {
                var headers = new Dictionary<string, string>();
                // Note: In a full implementation, you'd enumerate the zhash_t
                // For now, this is a placeholder
                _headers = new ReadOnlyDictionary<string, string>(headers);
            }
            return _headers;
        }
    }

    /// <summary>
    /// Prints the event to the system log
    /// </summary>
    public void Print()
    {
        ThrowIfDisposed();
        ZyreNative.zyre_event_print(_handle);
    }

    private void ThrowIfDisposed()
    {
        if (_disposed)
            throw new ObjectDisposedException(nameof(ZyreEvent));
    }

    /// <summary>
    /// Releases all resources used by this ZyreEvent instance
    /// </summary>
    public void Dispose()
    {
        if (!_disposed && _handle != IntPtr.Zero)
        {
            _message?.Dispose();
            ZyreNative.zyre_event_destroy(ref _handle);
            _disposed = true;
        }
    }
}