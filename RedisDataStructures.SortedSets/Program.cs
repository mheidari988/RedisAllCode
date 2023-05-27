using StackExchange.Redis;

// Say we have a connection string like this: "redis://default:redispw@localhost:32768"
var config = new ConfigurationOptions
{
    EndPoints = { "localhost:32771" },
    User = "default",
    Password = "redispw",
};

using var redis = await ConnectionMultiplexer.ConnectAsync(config);
var db = redis.GetDatabase();

var userAgeSet = "user:age";
var userLastAccessSet = "user:lastAccess";
var userHighScoreSet = "user:highScore";
var namesSet = "names";
var mostRecentlyActive = "users:mostRecentlyActive";

await db.KeyDeleteAsync(new RedisKey[]
{
    userAgeSet, userLastAccessSet, userHighScoreSet, namesSet, mostRecentlyActive
});


await db.SortedSetAddAsync(userAgeSet, new SortedSetEntry[]
{
    new("User:1", 20),
    new("User:2", 23),
    new("User:3", 18),
    new("User:4", 35),
    new("User:5", 55),
    new("User:6", 62)
});

db.SortedSetAdd(userLastAccessSet, new SortedSetEntry[]
{
    new("User:1", 1648483867),
    new("User:2", 1658074397),
    new("User:3", 1659132660),
    new("User:4", 1652082765),
    new("User:5", 1658087415),
    new("User:6", 1656530099)
});

db.SortedSetAdd(userHighScoreSet,
new SortedSetEntry[]
{
    new("User:1", 10),
    new("User:2", 55),
    new("User:3", 36),
    new("User:4", 25),
    new("User:5", 21),
    new("User:6", 44)
});

db.SortedSetAdd(namesSet,
new SortedSetEntry[]
{
    new("John", 0),
    new("Fred", 0),
    new("Bob", 0),
    new("Susan", 0),
    new("Alice", 0),
    new("Tom", 0)
});

var user3HighScore = db.SortedSetScore(userHighScoreSet, "User:3");
Console.WriteLine($"User:3 High Score: {user3HighScore}");


var user2Rank = db.SortedSetRank(userHighScoreSet, "User:2", Order.Descending);
Console.WriteLine($"User:2 Rank: {user2Rank}");

// printing out the three highest scorers.
var topScorers = db.SortedSetRangeByRank(userHighScoreSet, 0, 2, Order.Descending);
Console.WriteLine($"Top three: {string.Join(", ", topScorers)}");

// SortedSetRangeByScoreWithScores returns the members and their scores.
var eighteenToThirty = db.SortedSetRangeByScoreWithScores(userAgeSet, 18, 30, Exclude.None);
Console.WriteLine($"Users between 18 and 30: {string.Join(", ", eighteenToThirty)}");

// SortedSetRangeByValue returns the members and their scores.
var namesAlphabetized = db.SortedSetRangeByValue(namesSet);
Console.WriteLine($"Names Alphabetized: {string.Join(",", namesAlphabetized)}");

// SortedSetRangeByValue returns the members and their scores.
var namesBetweenAandJ = db.SortedSetRangeByValue(namesSet, "A", "K", Exclude.Stop);
Console.WriteLine($"Names between A and J: {string.Join(", ", namesBetweenAandJ)}");

// ByScoreWithScores returns the members and their scores.
db.SortedSetRangeAndStore(userLastAccessSet, mostRecentlyActive, 0, 2, order: Order.Descending);
var rankOrderMostRecentlyActive = db.SortedSetCombineWithScores(SetOperation.Intersect, new RedisKey[] { userHighScoreSet, mostRecentlyActive }, new double[] { 1, 0 }).Reverse();
Console.WriteLine($"Highest Scores Most Recently Active: {string.Join(", ", rankOrderMostRecentlyActive)}");