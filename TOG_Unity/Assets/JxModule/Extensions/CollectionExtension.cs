using System.Collections.Generic;

namespace JxModule
{
    public static class CollectionExtension
    {
        public static bool IsNullOrEmpty<T>(this ICollection<T> collection)
        {
            return collection == null || collection.Count == 0;
        }

        public static T FirstOrDefaultSafe<T>(this IList<T> list)
        {
            if (list == null || list.Count == 0)
            {
                return default;
            }

            return list[0];
        }
    }
}