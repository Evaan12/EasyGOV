using System;
using System.Collections.Generic;
using Domain.Common;
using Domain.Exceptions;

namespace Domain.ValueObjects
{
    public class AccessTimeWindow : ValueObject
    {
        public TimeSpan StartTime { get; }
        public TimeSpan EndTime { get; }

        private AccessTimeWindow() { }

        public AccessTimeWindow(TimeSpan startTime, TimeSpan endTime)
        {
            if (startTime == endTime && startTime != TimeSpan.Zero)
                throw new DomainException("Start time and end time cannot be equal unless both are zero.");

            StartTime = startTime;
            EndTime = endTime;
        }

        public bool IsWithinWindow(TimeSpan currentTime)
        {
            if (StartTime == TimeSpan.Zero && EndTime == TimeSpan.Zero) return true;

            if (StartTime <= EndTime)
            {
                return currentTime >= StartTime && currentTime <= EndTime;
            }
            else
            {
                return currentTime >= StartTime || currentTime <= EndTime;
            }
        }

        protected override IEnumerable<object?> GetEqualityComponents()
        {
            yield return StartTime;
            yield return EndTime;
        }
    }
}