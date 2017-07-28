using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Concurrent;
using System.Diagnostics;
using ZeroFormatter;

namespace Echo.Net
{
    public class StateBroadcast<TState> : IDisposable where TState : new()
    {
        // class init
        public StateBroadcast(Action<string, ConcurrentDictionary<string, TState>> onReceive = null)
        {
            OnReceive = onReceive;

            var type = typeof(TState);
            _stateBroadcastInternal.messageTypes[type.Name] = type;
            lock (_stateBroadcastInternal.threadLock)
            {
                listenerIndex = _stateBroadcastInternal.listeners.Count - 1;
                _stateBroadcastInternal.listeners.Add(new KeyValuePair<Type, Action<string, byte[]>>(type, receiveAndDeserialize));
            }

            // init MyState
            State[_stateBroadcastInternal.p2p.LocalIP] = new TState();
            lastInvoke = new Stopwatch();
            lastInvoke.Start();
            lastSend = new Stopwatch();
            lastSend.Start();
        }
        private int listenerIndex;
        private Stopwatch lastInvoke;
        private Stopwatch lastSend;

        public static ConcurrentDictionary<string, TState> State { get; set; } = new ConcurrentDictionary<string, TState>();
        public TState MyState => State[_stateBroadcastInternal.p2p.LocalIP];

        private void receiveAndDeserialize(string origin, byte[] data)
        {    
            // throttle OnReceive to 50 frames per second                                
            if (lastInvoke.ElapsedMilliseconds > 20)
            {                
                State[origin] = ZeroFormatterSerializer.Deserialize<TState>(data);
                if (OnReceive != null) OnReceive.Invoke(origin, State);
                lastInvoke.Restart();
            }            
        }
        

        public Action<string, ConcurrentDictionary<string, TState>> OnReceive { get; set; }

        public bool Send(TState state)
        {
            if (lastSend.ElapsedMilliseconds > 20)
            {
                // reinit MyState
                State[_stateBroadcastInternal.p2p.LocalIP] = state;

                var type = typeof(TState);
                var bytes = ZeroFormatterSerializer.Serialize(new Payload()
                {
                    Origin = _stateBroadcastInternal.p2p.LocalIP,
                    TypeName = type.Name,
                    Data = ZeroFormatterSerializer.Serialize(MyState)
                });

                _stateBroadcastInternal.p2p.Send(bytes);
                lastSend.Restart();
                return true;
            }
            return false;
        }

        // allow this class to be used in a using block to unregister listeners
        public void Dispose()
        {
            _stateBroadcastInternal.listeners.RemoveAt(listenerIndex);
        }
    }
    [ZeroFormattable]
    public class Payload
    {
        [Index(0)]
        public virtual string Origin { get; set; }
        [Index(1)]
        public virtual string TypeName { get; set; }
        [Index(2)]
        public virtual byte[] Data { get; set; }
    }
    internal static class _stateBroadcastInternal
    {
        // static constructor
        public static readonly Object threadLock = new Object();
        public static UDPMulticast p2p;        
        public static Dictionary<string, Type> messageTypes = new Dictionary<string, Type>();
        public static List<KeyValuePair<Type, Action<string,byte[]>>> listeners;
        static _stateBroadcastInternal()
        {
            if (p2p == null)
            {
                ZeroFormatterInitializer.Register();

                listeners = new List<KeyValuePair<Type, Action<string,byte[]>>>();
                p2p = new UDPMulticast(8888);
                p2p.OnReceive = bytes =>
                {
                    var result = ZeroFormatterSerializer.Deserialize<Payload>(bytes);
                    var type = messageTypes[result.TypeName];
                    foreach (var kvp in listeners.Where(x => x.Key == type))
                    {
                        kvp.Value.Invoke(result.Origin, result.Data);
                    }
                };
            }
        }
    }
}
