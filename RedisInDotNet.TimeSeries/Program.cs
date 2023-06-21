using NRedisTimeSeries;
using NRedisTimeSeries.Commands.Enums;
using NRedisTimeSeries.DataTypes;
using StackExchange.Redis;
using System.Runtime.CompilerServices;

// WARNING:
// This section requires RedisTimeSeries to be installed on your Redis server.
// I am using Docker to run RedisTimeSeries locally, but you can also install it on your machine.
// For using Docker, you can run the following command:
// docker pull redislabs/redistimeseries
// docker run -p 6379:6379 -it --rm redislabs/redistimeseries

var config = new ConfigurationOptions
{
    EndPoints = { "localhost:6379" },
    User = "default",
    Password = "redispw",
};

using var redis = ConnectionMultiplexer.Connect(config);
var db = redis.GetDatabase();

db.KeyDelete("sensor");

await db.TimeSeriesCreateAsync(
    "sensor",
    retentionTime: 60000,
    new List<TimeSeriesLabel>
    {
        new TimeSeriesLabel("id", "sensor-1")
    });

// After the Time Series are created, we'll then bind a rule to our primary Time Series,
// to run the compaction into the relevant Time Series. To do that we just need to
// call TimeSeriesCreate and TimeSeriesCreateRule for each rule we want to create:
var aggregations = new TsAggregation[] { TsAggregation.Avg, TsAggregation.Min, TsAggregation.Max };
foreach (var agg in aggregations)
{
    db.KeyDelete($"sensor:{agg}");

    await db.TimeSeriesCreateAsync($"sensor:{agg}", 60000, new List<TimeSeriesLabel>
    {
        new("type", agg.ToString()),
        new("aggregation-for", "sensor-1")
    });
    await db.TimeSeriesCreateRuleAsync("sensor", new TimeSeriesRule($"sensor:{agg}", 5000, agg));
}

// Now that we have our Time Series and rules created, we can start adding data to our primary Time Series.
var producerTask = Task.Run(async () => {
    while (true)
    {
        await db.TimeSeriesAddAsync("sensor", "*", Random.Shared.Next(50));
        await Task.Delay(1000);
    }
});


// Because we have the primary Time Series, and the extra compaction Time Series running in parallel,
// we will have two consumer tasks. The first of these will use TimeSeriesGet every second,
// to retrieve the most recent data from the Time Series.
var consumerTask = Task.Run(async () => {
    while (true)
    {
        await Task.Delay(1000);
        var result = await db.TimeSeriesGetAsync("sensor");
        Console.WriteLine($"{result.Time.Value}: {result.Val}");
    }
});

// The second Consumer will retrieve the aggregated data across our compacted Time Series.
// This will run every 5 seconds to coincide with our compaction rules.
// In this case we'll use TimeSeriesMGet.
// In this case however we'll be using the label that we created earlier to query multiple Time Series with the relevant label.
var aggregationConsumerTask = Task.Run(async () =>
{
    while (true)
    {
        await Task.Delay(5000);
        var results = await db.TimeSeriesMGetAsync(new List<string>() { "aggregation-for=sensor-1" }, true);
        foreach (var result in results)
        {
            Console.WriteLine($"{result.labels.First(x => x.Key == "type").Value}: {result.value.Val}");
        }

    }
});


Console.ReadKey();