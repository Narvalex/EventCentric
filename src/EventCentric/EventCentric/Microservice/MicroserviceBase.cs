using EventCentric.Log;
using EventCentric.Messaging;
using EventCentric.Utils;
using System;
using System.Threading;

namespace EventCentric
{
    public abstract class MicroserviceBase : MicroserviceWorker
    {
        protected MicroserviceBase(string name, IBus bus, ILogger log)
            : base(bus, log)
        {
            Ensure.NotNullNeitherEmtpyNorWhiteSpace(name, "name");

            this.Name = name;
        }

        public string Name { get; private set; }

        protected override void OnStarting()
        {
            var isRelease = true;
#if DEBUG
            isRelease = false;
#endif
            if (isRelease)
                this.log.Trace($"RELEASE build detected");
            else
                this.log.Trace($"DEBUG build detected");

            int workerThreads;
            int completionPortThreads;
            ThreadPool.GetAvailableThreads(out workerThreads, out completionPortThreads);
            var processorCount = Environment.ProcessorCount;

            log.Trace("Worker threads: {0}", workerThreads);
            log.Trace("OSVersion:      {0}", Environment.OSVersion);
            log.Trace("ProcessorCount: {0}", processorCount);
            log.Trace("ClockSpeed:     {0} MHZ", CpuSpeed());
        }

        private static uint CpuSpeed()
        {
            var mo = new System.Management.ManagementObject("Win32_Processor.DeviceID='CPU0'");
            var sp = (uint)(mo["CurrentClockSpeed"]);
            mo.Dispose();
            return sp;
        }
    }
}
