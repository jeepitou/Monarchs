using System;
using System.Collections.Generic;

namespace ChessTCG.Logic
{
    public static class CollectionExistenceExtensions
    {
        // Checks if any item in the collection matches the predicate
        public static bool Exists<T>(this IEnumerable<T> source, Func<T, bool> predicate)
        {
            foreach (var item in source)
                if (predicate(item))
                    return true;
            return false;
        }
    }
}

