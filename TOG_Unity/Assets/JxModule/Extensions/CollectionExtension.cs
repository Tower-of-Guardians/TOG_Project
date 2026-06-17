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

        public static bool AddIfNotContains<T>(this ICollection<T> collection, T item)
        {
            if (collection == null)
            {
                return false;
            }

            if (collection.Contains(item))
            {
                return false;
            }
            
            collection.Add(item);
            return true;
        }

        public static bool RemoveIfContains<T>(this ICollection<T> collection, T item)
        {
            if (IsNullOrEmpty(collection))
            {
                return false;
            }

            if (!collection.Contains(item))
            {
                return false;
            }
            
            collection.Remove(item);
            return true;
        }
    }
}