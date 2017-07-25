using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using System.Collections.Concurrent;
using System.Diagnostics;

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
                _stateBroadcastInternal.listeners.Add(new KeyValuePair<Type, Action<string, string>>(type, receiveAndDeserialize));
            }

            // init MyState
            State[_stateBroadcastInternal.p2p.LocalIP] = new TState();
            lastInvoke = new Stopwatch();
            lastInvoke.Start();
        }
        private int listenerIndex;

        public static ConcurrentDictionary<string, TState> State { get; set; } = new ConcurrentDictionary<string, TState>();
        public TState MyState => State[_stateBroadcastInternal.p2p.LocalIP];

        private void receiveAndDeserialize(string origin, string json)
        {    
            // throttle OnReceive to 50 frames per second                                
            if (lastInvoke.ElapsedMilliseconds > 20)
            {
                State[origin] = JsonConvert.DeserializeObject<TState>(json, _stateBroadcastInternal.jsonSettings);
                if (OnReceive != null) OnReceive.Invoke(origin, State);
                lastInvoke.Restart();
            }            
        }
        private Stopwatch lastInvoke;

        public Action<string, ConcurrentDictionary<string, TState>> OnReceive { get; set; }

        public void Send(TState state)
        {
            // reinit MyState
            State[_stateBroadcastInternal.p2p.LocalIP] = state;
            var json = JsonConvert.SerializeObject(MyState, _stateBroadcastInternal.jsonSettings);
            var type = typeof(TState);
            _stateBroadcastInternal.p2p.Send(type.Name + "=>" + json);            
        }

        // allow this class to be used in a using block to unregister listeners
        public void Dispose()
        {
            _stateBroadcastInternal.listeners.RemoveAt(listenerIndex);
        }
    }
    internal static class _stateBroadcastInternal
    {
        // static constructor
        public static readonly Object threadLock = new Object();
        public static UDPMulticast p2p;
        public static JsonSerializerSettings jsonSettings;
        public static Dictionary<string, Type> messageTypes = new Dictionary<string, Type>();
        public static List<KeyValuePair<Type, Action<string,string>>> listeners;
        static _stateBroadcastInternal()
        {
            if (p2p == null)
            {
                listeners = new List<KeyValuePair<Type, Action<string,string>>>();
                p2p = new UDPMulticast(8888);
                p2p.OnReceive = msg =>
                {
                    // decode header
                    var parts = msg.Split(new[] { "=>" }, StringSplitOptions.RemoveEmptyEntries);
                    var json = parts[1];
                    var parts2 = parts[0].Split(':');
                    var origin = parts2[0];
                    var typeName = parts2[1];

                    var type = messageTypes[typeName];
                    foreach (var kvp in listeners.Where(x => x.Key == type))
                    {
                        kvp.Value.Invoke(origin,json);
                    }
                };
                jsonSettings = new JsonSerializerSettings()
                {
                    NullValueHandling = NullValueHandling.Ignore,
                    MissingMemberHandling = MissingMemberHandling.Ignore,
                    
                };
            }
        }
    }
}
