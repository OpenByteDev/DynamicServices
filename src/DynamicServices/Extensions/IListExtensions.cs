using System.Collections.Generic;

namespace DynamicServices {
    internal static class IListExtensions {

        public static IList<T> Copy<T>(this IList<T> list) => new List<T>(list);

    }
}
