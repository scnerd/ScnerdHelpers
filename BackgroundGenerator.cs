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
    public class BackgroundGenerator<T> : Task, IEnumerator<T>
    {
        bool IsPopped = false;
        T PoppedCurrent = default(T);
        ConcurrentQueue<T> SavedItems;
        CancellationTokenSource Canceller;
        bool Enabled = false;

        private BackgroundGenerator(Action Looper, CancellationTokenSource Canceller, ConcurrentQueue<T> Queue, bool AutoStart = true)
            : base(Looper, Canceller.Token)
        {
            this.SavedItems = Queue;
            this.Canceller = Canceller;
            if (AutoStart)
                this.Start();
        }

        private BackgroundGenerator(IEnumerable<T> Precomputed)
            : base(() => { })
        {
            SavedItems = new ConcurrentQueue<T>(Precomputed);
        }

        public  static BackgroundGenerator<T> FromEnumerable(IEnumerable<T> Precomputed)
        {
            return new BackgroundGenerator<T>(Precomputed);
        }

        public static BackgroundGenerator<T> FromEnumerator(IEnumerator<T> Generator, int MaxCache = -1,
            bool AutoStart = true)
        {
            return BackgroundGenerator<T>.FromFunc(() =>
            {
                Generator.MoveNext();
                return Generator.Current;
            }, MaxCache, AutoStart);
        }

        public static BackgroundGenerator<T> FromFunc(Func<T> GenerateNext, int MaxCache = -1, bool AutoStart = true)
        {
            ConcurrentQueue<T> Queue = new ConcurrentQueue<T>();
            CancellationTokenSource Canceller = new CancellationTokenSource();
            return new BackgroundGenerator<T>(() =>
            {
                try
                {
                    while (!Canceller.Token.IsCancellationRequested)
                    {
                        Monitor.Enter(Queue);
                        if (Queue.Count == MaxCache)
                            Monitor.Wait(Queue);
                        Monitor.Exit(Queue);
                        T result = GenerateNext();
                        Monitor.Enter(Queue);
                        Queue.Enqueue(result);
                        Monitor.Pulse(Queue);
                        Monitor.Exit(Queue);
                    }
                }
                catch (InvalidOperationException)
                {
                }
                catch (OperationCanceledException)
                {
                }
                finally { Monitor.Exit(Queue); }
            }, Canceller, Queue, AutoStart);
        }

        public static BackgroundGenerator<T> Chain<R>(Func<R, T> GenerateNext, IEnumerator<R> Source, int MaxCache = -1,
            bool AutoStart = true)
        {
            return BackgroundGenerator<T>.FromFunc(() =>
            {
                if (Source.MoveNext())
                {
                    return GenerateNext(Source.Current);
                }
                else
                {
                    throw new OperationCanceledException("End of background generator reached");
                }
            }, MaxCache, AutoStart);
        }

        public BackgroundGenerator<R> Chain<R>(Func<T, R> GenerateNext, int MaxCache = -1, bool AutoStart = true)
        {
            return BackgroundGenerator<R>.FromFunc(() =>
            {
                if (this.MoveNext())
                {
                    return GenerateNext(this.Current);
                }
                else
                {
                    throw new OperationCanceledException("End of background generator reached");
                }
            }, MaxCache, AutoStart);
        } 

        public static BackgroundGenerator<T> FromFuncWithCarryover<R>(Func<R, Tuple<T, R>> CarryoverGenerateNext, int MaxCache = -1, bool AutoStart = true)
        {
            R ClosureCarryover = default(R);
            return BackgroundGenerator<T>.FromFunc(() =>
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
                Monitor.Enter(SavedItems);
                if (Monitor.IsEntered(SavedItems) && SavedItems.Count == 0)
                    Monitor.Wait(SavedItems);
                T Result;
                IsPopped = Succeeded = SavedItems.TryDequeue(out Result);
                PoppedCurrent = Result;
                Monitor.Pulse(SavedItems);
                Monitor.Exit(SavedItems);
                return Succeeded;
            }
        }

        public void Reset()
        {
            SavedItems = new ConcurrentQueue<T>();
            PoppedCurrent = default(T);
            IsPopped = false;
        }

        public IEnumerator GetEnumerator()
        {
            return this;
        }
    }
}
