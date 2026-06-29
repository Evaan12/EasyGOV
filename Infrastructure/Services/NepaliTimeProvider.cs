using System;

namespace Infrastructure.Services
{
    public class NepaliTimeProvider : TimeProvider
    {
        private static readonly TimeZoneInfo _nepalTimeZone;

        static NepaliTimeProvider()
        {
            try 
            { 
                // IANA Linux/Standard resolution
                _nepalTimeZone = TimeZoneInfo.FindSystemTimeZoneById("Asia/Kathmandu"); 
            }
            catch (TimeZoneNotFoundException) 
            { 
                try 
                { 
                    // Windows registry fallback
                    _nepalTimeZone = TimeZoneInfo.FindSystemTimeZoneById("Nepal Standard Time"); 
                }
                catch 
                { 
                    // Failsafe strictly defaulting to UTC if underlying OS ICU lacks resolution mapping
                    _nepalTimeZone = TimeZoneInfo.Utc; 
                }
            }
        }

        // Overrides Local timezone to ensure Nepali time offsets are utilized natively when .GetLocalNow() is invoked.
        // NOTE: .GetUtcNow() inherently retains standard UTC structures ensuring Postgres compatibility.
        public override TimeZoneInfo LocalTimeZone => _nepalTimeZone;
    }
}