using System;
using System.Collections.Generic;

namespace EventCentric.EntityFramework
{
    public partial class Stream
    {
        public System.Guid StreamId { get; set; }
        public int Version { get; set; }
        public string Memento { get; set; }
        public System.DateTime CreationDate { get; set; }
    }
}
