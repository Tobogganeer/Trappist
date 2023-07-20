using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Steamworks;
using Steamworks.Data;
using System;

namespace Tobo.Net
{
    public class SteamSocketManager : SocketManager
    {
		internal SteamNetServer server;

		public override void OnConnecting(Connection connection, ConnectionInfo data)
		{
			if (server.clients.Count >= NetworkManager.MaxPlayers)
            {
				Debug.Log($"Attempted connection from {data.Identity.SteamId}, but the server is full!");
				return;
            }

			base.OnConnecting(connection, data);//The base class will accept the connection
			Debug.Log("Incoming server connection...");// from " + new Friend(data.Identity.SteamId).Name);

			server.OnConnecting(connection, data);
		}

		public override void OnConnected(Connection connection, ConnectionInfo data)
		{
			base.OnConnected(connection, data);
			//Debug.Log(new Friend(data.Identity.SteamId).Name + " connected to the server");
			//Debug.Log("Address: " + data.Identity);
			//SteamManager.OnConnectionConnected(connection, data);
			server.OnConnected(connection, data);
		}

		public override void OnDisconnected(Connection connection, ConnectionInfo data)
		{
			base.OnDisconnected(connection, data);
			//Debug.Log(new Friend(data.Identity.SteamId).Name + " left the server.");
			//SteamManager.OnConnectionDisconnected(connection, data);
			server.OnDisconnected(connection, data);
		}

		public override void OnMessage(Connection connection, NetIdentity identity, IntPtr data, int size, long messageNum, long recvTime, int channel)
		{
			// Socket server received message, forward on message to all members of socket server
			//SteamManager.Instance.RelaySocketMessageReceived(data, size, connection.Id);
			//Debug.Log("Socket message received");

			//SteamManager.HandleDataFromClient(connection, identity, data, size);
			server.OnMessage(connection, identity, data, size);
		}
	}
}
