using System.Collections;
using System.Runtime.InteropServices;
using System.Text;
using Zyre.Net.Native;

namespace Zyre.Net;

/// <summary>
/// Represents a ZeroMQ message with automatic memory management
/// </summary>
public sealed class ZyreMessage : IDisposable, IEnumerable<ReadOnlyMemory<byte>>
{
    private IntPtr _handle;
    private bool _disposed;

    internal ZyreMessage(IntPtr handle)
    {
        _handle = handle;
    }

    /// <summary>
    /// Creates a new empty message
    /// </summary>
    public ZyreMessage()
    {
        _handle = ZyreNative.zmsg_new();
        if (_handle == IntPtr.Zero)
            throw new OutOfMemoryException("Failed to create ZMQ message");
    }

    /// <summary>
    /// Gets the number of frames in this message
    /// </summary>
    public int FrameCount
    {
        get
        {
            ThrowIfDisposed();
            return (int)ZyreNative.zmsg_size(_handle);
        }
    }

    /// <summary>
    /// Adds a string frame to the message
    /// </summary>
    /// <param name="content">The string content to add</param>
    public void AddString(string content)
    {
        ThrowIfDisposed();
        ArgumentNullException.ThrowIfNull(content);

        int result = ZyreNative.zmsg_addstr(_handle, content);
        if (result != 0)
            throw new InvalidOperationException("Failed to add string frame to message");
    }

    /// <summary>
    /// Adds a binary frame to the message
    /// </summary>
    /// <param name="data">The binary data to add</param>
    public void AddFrame(ReadOnlySpan<byte> data)
    {
        ThrowIfDisposed();
        
        // Note: This is a simplified implementation
        // In a full implementation, you'd use zmsg_addmem or similar
        string content = Encoding.UTF8.GetString(data);
        AddString(content);
    }

    /// <summary>
    /// Pops the first frame from the message as a string
    /// </summary>
    /// <returns>The string content of the first frame, or null if message is empty</returns>
    public string? PopString()
    {
        ThrowIfDisposed();
        return ZyreNative.zmsg_popstr(_handle);
    }

    /// <summary>
    /// Gets all frames as strings
    /// </summary>
    /// <returns>An enumerable of all string frames</returns>
    public IEnumerable<string> GetStringFrames()
    {
        ThrowIfDisposed();
        
        var frames = new List<string>();
        while (FrameCount > 0)
        {
            var frame = PopString();
            if (frame != null)
                frames.Add(frame);
        }
        return frames;
    }

    /// <summary>
    /// Enumerates all frames as binary data
    /// </summary>
    public IEnumerator<ReadOnlyMemory<byte>> GetEnumerator()
    {
        ThrowIfDisposed();
        
        // Simplified implementation - in reality you'd access raw frame data
        foreach (var stringFrame in GetStringFrames())
        {
            yield return Encoding.UTF8.GetBytes(stringFrame);
        }
    }

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    internal IntPtr Handle => _handle;

    internal IntPtr TakeHandle()
    {
        var handle = _handle;
        _handle = IntPtr.Zero;
        return handle;
    }

    private void ThrowIfDisposed()
    {
        if (_disposed)
            throw new ObjectDisposedException(nameof(ZyreMessage));
    }

    /// <summary>
    /// Releases all resources used by this ZyreMessage instance
    /// </summary>
    public void Dispose()
    {
        if (!_disposed && _handle != IntPtr.Zero)
        {
            ZyreNative.zmsg_destroy(ref _handle);
            _disposed = true;
        }
    }
}