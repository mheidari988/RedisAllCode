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

var person1 = "person:1";
var person2 = "person:2";
var person3 = "person:3";

await db.KeyDeleteAsync(new RedisKey[] { person1, person2, person3 });

await db.HashSetAsync(person1, new HashEntry[]
{
    new("name","Alice"),
    new("age", 33),
    new("email","alice@example.com")
});

await db.HashSetAsync(person2, new HashEntry[]
{
    new("name","Bob"),
    new("age", 44),
    new("email","bob@example.com")
});

await db.HashSetAsync(person3, new HashEntry[]
{
    new("name","Charlie"),
    new("age", 55),
    new("email","charlie@example.com")
});


// use HashIncrement to increment the age of person1 by 4
var newAge = await db.HashIncrementAsync(person1, "age", 4);
Console.WriteLine($"New age for {person1} is {newAge}");

// use HashGet to get the name of person1
var person1Name = await db.HashGetAsync(person1, "name");
Console.WriteLine($"Name of the {person1} is {person1Name}");

// If your hash is relatively small, in Redis terms this means less than 1000 fields. You can use HashGetAll.
var person2Fields = await db.HashGetAllAsync(person2);
Console.WriteLine($"Fields of {person2} : {string.Join(", ", person2Fields)}");

// If you are working with a very large hash with many thousands of fields, you may want to use HashScan instead.
var person3Fields = db.HashScan(person3);
Console.WriteLine($"person:3 fields: {string.Join(", ", person3Fields)}");