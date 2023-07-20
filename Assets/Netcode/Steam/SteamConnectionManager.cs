using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Steamworks;
using Steamworks.Data;
using System;

namespace Tobo.Net
{
    public class SteamConnectionManager : ConnectionManager
    {
        internal SteamNetClient client;

        public override void OnConnected(ConnectionInfo info)
        {
            base.OnConnected(info);
            client.OnConnected(info);
            //Debug.Log($"Connected to {new Friend(info.Identity.SteamId).Name}");
            //SteamManager.OnConnConnectedToServer(info);
        }

        public override void OnConnecting(ConnectionInfo info)
        {
            base.OnConnecting(info);
            client.OnConnecting(info);
            //client.OnConnecting();
            //Debug.Log($"Connecting to {new Friend(info.Identity.SteamId).Name}");
        }

        public override void OnDisconnected(ConnectionInfo info)
        {
            base.OnDisconnected(info);
            client.OnDisconnected(info);
            //Debug.Log($"Disconnected from server");
            //SteamManager.ConnectionClosed();
            //SteamManager.Leave();
        }

        public override void OnMessage(IntPtr data, int size, long messageNum, long recvTime, int channel)
        {
            client.OnMessage(data, size);
            //SteamManager.HandleDataFromServer(data, size);
        }
    }
}
