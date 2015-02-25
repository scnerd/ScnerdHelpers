using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Helpers
{
    public static class EnumerableFuncs
    {
        private static int? ResolveIndex<T>(this IEnumerable<T> Arr, int? Num)
        {
            return Num.HasValue ? Num >= 0 ? Num : Arr.Count() + Num : null;
        }

        public static IEnumerable<T> Slice<T>(this IEnumerable<T> Arr, int? Start, int? End)
        {
            int ResStart = ResolveIndex(Arr, Start) ?? 0;
            int ResEnd = ResolveIndex(Arr, End) ?? Arr.Count();
            return Arr.Skip(ResStart).Take(ResEnd - ResStart);
        }

        public static void AddIf<T>(this IList<T> Lst, T Element, Func<T, bool> Checker)
        {
            if (Checker(Element))
                Lst.Add(Element);
        }

        public static void AddIfNotIn<T>(this IList<T> Lst, T Element, IEnumerable<T> ExclusionList)
        {
            Lst.AddIf(Element, E => !ExclusionList.Contains(E));
        }

        public static void AddIf<T>(this HashSet<T> Lst, T Element, Func<T, bool> Checker)
        {
            if (Checker(Element))
                Lst.Add(Element);
        }

        public static void AddIfNotIn<T>(this HashSet<T> Lst, T Element, IEnumerable<T> ExclusionList)
        {
            Lst.AddIf(Element, E => !ExclusionList.Contains(E));
        }

        public static void AddIf<T>(this ConcurrentBag<T> Lst, T Element, Func<T, bool> Checker)
        {
            if (Checker(Element))
                Lst.Add(Element);
        }

        public static void AddIfNotIn<T>(this ConcurrentBag<T> Lst, T Element, IEnumerable<T> ExclusionList)
        {
            Lst.AddIf(Element, E => !ExclusionList.Contains(E));
        }
    }
}
