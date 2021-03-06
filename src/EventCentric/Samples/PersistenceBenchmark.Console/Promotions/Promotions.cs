﻿using EventCentric.EventSourcing;
using PersistenceBenchmark.PromotionsStream;
using System;
using System.Collections.Generic;

namespace PersistenceBenchmark
{
    public class Promotions : State<Promotions>,
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
