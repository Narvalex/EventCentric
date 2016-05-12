using EventCentric.Log;
using EventCentric.Messaging;
using EventCentric.Utils;
using System;
using System.Threading;

namespace EventCentric
{
    public abstract class MicroserviceBase : MicroserviceWorker
    {
        protected MicroserviceBase(string name, ISystemBus bus, ILogger log)
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
            var logLines = new string[6];
            if (isRelease)
                logLines[1] = $"| RELEASE build detected";
            else
                logLines[1] = $"| DEBUG build detected";

            int workerThreads;
            int completionPortThreads;
            ThreadPool.GetAvailableThreads(out workerThreads, out completionPortThreads);
            var processorCount = Environment.ProcessorCount;

            var mo = new System.Management.ManagementObject("Win32_Processor.DeviceID='CPU0'");
            var cpuSpeed = (uint)(mo["CurrentClockSpeed"]);
            mo.Dispose();

            logLines[0] = $"| Starting {this.Name} microservice...";
            logLines[2] = string.Format("| Worker threads: {0}", workerThreads);
            logLines[3] = string.Format("| OSVersion:      {0}", Environment.OSVersion);
            logLines[4] = string.Format("| ProcessorCount: {0}", processorCount);
            logLines[5] = string.Format("| ClockSpeed:     {0} MHZ", cpuSpeed);

            this.log.Log($"", logLines);
        }
    }
}
