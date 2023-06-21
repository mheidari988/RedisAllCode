using NRedisGraph;
using StackExchange.Redis;

// WARNING:
// This section requires RedisTimeSeries to be installed on your Redis server.
// I am using Docker to run RedisTimeSeries locally, but you can also install it on your machine.
// For using Docker, you can run the following command:
// docker pull redislabs/redistimeseries
// docker run --name my-redisgraph -p 6379:6379 -d redislabs/redisgraph

var config = new ConfigurationOptions
{
    EndPoints = { "localhost:6379" },
    User = "default",
    Password = "redispw",
};

using var redis = ConnectionMultiplexer.Connect(config);
var db = redis.GetDatabase();

var graph = new RedisGraph(db);
db.KeyDelete("pets");

// Lets create a person. We'll use a cypher query with a create command
var createBobResult = await graph.QueryAsync("pets", "CREATE(:human{name:'Bob',age:32})");
Console.WriteLine($"Create Bob Result: {createBobResult}");

// This creates a human (Bob), and returns a ResultSet that contains the results of the query.
// Since it's a create query, it doesn't respond with much, aside from the statistics from the query,
// how many nodes it created, how many properties it set, how many labels it added,
// and how long the query took to execute. You can print out all that information by accessing the ResultSet.
Console.WriteLine($"Nodes Created:{createBobResult.Statistics.NodesCreated}");
Console.WriteLine($"Properties Set:{createBobResult.Statistics.PropertiesSet}");
Console.WriteLine($"Labels Created:{createBobResult.Statistics.LabelsAdded}");
Console.WriteLine($"Operation took:{createBobResult.Statistics.QueryInternalExecutionTime}");

// Let's add one more Human to our graph (Alice), with a similar query:
await graph.QueryAsync("pets", "CREATE(:human{name:'Alice',age:30})");

// Finally we'll add a pet to our graph: Honey, a 5 year old Greyhound:
await graph.QueryAsync("pets", "CREATE(:pet{name:'Honey',age:5,species:'canine',breed:'Greyhound'})");

// A graph database consists of two primal types, Nodes and Edges, just like a graph. Nodes tend to be nouns,
// so in our example here our pet, Honey, and humans, Alice and Bob are all nodes.
// The Edges of our graph will tend to be verbs, so Honey might have an owner,
// so we can create an "OWNS" relationship between Honey and one of our Humans,
// let's make Honey's owner Bob, to do that we just match our pet and human and create a relationship between the nodes:
await graph.QueryAsync("pets",
    "MATCH(a:human),(p:pet) WHERE(a.name='Bob' and p.name='Honey') CREATE (a)-[:OWNS]->(p)");

//We can also create relationships between Honey and Both Alice and Bob to make them walkers of Honey:
await graph.QueryAsync("pets",
    "MATCH(a:human),(p:pet) WHERE(a.name='Alice' and p.name='Honey') CREATE (a)-[:WALKS]->(p)");
await graph.QueryAsync("pets",
    "MATCH(a:human),(p:pet) WHERE(a.name='Bob' and p.name='Honey') CREATE (a)-[:WALKS]->(p)");

//  To perform the query, perform a match between a human and pet, where there is an own relationship between
//  the human and the pet and the pet's name is honey, from this we'll return the human.
var matches = await graph.QueryAsync("pets", "MATCH(a:human),(p:pet) where (a)-[:OWNS]->(p) and p.name='Honey' return a");

//You can then pull out the first record from that result and print out that record,
//and you'll get the value of Bob (which is what we were expecting).
var record = matches.First();
Console.WriteLine($"Honey's owner nodes: {record}");

//We can then query all of the walkers of Honey by pulling back all of the human nodes that have an WALKS relationship with Honey.
//If we do a little bit more introspection on the result set, we can pull out individual nodes from each record,
//and print out only the information that we need, e.g. a name to print out the people who walk Honey.
matches = await graph.QueryAsync("pets", "MATCH(a:human),(p:pet) where (a)-[:WALKS]->(p) and p.name='Honey' return a");

foreach (var rec in matches)
{
    var node = (Node)rec.Values.First();
    Console.WriteLine($"{node.PropertyMap["name"].Value} walks honey");
}

//In reverse, we can also enumerate all the dogs owned by a particular human by matching that same owns
//relationship with that human's name and with a pet who's species is "canine":
matches = await graph.QueryAsync("pets", "MATCH(a:human),(p:pet) where (a)-[:OWNS]->(p) and p.species='canine' and a.name='Bob' return p");

foreach (var rec in matches)
{
    var dogs = rec.Values.Select(x => (Node)x).Select(x => x.PropertyMap["name"].Value.ToString());
    Console.WriteLine($"Bob's dogs are: {string.Join(", ", dogs)}");
}