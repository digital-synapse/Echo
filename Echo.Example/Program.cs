using Echo.Net;
using System;

namespace Echo.Example
{
    class Program
    {
        static void Main(string[] args)
        {
            // This is only required if running on mono, however its a good practice anyway because using formatters
            // generated at build time means faster app init since nothing has to be generated at runtime.
            // see ZeroFormatter docs on code generation for more details: https://github.com/neuecc/ZeroFormatter
            ZeroFormatter.ZeroFormatterInitializer.Register();
                               
            // create a broadcast listener for our type
            var net = new StateBroadcast<State>((o,states)=> {
                Console.Clear();
                foreach (var kvp in states)                
                    Console.WriteLine(kvp.Key + " -> " + kvp.Value.Position.X + ", "+ kvp.Value.Position.Y);                
            });

            // send data using the broadcast listener
            var state = new State();
            while (true) {                
                switch (Console.ReadKey().Key)
                {
                    case ConsoleKey.RightArrow: state.Position.X++; break;
                    case ConsoleKey.LeftArrow: state.Position.X--; break;
                    case ConsoleKey.UpArrow: state.Position.Y--; break;
                    case ConsoleKey.DownArrow: state.Position.Y++; break;
                }
                net.Send(state);                
            }            
        }
    }
}
