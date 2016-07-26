using System.Collections.Generic;

namespace Microsoft.Extensions.DependencyInjection
{
    public interface IOrdered<out T> : IEnumerable<T>
    {
    }
}