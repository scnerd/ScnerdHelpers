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

        public static byte LeftRotate(this byte b, byte amt)
        {
            const int sz = sizeof (byte)*8;
            return (byte)((b << (amt%sz)) | (b >> (sz - amt%sz)));
        }

        public static byte RightRotate(this byte b, byte amt)
        {
            const int sz = sizeof(byte) * 8;
            return (byte)((b >> (amt % sz)) | (b << (sz - amt % sz)));
        }

        public static ushort LeftRotate(this ushort b, ushort amt)
        {
            const int sz = sizeof(ushort) * 8;
            return (ushort)((b << (amt % sz)) | (b >> (sz - amt % sz)));
        }

        public static ushort RightRotate(this ushort b, ushort amt)
        {
            const int sz = sizeof(ushort) * 8;
            return (ushort)((b >> (amt % sz)) | (b << (sz - amt % sz)));
        }

        public static uint LeftRotate(this uint b, uint amt)
        {
            const int sz = sizeof(uint) * 8;
            return (uint)((b << (int)(amt % sz)) | (b >> (int)(sz - amt % sz)));
        }

        public static uint RightRotate(this uint b, uint amt)
        {
            const int sz = sizeof(uint) * 8;
            return (uint)((b >> (int)(amt % sz)) | (b << (int)(sz - amt % sz)));
        }

        public static ulong LeftRotate(this ulong b, ulong amt)
        {
            const int sz = sizeof(ulong) * 8;
            return (ulong)((b << (int)(amt % sz)) | (b >> (int)(sz - amt % sz)));
        }

        public static ulong RightRotate(this ulong b, ulong amt)
        {
            const int sz = sizeof(ulong) * 8;
            return (ulong)((b >> (int)(amt % sz)) | (b << (int)(sz - amt % sz)));
        }

        public static IEnumerable<ushort> Convert8To16(IEnumerable<byte> inStream)
        {
            var enumerator = inStream.GetEnumerator();
            while (enumerator.MoveNext())
            {
                byte b1 = enumerator.Current;
                byte b2 = enumerator.MoveNext() ? enumerator.Current : (byte)0;

                yield return BitConverter.ToUInt16(new[] { b1, b2 }, 0);
            }
        }

        public static IEnumerable<uint> Convert8To32(IEnumerable<byte> inStream)
        {
            var enumerator = inStream.GetEnumerator();
            while (enumerator.MoveNext())
            {
                byte b1 = enumerator.Current;
                byte b2 = enumerator.MoveNext() ? enumerator.Current : (byte)0;
                byte b3 = enumerator.MoveNext() ? enumerator.Current : (byte)0;
                byte b4 = enumerator.MoveNext() ? enumerator.Current : (byte)0;

                yield return BitConverter.ToUInt32(new[] { b1, b2, b3, b4 }, 0);
            }
        }

        public static IEnumerable<ulong> Convert8To64(IEnumerable<byte> inStream)
        {
            var enumerator = inStream.GetEnumerator();
            while (enumerator.MoveNext())
            {
                byte b1 = enumerator.Current;
                byte b2 = enumerator.MoveNext() ? enumerator.Current : (byte)0;
                byte b3 = enumerator.MoveNext() ? enumerator.Current : (byte)0;
                byte b4 = enumerator.MoveNext() ? enumerator.Current : (byte)0;
                byte b5 = enumerator.MoveNext() ? enumerator.Current : (byte)0;
                byte b6 = enumerator.MoveNext() ? enumerator.Current : (byte)0;
                byte b7 = enumerator.MoveNext() ? enumerator.Current : (byte)0;
                byte b8 = enumerator.MoveNext() ? enumerator.Current : (byte)0;

                yield return BitConverter.ToUInt64(new[] { b1, b2, b3, b4, b5, b6, b7, b8 }, 0);
            }
        }

        public static IEnumerable<byte> Convert16To8(IEnumerable<ushort> inStream)
        {
            return inStream.SelectMany(us => BitConverter.GetBytes(us));
        }

        public static IEnumerable<byte> Convert32To8(IEnumerable<uint> inStream)
        {
            return inStream.SelectMany(us => BitConverter.GetBytes(us));
        }

        public static IEnumerable<byte> Convert64To8(IEnumerable<ulong> inStream)
        {
            return inStream.SelectMany(us => BitConverter.GetBytes(us));
        }
    }
}