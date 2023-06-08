﻿using StackExchange.Redis;
using System.Collections;
using System.Collections.Generic;

// Say we have a connection string like this: "redis://default:redispw@localhost:32768"
var config = new ConfigurationOptions
{
    EndPoints = { "localhost:32768" },
    User = "default",
    Password = "redispw",
};

using var redis = await ConnectionMultiplexer.ConnectAsync(config);
var db = redis.GetDatabase();

var sensor1 = "sensor:1";
var sensor2 = "sensor:2";
db.KeyDelete(new RedisKey[] { sensor1, sensor2 });

var rnd = new Random();

#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
Task.Run(async () =>
{
    long numInserted = 0;
    var s1Temp = 28;
    var s2Temp = 5;
    var s1Humid = 35;
    var s2Humid = 87;
    while (true)
    {
        await db.StreamAddAsync(sensor1, new[]
        {
            new NameValueEntry("temp", s1Temp),
            new NameValueEntry("humid", s1Humid),
        });

        await db.StreamAddAsync(sensor2, new[]
        {
            new NameValueEntry("temp", s2Temp),
            new NameValueEntry("humid", s2Humid),
        });

        await Task.Delay(1000);

        numInserted++;
        if (numInserted % 5 == 0)
        {
            s1Temp = s1Temp + rnd.Next(3) - 2;
            s2Temp = s2Temp + rnd.Next(3) - 2;
            s1Humid = Math.Min(s1Humid + rnd.Next(3) - 2, 100);
            s2Humid = Math.Min(s2Humid + rnd.Next(3) - 2, 100);
        }
    }
});
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed



#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
Task.Run(async () =>
{
    var positions = new Dictionary<string, StreamPosition>
    {
        { sensor1, new StreamPosition(sensor1, "0-0") },
        { sensor2, new StreamPosition(sensor2, "0-0") }
    };

    while (true)
    {
        var readResults = await db.StreamReadAsync(positions.Values.ToArray(), countPerStream: 1);
        if (!readResults.Any(x => x.Entries.Any()))
        {
            await Task.Delay(1000);
            continue;
        }
        foreach (var stream in readResults)
        {
            foreach (var entry in stream.Entries)
            {
                Console.WriteLine($"{stream.Key} - {entry.Id}: {string.Join(", ", entry.Values)}");
                positions[stream.Key!] = new StreamPosition(stream.Key, entry.Id);
            }
        }
    }
});
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed

var groupName = "tempAverage";
db.StreamCreateConsumerGroup(sensor1, groupName, "0-0");
db.StreamCreateConsumerGroup(sensor2, groupName, "0-0");


#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
Task.Run(async () =>
{
    var tempTotals = new Dictionary<string, long> { { sensor1, 0 }, { sensor2, 0 } };

    var messageCountTotals = new Dictionary<string, long> { { sensor1, 0 }, { sensor2, 0 } };
    var consumerName = "consumer:1";
    var positions = new Dictionary<string, StreamPosition>
    {
        { sensor1, new StreamPosition(sensor1, ">") },
            { sensor2, new StreamPosition(sensor2, ">") }
    };

    while (true)
    {
        var result = await db.StreamReadGroupAsync(positions.Values.ToArray(), groupName, consumerName, countPerStream: 1);
        if (!result.Any(x => x.Entries.Any()))
        {
            await Task.Delay(1000);
            continue;
        }

        foreach (var stream in result)
        {
            foreach (var entry in stream.Entries)
            {
                var temp = (int)entry.Values.First(x => x.Name == "temp").Value;
                messageCountTotals[stream.Key!]++;
                tempTotals[stream.Key!] += temp;
                var avg = tempTotals[stream.Key!] / messageCountTotals[stream.Key!];
                Console.WriteLine($"{stream.Key} average Temp = {avg:0.###}");
                await db.StreamAcknowledgeAsync(stream.Key, groupName, entry.Id);
            }
        }
    }
});
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed

Console.ReadKey();