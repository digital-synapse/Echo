using Echo.Net;
using System.Collections.Generic;
using System.Runtime.Serialization;
using ZeroFormatter;

namespace Echo.Models
{
    [ZeroFormattable]
    public class State
    {
        [Index(0)]
        public virtual Position Position { get; set; } = new Position();
    }

    [ZeroFormattable]
    public class Position
    {
        [Index(0)]
        public virtual int X { get; set; }

        [Index(1)]
        public virtual int Y { get; set; }
    }    
}
