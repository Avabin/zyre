# Zyre.Net - C# Bindings for Zyre

This directory contains C# bindings for [Zyre](http://zyre.org), an open-source framework for proximity-based peer-to-peer applications.

## Features

- **Modern .NET**: Built for .NET 8+ with C# 12 language features
- **Memory Safe**: Uses `Span<T>`, `Memory<T>`, and other high-performance patterns
- **Native Interop**: Direct P/Invoke bindings to libzyre with automatic marshalling
- **RAII**: Automatic resource management with `IDisposable` pattern
- **Type Safe**: Strong typing for event types, timeouts, and message handling

## Quick Start

### Basic Usage

```csharp
using Zyre.Net;

// Create a new Zyre node
using var node = new ZyreNode("MyNode");

// Set some headers
node.SetHeader("app", "myapp");
node.SetHeader("version", "1.0");

// Start the node
if (!node.Start())
{
    Console.WriteLine("Failed to start node");
    return;
}

// Join a group
node.Join("CHAT");

// Send a message to the group
using var message = ZyreNode.CreateMessage("Hello, World!");
node.Shout("CHAT", message);

// Listen for events
while (true)
{
    using var @event = node.ReceiveEvent();
    if (@event == null) break;

    switch (@event.EventType)
    {
        case ZyreEventType.Enter:
            Console.WriteLine($"Peer {@@event.PeerName} entered");
            break;
        case ZyreEventType.Shout:
            Console.WriteLine($"Message from {@@event.PeerName}: {@@event.Message?.PopString()}");
            break;
    }
}
```

### Event Handling

```csharp
using var node = new ZyreNode();
node.Start();

while (true)
{
    using var @event = node.ReceiveEvent();
    if (@event == null) break;

    Console.WriteLine($"Event: {@event.EventType}");
    Console.WriteLine($"Peer: {@event.PeerName} ({@event.PeerUuid})");
    
    if (@event.EventType == ZyreEventType.Enter)
    {
        // Access peer headers
        foreach (var header in @event.Headers)
        {
            Console.WriteLine($"  {header.Key}: {header.Value}");
        }
    }
    
    if (@event.Message != null)
    {
        Console.WriteLine($"Message frames: {string.Join(", ", @event.Message.GetStringFrames())}");
    }
}
```

## Building

### Prerequisites

- .NET 8 SDK or later
- Native Zyre library (libzyre) and its dependencies:
  - libzmq (ZeroMQ)
  - czmq (ZeroMQ high-level C API)
  - libsodium (for security)

### Build Commands

```bash
# Build the solution
dotnet build

# Run tests
dotnet test

# Create NuGet package
dotnet pack
```

## Installation

### Installing Native Dependencies

#### Ubuntu/Debian
```bash
sudo apt-get install libzyre-dev libzmq3-dev libczmq-dev
```

#### macOS (with Homebrew)
```bash
brew install zyre zeromq czmq
```

#### Windows
Use vcpkg or build from source.

### Using the C# Library

Add the project reference or NuGet package to your project:

```xml
<PackageReference Include="Zyre.Net" Version="1.0.0" />
```

## API Reference

### ZyreNode

The main class representing a Zyre network node.

#### Properties
- `Uuid`: Gets the unique identifier of this node
- `Name`: Gets the display name of this node  
- `IsStarted`: Gets whether the node has been started

#### Methods
- `SetName(string name)`: Sets the display name
- `SetHeader(string name, string value)`: Sets a metadata header
- `SetPort(int port)`: Sets the UDP discovery port
- `Start()`: Starts network discovery and communication
- `Stop()`: Stops the node
- `Join(string group)`: Joins a named group
- `Leave(string group)`: Leaves a named group
- `Whisper(string peer, ZyreMessage message)`: Sends a direct message
- `Shout(string group, ZyreMessage message)`: Sends a group message
- `ReceiveEvent()`: Waits for and returns the next network event

### ZyreEvent

Represents an event received from the network.

#### Properties
- `EventType`: The type of event (Enter, Exit, Join, Leave, etc.)
- `PeerUuid`: UUID of the peer that triggered the event
- `PeerName`: Display name of the peer
- `PeerAddress`: IP address of the peer
- `Group`: Group name (for group-related events)
- `Message`: Message payload (for message events)
- `Headers`: Metadata headers (for Enter events)

### ZyreMessage

Represents a multi-frame message.

#### Methods
- `AddString(string content)`: Adds a string frame
- `AddFrame(ReadOnlySpan<byte> data)`: Adds a binary frame
- `PopString()`: Removes and returns the first frame as string
- `GetStringFrames()`: Gets all frames as strings

## Error Handling

The library uses standard .NET exception patterns:

- `ArgumentNullException`: For null arguments
- `ArgumentOutOfRangeException`: For invalid values
- `ObjectDisposedException`: When using disposed objects
- `DllNotFoundException`: When native libraries are not found
- `OutOfMemoryException`: When native allocation fails

## Thread Safety

The Zyre.Net library is **not thread-safe**. Each `ZyreNode` instance should only be used from a single thread. For multi-threaded applications, use separate node instances or implement your own synchronization.

## Performance Notes

- Use `using` statements or `IDisposable.Dispose()` to ensure proper cleanup
- Reuse `ZyreMessage` instances when possible
- Consider using `Span<T>` and `Memory<T>` for binary data manipulation
- The library uses modern .NET string marshalling for efficient interop

## License

This C# binding library is licensed under the Mozilla Public License 2.0, same as the Zyre project.