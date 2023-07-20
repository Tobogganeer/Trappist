using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Steamworks;
using Steamworks.Data;
using System.Runtime.InteropServices;
using System;
using System.Threading.Tasks;

namespace Tobo.Net
{
    public class SteamNetServer : Server
    {
        internal SteamSocketManager socketServer;
        internal Lobby currentLobby;
        internal SteamId lobbyOwnerID => currentLobby.Owner.Id;
        public static SteamLobbyPrivacyMode LobbyPrivacyOnCreation = SteamLobbyPrivacyMode.FriendsOnly;

        public SteamNetServer() : base()
        {
            //callback = StatusChanged;
            //statusPtr = Marshal.GetFunctionPointerForDelegate(callback);
            InitSteamEvents();
        }

        private void InitSteamEvents()
        {
            SteamFriends.OnGameLobbyJoinRequested += OnLobbyJoinRequested;
            SteamMatchmaking.OnLobbyEntered += OnLobbyEntered;
            //SteamMatchmaking.OnLobbyMemberJoined += SteamMatchmaking_OnLobbyMemberJoined;
            SteamMatchmaking.OnLobbyMemberLeave += OnMemberLeftLobby;
            //SteamUser.OnValidateAuthTicketResponse += SteamUser_OnValidateAuthTicketResponse;
        }

        public override async void Run(ushort port = 0)
        {
            if (Started)
                Stop();

            socketServer = SteamNetworkingSockets.CreateRelaySocket<SteamSocketManager>(0);
            socketServer.server = this;

            nextPlayerID = 1;
            clients = new Dictionary<uint, S_Client>();

            SteamNetworkingUtils.Timeout = 2000;

            if (!await CreateLobby(NetworkManager.MaxPlayers, LobbyPrivacyOnCreation, true))
            {
                Debug.Log("Error starting server: Lobby not created successfully.");
                Stop();
                return;
            }

            Started = true;
        }

        public override void Update()
        {
            if (socketServer != null)
                socketServer.Receive();
        }

        protected override void Stop_Internal()
        {
            Destroy_Internal();

            //LeaveCurrentLobby();
            //socketServer?.Close();
            //socketServer = null;
        }

        protected override void Destroy_Internal()
        {
            //LeaveCurrentLobby();
            socketServer?.Close();
            socketServer = null;
            if (currentLobby.Id.IsValid)
                currentLobby.Leave();
            currentLobby = default;
        }

        /*
        void StatusChanged(ref StatusInfo info)
        {
            if (NetworkManager.Quitting) return;

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
                    clients[info.connection] = new SocketS_Client(info.connection);
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
                        S_Client client = clients[info.connection];
                        if (info.connectionInfo.state == ConnectionState.ProblemDetectedLocally)
                            LogMessage($"{client} closed the connection - ID: " + info.connection + ", IP: " + info.connectionInfo.address.GetIP());
                        else
                            LogMessage($"{client} disconnected - ID: " + info.connection + ", IP: " + info.connectionInfo.address.GetIP());

                        // VVV change so only called if client is full joined?
                        ClientDisconnected_Raise(client);
                        if (clients[info.connection].ID != 0)
                        {
                            S_ClientDisconnected disc = new S_ClientDisconnected(clients[info.connection].ID);
                            disc.SendTo(clients[info.connection], true);
                        }
                        clients.Remove(info.connection);
                    }

                    socketInterface.CloseConnection(info.connection, 0, "", false);
                    break;
            }
        }
        */

        /*
        void OnMessage(in NetworkingMessage netMessage)
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
        */

        internal void OnMessage(Connection connection, NetIdentity identity, IntPtr data, int size)
        {
            if (NetworkManager.Quitting) return;

            ByteBuffer buf = ByteBuffer.Get();
            buf.ReadData(data, size);
            S_Client c = clients[connection];
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

        internal void OnConnecting(Connection connection, ConnectionInfo data)
        {
            //LogMessage("Incoming connection (connecting) from " + data.Identity.SteamId);
            // Message logged in calling class

            if (NetworkManager.Quitting) return;

            if (clients.ContainsKey(connection))
            {
                Debug.LogError("[SERVER]: Clients list already contains " + connection + "!");
                //socketInterface.CloseConnection(info.connection, 0, "", false);
                //break;
                connection.Close();
                return;
            }

            /*
            if (clients.Count >= NetworkManager.Instance.maxPlayers)
            {
                LogMessage("Rejecting " + info.connectionInfo.connectionDescription + ", max players reached.");
                socketInterface.CloseConnection(info.connection, 0, "", false);
                break;
            }
            */

            // Done in method ^^^

            /*
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
            */

            // No need ig? ^^^

            clients[connection] = new SteamNetS_Client(connection);
        }

        internal void OnConnected(Connection connection, ConnectionInfo data)
        {
            if (NetworkManager.Quitting) return;

            LogMessage("Incoming connection from " + data.Identity);

            new S_Handshake().SendTo(clients[connection]);
        }

        internal void OnDisconnected(Connection connection, ConnectionInfo data)
        {
            if (NetworkManager.Quitting) return;

            //if (data.oldState == ConnectionState.Connected || info.oldState == ConnectionState.Connecting)
            if (clients.TryGetValue(connection, out S_Client client))
            {
                if (data.State == ConnectionState.ProblemDetectedLocally)
                    LogMessage($"{client} closed the connection - ID: " + connection + ", SteamID: " + data.Identity.SteamId);
                else
                    LogMessage($"{client} disconnected - ID: " + connection + ", SteamID: " + data.Identity.SteamId);

                // VVV change so only called if client is full joined?
                ClientDisconnected_Raise(client);
                if (clients[connection].ID != 0)
                {
                    S_ClientDisconnected disc = new S_ClientDisconnected(clients[connection].ID);
                    disc.SendTo(clients[connection], true);
                    if (S_Client.All.ContainsKey(client.ID))
                        S_Client.All.Remove(client.ID);
                }
                clients.Remove(connection);
            }
            else
            {
                LogMessage($"S_Client disconnected - ID: " + connection + ", SteamID: " + data.Identity.SteamId);
            }

            //socketInterface.CloseConnection(info.connection, 0, "", false);
            //break;
            connection.Close();
        }

        internal void JoinLobby(Lobby lobby)
        {
            //lobby.Join();
            OnLobbyJoinRequested(lobby, lobby.Owner.Id);
        }

        private void OnLobbyEntered(Lobby lobby)
        {
            currentLobby = lobby;

            NetworkManager.Join(NetworkManager.MySteamName, lobby.Owner.Id.Value.ToString());
        }

        internal async void OnLobbyJoinRequested(Lobby lobby, SteamId id)
        {
            /*
            if (!ShouldJoinGame(lobby))
            {
                Debug.Log($"Tried joining {id.SteamName()} ({id}), but that attempt was rejected!");
                return;
            }
            */

            if (NetworkManager.ConnectedToServer)
                NetworkManager.Disconnect();

            //OnLobbyJoinStarted?.Invoke();

            RoomEnter result = await lobby.Join();
            if (result != RoomEnter.Success)
            {
                Debug.LogError($"Tried to join {id.SteamName()}'s lobby, but was not successful! Result: " + result);

                await Task.Delay(100);

                if (await lobby.Join() != RoomEnter.Success)
                {
                    Debug.LogError("Second attempt failed too!");
                    //OnLobbyJoinFailed?.Invoke();
                    //Leave();
                    NetworkManager.Disconnect();
                    return;
                }
            }
            currentLobby = lobby;
        }

        private void OnMemberLeftLobby(Lobby lobby, Friend friend)
        {
            if (lobby.IsOwnedBy(friend.Id))
                NetworkManager.Disconnect();
        }


        private async Task<bool> CreateLobby(int maxPlayers = 8, SteamLobbyPrivacyMode mode = SteamLobbyPrivacyMode.FriendsOnly, bool joinable = true)
        {
            Lobby? lobby = await SteamMatchmaking.CreateLobbyAsync(maxPlayers);
            if (lobby.HasValue)
            {
                currentLobby = lobby.Value;
                SetLobbyPrivacyMode(currentLobby, mode);
                currentLobby.SetJoinable(joinable);

                return true;
            }
            else
            {
                return false;
                //throw new NullReferenceException("Lobby created but returned null value!");
            }
        }

        private static void SetLobbyPrivacyMode(Lobby lobby, SteamLobbyPrivacyMode mode)
        {
            switch (mode)
            {
                case SteamLobbyPrivacyMode.Public:
                    lobby.SetPublic();
                    break;
                case SteamLobbyPrivacyMode.Private:
                    lobby.SetPrivate();
                    break;
                case SteamLobbyPrivacyMode.Invisible:
                    lobby.SetInvisible();
                    break;
                case SteamLobbyPrivacyMode.FriendsOnly:
                    lobby.SetFriendsOnly();
                    break;
            }
        }

        //private void LeaveCurrentLobby()
        //{
        //    if (CurrentLobby.Id.IsValid)
        //    {
        //        CurrentLobby.Leave();
        //        OnLobbyLeft?.Invoke(CurrentLobby);
        //        CurrentLobby = default;
        //    }
        //}
    }

    public enum SteamLobbyPrivacyMode
    {
        Public,
        Private,
        Invisible,
        FriendsOnly,
    }
}
