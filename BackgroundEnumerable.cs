using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Helpers
{
    public class BackgroundEnumerable<T> : IEnumerable<T>
    {
        private Func<int, T> Generator;
        private ConcurrentDictionary<int, bool> IsComputed;
        private ConcurrentDictionary<int, T> StoredContent;
        private ConcurrentStack<int> ToCompute;
        private bool IsSequentiallyComputed;
        private int? Length;

        public BackgroundEnumerable(Func<int, T> Generator, int? Length)
        {
            throw new NotImplementedException();
            this.Generator = Generator;
            StoredContent = new ConcurrentDictionary<int, T>();
            this.Length = Length;
            //IsComputed = new ConcurrentDictionary<int, Semaphore>();
            ToCompute = new ConcurrentStack<int>();
            if (Length.HasValue)
            {
                for (int i = 0; i < Length.Value; i++)
                {
                    //var Sem = Monitor.
                    //IsComputed.AddOrUpdate(i, );
                }
            }
        }

        public IEnumerator<T> GetEnumerator()
        {
            throw new NotImplementedException();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
