using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Helpers
{
    public class BackgroundGenerator<T> : IEnumerator<T>
    {
        bool IsPopped = false;
        T PoppedCurrent = default(T);
        ConcurrentQueue<T> SavedItems = new ConcurrentQueue<T>();
        Thread ComputeItems;
        int MaxCache;
        bool Enabled = false;

        public BackgroundGenerator(IEnumerator<T> Generator, int MaxCache = -1, bool AutoStart = true)
            : this(() => {Generator.MoveNext(); return Generator.Current;}, MaxCache, AutoStart)
        { }

        public BackgroundGenerator(Func<T> GenerateNext, int MaxCache = -1, bool AutoStart = true)
        {
            this.MaxCache = MaxCache;
            ComputeItems = new Thread(() =>
                {
                    try
                    {
                        while (Enabled && ComputeItems.IsAlive)
                        {
                            lock(SavedItems)
                            {
                                if (SavedItems.Count == MaxCache)
                                    Monitor.Wait(SavedItems);
                            }
                            T result = GenerateNext();
                            lock (SavedItems)
                            {
                                SavedItems.Enqueue(result);
                                Monitor.Pulse(SavedItems);
                            }
                        }
                    }
                    catch (InvalidOperationException) { }
                });
            ComputeItems.IsBackground = true;
            if (AutoStart)
                this.Start();
        }

        public static BackgroundGenerator<T> CreateWithCarryover<R>(Func<R, Tuple<T, R>> CarryoverGenerateNext, int MaxCache = -1, bool AutoStart = true)
        {
            R ClosureCarryover = default(R);
            return new BackgroundGenerator<T>(() =>
            {
                var tup = CarryoverGenerateNext(ClosureCarryover);
                ClosureCarryover = tup.Item2;
                return tup.Item1;
            });
        }

        public T Current
        {
            get
            {
                if (IsPopped)
                    return PoppedCurrent;
                throw new InvalidOperationException("No value is currently available from this BackgroundGenerator");
            }
        }

        public void Dispose()
        {
            Enabled = false;
        }

        object IEnumerator.Current
        {
            get { return this.Current; }
        }

        private readonly object _MoveNextSync = new Object();
        public bool MoveNext()
        {
            lock (_MoveNextSync)
            {
                bool Succeeded;
                lock (SavedItems)
                {
                    if (SavedItems.Count == 0)
                        Monitor.Wait(SavedItems);
                    T Result;
                    IsPopped = Succeeded = SavedItems.TryDequeue(out Result);
                    Monitor.Pulse(SavedItems);
                    PoppedCurrent = Result;
                }
                return Succeeded;
            }
        }

        public void Reset()
        {
            SavedItems = new ConcurrentQueue<T>();
            PoppedCurrent = default(T);
            IsPopped = false;
        }

        public void Start()
        {
            Enabled = true;
            ComputeItems.Start();
        }

        public void Stop()
        {
            Enabled = false;
        }
    }
}
