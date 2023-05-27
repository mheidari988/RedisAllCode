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

var fruitsKey = "fruits";
var vegetablesKey = "vegetables";

// First clear all the keys in the database with the same name
await db.KeyDeleteAsync(new RedisKey[] { fruitsKey, vegetablesKey });

// Add some fruits to the list with left push
await db.ListLeftPushAsync(fruitsKey, new RedisValue[]
{
    "Banana","Apple","Orange","Mango","Pineapple"
});
Console.WriteLine($"The first fruit in the list is : {await db.ListGetByIndexAsync(fruitsKey, 0)}");
Console.WriteLine($"The last fruit in the list is : {await db.ListGetByIndexAsync(fruitsKey, -1)}");

// Add some vegetables to the list with right push
await db.ListRightPushAsync(vegetablesKey, new RedisValue[]
{
    "Onion","Potato","Carrot","Cabbage","Tomato"
});
Console.WriteLine($"The first vegetable in the list is : {await db.ListGetByIndexAsync(vegetablesKey, 0)}");
Console.WriteLine($"The last vegetable in the list is : {await db.ListGetByIndexAsync(vegetablesKey, -1)}");


// Lets list all of the vegetables and fruits by ListRange
Console.WriteLine($"Fruit indexes 0 to -1: {string.Join(", ", await db.ListRangeAsync(fruitsKey))}");
Console.WriteLine($"Vegetable indexes 0 to -2: {string.Join(", ", await db.ListRangeAsync(vegetablesKey, 0, -2))}");

// Move Tomato from vegetables to fruits
db.ListMove(vegetablesKey, fruitsKey, ListSide.Right, ListSide.Left);
Console.WriteLine($"Updated vegetables : {string.Join(", ", await db.ListRangeAsync(vegetablesKey))}");
Console.WriteLine($"Updated fruits : {string.Join(", ", await db.ListRangeAsync(fruitsKey))}");

// Redis Lists also allow you to find the index of a particular item using the ListPosition method.
Console.WriteLine($"Position of Mango: {await db.ListPositionAsync(fruitsKey, "Mango")}");

// Push and Pop to semiulate a FIFO queue and then list all the fruits
Console.WriteLine("Pushing Grapes");
await db.ListLeftPushAsync(fruitsKey, "Grapes");
Console.WriteLine($"After push : {string.Join(", ", await db.ListRangeAsync(fruitsKey))}");
Console.WriteLine($"Popping fruits [Dequeue Right] : {string.Join(", ", await db.ListRightPopAsync(fruitsKey, 2))}");
Console.WriteLine($"After Poping 2 of them : {string.Join(", ", await db.ListRangeAsync(fruitsKey))}");

// You can also get your lists to act as LIFO stacks, by pushing and popping from the same, by convention you'd typically use the left side.
Console.WriteLine("Pushing Grapefruit");
await db.ListLeftPushAsync(fruitsKey, "Grapefruit");
Console.WriteLine($"Popping Fruit [Dequeue Left]: {string.Join(",", db.ListLeftPop(fruitsKey, 2))}");

// And finally, you use the ListLength method to determine the size of a given list.
Console.WriteLine($"Length of fruits list : {await db.ListLengthAsync(fruitsKey)}");