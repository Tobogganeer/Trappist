using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Tobo.Net
{
    public class NetHUD : MonoBehaviour
    {
        public Vector2 size = new Vector2(200, 600);
        public Vector2 position;
        string address = "127.0.0.1";

        private void Update()
        {
            UpdateCursor();
        }

        private void OnGUI()
        {
            UpdateStatus();
        }

        void UpdateCursor()
        {
            if (Keyboard.current.mKey.wasPressedThisFrame)
            {
                if (Cursor.visible)
                {
                    Cursor.visible = false;
                    Cursor.lockState = CursorLockMode.Locked;
                }
                else
                {
                    Cursor.visible = true;
                    Cursor.lockState = CursorLockMode.None;
                }
            }
        }

        void UpdateStatus()
        {
            Rect area = new Rect(position, size);
            GUILayout.BeginArea(area);
            GUILayout.BeginVertical();

            GUILayout.BeginHorizontal();
            GUILayout.Label("Address: ", GUILayout.Width(75));
            address = GUILayout.TextField(address);
            GUILayout.EndHorizontal();

            System.Text.StringBuilder text = new System.Text.StringBuilder();

            text.AppendLine("Cursor (press M): " + Cursor.visible);
            text.AppendLine("BACKEND: " + (NetworkManager.Instance.useSteamTransport ? "Steam" : "Sockets"));

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

            if (GUILayout.Button("Join"))
                Join();
            if (GUILayout.Button("Host"))
                Host();
            GUI.enabled = false; // Not fully functional yet
            if (GUILayout.Button("Singleplayer"))
                Singleplayer();
            GUI.enabled = true;
            if (GUILayout.Button("Leave"))
                Leave();

            GUILayout.EndVertical();
            GUILayout.EndArea();
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
