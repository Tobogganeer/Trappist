using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Valve.Sockets;
using System.Runtime.InteropServices;

namespace Tobo.Net
{
    public abstract class Client
    {
        public static Dictionary<ushort, Client> All = new Dictionary<ushort, Client>();
        //public static Dictionary<ushort, Client> Connecting = new Dictionary<ushort, Client>();
        public static Client This
        {
            get
            {
                if (All.TryGetValue(NetworkManager.MyID, out Client s))
                    return s;
                return null;
            }
        }

        public ushort ID { get; internal set; }
        public string Username { get; internal set; }
        public bool IsConnected { get; internal set; }
        protected bool connecting = false;
        //public bool IsConnected => connection != 0;
        //bool handshaken;

        /// <summary>
        /// Called when we connect to the server.
        /// </summary>
        public event Action Connected;
        /// <summary>
        /// Called when we fail to connect to the server.
        /// </summary>
        public event Action ConnectionFailed; // May change to Disconnected, and pass an arg (reason)
        /// <summary>
        /// Called when we get a message from the server.
        /// </summary>
        public event Action<ByteBuffer> MessageReceived;
        /// <summary>
        /// Called when we are disconnected from the server.
        /// </summary>
        public event Action Disconnected;
        /// <summary>
        /// Called when another (not local) client connects. When joining, called for every other client already in the server.
        /// </summary>
        public event Action<Client> ClientConnected;
        /// <summary>
        /// Called when another (not local) client disconnects.
        /// </summary>
        public event Action<Client> ClientDisconnected;

        protected Dictionary<uint, Action<ByteBuffer>> internalHandle;

        protected internal Client() { }

        internal void InitLocal()
        {
            // Client constructor
            internalHandle = new Dictionary<uint, Action<ByteBuffer>>()
            {
                { Packet.HashCache<S_Handshake>.ID, S_Handshake},
                { Packet.HashCache<S_Welcome>.ID, S_Welcome},
                { Packet.HashCache<S_ClientConnected>.ID, S_ClientConnected},
                { Packet.HashCache<S_ClientDisconnected>.ID, S_ClientDisconnected},
                { Packet.HashCache<Ping>.ID, Ping},
            };
        }

        private Client InitRemote(ushort id, string name)
        {
            ID = id;
            Username = name;
            return this;
        }


        public abstract void Connect(string username, string ip = "::0", ushort port = 26950);

        public abstract void Update();

        public void Disconnect()
        {
            Disconnect_Internal();

            IsConnected = false;
            connecting = false;
            ID = 0;
            Username = string.Empty;
        }

        protected abstract void Disconnect_Internal();
        internal abstract void Destroy();

        protected void Connected_Raise() => Connected?.Invoke();
        protected void ConnectionFailed_Raise() => ConnectionFailed?.Invoke();
        protected void MessageReceived_Raise(ByteBuffer message) => MessageReceived?.Invoke(message);
        protected void Disconnected_Raise() => Disconnected?.Invoke();
        protected void ClientConnected_Raise(Client c) => ClientConnected?.Invoke(c);
        protected void ClientDisconnected_Raise(Client c) => ClientDisconnected?.Invoke(c);



        public override string ToString()
        {
            return $"Client '{Username}' ({ID})";
        }

        public void LogMessage(string message)
        {
            Debug.Log($"[{(ID == 0 || ID == NetworkManager.MyID ? "LOCAL CLIENT" : "CLIENT " + ID)}]: {message}");
        }

        public abstract void Send(Packet packet, SendMode mode = SendMode.Reliable);

        void S_Handshake(ByteBuffer buf)
        {
            // Server handshake has no contents
            LogMessage("Negotiating with server...");
            C_Handshake handshake = new C_Handshake(Username);
            Send(handshake);
            //Debug.Log("CLIENT: Sent handshake back");
        }

        void S_Welcome(ByteBuffer buf)
        {
            //Debug.Log("CONNECTED YIPPEEEE");

            LogMessage("Connected.");
            IsConnected = true;
            S_Welcome welcome = new S_Welcome();
            welcome.Deserialize(buf, default);

            ID = welcome.id;
            All = new Dictionary<ushort, Client>();
            All.Add(ID, this);
            for (int i = 0; i < welcome.otherClientIDs.Length; i++)
            {
                All.Add(welcome.otherClientIDs[i], NetworkManager.Instance.GetClient().
                    InitRemote(welcome.otherClientIDs[i], welcome.otherClientNames[i]));
            }

            Connected?.Invoke();

            foreach (Client c in All.Values)
            {
                if (c != this)
                    ClientConnected?.Invoke(c);
            }
        }

        void S_ClientConnected(ByteBuffer buf)
        {
            S_ClientConnected packet = new S_ClientConnected();
            packet.Deserialize(buf, default);

            Client c = NetworkManager.Instance.GetClient().InitRemote(packet.id, packet.name);
            All.Add(packet.id, c);
            ClientConnected?.Invoke(c);
        }

        void S_ClientDisconnected(ByteBuffer buf)
        {
            S_ClientDisconnected packet = new S_ClientDisconnected();
            packet.Deserialize(buf, default);

            if (All.TryGetValue(packet.id, out Client c))
            {
                ClientDisconnected?.Invoke(c);
                All.Remove(packet.id);
            }
        }

        void Ping(ByteBuffer buf)
        {
            // Ping stuff
        }
    }

    public abstract class S_Client
    {
        public static Dictionary<ushort, S_Client> All = new Dictionary<ushort, S_Client>();
        public static S_Client This
        {
            get
            {
                if (All.TryGetValue(NetworkManager.MyID, out S_Client s))
                    return s;
                return null;
            }
        }

        public ushort ID { get; protected internal set; }
        public string Username { get; protected internal set; }

        public abstract void Kick(string reason);
        internal abstract void Send(IntPtr buf, int size, SendMode sendMode);

        public override string ToString()
        {
            return $"S_Client '{Username}' ({ID})";
        }
    }
}