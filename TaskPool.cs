using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Helpers
{
    public enum TaskPoolStatus
    {
        Working,
        Done
    }

    public class TaskPool
    {
        private List<Task> mToDo = new List<Task>();
        private int mThreadLimit, mCurrentlyRunning = 0;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="ThreadLimit">The maximum number of tasks to run consecutively. -1 indicates unlimited threads; 0 indicates the same number of threads as virtual cores.</param>
        public TaskPool(int ThreadLimit = 0)
        {
            if (ThreadLimit == 0)
                ThreadLimit = Environment.ProcessorCount;
            mThreadLimit = ThreadLimit;
        }

        public void AddAndRun(Action Function)
        {
            Task t = new Task(() => { Function(); RunNext(); mCurrentlyRunning--; });
            if (mCurrentlyRunning < mThreadLimit)
            {
                t.Start();
                mCurrentlyRunning++;
            }
            mToDo.Add(t);
        }

        private void RunNext()
        {
            foreach(Task t in mToDo)
                if (t.Status == TaskStatus.Created)
                {
                    t.Start();
                    break;
                }
        }

        public TaskPoolStatus Status
        { get { return mToDo.Any(t => t.Status == TaskStatus.Running) ? TaskPoolStatus.Working : TaskPoolStatus.Done; } }

        public void Wait()
        {
            foreach (Task t in mToDo)
                t.Wait();
        }
    }
}
