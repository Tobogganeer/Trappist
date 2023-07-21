using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Tobo.Net;

public class PlayerAnimation : MonoBehaviour
{
    public Animator animator;
    public float transitionSpeed = 5f;

    public PlayerAnimatorUpdateMode updateMode;
    public int updatesPerSecond = 10;

    static readonly string Grounded = "grounded";
    static readonly string XVar = "x";
    static readonly string YVar = "y";

    float x;
    float y;

    float time;

    private void Update()
    {
        if (updateMode == PlayerAnimatorUpdateMode.Remote) return;

        x = Mathf.Lerp(x, PlayerMovement.LocalVelocity.x, Time.deltaTime * transitionSpeed);
        y = Mathf.Lerp(y, PlayerMovement.LocalVelocity.z, Time.deltaTime * transitionSpeed);

        animator.SetBool(Grounded, PlayerMovement.Grounded);
        animator.SetFloat(XVar, x);
        animator.SetFloat(YVar, y);

        if (updateMode == PlayerAnimatorUpdateMode.Local) return;

        time -= Time.deltaTime;
        if (time <= 0)
        {
            if (NetworkManager.ConnectedToServer)
                new PlayerAnimationPacket(NetworkManager.MyID, x, y, PlayerMovement.Grounded).Send(Client.This);
            time = 1f / updatesPerSecond;
        }
    }

    public void Set(float x, float y, bool grounded)
    {
        animator.SetBool(Grounded, grounded);
        animator.SetFloat(XVar, x);
        animator.SetFloat(YVar, y);
    }

    public enum PlayerAnimatorUpdateMode
    {
        LocalAndNetwork,
        Local,
        Remote
    }
}

public class PlayerAnimationPacket : Packet
{
    ushort id;
    float x;
    float y;
    bool grounded;

    public PlayerAnimationPacket() { }

    public PlayerAnimationPacket(ushort id, float x, float y, bool grounded)
    {
        this.id = id;
        this.x = x;
        this.y = y;
        this.grounded = grounded;
    }

    public override void Serialize(ByteBuffer buf)
    {
        buf.Add(id);
        buf.Add(x);
        buf.Add(y);
        buf.Add(grounded);
    }

    public override void Deserialize(ByteBuffer buf, Args args)
    {
        id = buf.Read<ushort>();
        x = buf.Read<float>();
        y = buf.Read<float>();
        grounded = buf.Read<bool>();

        if (args.ServerSide)
        {
            new PlayerAnimationPacket(id, x, y, grounded).SendTo(S_Client.All[id], true);
        }
        else
        {
            if (Player.TryGet(id, out Player p))
                p.animator.Set(x, y, grounded);
        }
    }
}
