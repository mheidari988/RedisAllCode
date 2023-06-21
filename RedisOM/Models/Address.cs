using Redis.OM.Modeling;

namespace RedisOM.Models;
public class Address
{
    [Searchable]
    public string? StreetAddress { get; set; }

    [Indexed]
    public string? PostalCode { get; set; }

    [Indexed]
    public GeoLoc Location { get; set; }

    public Address? ForwardingAddress { get; set; }
}