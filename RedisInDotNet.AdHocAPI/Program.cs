using StackExchange.Redis;

// Say we have a connection string like this: "redis://default:redispw@localhost:32768"
var config = new ConfigurationOptions
{
    EndPoints = { "localhost:32768" },
    User = "default",
    Password = "redispw",
};

using var redis = ConnectionMultiplexer.Connect(config);
var db = redis.GetDatabase();

// We can use the ExecuteAsync method to send commands to Redis
await db.ExecuteAsync("SET", "foo", "bar");
var result = await db.ExecuteAsync("GET", "foo");
Console.WriteLine(result);

// We can also use the ExecuteAsync method to send commands to Redis with arguments
// Both work, the first way requires less code of course, but if you use it,
// you cannot pass any command flags in, since the params macro must be the last argument in the list.
var arguments = new object[] { "foo", "bar" };
await db.ExecuteAsync("SET", arguments);
result = await db.ExecuteAsync("GET", "foo");
Console.WriteLine(result);

