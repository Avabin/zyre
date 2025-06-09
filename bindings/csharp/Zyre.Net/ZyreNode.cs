using System.Collections.Concurrent;
using Zyre.Net.Native;

namespace Zyre.Net;

/// <summary>
/// Represents a Zyre node for peer-to-peer communication
/// </summary>
public sealed class ZyreNode : IDisposable
{
    private IntPtr _handle;
    private bool _disposed;
    private bool _started;
    private readonly ConcurrentDictionary<string, string> _headers = new();

    /// <summary>
    /// Creates a new Zyre node with an optional name
    /// </summary>
    /// <param name="name">The node name (null for auto-generated)</param>
    public ZyreNode(string? name = null)
    {
        _handle = ZyreNative.zyre_new(name);
        if (_handle == IntPtr.Zero)
            throw new OutOfMemoryException("Failed to create Zyre node");
    }

    /// <summary>
    /// Gets the UUID of this node
    /// </summary>
    public string Uuid
    {
        get
        {
            ThrowIfDisposed();
            return ZyreNative.zyre_uuid(_handle);
        }
    }

    /// <summary>
    /// Gets the name of this node
    /// </summary>
    public string Name
    {
        get
        {
            ThrowIfDisposed();
            return ZyreNative.zyre_name(_handle);
        }
    }

    /// <summary>
    /// Gets whether this node has been started
    /// </summary>
    public bool IsStarted => _started;

    /// <summary>
    /// Sets the public name of this node
    /// </summary>
    /// <param name="name">The new name</param>
    public void SetName(string name)
    {
        ThrowIfDisposed();
        ArgumentNullException.ThrowIfNull(name);
        
        ZyreNative.zyre_set_name(_handle, name);
    }

    /// <summary>
    /// Sets a header value for this node
    /// </summary>
    /// <param name="name">The header name</param>
    /// <param name="value">The header value</param>
    public void SetHeader(string name, string value)
    {
        ThrowIfDisposed();
        ArgumentNullException.ThrowIfNull(name);
        ArgumentNullException.ThrowIfNull(value);

        ZyreNative.zyre_set_header(_handle, name, value);
        _headers[name] = value;
    }

    /// <summary>
    /// Enables verbose logging for this node
    /// </summary>
    public void SetVerbose()
    {
        ThrowIfDisposed();
        ZyreNative.zyre_set_verbose(_handle);
    }

    /// <summary>
    /// Sets the UDP beacon discovery port
    /// </summary>
    /// <param name="port">The port number</param>
    public void SetPort(int port)
    {
        ThrowIfDisposed();
        if (port <= 0 || port > 65535)
            throw new ArgumentOutOfRangeException(nameof(port), "Port must be between 1 and 65535");

        ZyreNative.zyre_set_port(_handle, port);
    }

    /// <summary>
    /// Sets the peer evasiveness timeout in milliseconds
    /// </summary>
    /// <param name="timeout">The timeout in milliseconds</param>
    public void SetEvasiveTimeout(TimeSpan timeout)
    {
        ThrowIfDisposed();
        if (timeout < TimeSpan.Zero)
            throw new ArgumentOutOfRangeException(nameof(timeout), "Timeout cannot be negative");

        ZyreNative.zyre_set_evasive_timeout(_handle, (int)timeout.TotalMilliseconds);
    }

    /// <summary>
    /// Sets the peer silence timeout in milliseconds
    /// </summary>
    /// <param name="timeout">The timeout in milliseconds</param>
    public void SetSilentTimeout(TimeSpan timeout)
    {
        ThrowIfDisposed();
        if (timeout < TimeSpan.Zero)
            throw new ArgumentOutOfRangeException(nameof(timeout), "Timeout cannot be negative");

        ZyreNative.zyre_set_silent_timeout(_handle, (int)timeout.TotalMilliseconds);
    }

    /// <summary>
    /// Sets the peer expiration timeout in milliseconds
    /// </summary>
    /// <param name="timeout">The timeout in milliseconds</param>
    public void SetExpiredTimeout(TimeSpan timeout)
    {
        ThrowIfDisposed();
        if (timeout < TimeSpan.Zero)
            throw new ArgumentOutOfRangeException(nameof(timeout), "Timeout cannot be negative");

        ZyreNative.zyre_set_expired_timeout(_handle, (int)timeout.TotalMilliseconds);
    }

    /// <summary>
    /// Sets the UDP beacon discovery interval
    /// </summary>
    /// <param name="interval">The interval in milliseconds</param>
    public void SetInterval(TimeSpan interval)
    {
        ThrowIfDisposed();
        if (interval < TimeSpan.Zero)
            throw new ArgumentOutOfRangeException(nameof(interval), "Interval cannot be negative");

        ZyreNative.zyre_set_interval(_handle, (UIntPtr)(ulong)interval.TotalMilliseconds);
    }

    /// <summary>
    /// Sets the network interface for UDP beacons
    /// </summary>
    /// <param name="interfaceName">The interface name or IP address</param>
    public void SetInterface(string interfaceName)
    {
        ThrowIfDisposed();
        ArgumentNullException.ThrowIfNull(interfaceName);

        ZyreNative.zyre_set_interface(_handle, interfaceName);
    }

    /// <summary>
    /// Starts the node and begins discovery
    /// </summary>
    /// <returns>True if started successfully, false otherwise</returns>
    public bool Start()
    {
        ThrowIfDisposed();
        
        int result = ZyreNative.zyre_start(_handle);
        _started = result == 0;
        return _started;
    }

    /// <summary>
    /// Stops the node
    /// </summary>
    public void Stop()
    {
        ThrowIfDisposed();
        
        if (_started)
        {
            ZyreNative.zyre_stop(_handle);
            _started = false;
        }
    }

    /// <summary>
    /// Joins a named group
    /// </summary>
    /// <param name="group">The group name</param>
    /// <returns>True if joined successfully, false otherwise</returns>
    public bool Join(string group)
    {
        ThrowIfDisposed();
        ArgumentNullException.ThrowIfNull(group);

        int result = ZyreNative.zyre_join(_handle, group);
        return result == 0;
    }

    /// <summary>
    /// Leaves a named group
    /// </summary>
    /// <param name="group">The group name</param>
    /// <returns>True if left successfully, false otherwise</returns>
    public bool Leave(string group)
    {
        ThrowIfDisposed();
        ArgumentNullException.ThrowIfNull(group);

        int result = ZyreNative.zyre_leave(_handle, group);
        return result == 0;
    }

    /// <summary>
    /// Receives the next event from the network
    /// </summary>
    /// <returns>The received event, or null if interrupted</returns>
    public ZyreEvent? ReceiveEvent()
    {
        ThrowIfDisposed();

        var eventHandle = ZyreNative.zyre_event_new(_handle);
        return eventHandle != IntPtr.Zero ? new ZyreEvent(eventHandle) : null;
    }

    /// <summary>
    /// Sends a message to a specific peer (whisper)
    /// </summary>
    /// <param name="peerUuid">The UUID of the target peer</param>
    /// <param name="message">The message to send</param>
    /// <returns>True if sent successfully, false otherwise</returns>
    public bool Whisper(string peerUuid, ZyreMessage message)
    {
        ThrowIfDisposed();
        ArgumentNullException.ThrowIfNull(peerUuid);
        ArgumentNullException.ThrowIfNull(message);

        var msgHandle = message.TakeHandle();
        int result = ZyreNative.zyre_whisper(_handle, peerUuid, ref msgHandle);
        return result == 0;
    }

    /// <summary>
    /// Sends a message to a group (shout)
    /// </summary>
    /// <param name="group">The target group name</param>
    /// <param name="message">The message to send</param>
    /// <returns>True if sent successfully, false otherwise</returns>
    public bool Shout(string group, ZyreMessage message)
    {
        ThrowIfDisposed();
        ArgumentNullException.ThrowIfNull(group);
        ArgumentNullException.ThrowIfNull(message);

        var msgHandle = message.TakeHandle();
        int result = ZyreNative.zyre_shout(_handle, group, ref msgHandle);
        return result == 0;
    }

    /// <summary>
    /// Creates a simple message with a single string frame
    /// </summary>
    /// <param name="content">The message content</param>
    /// <returns>A new message</returns>
    public static ZyreMessage CreateMessage(string content)
    {
        var message = new ZyreMessage();
        message.AddString(content);
        return message;
    }

    /// <summary>
    /// Creates a message with multiple string frames
    /// </summary>
    /// <param name="frames">The message frames</param>
    /// <returns>A new message</returns>
    public static ZyreMessage CreateMessage(params string[] frames)
    {
        var message = new ZyreMessage();
        foreach (var frame in frames)
        {
            message.AddString(frame);
        }
        return message;
    }

    private void ThrowIfDisposed()
    {
        if (_disposed)
            throw new ObjectDisposedException(nameof(ZyreNode));
    }

    /// <summary>
    /// Releases all resources used by this ZyreNode instance
    /// </summary>
    public void Dispose()
    {
        if (!_disposed && _handle != IntPtr.Zero)
        {
            if (_started)
            {
                Stop();
            }
            
            ZyreNative.zyre_destroy(ref _handle);
            _disposed = true;
        }
    }
}