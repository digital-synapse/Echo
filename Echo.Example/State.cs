using ZeroFormatter;

namespace Echo.Example
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
