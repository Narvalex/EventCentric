﻿using System;
using System.Data.Entity.ModelConfiguration;

namespace EventCentric.Persistence.SqlServer
{
    public class SubscriberHeartbeatEntity
    {
        public string SubscriberName { get; set; }
        public string Url { get; set; }
        public long HeartbeatCount { get; set; }
        public DateTime LastHeartbeatTime { get; set; }
        public DateTime UpdateLocalTime { get; set; }
        public DateTime CreationLocalTime { get; set; }
    }

    public class SubscriberHeartbeatEntityMap : EntityTypeConfiguration<SubscriberHeartbeatEntity>
    {
        public SubscriberHeartbeatEntityMap()
        {
            // Primary key
            this.HasKey(t => t.SubscriberName);

            // Table & Column mappings
            this.ToTable("SubscribersHeartbeats", "EventStore");
            this.Property(t => t.SubscriberName).HasColumnName("SubscriberName");
            this.Property(t => t.Url).HasColumnName("Url");
            this.Property(t => t.HeartbeatCount).HasColumnName("HeartbeatCount");
            this.Property(t => t.LastHeartbeatTime).HasColumnName("LastHeartbeatTime");
            this.Property(t => t.UpdateLocalTime).HasColumnName("UpdateLocalTime");
            this.Property(t => t.CreationLocalTime).HasColumnName("CreationLocalTime");
        }
    }
}