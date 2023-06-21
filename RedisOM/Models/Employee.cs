using Redis.OM.Modeling;

namespace RedisOM.Models;

[Document(
    StorageType = StorageType.Json,
    Prefixes = new[] { "Employee" },
    IndexName = "employees")]
public class Employee
{
    [RedisIdField]
    [Indexed]
    public string? Id { get; set; }

    public List<string>? Sales { get; set; }

    public Address? Address { get; set; }

    [Indexed(Sortable = true)]
    public string? Name { get; set; }

    [Indexed(Sortable = true)]
    public int Age { get; set; }
}