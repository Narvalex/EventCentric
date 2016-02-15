using EventCentric.EventSourcing;
using PersistenceBenchmark.PromotionsStream;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace PersistenceBenchmark
{
    [Guid("f9344900-bcd3-32ec-866b-4da1b0aee120")]
    public class Promotions : StateOf<Promotions>,
        IUpdatesWhen<FreePointsRewardedToUser>
    {
        private int points = 0;

        public Promotions(Guid id) : base(id)
        {
        }

        public Promotions(Guid id, IEnumerable<IEvent> streamOfEvents) : base(id, streamOfEvents)
        {
        }

        public Promotions(Guid id, ISnapshot snapshot) : base(id, snapshot)
        {
            var state = (PromotionsSnapshot)snapshot;
            this.points = state.Points;
        }

        public override ISnapshot SaveToSnapshot()
        {
            return new PromotionsSnapshot(this.Version, this.points);
        }

        public void When(FreePointsRewardedToUser e) => this.points += e.Points;
    }
}
