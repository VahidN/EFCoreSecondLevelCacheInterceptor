using System.Collections.Generic;

namespace EFCoreSecondLevelCacheInterceptor.Tests.DataLayer.Entities
{
    public class Tag
    {
        public Tag()
        {
            TagProducts = new HashSet<TagProduct>();
        }

        public int Id { get; set; }
        public string Name { get; set; }

        public virtual ICollection<TagProduct> TagProducts { get; set; }
    }
}
