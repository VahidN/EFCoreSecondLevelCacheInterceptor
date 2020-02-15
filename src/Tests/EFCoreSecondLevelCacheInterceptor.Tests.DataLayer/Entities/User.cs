using System.Collections.Generic;

namespace EFCoreSecondLevelCacheInterceptor.Tests.DataLayer.Entities
{
    public class User
    {
        public User()
        {
            Posts = new HashSet<Post>();
            Products = new HashSet<Product>();
        }

        public int Id { get; set; }
        public string Name { get; set; }
        public UserStatus UserStatus { get; set; }

        public byte[] ImageData { get; set; }

        public virtual ICollection<Post> Posts { get; set; }
        public virtual ICollection<Product> Products { get; set; }
    }

    public enum UserStatus
    {
        Active,
        Disabled
    }
}
