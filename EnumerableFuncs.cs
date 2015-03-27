using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using Random = System.Random;

namespace Helpers
{
    public static class EnumerableFuncs
    {
        private static Random rand = new Random();

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

        public static T BoundedGet<T>(this IEnumerable<T> Arr, int Index)
        {
            return Arr.ElementAt(Math.Max(0, Math.Min(Arr.Count() - 1, Index)));
        }

        public static T RandomChoice<T>(this IEnumerable<T> Arr)
        {
            return Arr.ElementAt(rand.Next(Arr.Count()));
        }

        public static int IndexOfMax<T>(this T[] Values, Func<T, T, bool> AGreaterThanB)
        {
            if (Values.Length == 0)
                return -1;

            int Ind = 0;
            T Val = Values[0];

            for (int i = 1; i < Values.Length; i++)
                if (AGreaterThanB(Values[i], Val))
                {
                    Ind = i;
                    Val = Values[i];
                }

            return Ind;
        }

        public static int IndexOfMax<T>(this T[] Values) where T : IComparable
        {
            return Values.IndexOfMin((a, b) => a.CompareTo(b) > 0);
        }

        public static int IndexOfMin<T>(this T[] Values, Func<T, T, bool> ALessThanB)
        {
            if (Values.Length == 0)
                return -1;

            int Ind = 0;
            T Val = Values[0];

            for (int i = 1; i < Values.Length; i++)
                if (ALessThanB(Values[i], Val))
                {
                    Ind = i;
                    Val = Values[i];
                }

            return Ind;
        }

        public static int IndexOfMin<T>(this T[] Values, Func<T, T, bool> ALessThanB, int IteratedIndex, int[] BaseIndecis)
        {
            if (Values.Length == 0)
                return -1;

            int Ind = 0, Dim = Values.Rank;
            BaseIndecis[IteratedIndex] = 0;
            T Val = (T)Values.GetValue(BaseIndecis);

            for (int i = 1; i < Values.GetLength(IteratedIndex); i++)
            {
                BaseIndecis[IteratedIndex] = i;
                if (ALessThanB((T)Values.GetValue(BaseIndecis), Val))
                {
                    Ind = i;
                    Val = (T)Values.GetValue(BaseIndecis);
                }
            }

            return Ind;
        }

        public static int IndexOfMin<T>(this T[] Values) where T : IComparable
        {
            return Values.IndexOfMin((a, b) => a.CompareTo(b) < 0);
        }

        public static int IndexOfFirst<T>(this T[] Arr, Func<T, bool> Check)
        {
            for (int i = 0; i < Arr.Length; i++)
                if (Check(Arr[i]))
                    return i;
            return -1;
        }
    }
}
