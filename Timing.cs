using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization.Formatters;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;

namespace Helpers
{
    public class Timing
    {
        private static Timing staticTiming = new Timing();
        private System.Diagnostics.Stopwatch private_tic_time = new System.Diagnostics.Stopwatch();

        public static void Tic()
        {
            staticTiming.PrivateTic();
        }

        public static long Toc()
        {
            return staticTiming.PrivateToc();
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

        public static Tuple<Task, CancellationTokenSource> CorrectingTimer(double Interval, Action OnTick)
        {
            return CorrectingTimer(Interval, (i) =>
            {
                OnTick();
                return 0d;
            });
        }

        public static Tuple<Task, CancellationTokenSource> CorrectingTimer(double Interval, Func<long, double> OnTick)
        {
            var source = new CancellationTokenSource();
            var token = source.Token;
            Task passTick = null;
            passTick = new Task(() =>
            {
                DateTime start = DateTime.Now;
                double delay = 0d;
                OnTick(0);
                for (long step = 1; true; step++)
                {
                    var wait_time = start.AddMilliseconds(step*Interval + delay) - DateTime.Now;
                    if (wait_time > TimeSpan.Zero)
                    {
                        System.Threading.Thread.Sleep((int) wait_time.TotalMilliseconds);
                        delay = 0d;
                    }
                    if (token.IsCancellationRequested) return;
                    delay = OnTick(step);
                    if (token.IsCancellationRequested) return;
                }
            }, token);
            passTick.Start();
            return new Tuple<Task, CancellationTokenSource>(passTick, source);
        }



        /// <summary>
        /// Calls a function a given number of times over a time period, passing values 0...1 as time progresses
        /// </summary>
        /// <param name="Duration">The duration in milliseconds over which to ramp the value</param>
        /// <param name="Steps">The number of times in that duration to call the specified function</param>
        /// <param name="PerformStep">The function to be called throughout the given duration (gets passed 0...1)</param>
        public static Task Ramp(double Duration, int Steps, Action<double> PerformStep)
        {
            Task passRamp = new Task(() =>
            {
                DateTime start = DateTime.Now;
                PerformStep(0d);
                for (int step = 1; step <= Steps; step++)
                {
                    var target_value = (double) step/(double) Steps;
                    var wait_time = start.AddMilliseconds(target_value * Duration) - DateTime.Now;
                    if (wait_time > TimeSpan.Zero)
                        System.Threading.Thread.Sleep((int)wait_time.TotalMilliseconds);
                    var actual_value = (DateTime.Now - start).TotalMilliseconds/ Duration;
                    PerformStep(step == Steps ? 1d : Math.Min(1d, actual_value));
                }
            });
            passRamp.Start();
            return passRamp;
        }

        /// <summary>
        /// Performs the same task as Ramp, but passes a sinusoidal curve (smoothing the start and end) to the given function rather than a linear ramp
        /// </summary>
        /// <param name="Duration">The duration in milliseconds over which to ramp the value</param>
        /// <param name="Steps">The number of times in that duration to call the specified function</param>
        /// <param name="PerformStep">The function to be called throughout the given duration (gets passed 0...1)</param>
        public static Task RampSmoothBoth(double Duration, int Steps, Action<double> PerformStep)
        {
            return Ramp(Duration, Steps, (d) => PerformStep(0.5d * (1 - Math.Cos(d * Math.PI))));
        }

        /// <summary>
        /// Performs the same task as Ramp, but passes half a sinusoidal curve (smoothing the end) to the given function rather than a linear ramp
        /// </summary>
        /// <param name="Duration">The duration in milliseconds over which to ramp the value</param>
        /// <param name="Steps">The number of times in that duration to call the specified function</param>
        /// <param name="PerformStep">The function to be called throughout the given duration (gets passed 0...1)</param>
        public static Task RampSmoothEnd(double Duration, int Steps, Action<double> PerformStep)
        {
            return Ramp(Duration, Steps, (d) => PerformStep(Math.Sin(d * 0.5d * Math.PI)));
        }

        /// <summary>
        /// Performs the same task as Ramp, but passes half a sinusoidal curve (smoothing the start) to the given function rather than a linear ramp
        /// </summary>
        /// <param name="Duration">The duration in milliseconds over which to ramp the value</param>
        /// <param name="Steps">The number of times in that duration to call the specified function</param>
        /// <param name="PerformStep">The function to be called throughout the given duration (gets passed 0...1)</param>
        public static Task RampSmoothStart(double Duration, int Steps, Action<double> PerformStep)
        {
            return Ramp(Duration, Steps, (d) => PerformStep(1-Math.Cos(d * 0.5d * Math.PI)));
        }
    }
}
