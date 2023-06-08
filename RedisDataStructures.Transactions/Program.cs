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

var transaction = db.CreateTransaction();

_ = transaction.HashSetAsync("person:1", new HashEntry[]
{
    new("name", "John"),
    new("age", 32),
    new("postalCode", "223143"),
});
_ = transaction.SortedSetAddAsync("person:name:Steve", "person:2", 0);
_ = transaction.SortedSetAddAsync("person:postal_code:32999", "person:2", 0);
_ = transaction.SortedSetAddAsync("person:age", "person:2", 32);

var success = await transaction.ExecuteAsync();

Console.WriteLine($"Transaction Successful: {success}");


// Adding conditional logic to a transaction

transaction.AddCondition(Condition.HashEqual("person:1", "age", 32));
_ = transaction.HashIncrementAsync("person:1", "age");
_ = transaction.SortedSetIncrementAsync("person:age", "person:2", 1);

success = await transaction.ExecuteAsync();
Console.WriteLine($"Transaction Successful: {success}");