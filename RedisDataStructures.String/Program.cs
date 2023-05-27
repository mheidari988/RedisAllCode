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
var stopWatch = Stopwatch.StartNew();



// Simple example of a Redis Strings with set and get operations
var instructorNameKey = "instructors:name:1";
var instructorNameValue = "John Doe";

stopWatch.Restart();
await db.StringSetAsync(instructorNameKey, instructorNameValue);
var instructorName = await db.StringGetAsync(instructorNameKey);
stopWatch.Stop();
Console.WriteLine($"Set and Get an string took: {stopWatch.ElapsedMilliseconds}ms");



// Simple example of a Redis String Increment
var instructorAgeKey = "instructors:age:1";
var instructorAgeValue = 35;

stopWatch.Restart();
await db.StringSetAsync(instructorAgeKey, instructorAgeValue);
await db.StringIncrementAsync(instructorAgeKey);
var instructorAge = (int)await db.StringGetAsync(instructorAgeKey);
stopWatch.Stop();
Console.WriteLine($"instructor age: {instructorAge}");
Console.WriteLine($"Set, Increment and Get an string took: {stopWatch.ElapsedMilliseconds}ms");


// Simple example of a Redis String increment by a float value
var instructorSalaryKey = "instructors:salary:1";
var instructorSalaryValue = 1800.30;

stopWatch.Restart();
await db.StringSetAsync(instructorSalaryKey, instructorSalaryValue);
await db.StringIncrementAsync(instructorSalaryKey, 100.50);
var instructorSalary = (double)await db.StringGetAsync(instructorSalaryKey);
stopWatch.Stop();
Console.WriteLine($"instructor salary: {instructorSalary}");
Console.WriteLine($"Set, Increment and Get an string took: {stopWatch.ElapsedMilliseconds}ms");



// Simple example of a Redis String with expiration of 5 seconds.
// We can use TimeSpan.FromSeconds(5) and also we Delay the execution
// of the next line of code with Task.Delay(1000) and write to the console each time.
var instructorNameKeyWithExpiration = "instructors:name:2";
var instructorNameValueWithExpiration = "Reza Heidari";

await db.StringSetAsync(instructorNameKeyWithExpiration, instructorNameValueWithExpiration, TimeSpan.FromSeconds(5));
await Task.Delay(1000);
var instructorNameWithExpiration = await db.StringGetAsync(instructorNameKeyWithExpiration);
Console.WriteLine($"instructor name with expiration : {instructorNameWithExpiration}");
await Task.Delay(5000);
var instructorNameWithExpirationAfter5Seconds = await db.StringGetAsync(instructorNameKeyWithExpiration);
Console.WriteLine($"instructor name with expiration after 5 seconds: {instructorNameWithExpirationAfter5Seconds}");




// You can also specify a condition for when you want to set a key
// For example, if you only want to set a key when it does not exist
// you can by specifying the NotExists condition
var conditionalKey = "ConditionalKey";
var conditionalKeyText = "this has been set";

var wasSet = db.StringSet(conditionalKey, conditionalKeyText, when: When.NotExists);
Console.WriteLine($"Key set: {wasSet}");

// Of course, after the key has been set, if you try to set the key again
// it will not work, and you will get false back from StringSet
wasSet = db.StringSet(conditionalKey, "this text doesn't matter since it won't be set", when: When.NotExists);
Console.WriteLine($"Key set: {wasSet}");

// You can also use When.Exists, to set the key only if the key already exists
wasSet = db.StringSet(conditionalKey, "we reset the key!");
Console.WriteLine($"Key set: {wasSet}");