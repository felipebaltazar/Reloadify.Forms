using System.Collections.Generic;
using System.Linq;

namespace Reloadify.Forms.Extensions
{
    internal static class IEnumerableExtensions
    {
        public static bool IsNullOrEmpty<T>(this IEnumerable<T> collection)
        {
            if (collection is null)
                return true;

            return !collection.Any();
        }
    }
}
