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

var allUsersSet = "users";
var activeUsersSet = "users:state:active";
var inactiveUsersSet = "users:state:inactive";
var offlineUsersSet = "users:state:offline";
await db.KeyDeleteAsync(new RedisKey[] { allUsersSet, activeUsersSet, inactiveUsersSet, offlineUsersSet });




// Add some users to the different sets
await db.SetAddAsync(activeUsersSet, new RedisValue[] { "user:1", "user:2" });
await db.SetAddAsync(inactiveUsersSet, new RedisValue[] { "user:3", "user:4" });
await db.SetAddAsync(offlineUsersSet, new RedisValue[] { "user:5", "user:6" });


// Using SetCombineAndStore to populate the allUsersSet
await db.SetCombineAndStoreAsync(SetOperation.Union, allUsersSet, new RedisKey[] { activeUsersSet, inactiveUsersSet, offlineUsersSet });



// Check if a user is in the offline set or not
RedisValue user6 = "user:6";
var user6IsOffline = await db.SetContainsAsync(offlineUsersSet, user6);
Console.WriteLine($"User : {user6} is offline : {user6IsOffline}");



// Enumerate Entire Set
// If your set is relatively compact (under 1000 members), this is a perfectly valid way to pull back all of your set members.
var allUsers = await db.SetMembersAsync(allUsersSet);
Console.WriteLine($"All Users with SMEMBERS : {string.Join(", ", allUsers)}");



// Enumerate Set in Chunks (Scan)
var allUsersScan = db.SetScan(allUsersSet);
Console.WriteLine($"All Users with SSCAN : {string.Join(", ", allUsersScan)}");



//Move Elements Between Sets
// Move user:1 from active to inactive
await db.SetMoveAsync(activeUsersSet, inactiveUsersSet, "user:1");
Console.WriteLine($"All inactive users with SSCAN : {string.Join(", ", db.SetScan(inactiveUsersSet))}");
Console.WriteLine($"All active users with SSCAN : {string.Join(", ", db.SetScan(activeUsersSet))}");
