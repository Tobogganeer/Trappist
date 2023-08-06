using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Steamworks;
using Steamworks.Data;
using System;

namespace Tobo.Net
{
    //[DefaultExecutionOrder(10)]
    public class SteamManager : MonoBehaviour
    {
        public static SteamManager instance;

        private void Start()
        {
            instance = this; // Attached to app

            if (Application.isEditor && !useSteamInEditor)
            {
                Destroy(this);
                return;
            }

            /*
            if (instance == null)
            {
                
                // Attached to App object
                //DontDestroyOnLoad(this);
            }
            else
            {
                Destroy(gameObject);
                return;
            }
            */

            try
            {
                SteamClient.Init(appID, false);
            }
            catch (Exception ex)
            {
                Debug.LogWarning("Couldn't log onto steam! " + ex);
                return;
            }

            SteamNetworkingUtils.InitRelayNetworkAccess();
            instance.CheckForCommandLineJoins();

            if (SteamClient.IsValid)
            {
                Debug.Log($"Successfully logged into steam as {SteamName} ({SteamID})");
            }
            else
            {
#if !UNITY_EDITOR
                bool launchedThroughSteam = SteamClient.RestartAppIfNecessary(appID);
                Debug.Log("Launched through steam? " + launchedThroughSteam);

                if (!launchedThroughSteam)
                {
                    Debug.Log("Attempting restart through steam...");
                    Application.Quit();
                }
#endif
            }
        }

        //[SerializeField] private bool useSteamInEditor = true;
        [SerializeField] private uint appID = 480;
        //public static bool UseSteamInEditor => instance.useSteamInEditor;
        public bool useSteamInEditor = true;
        public static uint AppID => instance.appID;

        private static SteamId steamID = 0;
        public static SteamId SteamID
        {
            get
            {
                if (steamID == 0)
                    steamID = SteamClient.SteamId;

                return steamID;
            }
        }

        public static string SteamName => SteamID.SteamName();

        public static void OpenOverlay(SteamOverlayOpenType openType = SteamOverlayOpenType.Friends)
        {
            SteamFriends.OpenOverlay(GetValidOverlayStringFromEnum(openType));
        }

        private static string GetValidOverlayStringFromEnum(SteamOverlayOpenType type)
        {
            return type switch
            {
                SteamOverlayOpenType.Friends => "friends",
                SteamOverlayOpenType.Community => "community",
                SteamOverlayOpenType.Players => "players",
                SteamOverlayOpenType.Settings => "settings",
                SteamOverlayOpenType.OfficialGameGroup => "officalgamegroup",
                SteamOverlayOpenType.Stats => "stats",
                SteamOverlayOpenType.Achievements => "achievements",
                _ => throw new NotImplementedException(),
            };
        }

        private void OnDestroy()
        {
#if STEAM
            if (NetworkManager.Instance != null && NetworkManager.Instance.useSteamTransport)
#endif
            {
                Debug.Log("Shutting down Steam...");
                SteamClient.Shutdown();
            }
        }

        private void Update()
        {

            if (!SteamClient.IsValid)
            {
//#if STEAM
//                if (NetworkManager.Instance != null && NetworkManager.Instance.useSteamTransport)
//#endif
//                {
                Debug.LogWarning("Re-initializing steam client...");
                SteamClient.Init(appID, false);
//                }
            }

            SteamClient.RunCallbacks();
        }

        #region Steam Networking
#if MULTIPLAYER
        private void CheckForCommandLineJoins()
        {
            Invoke(nameof(CheckForArgsDelayed), 1.0f);
        }

        private void CheckForArgsDelayed()
        {
            const string CONNECT_ARG = "+connect_lobby";
            string[] args = Environment.GetCommandLineArgs();

            for (int i = 0; i < args.Length - 1; i++)
            {
                if (args[i] == CONNECT_ARG)
                {
                    Lobby lobby = new Lobby(ulong.Parse(args[i + 1]));
                    (NetworkManager.Instance.server as SteamNetServer).JoinLobby(lobby);
                    //SteamFriends_OnGameLobbyJoinRequested(lobby, 0);
                }
            }
        }
#endif
#endregion
    }

    public enum SteamOverlayOpenType
    {
        Friends,
        Community,
        Players,
        Settings,
        OfficialGameGroup,
        Stats,
        Achievements
    }

    public static class SteamIDUtils
    {
        public static string SteamName(this SteamId id) => new Friend(id).Name;
    }
}
