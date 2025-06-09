using Zyre.Net;

Console.WriteLine("Zyre.Net Example - Chat Application");
Console.WriteLine("===================================");

if (args.Length == 0)
{
    Console.WriteLine("Usage: Zyre.Net.Examples <node-name>");
    Console.WriteLine("Example: Zyre.Net.Examples Alice");
    return;
}

var nodeName = args[0];

try
{
    using var node = new ZyreNode(nodeName);
    
    // Set some metadata
    node.SetHeader("app", "chat-example");
    node.SetHeader("version", "1.0");
    
    Console.WriteLine($"Created node: {node.Name} ({node.Uuid})");
    
    // Start the node
    if (!node.Start())
    {
        Console.WriteLine("Failed to start node");
        return;
    }
    
    Console.WriteLine("Node started successfully");
    
    // Join the chat group
    if (!node.Join("CHAT"))
    {
        Console.WriteLine("Failed to join CHAT group");
        return;
    }
    
    Console.WriteLine("Joined CHAT group");
    Console.WriteLine("Type messages to send, or 'quit' to exit");
    Console.WriteLine();

    // Start a background task to listen for events
    var cts = new CancellationTokenSource();
    var eventTask = Task.Run(async () =>
    {
        while (!cts.Token.IsCancellationRequested)
        {
            try
            {
                using var @event = node.ReceiveEvent();
                if (@event == null) continue;

                switch (@event.EventType)
                {
                    case ZyreEventType.Enter:
                        Console.WriteLine($"[SYSTEM] {@event.PeerName} joined the network");
                        break;
                    
                    case ZyreEventType.Exit:
                        Console.WriteLine($"[SYSTEM] {@event.PeerName} left the network");
                        break;
                    
                    case ZyreEventType.Join:
                        if (@event.Group == "CHAT")
                            Console.WriteLine($"[SYSTEM] {@event.PeerName} joined the chat");
                        break;
                    
                    case ZyreEventType.Leave:
                        if (@event.Group == "CHAT")
                            Console.WriteLine($"[SYSTEM] {@event.PeerName} left the chat");
                        break;
                    
                    case ZyreEventType.Shout:
                        if (@event.Group == "CHAT" && @event.Message != null)
                        {
                            var message = @event.Message.PopString();
                            Console.WriteLine($"[{@event.PeerName}] {message}");
                        }
                        break;
                    
                    case ZyreEventType.Whisper:
                        if (@event.Message != null)
                        {
                            var message = @event.Message.PopString();
                            Console.WriteLine($"[WHISPER from {@event.PeerName}] {message}");
                        }
                        break;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error receiving event: {ex.Message}");
            }
            
            await Task.Delay(10, cts.Token);
        }
    }, cts.Token);

    // Main input loop
    string? input;
    while ((input = Console.ReadLine()) != null)
    {
        if (input.Equals("quit", StringComparison.OrdinalIgnoreCase))
            break;
        
        if (string.IsNullOrWhiteSpace(input))
            continue;
        
        try
        {
            using var message = ZyreNode.CreateMessage(input);
            if (!node.Shout("CHAT", message))
            {
                Console.WriteLine("Failed to send message");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error sending message: {ex.Message}");
        }
    }

    cts.Cancel();
    
    // Wait a bit for the event task to finish
    try
    {
        await eventTask.WaitAsync(TimeSpan.FromSeconds(1));
    }
    catch (TimeoutException)
    {
        // Ignore timeout
    }
    
    Console.WriteLine("Goodbye!");
}
catch (DllNotFoundException)
{
    Console.WriteLine("ERROR: Native Zyre library not found!");
    Console.WriteLine("Please install libzyre and its dependencies:");
    Console.WriteLine("  Ubuntu/Debian: sudo apt-get install libzyre-dev libzmq3-dev libczmq-dev");
    Console.WriteLine("  macOS: brew install zyre zeromq czmq");
    Console.WriteLine("  Windows: Use vcpkg or build from source");
}
catch (Exception ex)
{
    Console.WriteLine($"Error: {ex.Message}");
    if (ex.InnerException != null)
    {
        Console.WriteLine($"Inner exception: {ex.InnerException.Message}");
    }
}
