using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace StackOverflow_14133330
{
    public class PlaylistAssignment
    {
        public virtual long? Id { get; set; }
        public virtual int AssignmentRank { get; set; }
        public virtual VideoFeed VideoFeed { get; set; }
    }
}
