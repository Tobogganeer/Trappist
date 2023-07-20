using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Tobo.Net;

public class AudioPacket : Packet
{
    public Audio audio;

    public AudioPacket() { }
    public AudioPacket(Audio audio)
    {
        this.audio = audio;
    }

    public override void Serialize(ByteBuffer buf)
    {
        buf.AddStruct(audio);
    }

    public override void Deserialize(ByteBuffer buf, Args args)
    {
        audio = buf.GetStruct<Audio>();

        if (args.ServerSide)
        {
            // Bounce from server side to all clients
            AudioPacket p = new AudioPacket(audio);
            p.SendTo(args.from, true); // Blacklist = true, so it won't be sent to args.from
            // (they sent the message and will play the audio locally)
        }
        else
        {
            AudioManager.OnNetworkAudio(audio);
        }
    }
}
