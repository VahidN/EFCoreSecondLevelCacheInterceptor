using System.Collections.Generic;

namespace EFCoreSecondLevelCacheInterceptor.Tests.DataLayer.Entities
{
    public class Product
    {
        public Product()
        {
            TagProducts = new HashSet<TagProduct>();
        }

        public int ProductId { get; set; }
        public string ProductNumber { get; set; }
        public string ProductName { get; set; }
        public string Notes { get; set; }
        public bool IsActive { get; set; }

        public virtual ICollection<TagProduct> TagProducts { get; set; }

        public virtual User User { get; set; }
        public int UserId { get; set; }
    }
}
