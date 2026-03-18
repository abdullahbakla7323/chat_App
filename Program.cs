using Microsoft.AspNetCore.SignalR.Client;

static string Prompt(string label, string defaultValue)
{
    Console.Write($"{label} (default: {defaultValue}): ");
    var value = Console.ReadLine();
    return string.IsNullOrWhiteSpace(value) ? defaultValue : value.Trim();
}

static string NormalizeServerBaseUrl(string input)
{
    var value = (input ?? string.Empty).Trim();
    if (value.Length == 0)
        return "http://localhost:5000";

    if (!value.Contains("://", StringComparison.Ordinal))
        value = $"http://{value}";

    if (!Uri.TryCreate(value, UriKind.Absolute, out var uri) ||
        (uri.Scheme != Uri.UriSchemeHttp && uri.Scheme != Uri.UriSchemeHttps))
        throw new UriFormatException($"Invalid server address: '{input}'. Example: http://localhost:5294");

    return uri.GetLeftPart(UriPartial.Authority);
}

string serverBaseUrl;
while (true)
{
    try
    {
        serverBaseUrl = NormalizeServerBaseUrl(Prompt("Server address", "http://localhost:5000"));
        break;
    }
    catch (UriFormatException ex)
    {
        Console.WriteLine(ex.Message);
    }
}

var user = Prompt("Username", Environment.UserName);

HubConnection BuildConnection(string baseUrl) =>
    new HubConnectionBuilder()
        .WithUrl($"{baseUrl.TrimEnd('/')}/chat")
        .WithAutomaticReconnect()
        .Build();

var connection = BuildConnection(serverBaseUrl);

connection.On<string, string, DateTimeOffset>("ReceiveMessage", (fromUser, message, utcTime) =>
{
    var localTime = utcTime.ToLocalTime().ToString("HH:mm:ss");
    Console.WriteLine($"[{localTime}] {fromUser}: {message}");
});

connection.Reconnecting += error =>
{
    Console.WriteLine("Connection lost, reconnecting...");
    return Task.CompletedTask;
};

connection.Reconnected += _ =>
{
    Console.WriteLine("Reconnected.");
    return Task.CompletedTask;
};

connection.Closed += error =>
{
    Console.WriteLine("Connection closed.");
    return Task.CompletedTask;
};

Console.WriteLine("Connecting...");
while (true)
{
    try
    {
        await connection.StartAsync();
        break;
    }
    catch (Exception ex) when (ex is HttpRequestException or InvalidOperationException)
    {
        Console.WriteLine($"Failed to connect: {ex.Message}");

        // Ask for a new address and retry
        while (true)
        {
            try
            {
                serverBaseUrl = NormalizeServerBaseUrl(Prompt("Server address", serverBaseUrl));
                break;
            }
            catch (UriFormatException uriEx)
            {
                Console.WriteLine(uriEx.Message);
            }
        }

        await connection.DisposeAsync();
        connection = BuildConnection(serverBaseUrl);
    }
}
Console.WriteLine("Connected. Type /quit to exit.");

while (true)
{
    var line = Console.ReadLine();
    if (line is null)
        continue;

    var text = line.Trim();
    if (text.Equals("/quit", StringComparison.OrdinalIgnoreCase))
        break;

    if (text.Length == 0)
        continue;

    await connection.InvokeAsync("SendMessage", user, text);
}

await connection.StopAsync();
