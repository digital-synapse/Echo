using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Echo.Net
{
    public class UDPMulticast
    {
        string userName;
        int port;
        private readonly string broadcastAddress;

        UdpClient receivingClient;
        UdpClient sendingClient;

        Thread receivingThread;

        public Action<byte[]> OnReceive;

        public string LocalIP => userName;
        private IPEndPoint localIPEndpoint;

        public UDPMulticast( int port=54545)
        {
            this.port = port;
            localIPEndpoint = getOutboundIP();

            broadcastAddress = localIPEndpoint.Address.GetBroadcastAddress().ToString();
            userName = localIPEndpoint.Address.ToString();

            initializeSender();
            initializeReceiver();
        }        

        public void Send(string message)
        {
            
            if (!string.IsNullOrEmpty(message))
            {
                string toSend = userName + ":" + message;
                byte[] data = Encoding.ASCII.GetBytes(toSend);
                sendingClient.Send(data, data.Length);                
            }
        }
        public void Send(byte[] bytes)
        {

            if (bytes!=null && bytes.Length > 0)
            {
                sendingClient.Send(bytes, bytes.Length);
            }
        }

        private void initializeSender()
        {
            sendingClient = new UdpClient(broadcastAddress, port);
            sendingClient.EnableBroadcast = true;
            
        }

        private void initializeReceiver()
        {
            receivingClient = new UdpClient( port, AddressFamily.InterNetwork);

            ThreadStart start = new ThreadStart(receiver);
            receivingThread = new Thread(start);
            receivingThread.IsBackground = true;
            receivingThread.Start();
        }

        private void receiver()
        {
            IPEndPoint endPoint = new IPEndPoint(IPAddress.Any, 0);
            //IPEndPoint endPoint = localIPEndpoint;

            while (true)
            {
                byte[] data = receivingClient.Receive(ref endPoint);                
                // start the callback as a new task (in case its long running we dont want to tie up the listener loop and potentially miss messages)
                if (OnReceive != null)
                    Task.Factory.StartNew(()=> OnReceive(data));                
            }
        }

        // utility method to get outbound IP
        private IPEndPoint getOutboundIP()
        {
            
            using (Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, 0))
            {
                socket.Connect("8.8.8.8", 65530);
                IPEndPoint endPoint = socket.LocalEndPoint as IPEndPoint;
                return endPoint;
                //localIP = endPoint.Address.ToString();
            }
        }
    }
}
