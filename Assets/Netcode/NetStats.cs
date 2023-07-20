using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System;

namespace Tobo.Net
{
    public class NetStats
    {
        private static readonly System.Diagnostics.Stopwatch pingTimer = new System.Diagnostics.Stopwatch();
        private static readonly List<long> pings = new List<long>(MAX_PINGS);
        private const int MAX_PINGS = 5;
        public float secondsPerUpdate = 1;
        private float oldSecondsPerUpdate;

        /*
        private void SlowUpdate()
        {
            pingTimer.Start();
            if (!SteamManager.ConnectedToServer) return;

            InternalClientSend.SendPing();

            UpdateUI();
            ClearSettings();
        }

        public static void OnPongReceived()
        {
            if (pings.Count >= MAX_PINGS) pings.RemoveAt(0);

            pingTimer.Stop();

            pings.Add(pingTimer.ElapsedMilliseconds);

            pingTimer.Reset();

            Ping = pings.Average();
        }
        */

        public static int PacketsReceived { get; private set; }
        public static int PacketsSent { get; private set; }

        public static int BytesReceived { get; private set; }
        public static int BytesSent { get; private set; }

        public static double Ping { get; private set; }

        public static void ClearSettings()
        {
            PacketsReceived = 0;
            PacketsSent = 0;
            BytesReceived = 0;
            BytesSent = 0;
        }

        public static void OnPacketSent(int size)
        {
            BytesSent += size;
            PacketsSent++;

            //instance?.UpdateUI();
        }

        public static void OnPacketReceived(int size)
        {
            BytesReceived += size;
            PacketsReceived++;

            //instance?.UpdateUI();
        }
    }
}
