using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Tobo.Net;

[RequireComponent(typeof(Player))]  
public class NetTransform : MonoBehaviour
{
    // CURRENTLY ASSUMES ATTACHED TO PLAYER

    public int syncsPerSecond = 0;

    Vector3 lastPosition = Vector3.zero;
    Quaternion lastRotation = Quaternion.identity;
    Vector3 lastScale = Vector3.one;
    float lastSyncTime = 0;

    Player player;

    bool LocalPlayer => player.ID == NetworkManager.MyID;

    private float syncDelay => 1f / syncsPerSecond;
    private float currentInterpolation
    {
        get
        {
            float difference = target.time - current.time;

            float elapsed = Time.time - target.time;
            return difference > 0 ? elapsed / difference : 0;
            // Thanks mirror for this useful bit of code
        }
    }

    private readonly TransformSnapshot current = new TransformSnapshot();
    private readonly TransformSnapshot target = new TransformSnapshot();

    private void Start()
    {
        if (!NetworkManager.ConnectedToServer)
        {
            Destroy(this);
            return;
        }

        player = GetComponent<Player>();

        if (!LocalPlayer)
        {
            current.Update(transform.localPosition, transform.localRotation, transform.localScale, Time.time - syncDelay);
            target.Update(transform.localPosition, transform.localRotation, transform.localScale, Time.time);
        }

        ForceSendTransform();
    }

    const float ForceUpdateTime = 1f; // Every second

    private void Update()
    {
        if (!LocalPlayer)
        {
            UpdateTransform();
            return;
        }

        lastSyncTime -= Time.deltaTime;

        if (lastSyncTime <= 0)
        {
            TransformUpdateFlags flags = GetFlags();

            if (flags != TransformUpdateFlags.None)
            {
                lastSyncTime = 1f / syncsPerSecond;
                NetTransformPacket send = new NetTransformPacket(player.ID, lastPosition, lastRotation, lastScale, flags);
                NetworkManager.SendToServer(send);
            }
            else if (lastSyncTime < -ForceUpdateTime) // Force update every second at least
            {
                lastSyncTime = 1f / syncsPerSecond;
                ForceSendTransform();
            }
        }
    }

    void ForceSendTransform()
    {
        lastPosition = transform.localPosition;
        lastRotation = transform.localRotation;
        lastScale = transform.localScale;
        NetTransformPacket send = new NetTransformPacket(player.ID, lastPosition, lastRotation, lastScale, TransformUpdateFlags.All);
        NetworkManager.SendToServer(send);
    }

    void UpdateTransform()
    {
        transform.localPosition = Vector3.Lerp(current.position, target.position, currentInterpolation);
        transform.localRotation = Quaternion.Slerp(current.rotation, target.rotation, currentInterpolation);
        transform.localScale = Vector3.Lerp(current.scale, target.scale, currentInterpolation);
    }

    public void NewTransformReceived(Vector3 position, Quaternion rotation, Vector3 scale, TransformUpdateFlags flags)
    {
        if (!flags.HasFlag(TransformUpdateFlags.Position))
            position = target.position;
        if (!flags.HasFlag(TransformUpdateFlags.Rotation))
            rotation = target.rotation;
        if (!flags.HasFlag(TransformUpdateFlags.Scale))
            scale = target.scale;

        current.Update(target.position, target.rotation, target.scale, Time.time - syncDelay);

        target.Update(position, rotation, scale, Time.time);
    }

    private TransformUpdateFlags GetFlags()
    {
        TransformUpdateFlags flags = TransformUpdateFlags.None;

        if (HasMoved())
            flags |= TransformUpdateFlags.Position;

        if (HasRotated())
            flags |= TransformUpdateFlags.Rotation;

        if (HasScaled())
            flags |= TransformUpdateFlags.Scale;

        return flags;
    }

    #region Checks

    private bool HasMoved()
    {
        Vector3 currentPos = transform.localPosition;
        bool changed = Vector3.Distance(lastPosition, currentPos) > 0.01f;
        if (changed)
            lastPosition = currentPos;

        return changed;
    }

    private bool HasRotated()
    {
        Quaternion currentRot = transform.localRotation;
        bool changed = Quaternion.Angle(lastRotation, currentRot) > 0.1f;
        if (changed)
            lastRotation = currentRot;

        return changed;
    }

    private bool HasScaled()
    {
        Vector3 currentScale = transform.localScale;
        bool changed = Vector3.Distance(lastScale, currentScale) > 0.01f;
        if (changed)
            lastScale = currentScale;

        return changed;
    }

    #endregion


    public class TransformSnapshot
    {
        public Vector3 position;
        public Quaternion rotation;
        public Vector3 scale;
        public float time;

        public void Update(Vector3 position, Quaternion rotation, Vector3 scale, float time)
        {
            this.position = position;
            this.rotation = rotation;
            this.scale = scale;
            this.time = time;
        }
    }

    [System.Flags]
    public enum TransformUpdateFlags : byte
    {
        None = 0,
        Position = 1 << 0,
        Rotation = 1 << 1,
        Scale = 1 << 2,
        All = Position | Rotation | Scale
    }
}

public class NetTransformPacket : Packet
{
    public ushort id;
    public Vector3 position;
    public Quaternion rot;
    public Vector3 scale;
    public NetTransform.TransformUpdateFlags flags;
    //public Vector3 velocity;

    public NetTransformPacket() { }
    //public PlayerPositionPacket(ushort id, Vector3 position, Vector3 velocity)
    public NetTransformPacket(ushort id, Vector3 position, Quaternion rot, Vector3 scale, NetTransform.TransformUpdateFlags flags)
    {
        this.id = id;
        this.position = position;
        this.rot = rot;
        this.scale = scale;
        this.flags = flags;
    }

    public override void Serialize(ByteBuffer buf)
    {
        buf.Add(id);
        buf.Add((byte)flags);

        if (flags.HasFlag(NetTransform.TransformUpdateFlags.Position))
            buf.Add(position);
        if (flags.HasFlag(NetTransform.TransformUpdateFlags.Rotation))
            ByteBufferExtensions.Add(buf, rot); // Compressed
        if (flags.HasFlag(NetTransform.TransformUpdateFlags.Scale))
            buf.Add(scale);
    }

    public override void Deserialize(ByteBuffer buf, Args args)
    {
        id = buf.Read<ushort>();
        flags = (NetTransform.TransformUpdateFlags)buf.GetByte();

        if (flags.HasFlag(NetTransform.TransformUpdateFlags.Position))
            position = buf.Read<Vector3>();
        if (flags.HasFlag(NetTransform.TransformUpdateFlags.Rotation))
            rot = buf.GetQuaternion(); // Compressed
        if (flags.HasFlag(NetTransform.TransformUpdateFlags.Scale))
            scale = buf.Read<Vector3>();

        if (args.ServerSide)
        {
            NetTransformPacket bounce = new NetTransformPacket(id, position, rot, scale, flags);
            bounce.SendTo(args.from, true);
        }
        else
        {
            if (Player.All.TryGetValue(id, out Player p))
            {
                p.netTransform.NewTransformReceived(position, rot, scale, flags);
            }
            //p.rb.position = position;
            //p.rb.velocity = velocity;
        }
    }
}