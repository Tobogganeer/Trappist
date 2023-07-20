using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Tobo.Net
{
    public class NetHUD : MonoBehaviour
    {
        public float width = 150;
        string address;

        private void OnGUI()
        {
            UpdateStatus();
        }

        void UpdateStatus()
        {
            GUILayout.BeginVertical(GUILayout.Width(width));

            GUILayout.BeginHorizontal();
            GUILayout.Label("Address: ");
            address = GUILayout.TextField(address);
            GUILayout.EndHorizontal();

            System.Text.StringBuilder text = new System.Text.StringBuilder();

            if (NetworkManager.SinglePlayer)
            {
                text.Append("Singleplayer");
            }
            else
            {
                if (NetworkManager.IsServer)
                    text.Append("Server\n");
                if (NetworkManager.ConnectedToServer)
                    text.Append("Connected to Server\n");
                text.Append(NetworkManager.Instance.useSteamTransport ? "Steam\n" : "Sockets\n");
                text.Append("Players: " + Client.All.Count + "\n");
                text.Append("-Client\n");
                foreach (Client c in Client.All.Values)
                    text.Append(" - " + c + "\n");
                text.Append("\n-Server\n");
                foreach (S_Client s in S_Client.All.Values)
                    text.Append(" - " + s + "\n");
            }

            GUILayout.Label(text.ToString());

            GUILayout.EndVertical();
        }

        public void Join()
        {
            NetworkManager.Join("Player " + Random.Range(0, 1000), address);
        }

        public void Host()
        {
            NetworkManager.Host("Player " + Random.Range(0, 1000));
        }

        public void Singleplayer()
        {
            NetworkManager.Singleplayer("Player " + Random.Range(0, 1000));
        }

        public void Leave()
        {
            NetworkManager.Disconnect();
        }
    }
}
