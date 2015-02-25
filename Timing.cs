using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Helpers
{
    public class Timing
    {
        private static System.Diagnostics.Stopwatch tic_time = new System.Diagnostics.Stopwatch();
        private System.Diagnostics.Stopwatch private_tic_time = new System.Diagnostics.Stopwatch();

        public static void Tic()
        {
            tic_time.Restart();
        }

        public static long Toc()
        {
            tic_time.Stop();
            return tic_time.ElapsedMilliseconds;
        }

        public void PrivateTic()
        {
            private_tic_time.Restart();
        }

        public long PrivateToc()
        {
            private_tic_time.Stop();
            return private_tic_time.ElapsedMilliseconds;
        }
    }
}
