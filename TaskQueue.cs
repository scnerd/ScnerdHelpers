using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Helpers
{
    public class TaskQueue
    {
        Queue<Task> mToDo = new Queue<Task>(), mWaiting = new Queue<Task>(), mDone = new Queue<Task>();
        private int mThreadLimit;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="ThreadLimit">The maximum number of tasks to run consecutively. -1 indicates unlimited threads; 0 indicates the same number of threads as virtual cores.</param>
        public TaskQueue(int ThreadLimit = 0)
        {
            if (ThreadLimit == 0)
                ThreadLimit = Environment.ProcessorCount;
            mThreadLimit = ThreadLimit;
        }

        public int AddAndRun(Action Function)
        {
            Task t = new Task(() => { Function(); RunNext(); });
            if (mToDo.Count < mThreadLimit)
            {
                mToDo.Enqueue(t);
                t.Start();
            }
            else
            {
                mWaiting.Enqueue(t);
            }
            return t.Id;
        }

        private void RunNext()
        {
            MoveCompleted();
            if (mWaiting.Count > 0)
            {
                Task t = mWaiting.Dequeue();
                t.Start();
                mToDo.Enqueue(t);
            }
        }

        public TaskPoolStatus Status
        { get { return mToDo.Any(t => t.Status == TaskStatus.Running) ? TaskPoolStatus.Working : TaskPoolStatus.Done; } }

        public void Wait(int NumberOfResults)
        {
            NumberOfResults -= MoveCompleted();
            for (int i = 0; i < NumberOfResults; i++)
            {
                mToDo.ElementAt(i).Wait();
            }
        }

        private int MoveCompleted()
        {
            int Moved = 0;
            while(mToDo.ElementAt(0).IsCompleted)
            {
                mDone.Enqueue(mToDo.Dequeue());
                Moved++;
            }
            return Moved;
        }

        public T GetResult<T>()
        {
            Wait(1);
            Task t = mDone.Dequeue();
            return ((Task<T>)t).Result;
        }
    }
}
