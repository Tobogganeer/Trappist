using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

#if OLD_PACKET_STUFF
public interface IPacket
{
    /*
    /// <summary>
    /// Add this packet to a message
    /// </summary>
    /// <param name="buf"></param>
    void Serialize(ByteBuffer buf);

    /// <summary>
    /// Handle data sent by the server
    /// </summary>
    /// <remarks>WILL BE MADE STATIC BY THE PROCESSOR</remarks>
    /// <param name="buf"></param>
    void Handle(ByteBuffer buf);
    */
}

/*
public interface IServerPacket
{
    
    /// <summary>
    /// Add this packet to a message
    /// </summary>
    /// <param name="buf"></param>
    void Serialize(ByteBuffer buf);

    /// <summary>
    /// Handle data sent by client <paramref name="from"/>
    /// </summary>
    /// <remarks>WILL BE MADE STATIC BY THE PROCESSOR</remarks>
    /// <param name="buf"></param>
    void Handle(ByteBuffer buf, Client from);
    
}
*/

/*
[AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
public class PacketAttribute : Attribute
{
    public readonly ushort ID;

    public PacketAttribute(ushort id)
    {
        ID = id;
    }
}
*/

public enum SendType
{
    Reliable,
    Unreliable
}

public class SomeClientPacket : IPacket
{
    public string message;

    public void Serialize(ByteBuffer buf)
    {

    }

    public void Deserialize(ByteBuffer buf)
    {

    }

    public void Send() { }

    public static void Handle(SomeClientPacket packet)
    {
        Debug.Log(packet.message);
    }
}

public class SomeServerPacket : IPacket
{
    public string message;
    public ushort id = 3;

    public void Serialize(ByteBuffer buf)
    {
        buf.Write(message);
    }

    public static SomeServerPacket Deserialize(ByteBuffer buf)
    {
        SomeServerPacket packet = new SomeServerPacket();
        packet.message = buf.Read();
        return packet;
    }

    ByteBuffer GetBuffer()
    {
        ByteBuffer buf = ByteBuffer.Get();
        buf.Write(id);
        Serialize(buf);
        return buf;
    }

    public void SendToAll()
    {
        NetworkManager.SendBufferToAllClients(GetBuffer());
    }

    public void SendTo(Client client, bool blacklist = false) { }

    public static void Handle(SomeServerPacket packet, Client client)
    {
        Debug.Log(packet.message);
    }
}
/*
public class FullPacket : IPacket
{
    public string data;
    public ushort id = 3;

    public void Serialize(ByteBuffer buf)
    {
        buf.Write(data);
    }

    public static FullPacket Deserialize(ByteBuffer buf)
    {
        FullPacket packet = new FullPacket();
        packet.data = buf.Read();
        return packet;
    }

    ByteBuffer GetBuffer()
    {
        ByteBuffer buf = ByteBuffer.Get();
        buf.Write(id);
        Serialize(buf);
        return buf;
    }

    public void Send()
    {
        NetworkManager.SendBufferToServer(GetBuffer());
    }

    public void SendToAll()
    {
        NetworkManager.SendBufferToAllClients(GetBuffer());
    }

    public void SendTo(Client client, bool blacklist = false)
    {
        if (blacklist)
            NetworkManager.SendBufferToAllClients(GetBuffer(), client);
        else
            NetworkManager.SendBufferToClient(GetBuffer(), client);
    }

    public static void Handle(FullPacket packet, Client client)
    {
        Debug.Log(packet.data);
    }

    public static void Handle(FullPacket packet)
    {
        Debug.Log(packet.data);
    }
}

public class EmptyPacket : IPacket
{
    public string data;

    public void Send() { }

    public void SendToAll() { }

    public void SendTo(Client client, bool blacklist = false) { }

    public static void Handle(FullPacket packet, Client client) { }

    public static void Handle(FullPacket packet) { }
}
*/
#endif

namespace Tobo.Net
{
    public abstract class Packet
    {
        internal static class HashCache<THash>
        {
            // https://github.com/RevenantX/LiteNetLib/blob/a4c61341b90f475b058cd14188d145eaa40544ed/LiteNetLib/Utils/NetSerializer.cs#L677
            // https://github.com/RevenantX/LiteNetLib/blob/master/LiteNetLib/Utils/NetPacketProcessor.cs#L14
            public static readonly ulong IDULong;
            public static readonly uint ID;

            //FNV-1 64 bit hash
            static HashCache()
            {
                ulong hash = 14695981039346656037UL; //offset
                string typeName = typeof(THash).ToString();
                for (var i = 0; i < typeName.Length; i++)
                {
                    hash ^= typeName[i];
                    hash *= 1099511628211UL; //prime
                }
                IDULong = hash;
                ID = (uint)(IDULong >> 32);
            }
        }

        internal static Dictionary<Type, uint> hashes = new Dictionary<Type, uint>();
        internal static Dictionary<uint, Action<ByteBuffer, Args>> handlers = new Dictionary<uint, Action<ByteBuffer, Args>>();

        public abstract void Serialize(ByteBuffer buf);

        public abstract void Deserialize(ByteBuffer buf, Args args);

        public ByteBuffer GetBuffer()
        {
            Type t = GetType();
            if (!hashes.TryGetValue(t, out var hash))
            {
                Debug.LogWarning($"Did not call Packet.Register<{t}>()!");
                throw new ArgumentException("Unregistered packet!", "T");
            }    

            ByteBuffer buf = ByteBuffer.Get();
            buf.Add(hash);
            Serialize(buf);
            return buf;
        }

        public void Send(Client client, SendMode mode = SendMode.Reliable)
        {
            client.Send(this, mode);
            //NetworkManager.SendToServer(this, mode);
        }

        public void SendToAll(SendMode mode = SendMode.Reliable)
        {
            NetworkManager.SendToAll(this, mode);
        }

        public void SendTo(S_Client client, bool blacklist = false, SendMode mode = SendMode.Reliable)
        {
            if (blacklist)
                NetworkManager.SendToAll(this, client, mode);
            else
                NetworkManager.SendTo(this, client, mode);
        }


        internal static void Handle(ByteBuffer packet, S_Client client)
        {
            handlers[packet.Read<uint>()]?.Invoke(packet, new Args(client, true));
        }

        internal static void Handle(ByteBuffer packet)
        {
            handlers[packet.Read<uint>()]?.Invoke(packet, new Args());
        }

        public static void Register<TPacket>() where TPacket : Packet, new()
        {
            hashes[typeof(TPacket)] = HashCache<TPacket>.ID;
            handlers[HashCache<TPacket>.ID] = (buf, args) =>
            {
                //TPacket t = new TPacket();
                TPacket t = FastActivator<TPacket>.Create();
                t.Deserialize(buf, args);
            };
        }

        public struct Args
        {
            public S_Client from;
            public bool ServerSide { get; private set; }
            //public Client Client => Client.All[clientID] ?? null;

            public Args(S_Client from, bool serverSide)
            {
                this.from = from;
                this.ServerSide = serverSide;
            }
        }
    }

    [Flags]
    public enum SendMode
    {
        Unreliable = 0,
        NoNagle = 1 << 0,
        NoDelay = 1 << 2,
        Reliable = 1 << 3
    }

    // https://eddieabbondanz.io/post/unity/litenetlib-sending-data/
    // https://github.com/RevenantX/LiteNetLib/blob/a4c61341b90f475b058cd14188d145eaa40544ed/LiteNetLib/Utils/NetSerializer.cs#L677
}