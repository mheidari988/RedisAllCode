using StackExchange.Redis;
using System.Diagnostics;

// Say we have a connection string like this: "redis://default:redispw@localhost:32768"
var config = new ConfigurationOptions
{
    EndPoints = { "localhost:32768" },
    User = "default",
    Password = "redispw",
};

using var redis = ConnectionMultiplexer.Connect(config);
var db = redis.GetDatabase();

// ping 1000 times with a for loop with Un-Pipelined approach
var sw = Stopwatch.StartNew();
for (int i = 0; i < 1000; i++)
{
    await db.PingAsync();
}
sw.Stop();
Console.WriteLine($"Without pipelining: {sw.ElapsedMilliseconds}ms");



// ping 1000 times with a for loop and add all PingAsync to a list of Tasks with Implicitly Pipelined approach
var tasks = new List<Task>();
sw.Restart();
for (int i = 0; i < 1000; i++)
{
    tasks.Add(db.PingAsync());
}
await Task.WhenAll(tasks);
sw.Stop();
Console.WriteLine($"With implicit pipelining: {sw.ElapsedMilliseconds}ms");



// Ping 1000 times with a for loop and add all PingAsync to a list of tasks and IBatch with Explicitly Pipelined approach
var batch = db.CreateBatch();
tasks.Clear();
sw.Restart();
for (int i = 0; i < 1000; i++)
{
    tasks.Add(batch.PingAsync());
}
batch.Execute();
await Task.WhenAll(tasks);
sw.Stop();
Console.WriteLine($"With explicit pipelining: {sw.ElapsedMilliseconds}ms");
