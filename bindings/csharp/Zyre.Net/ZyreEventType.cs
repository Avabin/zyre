namespace Zyre.Net;

/// <summary>
/// Zyre event types that can be received from the network
/// </summary>
public enum ZyreEventType
{
    /// <summary>
    /// A peer has joined the network
    /// </summary>
    Enter,

    /// <summary>
    /// A peer has left the network
    /// </summary>
    Exit,

    /// <summary>
    /// A peer has joined a group
    /// </summary>
    Join,

    /// <summary>
    /// A peer has left a group
    /// </summary>
    Leave,

    /// <summary>
    /// A peer is being evasive (not responding to beacons)
    /// </summary>
    Evasive,

    /// <summary>
    /// A direct message (whisper) was received
    /// </summary>
    Whisper,

    /// <summary>
    /// A group message (shout) was received
    /// </summary>
    Shout,

    /// <summary>
    /// The local node has stopped
    /// </summary>
    Stop
}