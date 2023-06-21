using Redis.OM.Modeling;

namespace RedisOM.Models;

[Document(
    StorageType = StorageType.Json,
    Prefixes = new[] { "Sale" },
    IndexName = "sales")]
public class Sale
{
    [RedisIdField]
    [Indexed]
    public string? Id { get; set; }

    [Indexed(Aggregatable = true)]
    public string? EmployeeId { get; set; }

    [Indexed(Aggregatable = true)]
    public int Total { get; set; }

    public Address? Address { get; set; }
}