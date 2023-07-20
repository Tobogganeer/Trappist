using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Tobo.Net
{
    public class NetHUD : MonoBehaviour
    {
        public bool draw = true;
        public int offsetX = 5;
        public int offsetY = 150;
        public int width = 500, height = 400;

        readonly List<NetworkDiscovery.DiscoveryInfo> discoveredServers = new List<NetworkDiscovery.DiscoveryInfo>();

        /*
        void OnGUI()
        {
            if (draw)
            {
                Rect r = new Rect(offsetX, offsetY, width, height);
                if (NetworkManager.IsServer)
                    DisplayServer(r);
                else if (NetworkManager.ConnectedToServer)
                    DisplayClient(r);
                else
                    DisplayConnect(r);
            }
        }

        public void DisplayServer(Rect displayRect)
        {
            GUILayout.BeginArea(displayRect);

            this.DisplayRefreshButton();

            // lookup a server

            GUILayout.Label("Lookup server: ");
            GUILayout.BeginHorizontal();
            GUILayout.Label("IP:");
            m_lookupServerIP = GUILayout.TextField(m_lookupServerIP, GUILayout.Width(120));
            GUILayout.Space(10);
            GUILayout.Label("Port:");
            m_lookupServerPort = GUILayout.TextField(m_lookupServerPort, GUILayout.Width(60));
            GUILayout.Space(10);
            if (IsLookingUpAnyServer)
            {
                GUILayout.Button("Lookup...", GUILayout.Height(25), GUILayout.MinWidth(80));
            }
            else
            {
                if (GUILayout.Button("Lookup", GUILayout.Height(25), GUILayout.MinWidth(80)))
                    LookupServer();
            }
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            m_displayBroadcastAddresses = GUILayout.Toggle(m_displayBroadcastAddresses, "Display broadcast addresses", GUILayout.ExpandWidth(false));
            if (m_displayBroadcastAddresses)
            {
                GUILayout.Space(10);
                GUILayout.Label(string.Join(", ", NetworkDiscovery.GetBroadcastAdresses().Select(ip => ip.ToString())));
            }
            GUILayout.EndHorizontal();

            GUILayout.Label(string.Format("Servers [{0}]:", m_discoveredServers.Count));

            this.DisplayServers();

            GUILayout.EndArea();

        }

        public void DisplayClient(Rect displayRect)
        {
            GUILayout.BeginArea(displayRect);

            this.DisplayRefreshButton();

            // lookup a server

            GUILayout.Label("Lookup server: ");
            GUILayout.BeginHorizontal();
            GUILayout.Label("IP:");
            m_lookupServerIP = GUILayout.TextField(m_lookupServerIP, GUILayout.Width(120));
            GUILayout.Space(10);
            GUILayout.Label("Port:");
            m_lookupServerPort = GUILayout.TextField(m_lookupServerPort, GUILayout.Width(60));
            GUILayout.Space(10);
            if (IsLookingUpAnyServer)
            {
                GUILayout.Button("Lookup...", GUILayout.Height(25), GUILayout.MinWidth(80));
            }
            else
            {
                if (GUILayout.Button("Lookup", GUILayout.Height(25), GUILayout.MinWidth(80)))
                    LookupServer();
            }
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            m_displayBroadcastAddresses = GUILayout.Toggle(m_displayBroadcastAddresses, "Display broadcast addresses", GUILayout.ExpandWidth(false));
            if (m_displayBroadcastAddresses)
            {
                GUILayout.Space(10);
                GUILayout.Label(string.Join(", ", NetworkDiscovery.GetBroadcastAdresses().Select(ip => ip.ToString())));
            }
            GUILayout.EndHorizontal();

            GUILayout.Label(string.Format("Servers [{0}]:", m_discoveredServers.Count));

            this.DisplayServers();

            GUILayout.EndArea();

        }

        public void DisplayConnect(Rect displayRect)
        {
            GUILayout.BeginArea(displayRect);

            this.DisplayRefreshButton();

            // lookup a server

            GUILayout.Label("Lookup server: ");
            GUILayout.BeginHorizontal();
            GUILayout.Label("IP:");
            m_lookupServerIP = GUILayout.TextField(m_lookupServerIP, GUILayout.Width(120));
            GUILayout.Space(10);
            GUILayout.Label("Port:");
            m_lookupServerPort = GUILayout.TextField(m_lookupServerPort, GUILayout.Width(60));
            GUILayout.Space(10);
            if (IsLookingUpAnyServer)
            {
                GUILayout.Button("Lookup...", GUILayout.Height(25), GUILayout.MinWidth(80));
            }
            else
            {
                if (GUILayout.Button("Lookup", GUILayout.Height(25), GUILayout.MinWidth(80)))
                    LookupServer();
            }
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            m_displayBroadcastAddresses = GUILayout.Toggle(m_displayBroadcastAddresses, "Display broadcast addresses", GUILayout.ExpandWidth(false));
            if (m_displayBroadcastAddresses)
            {
                GUILayout.Space(10);
                GUILayout.Label(string.Join(", ", NetworkDiscovery.GetBroadcastAdresses().Select(ip => ip.ToString())));
            }
            GUILayout.EndHorizontal();

            GUILayout.Label(string.Format("Servers [{0}]:", m_discoveredServers.Count));

            this.DisplayServers();

            GUILayout.EndArea();

        }
        */



        void OnEnable()
        {
            NetworkDiscovery.Instance.OnServerDiscovered += OnDiscoveredServer;
        }

        void OnDisable()
        {
            NetworkDiscovery.Instance.OnServerDiscovered -= OnDiscoveredServer;
        }

        void OnDiscoveredServer(NetworkDiscovery.DiscoveryInfo info)
        {
            // Use this method to search by endpoint rather than ref
            int index = discoveredServers.FindIndex(item => item.EndPoint.Equals(info.EndPoint));
            if (index < 0) // Not in list
                discoveredServers.Add(info);
            else
                discoveredServers[index] = info;

        }
    }
}
