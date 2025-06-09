using System.Runtime.InteropServices;

namespace Zyre.Net.Native;

/// <summary>
/// Native P/Invoke declarations for libzyre
/// </summary>
internal static partial class ZyreNative
{
    private const string LibraryName = "zyre";

    #region Core Zyre Functions

    /// <summary>
    /// Constructor, creates a new Zyre node. Note that until you start the
    /// node it is silent and invisible to other nodes on the network.
    /// The node name is provided to other nodes during discovery. If you
    /// specify NULL, Zyre generates a randomized node name from the UUID.
    /// </summary>
    [LibraryImport(LibraryName, StringMarshalling = StringMarshalling.Utf8)]
    internal static partial IntPtr zyre_new(string? name);

    /// <summary>
    /// Destructor, destroys a Zyre node. When you destroy a node, any
    /// messages it is sending or receiving will be discarded.
    /// </summary>
    [LibraryImport(LibraryName)]
    internal static partial void zyre_destroy(ref IntPtr self);

    /// <summary>
    /// Return our node UUID string, after successful initialization
    /// </summary>
    [LibraryImport(LibraryName, StringMarshalling = StringMarshalling.Utf8)]
    internal static partial string zyre_uuid(IntPtr self);

    /// <summary>
    /// Return our node name, after successful initialization. First 6
    /// characters of UUID by default.
    /// </summary>
    [LibraryImport(LibraryName, StringMarshalling = StringMarshalling.Utf8)]
    internal static partial string zyre_name(IntPtr self);

    /// <summary>
    /// Set the public name of this node overriding the default. The name is
    /// provided during discovery and come in each ENTER message.
    /// </summary>
    [LibraryImport(LibraryName, StringMarshalling = StringMarshalling.Utf8)]
    internal static partial void zyre_set_name(IntPtr self, string name);

    /// <summary>
    /// Set node header; these are provided to other nodes during discovery
    /// and come in each ENTER message.
    /// </summary>
    [LibraryImport(LibraryName, StringMarshalling = StringMarshalling.Utf8)]
    internal static partial void zyre_set_header(IntPtr self, string name, string value);

    /// <summary>
    /// Set verbose mode; this tells the node to log all traffic as well as
    /// all major events.
    /// </summary>
    [LibraryImport(LibraryName)]
    internal static partial void zyre_set_verbose(IntPtr self);

    /// <summary>
    /// Set UDP beacon discovery port; defaults to 5670, this call overrides
    /// that so you can create independent clusters on the same network, for
    /// e.g. development vs. production. Has no effect after zyre_start().
    /// </summary>
    [LibraryImport(LibraryName)]
    internal static partial void zyre_set_port(IntPtr self, int portNumber);

    /// <summary>
    /// Set the peer evasiveness timeout, in milliseconds. Default is 5000.
    /// </summary>
    [LibraryImport(LibraryName)]
    internal static partial void zyre_set_evasive_timeout(IntPtr self, int interval);

    /// <summary>
    /// Set the peer silence timeout, in milliseconds. Default is 5000.
    /// </summary>
    [LibraryImport(LibraryName)]
    internal static partial void zyre_set_silent_timeout(IntPtr self, int interval);

    /// <summary>
    /// Set the peer expiration timeout, in milliseconds. Default is 30000.
    /// </summary>
    [LibraryImport(LibraryName)]
    internal static partial void zyre_set_expired_timeout(IntPtr self, int interval);

    /// <summary>
    /// Set UDP beacon discovery interval, in milliseconds. Default is instant
    /// beacon exploration followed by pinging every 1,000 msecs.
    /// </summary>
    [LibraryImport(LibraryName)]
    internal static partial void zyre_set_interval(IntPtr self, UIntPtr interval);

    /// <summary>
    /// Set network interface for UDP beacons.
    /// </summary>
    [LibraryImport(LibraryName, StringMarshalling = StringMarshalling.Utf8)]
    internal static partial void zyre_set_interface(IntPtr self, string value);

    /// <summary>
    /// Start node, after setting header values. When you start a node it
    /// begins discovery and connection. Returns 0 if OK, -1 if it wasn't
    /// possible to start the node.
    /// </summary>
    [LibraryImport(LibraryName)]
    internal static partial int zyre_start(IntPtr self);

    /// <summary>
    /// Stop node; this signals to other peers that this node will go away.
    /// </summary>
    [LibraryImport(LibraryName)]
    internal static partial void zyre_stop(IntPtr self);

    /// <summary>
    /// Join a named group; after joining a group you can send messages to
    /// the group and all Zyre nodes in that group will receive them.
    /// </summary>
    [LibraryImport(LibraryName, StringMarshalling = StringMarshalling.Utf8)]
    internal static partial int zyre_join(IntPtr self, string group);

    /// <summary>
    /// Leave a group
    /// </summary>
    [LibraryImport(LibraryName, StringMarshalling = StringMarshalling.Utf8)]
    internal static partial int zyre_leave(IntPtr self, string group);

    /// <summary>
    /// Receive next message from network; the message may be a control
    /// message (ENTER, EXIT, JOIN, LEAVE) or data (WHISPER, SHOUT).
    /// Returns zmsg_t object, or NULL if interrupted
    /// </summary>
    [LibraryImport(LibraryName)]
    internal static partial IntPtr zyre_recv(IntPtr self);

    /// <summary>
    /// Send message to single peer, specified as a UUID string
    /// Destroys message after sending
    /// </summary>
    [LibraryImport(LibraryName, StringMarshalling = StringMarshalling.Utf8)]
    internal static partial int zyre_whisper(IntPtr self, string peer, ref IntPtr msg);

    /// <summary>
    /// Send message to a named group
    /// Destroys message after sending
    /// </summary>
    [LibraryImport(LibraryName, StringMarshalling = StringMarshalling.Utf8)]
    internal static partial int zyre_shout(IntPtr self, string group, ref IntPtr msg);

    #endregion

    #region Event Functions

    /// <summary>
    /// Constructor: receive an event from the zyre node, wraps zyre_recv.
    /// The event may be a control message (ENTER, EXIT, JOIN, LEAVE) or
    /// data (WHISPER, SHOUT).
    /// </summary>
    [LibraryImport(LibraryName)]
    internal static partial IntPtr zyre_event_new(IntPtr node);

    /// <summary>
    /// Destructor; destroys an event instance
    /// </summary>
    [LibraryImport(LibraryName)]
    internal static partial void zyre_event_destroy(ref IntPtr self);

    /// <summary>
    /// Returns event type, as printable uppercase string. Choices are:
    /// "ENTER", "EXIT", "JOIN", "LEAVE", "EVASIVE", "WHISPER" and "SHOUT"
    /// and for the local node: "STOP"
    /// </summary>
    [LibraryImport(LibraryName, StringMarshalling = StringMarshalling.Utf8)]
    internal static partial string zyre_event_type(IntPtr self);

    /// <summary>
    /// Return the sending peer's uuid as a string
    /// </summary>
    [LibraryImport(LibraryName, StringMarshalling = StringMarshalling.Utf8)]
    internal static partial string zyre_event_peer_uuid(IntPtr self);

    /// <summary>
    /// Return the sending peer's public name as a string
    /// </summary>
    [LibraryImport(LibraryName, StringMarshalling = StringMarshalling.Utf8)]
    internal static partial string zyre_event_peer_name(IntPtr self);

    /// <summary>
    /// Return the sending peer's ipaddress as a string
    /// </summary>
    [LibraryImport(LibraryName, StringMarshalling = StringMarshalling.Utf8)]
    internal static partial string zyre_event_peer_addr(IntPtr self);

    /// <summary>
    /// Returns value of a header from the message headers
    /// obtained by ENTER. Return NULL if no value was found.
    /// </summary>
    [LibraryImport(LibraryName, StringMarshalling = StringMarshalling.Utf8)]
    internal static partial string? zyre_event_header(IntPtr self, string name);

    /// <summary>
    /// Returns the group name that a SHOUT event was sent to
    /// </summary>
    [LibraryImport(LibraryName, StringMarshalling = StringMarshalling.Utf8)]
    internal static partial string zyre_event_group(IntPtr self);

    /// <summary>
    /// Returns the incoming message payload; the caller can modify the
    /// message but does not own it and should not destroy it.
    /// </summary>
    [LibraryImport(LibraryName)]
    internal static partial IntPtr zyre_event_msg(IntPtr self);

    /// <summary>
    /// Returns the incoming message payload, and pass ownership to the
    /// caller. The caller must destroy the message when finished with it.
    /// After called on the given event, further calls will return NULL.
    /// </summary>
    [LibraryImport(LibraryName)]
    internal static partial IntPtr zyre_event_get_msg(IntPtr self);

    /// <summary>
    /// Print event to zsys log
    /// </summary>
    [LibraryImport(LibraryName)]
    internal static partial void zyre_event_print(IntPtr self);

    #endregion

    #region ZMQ Message Functions (from czmq)

    /// <summary>
    /// Create a new empty message
    /// </summary>
    [LibraryImport("czmq")]
    internal static partial IntPtr zmsg_new();

    /// <summary>
    /// Destroy a message
    /// </summary>
    [LibraryImport("czmq")]
    internal static partial void zmsg_destroy(ref IntPtr self);

    /// <summary>
    /// Add string frame to message
    /// </summary>
    [LibraryImport("czmq", StringMarshalling = StringMarshalling.Utf8)]
    internal static partial int zmsg_addstr(IntPtr self, string str);

    /// <summary>
    /// Pop first frame off message as string
    /// </summary>
    [LibraryImport("czmq", StringMarshalling = StringMarshalling.Utf8)]
    internal static partial string? zmsg_popstr(IntPtr self);

    /// <summary>
    /// Get message size (number of frames)
    /// </summary>
    [LibraryImport("czmq")]
    internal static partial UIntPtr zmsg_size(IntPtr self);

    #endregion
}