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

