using Echo.Models;
using Echo.Net;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Echo
{
    class Program
    {
        static void Main(string[] args)
        {            
            var net = new StateBroadcast<State>((o,states)=> {
                Console.Clear();
                foreach (var kvp in states)                
                    Console.WriteLine(kvp.Key + " -> " + kvp.Value.Position.X + ", "+ kvp.Value.Position.Y);                
            });

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
