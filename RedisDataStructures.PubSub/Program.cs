using StackExchange.Redis;

// Say we have a connection string like this: "redis://default:redispw@localhost:32768"
var config = new ConfigurationOptions
{
    EndPoints = { "localhost:32768" },
    User = "default",
    Password = "redispw",
};

using var redis = await ConnectionMultiplexer.ConnectAsync(config);
var db = redis.GetDatabase();

var subscriber = redis.GetSubscriber();
var cancellationTokenSource = new CancellationTokenSource();
var token = cancellationTokenSource.Token;

var channel = await subscriber.SubscribeAsync("test-channel1");
channel.OnMessage(msg =>
{
    Console.WriteLine($"Sequentially received: {msg.Message} on channel: {msg.Channel}");
});

await subscriber.SubscribeAsync("test-channel2", (channel, value) =>
{
    Console.WriteLine($"Received: {value} on channel: {channel}");
});

var basicSendTask = Task.Run(async () =>
{
    var i = 0;
    while (!token.IsCancellationRequested)
    {
        await db.PublishAsync("test-channel1", i++);
        await db.PublishAsync("test-channel2", i += 2);
        await Task.Delay(1000);
    }
});


var patternSendTask = Task.Run(async () =>
{
    var i = 0;
    while (!token.IsCancellationRequested)
    {
        await db.PublishAsync($"pattern:{Guid.NewGuid()}", i++);
        await Task.Delay(1000);
    }
});


// put all other producer/subscriber stuff above here.
Console.ReadKey();
// put cancellation & unsubscribe down here.

Console.WriteLine("Unsubscribing to a single channel");
await channel.UnsubscribeAsync();
Console.ReadKey();


Console.WriteLine("Unsubscribing from all");
await subscriber.UnsubscribeAllAsync();
Console.ReadKey();