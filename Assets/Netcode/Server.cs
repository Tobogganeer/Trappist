using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Valve.Sockets;
using System.Runtime.InteropServices;
using System;

namespace Tobo.Net
{
    public abstract class Server
    {
        public Dictionary<uint, S_Client> clients;
        protected ushort nextPlayerID = 1;

        public bool Started { get; protected set; }

        /// <summary>
        /// Called when a client connects.
        /// </summary>
        public event Action<S_Client> ClientConnected;
        /// <summary>
        /// Called when a message is received from a client.
        /// </summary>
        public event Action<S_Client, ByteBuffer> MessageReceived;
        /// <summary>
        /// Called when a client leaves.
        /// </summary>
        public event Action<S_Client> ClientDisconnected;

        protected Dictionary<uint, Action<ByteBuffer, S_Client>> internalHandle;

        protected void ClientConnected_Raise(S_Client c) => ClientConnected?.Invoke(c);
        protected void MessageReceived_Raise(S_Client c, ByteBuffer b) => MessageReceived?.Invoke(c, b);
        protected void ClientDisconnected_Raise(S_Client c) => ClientDisconnected?.Invoke(c);


        //public event MessageCallback OnMessage = delegate { };

        public Server()
        {
            internalHandle = new Dictionary<uint, Action<ByteBuffer, S_Client>>()
            {
                { Packet.HashCache<C_Handshake>.ID, C_Handshake},
                { Packet.HashCache<Ping>.ID, Ping},
            };
        }

        public abstract void Run(ushort port = 26950);

        public abstract void Update();

        public void Stop()
        {
            if (clients != null)
                foreach (S_Client c in clients.Values)
                    c.Kick("Server closed");

            clients?.Clear();

            Started = false;

            Stop_Internal();
        }

        protected abstract void Stop_Internal();

        public void Kick(S_Client c)
        {
            if (S_Client.All.ContainsKey(c.ID))
                S_Client.All.Remove(c.ID);

            c.Kick("Kicked");
        }

        internal void Destroy()
        {
            if (clients != null)
            {
                foreach (S_Client c in clients.Values)
                    c.Kick("Server Closed");
                clients.Clear();
            }

            Started = false;

            Destroy_Internal();
        }

        protected abstract void Destroy_Internal();

        public void LogMessage(string message)
        {
            Debug.Log($"[SERVER]: {message}");
        }



        void C_Handshake(ByteBuffer buf, S_Client c)
        {
            //Debug.Log("SERVER: Got client handshake");
            C_Handshake packet = new C_Handshake();
            packet.Deserialize(buf, default);
            //Debug.Log("SERVER: Handshaking data for " + c);
            c.Username = packet.username;
            c.ID = nextPlayerID++;
            S_Client.All.Add(c.ID, c);

            if (NetworkManager.Instance.AllowConnection(c, buf, out string failReason))
            {
                //Debug.Log("Authing " + c);
                //DumpClients();

                LogMessage($"{c} connected.");
                S_Welcome welcome = new S_Welcome(c.ID);
                //Debug.Log("-----");
                //DumpClients();
                //Debug.Log("SEND WELCOME: " + welcome.GetBuffer().Dump());
                welcome.SendTo(c);
                ClientConnected?.Invoke(c);
                S_ClientConnected conn = new S_ClientConnected(c.ID, c.Username);
                conn.SendTo(c, true);
            }
            else
            {
                nextPlayerID--;
                c.Kick(failReason);
                // Removed in status method
                //clients.Remove(c.connection);
                //clients.Remove(c.connection);
                //ClientDisconnected?.Invoke(c);
            }
        }

        void Ping(ByteBuffer buf, S_Client c)
        {
            new Ping().SendTo(c);
        }


        public void DumpClients()
        {
            foreach (S_Client c in clients.Values)
            {
                Debug.Log($"CLIENT DUMP: {c}");
            }
        }
    }
}
