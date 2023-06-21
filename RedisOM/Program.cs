using Redis.OM;
using Redis.OM.Modeling;
using RedisOM.Models;

// docker run -p 6379:6379 -p 8001:8001 redis/redis-stack
var provider = new RedisConnectionProvider("redis://localhost:6379");

// For the sake of having a clean start, we'll drop the indexes and associated records
provider.Connection.DropIndexAndAssociatedRecords(typeof(Sale));
provider.Connection.DropIndexAndAssociatedRecords(typeof(Employee));

// Create the indexes
await provider.Connection.CreateIndexAsync(typeof(Sale));
await provider.Connection.CreateIndexAsync(typeof(Employee));

// ====================================================================================================

// Lets get the collection amd insert a new employee
var employees = provider.RedisCollection<Employee>();
var employee = new Employee
{
    Name = "John Doe",
    Age = 42,
    Address = new Address
    {
        StreetAddress = "123 Main St",
        PostalCode = "12345",
        Location = new GeoLoc
        {
            Latitude = 1.2345,
            Longitude = 5.4321
        }
    },
    Sales = new List<string>
    {
        "1",
        "2"
    }
};

var key = await employees.InsertAsync(employee);

Console.WriteLine($"Employee Id: {employee.Id}");
Console.WriteLine($"Key Name: {key}");

// ====================================================================================================

// Lets get the collection amd insert a new sale
var sale = new Sale
{
    Id = Guid.NewGuid().ToString(),
    Address = new Address
    {
        StreetAddress = "Pinewood Ave",
        PostalCode = "10001",
        Location = new GeoLoc(-73.991, 40.753)
    },
    EmployeeId = employee.Id,
    Total = 5000,
};


// Insert the sale with expiration of 5 minutes
key = provider.Connection.Set(sale, TimeSpan.FromMinutes(5));
Console.WriteLine($"Sale Id: {sale.Id}");
Console.WriteLine($"Key Name: {key}");

// ====================================================================================================

// Now lets insert 5 new employees
var alice = new Employee
{
    Name = "Alice",
    Age = 45,
    Address = new Address { StreetAddress = "Elm Street", Location = new GeoLoc(-81.957, 27.058), PostalCode = "34269" }
};

var bob = new Employee
{
    Name = "Bob",
    Age = 60,
    Address = new Address() { StreetAddress = "Bleecker Street", Location = new GeoLoc(-74.003, 40.732), PostalCode = "10014" }
};

var charlie = new Employee
{
    Name = "Charlie",
    Age = 26,
    Address = new Address() { StreetAddress = "Ocean Boulevard", Location = new GeoLoc(-121.869, 36.604), PostalCode = "93940" }
};

var dan = new Employee
{
    Name = "Dan",
    Age = 42,
    Address = new Address() { StreetAddress = "Baker Street", Location = new GeoLoc(-0.158, 51.523), PostalCode = "NW1 6XE" }
};

var yves = new Employee
{
    Name = "Yves",
    Age = 19,
    Address = new Address() { StreetAddress = "Rue de Rivoli", Location = new GeoLoc(2.361, 48.863), PostalCode = "75003" }
};

await employees.InsertAsync(bob);
await employees.InsertAsync(alice);
await employees.InsertAsync(charlie);
await employees.InsertAsync(dan);
await employees.InsertAsync(yves);

// ====================================================================================================
// Now lets query the employees by name
Console.WriteLine($"----Employees Named Bob----");
var alsoBob = await employees.FirstAsync(x => x.Name == "Bob");
Console.WriteLine($"Bob's age is: {alsoBob.Age} and his postal code is: {alsoBob.Address!.PostalCode}");

// ====================================================================================================
// Now lets query the employees by age
var employeesUnderForty = employees.Where(x => x.Age < 40);
Console.WriteLine("----Employees under 40----");
await foreach (var emp in employeesUnderForty)
{
    Console.WriteLine($"{emp.Name} is {emp.Age}");
}
//====================================================================================================
// Now lets query the employees by location
var employeesNearPhilly = await employees.GeoFilter(x => x.Address!.Location, -75.159, 39.963, 1500, GeoLocDistanceUnit.Miles).ToListAsync();
Console.WriteLine("----Employees near Philly----");
foreach (var emp in employeesNearPhilly)
{
    Console.WriteLine($"{emp.Name} lives in the postal code: {emp.Address!.PostalCode}");
}
//====================================================================================================
// Now lets use order by
var employeesByAge = await employees.OrderBy(x => x.Age).Select(x => x.Name!).ToListAsync();
Console.WriteLine($"In Ascending order: {string.Join(", ", employeesByAge)}");
//====================================================================================================
// Now lets use order by descending
var employeesInReverseAlphabeticalOrder = await employees.OrderByDescending(x => x.Name).Select(x => x.Name!).ToListAsync();
Console.WriteLine($"In Reverse Alphabetical Order: {string.Join(", ", employeesInReverseAlphabeticalOrder)}");










// we'll be exploring how to Update and Delete Documents in Redis OM .NET.
//====================================================================================================

// For the sake of having a clean start, we'll drop the indexes and associated records
provider.Connection.DropIndexAndAssociatedRecords(typeof(Sale));
provider.Connection.DropIndexAndAssociatedRecords(typeof(Employee));

// Create the indexes
await provider.Connection.CreateIndexAsync(typeof(Sale));
await provider.Connection.CreateIndexAsync(typeof(Employee));



// Lets get the collection and insert a new employee
alice = new Employee
{
    Name = "Alice",
    Age = 45,
    Address = new Address { StreetAddress = "Elm Street", Location = new GeoLoc(-81.957, 27.058), PostalCode = "34269" },
    Sales = new List<string>()
};

bob = new Employee
{
    Name = "Bob",
    Age = 60,
    Address = new Address() { StreetAddress = "Bleecker Street", Location = new GeoLoc(-74.003, 40.732), PostalCode = "10014" },
    Sales = new List<string>()
};

var bobKeyName = await employees.InsertAsync(bob);
await employees.InsertAsync(alice);

// ====================================================================================================
// Now lets insert 500 new sales
var sales = provider.RedisCollection<Sale>();
var saleInsertTasks = new List<Task<string>>();
var random = new Random();

for (var i = 0; i < 500; i++)
{
    saleInsertTasks.Add(sales.InsertAsync(new Sale
    {
        Total = random.Next(1000, 30000),
        EmployeeId = bob.Id
    }));
}

await Task.WhenAll(saleInsertTasks);

bob.Sales.AddRange(saleInsertTasks.Select(x => x.Result.Split(":")[1]));

await employees.UpdateAsync(bob);

var bobFromDb = employees.FindById(bob.Id!);
Console.WriteLine($"Bob has: {bobFromDb!.Sales!.Count} sales");

// ====================================================================================================
// Now lets use save method

await foreach (var emp in employees.Where(x => x.Name == "Alice"))
{
    Console.WriteLine($"Alice's old age: {emp.Age}");
    emp.Age++;
}

employees.Save();

Console.WriteLine($"Alice's new age: {employees.First(x => x.Name == "Alice").Age}");

// ====================================================================================================
// There are two ways to delete Documents in Redis.
// If your model has an Id field, you can use the IRedisCollection.Delete method,
// passing in the document you want to delete. Redis OM will then remove the document from Redis for you.

// Lets delete Alice
await employees.DeleteAsync(alice);
Console.WriteLine($"Alice's present in Redis: {await employees.AnyAsync(x => x.Name == "Alice")}");

// ====================================================================================================
// If our model does not have an Id field on it:
// The other way to delete documents is to use the IRedisCollection.Unlink method.
await provider.Connection.UnlinkAsync(bobKeyName);
Console.WriteLine($"Bob's present in Redis: {await employees.AnyAsync(x => x.Name == "Bob")}");









//====================================================================================================
// we'll be exploring how to use Aggregates in Redis OM .NET.
//====================================================================================================
// For the sake of having a clean start, we'll drop the indexes and associated records
provider.Connection.DropIndexAndAssociatedRecords(typeof(Sale));
provider.Connection.DropIndexAndAssociatedRecords(typeof(Employee));

// Create the indexes
await provider.Connection.CreateIndexAsync(typeof(Sale));
await provider.Connection.CreateIndexAsync(typeof(Employee));

var saleAggregations = provider.AggregationSet<Sale>(); // init aggregation set.

var sumBobSales = saleAggregations.Filter(x => x.RecordShell!.EmployeeId == bob.Id).Sum(x => x.RecordShell!.Total);
Console.WriteLine($"Bob's total sales: {sumBobSales}");