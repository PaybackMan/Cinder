using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cinder.Platform.ExtensionMethods
{
    public static class FlattenHierarchy
    {
        public static IEnumerable<T> Recurse<T, R>(this IEnumerable<T> source, Func<T, R> recursion) where R : IEnumerable<T>
        {
            var flattened = source.ToList();

            var children = source.Select(recursion);

            if (children != null)
            {
                foreach (var child in children)
                {
                    flattened.AddRange(child.Recurse(recursion));
                }
            }

            return flattened;
        }
    }
}
