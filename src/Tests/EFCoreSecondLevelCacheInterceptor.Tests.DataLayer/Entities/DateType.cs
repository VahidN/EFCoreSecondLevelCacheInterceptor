using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace EFCoreSecondLevelCacheInterceptor.Tests.DataLayer.Entities
{
    public class DateType
    {
        public int Id { set; get; }

        public DateTime? AddDate { get; set; }

        public DateTime UpdateDate { get; set; }

        public DateTimeOffset? AddDateValue { get; set; }

        public DateTimeOffset UpdateDateValue { get; set; }

        public TimeSpan? RelativeAddTimeValue { set; get; }

        public TimeSpan RelativeUpdateTimeValue { set; get; }

        [NotMapped] public DateOnly? AddDateOnlyValue { get; set; }

        [NotMapped] public DateOnly UpdateDateOnlyValue { get; set; }

        [NotMapped] public TimeOnly? RelativeAddTimeOnlyValue { set; get; }

        [NotMapped] public TimeOnly RelativeUpdateTimeOnlyValue { set; get; }
    }
}