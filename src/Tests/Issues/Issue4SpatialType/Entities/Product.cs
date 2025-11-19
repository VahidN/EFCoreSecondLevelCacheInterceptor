using NetTopologySuite.Geometries;

namespace Issue4SpatialType.Entities;

public class Product
{
    public int Id { get; set; }

    public required Point Location { get; set; }
}