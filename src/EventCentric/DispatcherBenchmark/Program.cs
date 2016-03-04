using System;
using System.Diagnostics;
using System.Threading;

namespace DispatcherBenchmark
{
    class Program
    {
        static void Main(string[] args)
        {
            var iterations = int.MaxValue;
            //Poll(iterations, 100);
            //Poll(iterations, 10);
            //Poll(iterations, 2);
            Poll(iterations, 1);
            //Poll(iterations, 0);
            //PollWithSpinWait(iterations);

            Console.ReadLine();
        }

        private static void Poll(int iterations, int sleepTime)
        {
            Console.WriteLine($"Starting to loop {iterations} times with a {sleepTime} miliseconds of interval.");
            var sw = new Stopwatch();
            sw.Start();
            for (int i = 0; i < iterations; i++)
            {
                var payload = new object();
                Thread.Sleep(sleepTime);
            }
            var elapsed = sw.Elapsed;
            Console.WriteLine($"Ended looping {iterations} times with {sleepTime} miliseconds of interval. Time elapsed: {elapsed.TotalMilliseconds} miliseconds.");
        }

        private static void PollWithSpinWait(int iterations)
        {
            var sw = new Stopwatch();
            sw.Start();
            Console.WriteLine($"Starting to loop {iterations} times SpinWait wait.");
            var count = 0;
            SpinWait.SpinUntil(() =>
            {
                count += 1;
                return iterations == count;
            });
            var elapsed = sw.Elapsed;
            Console.WriteLine($"Ended looping {iterations} with SpinWait. Time elapsed: {elapsed.TotalMilliseconds} miliseconds.");
        }
    }
}
