using System.Collections.Generic;

namespace EFCoreSecondLevelCacheInterceptor.Tests.DataLayer.Entities
{
    public class Blog
    {
        public Blog()
        {
            Posts = new HashSet<Post>();
        }

        public int BlogId { get; set; }
        public string Url { get; set; }

        public virtual ICollection<Post> Posts { get; set; }
    }
}
