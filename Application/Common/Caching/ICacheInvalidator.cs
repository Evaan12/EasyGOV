using System.Collections.Generic;

namespace Application.Common.Caching
{
    public interface ICacheInvalidator
    {
        IEnumerable<string> InvalidateTags { get; }
    }
}