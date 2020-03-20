using NetTopologySuite.Geometries;

namespace Issue4SpatialType.Entities
{
    public class Product
    {
        public int Id { get; set; }
        public Point Location { get; set; }
    }
}