using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Tobo.Net
{
    /*
    
    - Client connects
    - Server handshake
    - Client handshake + password
    - Reject or accept
    - Welcome packet + other clients + id

    S_Handshake
    C_Handshake
    S_Welcome
    Ping
    S_ClientConnected
    S_ClientDisconnected

    */

    internal class S_Handshake : Packet
    {
        public override void Serialize(ByteBuffer buf) { }

        public override void Deserialize(ByteBuffer buf, Args args) { }
    }

    internal class C_Handshake : Packet
    {
        public string username;
        const int MaxStringLength = 64;

        public C_Handshake() { }
        public C_Handshake(string username)
        {
            this.username = username;
        }

        public override void Serialize(ByteBuffer buf)
        {
            if (username.Length > MaxStringLength)
                throw new System.ArgumentException("Max username length: " + MaxStringLength, "username");

            buf.AddString(username);
            NetworkManager.Instance.AddConnectData(buf);
            //Debug.Log("SEND CHAND: " + buf.Dump());
        }

        public override void Deserialize(ByteBuffer buf, Args args)
        {
            username = buf.Read();
            //Debug.Log("HAND CHAND: " + buf.Dump());
            // Will then be passed to server to accept or reject
        }
    }

    internal class S_Welcome : Packet
    {
        public ushort id;
        public ushort[] otherClientIDs;
        public string[] otherClientNames;

        public S_Welcome() { }
        public S_Welcome(ushort id)
        {
            this.id = id;

            int numOtherClients = NetworkManager.Instance.server.clients.Count - 1; // Don't include ourselves

            otherClientIDs = new ushort[numOtherClients];
            otherClientNames = new string[numOtherClients];

            int i = 0;
            foreach (var client in NetworkManager.Instance.server.clients.Values)
            {
                if (client.ID != id)
                {
                    otherClientIDs[i] = client.ID;
                    otherClientNames[i] = client.Username;
                    i++;
                }
            }
        }

        public override void Serialize(ByteBuffer buf)
        {
            // 176 190 158 23 1 0 0 0 0 0     FIRST
            // 176 190 158 23 1 0 1 0 0 0 2 0 0 0    SECOND
            // PACKET^^^^^^^^ ID^ CLIENT^ CID STR

            // NEW
            // 176 190 158 23 2 0 1 0 0 0 0 0 0 0
            buf.Add(id);
            buf.Add(otherClientIDs.Length);

            for (int i = 0; i < otherClientIDs.Length; i++)
            {
                //Debug.Log($"ADDING: '{otherClientNames[i]}' ({otherClientIDs[i]})");
                buf.Add(otherClientIDs[i]);
                buf.AddString(otherClientNames[i]);
            }

            //Debug.Log("SEND: " + buf.Dump());
            //buf.Write(otherClientIDs);
            //buf.Write(otherClientNames);
        }

        public override void Deserialize(ByteBuffer buf, Args args)
        {
            //Debug.Log("RECV: " + buf.Dump());

            id = buf.Read<ushort>();
            int num = buf.Read<int>();
            otherClientIDs = new ushort[num];
            otherClientNames = new string[num];

            for (int i = 0; i < num; i++)
            {
                otherClientIDs[i] = buf.Read<ushort>();
                otherClientNames[i] = buf.Read();
            }

            /*
            if (num == 0)
            {
                otherClientIDs = new ushort[0];
                otherClientNames = new string[0];
            }
            else
            {
                otherClientIDs = buf.ReadArray<ushort>();
                otherClientNames = buf.ReadStrArray();
            }
            */
        }
    }

    internal class Ping : Packet
    {
        public override void Serialize(ByteBuffer buf) { }

        public override void Deserialize(ByteBuffer buf, Args args) { }
    }

    internal class S_ClientConnected : Packet
    {
        public ushort id;
        public string name;

        public S_ClientConnected() { }
        public S_ClientConnected(ushort id, string name)
        {
            this.id = id;
            this.name = name;
        }

        public override void Serialize(ByteBuffer buf)
        {
            buf.Add(id);
            buf.AddString(name);
        }

        public override void Deserialize(ByteBuffer buf, Args args)
        {
            id = buf.Read<ushort>();
            name = buf.Read();
        }
    }

    internal class S_ClientDisconnected : Packet
    {
        public ushort id;

        public S_ClientDisconnected() { }
        public S_ClientDisconnected(ushort id)
        {
            this.id = id;
        }

        public override void Serialize(ByteBuffer buf)
        {
            buf.Add(id);
        }

        public override void Deserialize(ByteBuffer buf, Args args)
        {
            id = buf.Read<ushort>();
        }
    }

    /*
    
    public class PACKET_NAME : Packet
    {
        public override void Serialize(ByteBuffer buf)
        {

        }

        public override void Deserialize(ByteBuffer buf, Args args)
        {

        }
    }
    
    */
}
