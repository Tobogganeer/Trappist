using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Valve.Sockets;
using System.Runtime.InteropServices;
using System;

namespace Tobo.Net
{
    public class SocketServer : Server
    {
        internal NetworkingSockets socketInterface;
        uint pollGroup;
        uint listenSocket;

        StatusCallback callback;
        IntPtr statusPtr;

        public SocketServer() : base()
        {
            callback = StatusChanged;
            statusPtr = Marshal.GetFunctionPointerForDelegate(callback);
        }

        public override void Run(ushort port = 26950)
        {
            if (Started)
                Stop();

            nextPlayerID = 1;
            clients = new Dictionary<uint, S_Client>();
            if (!NetworkManager.SinglePlayer)
            {
                socketInterface = new NetworkingSockets();
                pollGroup = socketInterface.CreatePollGroup();

                /* CPP

                https://github.com/ValveSoftware/GameNetworkingSockets/blob/master/examples/example_chat.cpp#L245

                SteamNetworkingConfigValue_t opt;
                opt.SetPtr( k_ESteamNetworkingConfig_Callback_ConnectionStatusChanged, (void*)SteamNetConnectionStatusChangedCallback );
                m_hListenSock = m_pInterface->CreateListenSocketIP( serverLocalAddr, 1, &opt );

                */

                //utils.SetStatusCallback(status);

                Configuration cfg = new Configuration();
                cfg.dataType = ConfigurationDataType.FunctionPtr;
                cfg.value = ConfigurationValue.ConnectionStatusChanged;
                //cfg.data.FunctionPtr = Marshal.GetFunctionPointerForDelegate<StatusCallback>(StatusChanged);
                cfg.data.FunctionPtr = statusPtr;

                Address address = new Address();

                address.SetAddress("::0", port);

                listenSocket = socketInterface.CreateListenSocket(ref address, new Configuration[] { cfg });
            }
            //OnMessage += (in NetworkingMessage netMessage) =>
            //{
            //    Debug.Log("Message received from - ID: " + netMessage.connection + ", Channel ID: " + netMessage.channel + ", Data length: " + netMessage.length);
            //};

            Started = true;
        }

        public override void Update()
        {
            // Don't even think necessary; they arent created in singleplayer
            //if (!NetworkManager.SinglePlayer)
            //{
            socketInterface?.RunCallbacks();
            socketInterface?.ReceiveMessagesOnPollGroup(pollGroup, OnMessage, 20);
            //}
        }

        protected override void Stop_Internal()
        {
            socketInterface?.CloseListenSocket(listenSocket);
            socketInterface?.DestroyPollGroup(pollGroup);
        }

        protected override void Destroy_Internal()
        {
            socketInterface?.CloseListenSocket(listenSocket);
            socketInterface?.DestroyPollGroup(pollGroup);
            socketInterface = null;
        }

        void StatusChanged(ref StatusInfo info)
        {
            if (NetworkManager.Quitting || !NetworkManager.IsServer) return;

            // https://github.com/ValveSoftware/GameNetworkingSockets/blob/master/examples/example_chat.cpp
            // All just taken from here ^^^
            switch (info.connectionInfo.state)
            {
                case ConnectionState.None:
                    break;

                case ConnectionState.Connecting:
                    //Debug.Log("Connection from " + info.connectionInfo.connectionDescription);
                    LogMessage("Incoming connection from " + info.connectionInfo.connectionDescription);

                    if (clients.ContainsKey(info.connection))
                    {
                        Debug.LogError("[SERVER]: Clients list already contains " + info.connection + "!");
                        socketInterface.CloseConnection(info.connection, 0, "", false);
                        break;
                    }

                    if (clients.Count >= NetworkManager.Instance.maxPlayers)
                    {
                        LogMessage("Rejecting " + info.connectionInfo.connectionDescription + ", max players reached.");
                        socketInterface.CloseConnection(info.connection, 0, "", false);
                        break;
                    }

                    if (socketInterface.AcceptConnection(info.connection) != Result.OK)
                    {
                        socketInterface.CloseConnection(info.connection, 0, "", false);
                        LogMessage("Can't accept connection, already closed?");
                        break;
                    }

                    if (!socketInterface.SetConnectionPollGroup(pollGroup, info.connection))
                    {
                        socketInterface.CloseConnection(info.connection, 0, "", false);
                        LogMessage("Failed to set poll group?");
                        break;
                    }

                    //Debug.Log("SERVER: Adding new client, conn " + info.connection);
                    //DumpClients();
                    clients[info.connection] = new SocketS_Client(info.connection, this);
                    //Debug.Log("-----");
                    //DumpClients();

                    break;

                case ConnectionState.Connected:
                    //Debug.Log("Client connected - ID: " + info.connection + ", IP: " + info.connectionInfo.address.GetIP());
                    new S_Handshake().SendTo(clients[info.connection]);
                    //ClientConnected?.Invoke(clients[info.connection]);
                    // Moved to after handshake
                    break;

                case ConnectionState.ClosedByPeer:
                case ConnectionState.ProblemDetectedLocally:
                    if (info.oldState == ConnectionState.Connected || info.oldState == ConnectionState.Connecting)
                    {
                        if (clients.TryGetValue(info.connection, out S_Client client))
                        {
                            if (info.connectionInfo.state == ConnectionState.ProblemDetectedLocally)
                                LogMessage($"{client} closed the connection - ID: " + info.connection + ", IP: " + info.connectionInfo.address.GetIP());
                            else
                                LogMessage($"{client} disconnected - ID: " + info.connection + ", IP: " + info.connectionInfo.address.GetIP());

                            ClientDisconnected_Raise(client);
                            if (client.ID != 0)
                            {
                                S_ClientDisconnected disc = new S_ClientDisconnected(clients[info.connection].ID);
                                disc.SendTo(clients[info.connection], true);
                                if (S_Client.All.ContainsKey(client.ID))
                                    S_Client.All.Remove(client.ID);
                            }
                            clients.Remove(info.connection);
                        }
                        else
                        {
                            LogMessage($"S_Client disconnected - ID: " + info.connection + ", IP: " + info.connectionInfo.address.GetIP());
                        }
                    }

                    socketInterface.CloseConnection(info.connection, 0, "", false);
                    break;
            }
        }

        public void SingleplayerConnect()
        {
            clients[1] = new SocketS_Client(1, this);
            //new S_Handshake().SendTo(clients[1]);
            //Debug.Log("Singleplayer Connect");
        }

        public void OnMessage(in NetworkingMessage netMessage)
        {
            if (NetworkManager.Quitting) return;

            //Debug.Log("Message received from - ID: " + netMessage.connection + ", Channel ID: " + netMessage.channel + ", Data length: " + netMessage.length);
            //netMessage.CopyTo
            //netMessage.
            //Marshal.Copy(data, destination, 0, length);

            ByteBuffer buf = ByteBuffer.Get();
            buf.ReadData(netMessage.data, netMessage.length);
            S_Client c = clients[netMessage.connection];
            //Debug.Log("Got message from " + c + ", conn " + netMessage.connection);

            if (internalHandle.TryGetValue(buf.Peek<uint>(), out var action))
            {
                // Imma be real, idk why I did it this way but yknow
                buf.ReadPosition += 4;
                action(buf, c);
            }
            else
            {
                if (c.ID == 0)
                {
                    Debug.LogWarning($"[SERVER]: Got message from limbo client {c}");
                    return;
                }
                MessageReceived_Raise(c, buf);
                Packet.Handle(buf, c);
            }
        }
    }
}
