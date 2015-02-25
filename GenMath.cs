using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting.Messaging;
using System.Text;
using System.Drawing;

namespace Helpers
{
    public static class GenMath
    {
        public class PointBase<T> : IEquatable<PointBase<T>>  where T : IEquatable<T>
        {
            //private static int s_id = 0;
            //private readonly int id = s_id++;
            public T[] x;

            public PointBase(params T[] i)
            { x = i; }

            public T this[int index]
            {
                get { return x[index]; }
                set { x[index] = value; }
            }

            public override bool Equals(object obj)
            {
                return obj is PointBase<T> && this.Equals((PointBase<T>)obj);
            }

            public bool Equals(PointBase<T> other)
            {
                return other.x.Length == this.x.Length && other.x.Zip(this.x, (a, b) => a.Equals(b)).All(val => val);
            }

            public override int GetHashCode()
            {
                return x.Select(v => v.GetHashCode()).Aggregate((a, b) => a ^ b);
            }

            public override string ToString()
            {
                return "[{0}]".QuickFormat(",".Combine(x.Select(v => v.ToString()).ToArray()));
            }
        }

        public class Point : PointBase<int>, IEquatable<Point>
        {
            public Point(params int[] i) : base(i)
            {
            }

            public static explicit operator Point(System.Drawing.Point p)
            { return new Point(p.X, p.Y); }

            public static explicit operator System.Drawing.Point(Point p)
            { return new System.Drawing.Point(p[0], p[1]); }

            public bool Equals(Point other)
            {
                return this.Equals((PointBase<int>) other);
            }
        }

        public class PointD : PointBase<double>, IEquatable<PointD>
        {
            public PointD(params double[] i) : base(i)
            {
            }

            public static explicit operator PointD(System.Drawing.Point p)
            { return new PointD(p.X, p.Y); }

            public static explicit operator PointD(System.Drawing.PointF p)
            { return new PointD(p.X, p.Y); }

            public static explicit operator System.Drawing.PointF(PointD p)
            { return new System.Drawing.PointF((float)p[0], (float)p[1]); }

            public bool Equals(PointD other)
            {
                return this.Equals((PointBase<double>)other);
            }
        }

        public static double Distance(this Point a, Point b)
        {
            return Math.Sqrt(a.x.Select<int, int>((ax, i) =>
                    (ax - b.x[i]) * (ax - b.x[i])
                    ).Sum());
        }

        public static double Distance(this PointD a, PointD b)
        {
            return Math.Sqrt(a.x.Select<double, double>((ax, i) =>
                    (ax - b.x[i]) * (ax - b.x[i])
                    ).Sum());
        }

        public static PointD Centroid(this Point[] vertices)
        {
            return new PointD((double)vertices.Sum(p => p[0]) / vertices.Length, (double)vertices.Sum(p => p[1]) / vertices.Length);
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

        public static int IndexOfFirst<T>(this T[] Arr, Func<T, bool> Check)
        {
            for (int i = 0; i < Arr.Length; i++)
                if (Check(Arr[i]))
                    return i;
            return -1;
        }

        public static T BoundedGet<T>(this IEnumerable<T> Arr, int Index)
        {
            return Arr.ElementAt(Math.Max(0, Math.Min(Arr.Count() - 1, Index)));
        }
    }
}