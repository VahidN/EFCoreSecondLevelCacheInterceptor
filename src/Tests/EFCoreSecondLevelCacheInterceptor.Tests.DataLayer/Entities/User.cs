using System;
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

        public DateTime? AddDate { get; set; }

        public DateTime? UpdateDate { get; set; }

        public long Points { get; set; }

        public bool IsActive { get; set; }

        public byte ByteValue { get; set; }

        public char CharValue { get; set; }

        public DateTimeOffset? DateTimeOffsetValue { get; set; }

        public decimal DecimalValue { get; set; }

        public double DoubleValue { set; get; }

        public float FloatValue { set; get; }

        public Guid GuidValue { set; get; }

        public TimeSpan? TimeSpanValue { set; get; }

        public short ShortValue { set; get; }

        public byte[] ByteArrayValue { get; set; }

        public uint UintValue { set; get; }

        public ulong UlongValue { set; get; }

        public ulong UshortValue { set; get; }

        public virtual ICollection<Post> Posts { get; set; }
        public virtual ICollection<Product> Products { get; set; }
    }

    public enum UserStatus
    {
        Active,
        Disabled
    }
}
