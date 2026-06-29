using System;
using System.Collections.Generic;

namespace Application.Common.Caching
{
    public interface ICacheable
    {
        string CacheKey { get; }
        TimeSpan? Expiration { get; }
        IEnumerable<string>? Tags { get; }
    }
}