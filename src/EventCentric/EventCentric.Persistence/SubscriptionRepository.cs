using EventCentric.EventSourcing;
using EventCentric.Messaging;
using EventCentric.Persistence;
using EventCentric.Serialization;
using EventCentric.Utils;
using System;
using System.Collections.Generic;
using System.Linq;

namespace EventCentric.Polling
{
    public class SubscriptionRepository : ISubscriptionRepository
    {
        private readonly Func<bool, IEventStoreDbContext> contextFactory;
        private readonly ITextSerializer serializer;
        private readonly IUtcTimeProvider time;
        private readonly string streamType;

        public SubscriptionRepository(Func<bool, IEventStoreDbContext> contextFactory, string streamType, ITextSerializer serializer, IUtcTimeProvider time)
        {
            Ensure.NotNull(contextFactory, "contextFactory");
            Ensure.NotNull(serializer, "serializer");
            Ensure.NotNull(time, "time");
            Ensure.NotNullNeitherEmtpyNorWhiteSpace(streamType, nameof(streamType));

            this.contextFactory = contextFactory;
            this.serializer = serializer;
            this.time = time;
            this.streamType = streamType;
        }

        public SubscriptionBuffer[] GetSubscriptions()
        {
            var subscriptions = new List<SubscriptionBuffer>();

            using (var context = this.contextFactory(true))
            {
                var subscriptionsQuery = context.Subscriptions.Where(s => s.SubscriberStreamType == this.streamType && !s.IsPoisoned && !s.WasCanceled);
                if (subscriptionsQuery.Any())
                    foreach (var s in subscriptionsQuery)
                        // We substract one version in order to set the current version bellow the last one, in case that first event
                        // was not yet processed.
                        subscriptions.Add(new SubscriptionBuffer(s.StreamType.Trim(), s.Url.Trim(), s.Token, s.ProcessorBufferVersion - 1, s.IsPoisoned));
            }

            return subscriptions.ToArray();
        }

        public void FlagSubscriptionAsPoisoned(IEvent poisonedEvent, PoisonMessageException exception)
        {
            using (var context = this.contextFactory.Invoke(false))
            {
                var subscription = context.Subscriptions.Where(s => s.StreamType == poisonedEvent.StreamType && s.SubscriberStreamType == this.streamType).Single();
                subscription.IsPoisoned = true;
                subscription.UpdateLocalTime = this.time.Now.ToLocalTime();
                subscription.PoisonEventCollectionVersion = poisonedEvent.EventCollectionVersion;
                try
                {
                    subscription.ExceptionMessage = this.serializer.Serialize(exception);
                }
                catch (Exception)
                {
                    subscription.ExceptionMessage = string.Format("Exception type: {0}. Exception message: {1}. Inner exception: {2}", exception.GetType().Name, exception.Message, exception.InnerException.Message != null ? exception.InnerException.Message : "null");
                }
                try
                {
                    subscription.DeadLetterPayload = this.serializer.Serialize(poisonedEvent);
                }
                catch (Exception)
                {
                    subscription.DeadLetterPayload = string.Format("EventType: {0}", poisonedEvent.GetType().Name);
                }

                context.SaveChanges();
            }
        }

        public bool TryAddNewSubscriptionOnTheFly(string streamType, string url, string token)
        {
            using (var context = this.contextFactory.Invoke(false))
            {
                if (context.Subscriptions.Any(s => s.SubscriberStreamType == this.streamType && s.StreamType == streamType))
                    return false;

                var now = DateTime.Now;
                context.Subscriptions.Add(new SubscriptionEntity
                {
                    SubscriberStreamType = this.streamType,
                    StreamType = streamType,
                    Url = url,
                    Token = token,
                    CreationLocalTime = now,
                    UpdateLocalTime = now
                });

                context.SaveChanges();
                return true;
            }
        }
    }
}
