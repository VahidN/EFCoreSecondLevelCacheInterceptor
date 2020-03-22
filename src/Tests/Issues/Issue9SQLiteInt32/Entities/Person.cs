using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace Issue9SQLiteInt32.Entities
{
    public class Person
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public DateTime AddDate { get; set; }

        public DateTime? UpdateDate { get; set; }

        public long Points { get; set; }

        public bool IsActive { get; set; }

        public byte ByteValue { get; set; }

        public char CharValue { get; set; }

        public DateTimeOffset DateTimeOffsetValue { get; set; }

        public decimal DecimalValue { get; set; }

        public double DoubleValue { set; get; }

        public float FloatValue { set; get; }

        public Guid GuidValue { set; get; }

        public TimeSpan TimeSpanValue { set; get; }

        public short ShortValue { set; get; }

        public byte[] ByteArrayValue { get; set; }

        public uint UintValue { set; get; }

        public ulong UlongValue { set; get; }

        public ulong UshortValue { set; get; }

        [Column(TypeName = "numeric")]
        public decimal NumericDecimalValue { get; set; }
    }
}