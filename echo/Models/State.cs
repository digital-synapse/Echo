using Echo.Net;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Echo.Models
{
    public class State
    {
        public Position Position { get; set; } = new Position();
    }
    public class Position
    {
        public int X { get; set; }
        public int Y { get; set; }
    }    
}
