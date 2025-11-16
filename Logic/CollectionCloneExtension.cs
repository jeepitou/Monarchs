using System;
using System.Collections.Generic;

namespace ChessTCG.Logic
{
    public static class CollectionCloneExtension
    {
        public static void CloneList<T>(this List<T> source, List<T> dest) where T : IClonable<T>
        {
            for (int i = 0; i < source.Count; i++)
            {
                if (i < dest.Count)
                {
                    source[i].Clone(source[i], dest[i]);
                }
                else
                {
                    dest.Add(source[i].CloneNew(source[i]));
                }
            }
            if (dest.Count > source.Count)
                dest.RemoveRange(source.Count, dest.Count - source.Count);
        }
    }

    public interface IClonable<T>
    {
        void Clone(T source, T dest);
        T CloneNew(T source);
    }
}
