using System;
using System.Linq;
using System.Text;
using Iesi.Collections.Generic;

namespace StackOverflow_14133330
{
    public class VideoFeed : VisualFeed
    {
        public virtual ISet<PlaylistAssignment> PlaylistAssignments { get; set; }
    }
}
