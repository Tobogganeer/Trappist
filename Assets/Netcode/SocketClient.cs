using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Valve.Sockets;
using System.Runtime.InteropServices;

namespace Tobo.Net
{
    public class SocketClient : Client
    {
        internal NetworkingSockets socketInterface;
        internal uint connection;
        IntPtr statusPtr;
        StatusCallback callback;

        internal SocketClient() : base()
        {
            if (!NetworkManager.SinglePlayer)
                socketInterface = new NetworkingSockets();
            callback = StatusChanged;
            statusPtr = Marshal.GetFunctionPointerForDelegate(callback);
        }

        public override void Connect(string username, string ip = "::0", ushort port = 26950)
        {
            if (!NetworkManager.SinglePlayer)
            {
                socketInterface ??= new NetworkingSockets();

                if (connection != 0)
                {
                    socketInterface?.CloseConnection(connection, 0, "", false);
                    connection = 0;
                }
            }

            IsConnected = false;
            connecting = false;
            ID = 0;
            Username = username;

            if (!NetworkManager.SinglePlayer)
            {
                Configuration cfg = new Configuration();
                cfg.dataType = ConfigurationDataType.FunctionPtr;
                cfg.value = ConfigurationValue.ConnectionStatusChanged;
                cfg.data.FunctionPtr = statusPtr;

                Address address = new Address();

                address.SetAddress(ip, port);

                connection = socketInterface.Connect(ref address, new Configuration[] { cfg });
                if (connection == 0)
                    LogMessage("Failed to connect!");
            }
            else
            {
                connecting = true;
                (NetworkManager.Instance.server as SocketServer).SingleplayerConnect();
                LogMessage("Connecting to virtual server...");
                C_Handshake handshake = new C_Handshake(Username);
                Send(handshake);
            }
        }

        public override void Update()
        {
            socketInterface?.RunCallbacks();
            socketInterface?.ReceiveMessagesOnConnection(connection, OnMessage, 20);
        }

        protected override void Disconnect_Internal()
        {
            if (connection != 0)
            {
                socketInterface?.CloseConnection(connection, 0, "Disconnect", true);
                connection = 0;
                if (IsConnected)
                    Disconnected_Raise(); // Not being invoked by status callback
                // Now raised by status callback lol
                // ^^^ Only if not called from NetworkManager.Disconnect();
            }
        }

        internal override void Destroy()
        {
            if (connection != 0)
            {
                socketInterface?.CloseConnection(connection, 0, "", false);
            }

            socketInterface = null;
            connection = 0;
        }


        public override void Send(Packet packet, SendMode mode = SendMode.Reliable)
        {
            if (!NetworkManager.SinglePlayer && (connection == 0 || socketInterface == null))
            {
                Debug.LogWarning("Client send fail.");
                return;
            }

            (IntPtr buf, int size) = NetworkManager.Prepare(packet.GetBuffer());
            try
            {
                //Debug.Log($"Sending ");
                if (NetworkManager.SinglePlayer)
                {
                    NetworkingMessage m = new NetworkingMessage();
                    m.data = buf;
                    m.length = size;
                    m.connection = 1;
                    (NetworkManager.Instance.server as SocketServer).OnMessage(m);
                }
                else
                {
                    NetworkManager.SendBuffer(buf, size, connection, socketInterface, mode);
                }
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

        void StatusChanged(ref StatusInfo info)
        {
            if (NetworkManager.Quitting) return;

            switch (info.connectionInfo.state)
            {
                case ConnectionState.None:
                    break;

                case ConnectionState.Connecting:
                    if (connecting)
                    {
                        LogMessage("Double connect?: " + info.connectionInfo.endDebug);
                        socketInterface.CloseConnection(info.connection, 0, "", false);
                        connection = 0;
                        IsConnected = false;
                        ConnectionFailed_Raise();
                        break;
                    }
                    connecting = true;
                    //Debug.Log("Connecting to server");
                    break;

                case ConnectionState.Connected:
                    //Debug.Log("Connected to server");
                    //Connected?.Invoke();
                    break;

                case ConnectionState.ClosedByPeer:
                case ConnectionState.ProblemDetectedLocally:
                    if (info.oldState == ConnectionState.Connecting)
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

                    //Debug.Log("Client disconnected - ID: " + info.connection + ", IP: " + info.connectionInfo.address.GetIP());
                    socketInterface.CloseConnection(info.connection, 0, "", false);
                    connection = 0;
                    IsConnected = false;
                    break;
            }
        }

        public void OnMessage(in NetworkingMessage netMessage)
        {
            if (NetworkManager.Quitting) return;

            //Debug.Log("Message received from SERVER: " + netMessage.connection + ", Channel ID: " + netMessage.channel + ", Data length: " + netMessage.length);
            ByteBuffer buf = ByteBuffer.Get();
            buf.ReadData(netMessage.data, netMessage.length);
            //Debug.Log($"GOT SERVER MES: {buf.Peek<uint>()} -> {Packet.HashCache<S_Welcome>.ID}");

            if (netMessage.length < 4)
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

    public class SocketS_Client : S_Client
    {
        internal uint connection;
        SocketServer server;

        internal SocketS_Client(uint connection, SocketServer server)
        {
            this.connection = connection;
            this.server = server;
        }

        public override void Kick(string reason)
        {
            server.socketInterface.CloseConnection(connection, 0, reason, true);
            connection = 0;
        }

        internal override void Send(IntPtr buf, int size, SendMode sendMode)
        {
            if (NetworkManager.SinglePlayer)
            {
                NetworkingMessage m = new NetworkingMessage();
                m.data = buf;
                m.length = size;
                //m.connection = 1;
                (NetworkManager.Instance.client as SocketClient).OnMessage(m);
                //(Client.All[1] as SocketClient).OnMessage(m);
            }
            else
            {
                NetworkManager.SendBuffer(buf, size, connection, server.socketInterface, sendMode);
            }
        }
    }
}
