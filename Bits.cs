using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;

namespace Helpers
{
    public abstract class IBitsStorage
    {
        public const int BITS_PER_BYTE = 8;

        public abstract byte[] GetData { get; }
        public abstract int GetCount { get; }
        public abstract bool this[int ind] { get; set; }

        public string ToHex(string Separator = "")
        {
            return this.GetData.Select(b => "{0:x2}".QuickFormat(b)).Aggregate((a, b) => a + Separator + b);
        }

        public byte[] this[int start, int end]
        {
            get
            {
                BitsBuilder b = new BitsBuilder();
                for (int i = start; i < end; i++)
                    b.Append(1, this[i] ? (byte)0x1 : (byte)0x0);
                return b.ToBits().GetData;
            }
        }

        public static IBitsStorage operator +(IBitsStorage a, IBitsStorage b)
        {
            BitsBuilder c = new BitsBuilder();
            c.Append(a.GetCount, a.GetData);
            c.Append(b.GetCount, b.GetData);
            return c;
        }

        public static Bits operator |(IBitsStorage a, IBitsStorage b)
        {
            IBitsStorage longer = a.GetCount > b.GetCount ? a : b;
            IBitsStorage shorter = a.GetCount > b.GetCount ? b : a;
            shorter += Bits.Zeroes(longer.GetCount - shorter.GetCount);
            return new Bits(longer.GetData
                .Zip(shorter.GetData, (c, d) => (byte)(c | d))
                .ToArray(),
                longer.GetCount);
        }

        public static Bits operator &(IBitsStorage a, IBitsStorage b)
        {
            IBitsStorage longer = a.GetCount > b.GetCount ? a : b;
            IBitsStorage shorter = a.GetCount > b.GetCount ? b : a;
            shorter += Bits.Zeroes(longer.GetCount - shorter.GetCount);
            return new Bits(longer.GetData
                .Zip(shorter.GetData, (c, d) => (byte)(c & d))
                .ToArray(),
                longer.GetCount);
        }

        public static Bits operator ^(IBitsStorage a, IBitsStorage b)
        {
            IBitsStorage longer = a.GetCount > b.GetCount ? a : b;
            IBitsStorage shorter = a.GetCount > b.GetCount ? b : a;
            shorter += Bits.Zeroes(longer.GetCount - shorter.GetCount);
            return new Bits(longer.GetData
                .Zip(shorter.GetData, (c, d) => (byte)(c ^ d))
                .ToArray(),
                longer.GetCount);
        }

        public static Bits operator ~(IBitsStorage a)
        {
            return new Bits(a.GetData.Select(b => (byte)~b).ToArray(), a.GetCount);
        }

        public static Bits operator <<(IBitsStorage a, int b)
        {
            return Bits.Zeroes(b) + a;
        }

        public static Bits operator >>(IBitsStorage a, int b)
        {
            return new Bits(a[b, a.GetCount], a.GetCount - b);
        }
    }

    public class Bits : IBitsStorage
    {
        public static readonly Func<Bits>
            EMPTY = () => new Bits { count = 0, data = new byte[0] },
            ONE = () => new Bits { count = 1, data = new byte[] { 1 } },
            ZERO = () => new Bits { count = 1, data = new byte[] { 0 } };

        private byte[] data;
        private int count;

        public Bits()
        { }

        public Bits(byte[] Data)
            : this(Data, Data.Length *BITS_PER_BYTE)
        { }

        public Bits(byte[] Data, int Count)
        {
            this.data = Data;
            this.count = Count;
        }

        public static Bits Zeroes(int count)
        {
            return new Bits(new byte[(int) Math.Ceiling(count/8f)], count);
        }

        public static Bits Ones(int count)
        {
            return ~(new Bits(new byte[(int)Math.Ceiling(count / 8f)], count));
        }

        private void Validate()
        {
            if (this.count > 0 && this.data.Length < Math.Ceiling(this.count / (double) BITS_PER_BYTE))
                throw new IndexOutOfRangeException("Bits object has too few bytes for its bits");
        }

        public override byte[] GetData
        { get { return data; } }

        public override int GetCount
        { get { return count; } }

        public static Bits operator *(Bits a, int n)
        {
            Bits res = Bits.EMPTY();
            for (int i = 0; i < n; i++)
                res += a;

            return res;
        }

        public override bool this[int ind]
        {
            get
            {
                if (ind < this.count)
                    return (this.data[ind /BITS_PER_BYTE] & (1 << (ind %BITS_PER_BYTE))) != 0;
                else
                    throw new IndexOutOfRangeException("Cannot get get bit " + ind + ", only have " + this.count + " bits");
            }
            set
            {
                if (ind < this.count)
                    if (value)
                        this.data[ind /BITS_PER_BYTE] |= (byte)(1 << (ind %BITS_PER_BYTE));
                    else
                        this.data[ind /BITS_PER_BYTE] &= (byte)~(1 << (ind %BITS_PER_BYTE));
                else
                    throw new IndexOutOfRangeException("Cannot get get bit " + ind + ", only have " + this.count + " bits");
            }
        }

        public override string ToString()
        {
            StringBuilder s = new StringBuilder();
            s.Append("Bits { ");

            bool overflow;
            s.Append(this.ToBitString(out overflow));

            if (overflow)
                s.Append("...");

            s.Append(" }");
            return s.ToString();
        }

        private const int PRINT_LIMIT = 16;

        public string ToBitString(int ByteLimit = PRINT_LIMIT)
        {
            bool junk;
            return ToBitString(out junk, ByteLimit);
        }

        public string ToBitString(out bool overflow, int ByteLimit = PRINT_LIMIT)
        {
            StringBuilder s = new StringBuilder();
            if (ByteLimit < 0)
                ByteLimit = this.data.Length;
            else
                ByteLimit = Math.Min(ByteLimit, this.data.Length);
            overflow = ByteLimit < this.data.Length;

            for (int b = 0; b < ByteLimit; b++)
                for (int i = Math.Min((b + 1) * BITS_PER_BYTE, count) - 1; i >= b * BITS_PER_BYTE; i--)
                {
                    s.Append(this[i] ? '1' : '0');
                    if (i % 8 == 0)
                        s.Append(' ');
                }
            return s.ToString();
        }

        public static string ToBitString(params byte[] data)
        {
            StringBuilder s = new StringBuilder();
            for (int i = data.Length - 1; i >= 0; i--)
            {
                for (int j = BITS_PER_BYTE - 1; j >= 0; j--)
                {
                    s.Append((data[i] & (1 << j)) != 0 ? '1' : '0');
                }
            }
            return s.ToString();
        }

        public static Bits operator +(Bits a, IBitsStorage b)
        {
            BitsBuilder c = new BitsBuilder();
            c.Append(a.GetCount, a.GetData);
            c.Append(b.GetCount, b.GetData);
            return c.ToBits();
        }

        public override bool Equals(object obj)
        {
            return obj is IBitsStorage && ((IBitsStorage)obj).GetCount == this.count && Enumerable.SequenceEqual(((IBitsStorage)obj).GetData, this.data);
        }

        public static Bits FromBitString(string BitString)
        {
            BitsBuilder b = new BitsBuilder();
            foreach (char c in BitString)
            {
                if (c == '0')
                    b.Append(1, 0);
                else if (c == '1')
                    b.Append(1, 1);
                else
                    throw new ArgumentException("Invalid bit string, can only contain 1's and 0's");
            }
            return b.ToBits();
        }
    }

    public class BitsBuilder : IBitsStorage
    {
        private List<byte> bytes = new List<byte>();
        private int bit_index = 0;

        public BitsBuilder()
        {
            bytes.Add(0);
        }

        public BitsBuilder(byte[] Data)
            : this(Data, Data.Count() * Bits.BITS_PER_BYTE)
        { }

        public BitsBuilder(byte[] Data, int Count)
        {
            bytes.AddRange(Data);
            bit_index = Count;
        }

        public override bool this[int ind]
        {
            get
            {
                if (ind < this.bit_index)
                    return (this.bytes[ind / BITS_PER_BYTE] & (1 << (ind % BITS_PER_BYTE))) != 0;
                else
                    throw new IndexOutOfRangeException("Cannot get get bit " + ind + ", only have " + this.bit_index + " bits");
            }
            set
            {
                if (ind < this.bit_index)
                    if (value)
                        this.bytes[ind / BITS_PER_BYTE] |= (byte)(1 << (ind % BITS_PER_BYTE));
                    else
                        this.bytes[ind / BITS_PER_BYTE] &= (byte)~(1 << (ind % BITS_PER_BYTE));
                else
                    throw new IndexOutOfRangeException("Cannot get get bit " + ind + ", only have " + this.bit_index + " bits");
            }
        }

        public void Append(int BitCount, params byte[] Data)
        {
            int leftover = 0;
            int shift = bit_index % Bits.BITS_PER_BYTE;
            int byte_index = bit_index / Bits.BITS_PER_BYTE;
            int ByteCount = (int)Math.Ceiling(BitCount / (double)Bits.BITS_PER_BYTE);

            int bytes_needed = (int)Math.Ceiling((bit_index + BitCount) / (double)Bits.BITS_PER_BYTE) - bytes.Count;
            if (bytes_needed > 0)
                bytes.AddRange(Enumerable.Repeat<byte>((byte)0, bytes_needed));

            int i;
            for (i = 0; i < ByteCount; i++)
            {
                bytes[i + byte_index] |= (byte)
                    (leftover | (Data[i] << shift));
                leftover = Data[i] >> (Bits.BITS_PER_BYTE - shift);
            }
            if (leftover != 0)
            {
                bytes[i + byte_index] |= (byte)leftover;
                leftover = 0;

            }

            bit_index += BitCount;
        }

        public void Append(IBitsStorage Data)
        {
            Append(Data.GetCount, Data.GetData);
        }

        public override int GetCount
        { get { return bit_index; } }

        public override byte[] GetData
        { get { return bytes.ToArray(); } }

        public Bits ToBits()
        {
            return new Bits(bytes.ToArray(), bit_index);
        }

        public static BitsBuilder operator +(BitsBuilder a, IBitsStorage b)
        {
            BitsBuilder c = new BitsBuilder();
            c.Append(a.GetCount, a.GetData);
            c.Append(b.GetCount, b.GetData);
            return c;
        }

        public override string ToString()
        {
            return "BitsBuilder { " + bit_index + " bits }";
        }
    }
}