using System;

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

        public DateOnly? AddDateOnlyValue { get; set; }

        public DateOnly UpdateDateOnlyValue { get; set; }

        public TimeOnly? RelativeAddTimeOnlyValue { set; get; }

        public TimeOnly RelativeUpdateTimeOnlyValue { set; get; }
    }
}