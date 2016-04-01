using EventCentric.Log;
using EventCentric.Messaging;
using EventCentric.Messaging.Commands;
using EventCentric.Messaging.Events;
using EventCentric.Repository;
using EventCentric.Utils;
using System;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace EventCentric
{
    public class HeartbeatListener : MicroserviceWorker,
        IMessageHandler<StartHeartbeatListener>
    {
        private readonly Func<bool, HeartbeatDbContext> contextFactory;
        private Tuple<string, string>[] subscribersNamesAndUrls;
        private readonly TimeSpan timeout;
        private readonly TimeSpan interval;
        private readonly IUtcTimeProvider time;
        private readonly string nodeName;

        public HeartbeatListener(string nodeName, IBus bus, ILogger log, IUtcTimeProvider time, TimeSpan timeout, TimeSpan interval, Func<bool, HeartbeatDbContext> contextFactory)
            : base(bus, log)
        {
            Ensure.NotNullNeitherEmtpyNorWhiteSpace(nodeName, nameof(nodeName));
            Ensure.NotNull(contextFactory, "contextFactory");
            Ensure.NotNull(time, "time");
            if (timeout.TotalSeconds <= 1)
                throw new ArgumentOutOfRangeException("timeout", "The timeout value must be greater than one second.");
            if (interval.TotalSeconds <= 1)
                throw new ArgumentOutOfRangeException("interval", "The interval value must be greater than one second.");

            this.timeout = timeout;
            this.interval = interval;
            this.time = time;
            this.contextFactory = contextFactory;
            this.nodeName = nodeName.Replace('.', '@');
        }

        protected override void OnStarting()
        {
            this.subscribersNamesAndUrls = this.GetSubscribersNamesAndUrls();
            this.RequestHeartbeats();
            this.log.Trace("Heartbeat listener started");
        }

        protected override void OnStopping()
        {
            this.log.Trace("Heartbeat listener stopped");
        }

        private void RequestHeartbeats()
        {
            if (this.subscribersNamesAndUrls == null)
            {
                this.log.Trace("Heartbeat listener does not have any registered subscriber to listen to");
                return;
            }

            foreach (var sub in this.subscribersNamesAndUrls)
                Task.Factory.StartNewLongRunning(() =>
                {
                    this.RequestHeartbeat(sub.Item1, sub.Item2);
                });
        }

        private void RequestHeartbeat(string name, string url)
        {
            while (!base.stopping)
            {
                try
                {
                    using (var client = this.CreateHttpClient())
                    {
                        var response = client.GetAsync($"{url}/{this.nodeName}").Result;
                        if (!response.IsSuccessStatusCode)
                            throw new InvalidOperationException(string.Format($"Heartbeat request to '{url}/{this.nodeName}' received an status code of: {0}", response.StatusCode.ToString()));

                        this.log.Trace($"{response.Content.ReadAsStringAsync().Result}");
                        using (var context = this.contextFactory(false))
                        {
                            var subscription = context.SubscribersHeartbeats.Single(s => s.SubscriberName == name);
                            var now = this.time.Now;
                            subscription.UpdateLocalTime = now;
                            subscription.HeartbeatCount = subscription.HeartbeatCount + 1;
                            subscription.LastHeartbeatTime = now;

                            context.SaveChanges();
                        }
                    }
                }
                catch (Exception ex)
                {
                    this.log.Error(ex, string.Format("Request heartbeat error on subscriber {0} with url {1}", name, url));

                    try
                    {
                        using (var context = this.contextFactory(false))
                        {
                            var subscription = context.SubscribersHeartbeats.Single(s => s.SubscriberName == name);
                            var now = this.time.Now;
                            subscription.UpdateLocalTime = now;

                            context.SaveChanges();
                        }
                    }
                    catch (Exception ex2)
                    {
                        this.log.Error(ex2, string.Format("Request heartbeat error while logging error on subscriber {0} with url {1}", name, url));
                    }
                }
                finally
                {
                    Thread.Sleep(this.interval);
                }
            }
        }

        private Tuple<string, string>[] GetSubscribersNamesAndUrls()
        {
            try
            {
                using (var context = this.contextFactory(true))
                {
                    if (!context.SubscribersHeartbeats.Any())
                        return null;

                    var entities = context.SubscribersHeartbeats.ToArray();
                    return entities.Select(e => new Tuple<string, string>(e.SubscriberName, e.Url)).ToArray();
                }
            }
            catch (Exception ex)
            {
                this.log.Error(ex, "Heartbeat initialization error while geting subscribers names and urls.");

                this.bus.Publish(
                    new FatalErrorOcurred(
                        new FatalErrorException("Fatal error: Heartbeat initialization error.", ex)));

                throw;
            }
        }

        private HttpClient CreateHttpClient()
        {
            var client = new HttpClient();
            client.Timeout = this.timeout;
            return client;
        }

        public void Handle(StartHeartbeatListener message)
        {
            this.Start();
        }
    }
}
