using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Steamworks;
using Steamworks.Data;
using System.Runtime.InteropServices;

namespace Tobo.Net
{
    public class SteamNetClient : Client
    {
        //internal NetworkingSockets socketInterface;
        //internal uint connection;
        //IntPtr statusPtr;
        //StatusCallback callback;
        //private static SteamSocketManager socketServer;

        //internal Connection connection;
        internal SteamConnectionManager connectionToServer;
        internal SteamId steamID;
        //private static AuthTicket currentAuthTicket;

        internal SteamNetClient() : base()
        {
            //socketInterface = new NetworkingSockets();
            //callback = StatusChanged;
            //statusPtr = Marshal.GetFunctionPointerForDelegate(callback);
        }

        public override void Connect(string username, string serverId, ushort port = 0)
        {
            /*
            socketInterface ??= new NetworkingSockets();

            if (connection != 0)
            {
                socketInterface?.CloseConnection(connection, 0, "", false);
                connection = 0;
            }

            IsConnected = false;
            connecting = false;
            ID = 0;
            this.Username = username;

            Configuration cfg = new Configuration();
            cfg.dataType = ConfigurationDataType.FunctionPtr;
            cfg.value = ConfigurationValue.ConnectionStatusChanged;
            cfg.data.FunctionPtr = statusPtr;

            Address address = new Address();

            address.SetAddress(ip, port);

            connection = socketInterface.Connect(ref address, new Configuration[] { cfg });
            if (connection == 0)
                LogMessage("Failed to connect!");
            */

            IsConnected = false;
            connecting = false;
            ID = 0;
            steamID = SteamManager.SteamID;
            this.Username = username;

            if (!ulong.TryParse(serverId, out ulong targetID))
            {
                Debug.LogError("Tried to connect to invalid SteamID: " + serverId);
                NetworkManager.Disconnect();
                return;
            }

            Debug.Log("Connecting to " + targetID);
            connectionToServer = SteamNetworkingSockets.ConnectRelay<SteamConnectionManager>(targetID, 0);
            connectionToServer.client = this;
        }

        public override void Update()
        {
            //socketInterface?.RunCallbacks();
            //socketInterface?.ReceiveMessagesOnConnection(connection, OnMessage, 20);
            connectionToServer?.Receive();
        }

        protected override void Disconnect_Internal()
        {
            if (connectionToServer != null)
            {
                connectionToServer.Close();// (connection, 0, "Disconnect", true);
                connectionToServer = null;
                //connection = 0;
                //Disconnected_Raise(); // Not being invoked by status callback
                // Maybe, test for steam
            }
        }

        internal override void Destroy()
        {
            if (connectionToServer != null)
            {
                connectionToServer.Close(); // (connection, 0, "", false);
                connectionToServer = null;
            }
        }


        public override void Send(Packet packet, SendMode mode = SendMode.Reliable)
        {
            if (connectionToServer == null)
            {
                Debug.LogWarning("Client send fail.");
                return;
            }

            (IntPtr buf, int size) = NetworkManager.Prepare(packet.GetBuffer());
            try
            {
                //Debug.Log($"Sending ");
                //NetworkManager.SendBuffer(buf, size, connection, socketInterface, mode);
                NetworkManager.SendBuffer(buf, size, connectionToServer.Connection, mode);
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                NetworkManager.Free(buf);
            }
        }

        internal void OnConnected(ConnectionInfo info)
        {
            // Do nothing I guess
        }

        internal void OnConnecting(ConnectionInfo info)
        {
            if (connecting)
            {
                LogMessage("Steam double connect?: " + info.EndReason);
                connectionToServer.Close();
                //socketInterface.CloseConnection(info.connection, 0, "", false);
                connectionToServer = null;
                IsConnected = false;
                ConnectionFailed_Raise();
                return;
            }
            connecting = true;
        }

        internal void OnDisconnected(ConnectionInfo info)
        {
            if (NetworkManager.Quitting) return;

            /*
            if (info.State == ConnectionState.Connecting)
            {
                LogMessage("Could not connect to server: " + info.connectionInfo.endDebug);
                ConnectionFailed_Raise();
            }
            else if (info.oldState == ConnectionState.ProblemDetectedLocally)
            {
                LogMessage("Lost contact with server: " + info.connectionInfo.endDebug);
                Disconnected_Raise();
            }
            else
            {
                LogMessage("Connection closed: " + info.connectionInfo.endDebug);
                Disconnected_Raise();
            }
            */

            LogMessage("Steam connection closed: " + info.EndReason);
            Disconnected_Raise();

            //Debug.Log("Client disconnected - ID: " + info.connection + ", IP: " + info.connectionInfo.address.GetIP());
            connectionToServer.Close();
            connectionToServer = null;
            IsConnected = false;

            //NetworkManager.Disconnect();
        }

        internal void OnMessage(IntPtr data, int size)
        {
            if (NetworkManager.Quitting) return;

            //Debug.Log("Message received from SERVER: " + netMessage.connection + ", Channel ID: " + netMessage.channel + ", Data length: " + netMessage.length);
            ByteBuffer buf = ByteBuffer.Get();
            buf.ReadData(data, size);
            //Debug.Log($"GOT SERVER MES: {buf.Peek<uint>()} -> {Packet.HashCache<S_Welcome>.ID}");

            if (size < 4)
            {
                Debug.LogError("Error: Received message with length <4!");
                return;
            }

            if (internalHandle.TryGetValue(buf.Peek<uint>(), out var action))
            {
                buf.ReadPosition += 4; // Size of uint
                action(buf);
            }
            else
            {
                MessageReceived_Raise(buf);
                Packet.Handle(buf);
            }
            //MessageReceived?.Invoke(buf);
        }
    }

    public class SteamNetS_Client : S_Client
    {
        internal Connection connection;
        //internal SteamId steamID;

        internal SteamNetS_Client(Connection connection)
        {
            this.connection = connection;
        }

        public override void Kick(string reason)
        {
            //NetworkManager.Instance.server.socketInterface.CloseConnection(connection, 0, reason, true);
            connection.Close(true, 0, reason);
            connection = 0;
        }

        internal override void Send(IntPtr buf, int size, SendMode sendMode)
        {
            NetworkManager.SendBuffer(buf, size, connection, sendMode);
        }
    }
}
