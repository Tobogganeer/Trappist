using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Steamworks;
using Steamworks.Data;
using System;
using System.Threading.Tasks;
using System.Linq;
//using ClientMessageReceivedCallback = System.Action<ushort, Steamworks.SteamId, VirtualVoid.Net.Message>;
//using ServerMessageReceivedCallback = System.Action<ushort, VirtualVoid.Net.Message>;
//using ClientMessageCallback = System.Action<Steamworks.SteamId, VirtualVoid.Net.Message>;
//using MessageCallback = System.Action<VirtualVoid.Net.Message>;

namespace VirtualVoid.Net
{
    /*
    public class SteamManager : MonoBehaviour
    {
        // In Subclasses, do not use built in Awake, Update, or OnDisable. Override the suitable methods


        // Singleton
        public static SteamManager instance;
        private void Awake()
        {
            if (instance == null)
            {
                instance = this;
                DontDestroyOnLoad(this);
            }
            else
            {
                Destroy(gameObject);
                return;
            }

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

            InitSteamEvents();
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

            if (tickRate > 0)
            {
                Time.fixedDeltaTime = 1f / tickRate;
                Debug.Log($"Set physics tickrate to {tickRate} ticks per second ({Time.fixedDeltaTime}s per physics update).");
            }

            InternalMessages.InitHandlers();

            //TryRegisterSpawnablePrefab(playerPrefab.gameObject);

            foreach (GameObject obj in spawnablePrefabs)
            {
                TryRegisterSpawnablePrefab(obj);
            }

            OnAwake();

            CheckForCommandLineJoins();
        }

        // Inspector Stuff
        [Header("The steam app id of your app.")]
        [SerializeField] private uint appID = 480;
        public static uint AppID => instance.appID;

        [Header("The maximum number of players who can join at once.")]
        [SerializeField] private uint maxPlayers = 4;
        public static uint MaxPlayers => instance.maxPlayers;

        [Header("Sets the fixed update rate. Set to 0 to keep as it is.")]
        [Range(0, 128)]
        [SerializeField] private int tickRate;
        public static int TickRate => instance.tickRate;

        [Header("All prefabs that can be spawned on the client should be in this list.")]
        [SerializeField] private GameObject[] spawnablePrefabs;

        //[Header("The object spawned in when a client joins.")]
        //[SerializeField] private NetworkBehaviour playerPrefab;
        //internal static Client PlayerPrefab => instance.playerPrefab;

        //private

        // Static Members
        public static Lobby CurrentLobby { get; private set; }
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

        private static SteamSocketManager socketServer;
        private static SteamConnectionManager connectionToServer;
        private static AuthTicket currentAuthTicket;
        private static uint FirstConnectionID; // used to skip auth for host

        public static SteamId LobbyOwnerID => CurrentLobby.Owner.Id; //{ get; private set; }
        public static bool ConnectedToServer => connectionToServer != null;// && connectionToServer.Connected;
        public static bool IsServer => socketServer != null;

        private static readonly bool FORCE_VERSION_CHECK = false;


#region Events
        public static event Action<Lobby> OnLobbyCreated; // Server
        public static event Action<Lobby> OnLobbyJoined; // Both
        public static event Action<Lobby, Friend> OnLobbyMemberJoined; // Both
        public static event Action<Lobby> OnLobbyLeft; // Both
        public static event Action<Lobby, Friend> OnLobbyMemberLeave; // Both

        public static event Action<SteamId> OnClientSceneLoaded; // Both
        public static event Action OnAllClientsSceneLoaded; // Both

        public static event ClientMessageReceivedCallback OnMessageFromClient;
        public static event ServerMessageReceivedCallback OnMessageFromServer;

        internal static event ClientMessageReceivedCallback OnInternalMessageFromClient;
        internal static event ServerMessageReceivedCallback OnInternalMessageFromServer;

        public static event Action<Client> OnClientConnected;
        public static event Action<Client> OnClientDisconnected;

        public static event Action OnConnectedToServer;
        public static event Action OnDisconnectedFromServer;

        public static event Action OnConnectionClosed;
        public static event Action OnLobbyJoinFailed;
        public static event Action OnLobbyJoinStarted;
        #endregion

        // Delegates (Really just used so the parameters have names)
        //public delegate void ClientMessageReceivedCallback(ushort messageID, SteamId clientSteamID, Message message);
        //public delegate void ServerMessageReceivedCallback(ushort messageID, Message message);
        //
        //public delegate void ClientMessageCallback(SteamId clientSteamID, Message message);
        //public delegate void MessageCallback(Message message);
        //
        //public delegate void LobbyMemberCallback(Lobby lobby, Friend member);
        //public delegate void LobbyCallback(Lobby lobby);
        //public delegate void LobbyCreatedCallback(Lobby lobby, bool success);

        // Dictionaries

        public static readonly Dictionary<byte, SteamId> clientIDToSteamID = new Dictionary<byte, SteamId>();
        public static readonly Dictionary<uint, SteamId> connIDToSteamID = new Dictionary<uint, SteamId>();
        public static readonly Dictionary<SteamId, Client> clients = new Dictionary<SteamId, Client>();
        internal static readonly Dictionary<uint, Client> clientsPendingAuth = new Dictionary<uint, Client>();
        internal static readonly Dictionary<SteamId, uint> unverifiedSteamIDToConnID = new Dictionary<SteamId, uint>();

#region Callback Dicts
        private static readonly Dictionary<ushort, ClientMessageCallback> messagesFromClientCallbacks = new Dictionary<ushort, ClientMessageCallback>();
        private static readonly Dictionary<ushort, MessageCallback> messagesFromServerCallbacks = new Dictionary<ushort, MessageCallback>();

        private static readonly Dictionary<ushort, ClientMessageCallback> internalMessagesFromClientCallbacks = new Dictionary<ushort, ClientMessageCallback>();
        private static readonly Dictionary<ushort, MessageCallback> internalMessagesFromServerCallbacks = new Dictionary<ushort, MessageCallback>();
#endregion

        internal static readonly Dictionary<Guid, GameObject> registeredPrefabs = new Dictionary<Guid, GameObject>();


        // Constants
        private const string LOBBY_SERVER_VERSION = "server_version";
        internal const ushort PUBLIC_MESSAGE_BUFFER_SIZE = 4096;
        private const int STEAM_VIRTUAL_PORT = 225;
        private const float UNAUTHENTICATED_CLIENT_TIMEOUT = 5f;

        private bool suppressNetworkIDDestroyMessages;

        // Allocation Reduction
        //internal static byte[] tempMessageByteBuffer = new byte[PUBLIC_MESSAGE_BUFFER_SIZE];
        
        private void InitSteamEvents()
        {
            SteamFriends.OnGameLobbyJoinRequested += SteamFriends_OnGameLobbyJoinRequested;
            SteamMatchmaking.OnLobbyEntered += SteamMatchmaking_OnLobbyEntered;
            SteamMatchmaking.OnLobbyMemberJoined += SteamMatchmaking_OnLobbyMemberJoined;
            SteamMatchmaking.OnLobbyMemberLeave += SteamMatchmaking_OnLobbyMemberLeave;
            SteamUser.OnValidateAuthTicketResponse += SteamUser_OnValidateAuthTicketResponse;
        }



#region Steam Callbacks

        private void SteamMatchmaking_OnLobbyMemberJoined(Lobby lobby, Friend friend)
        {
            OnLobbyMemberJoined?.Invoke(lobby, friend);
        }

        private void SteamMatchmaking_OnLobbyEntered(Lobby lobby)
        {
            if (!IsServer && FORCE_VERSION_CHECK)
            {
                string version = Application.version;
                string data = lobby.GetData(LOBBY_SERVER_VERSION);
                if (version != data)
                {
                    Debug.Log($"Current version is {version}, but server version is {data}! Please update game...");
                    Leave();
                    return;
                }
            }

            OnLobbyJoined?.Invoke(lobby);
        }

        public async void SteamFriends_OnGameLobbyJoinRequested(Lobby lobby, SteamId id)
        {
            if (!ShouldJoinGame(lobby))
            {
                Debug.Log($"Tried joining {id.SteamName()} ({id}), but that attempt was rejected!");
                return;
            }

            if (ConnectedToServer)
                Leave();

            OnLobbyJoinStarted?.Invoke();

            RoomEnter result = await lobby.Join();
            if (result != RoomEnter.Success)
            {
                Debug.LogError($"Tried to join {id.SteamName()}'s lobby, but was not successful! Result: " + result);

                await Task.Delay(100);

                if (await lobby.Join() != RoomEnter.Success)
                {
                    Debug.LogError("Second attempt failed too!");
                    OnLobbyJoinFailed?.Invoke();
                    Leave();
                    return;
                }
            }
            CurrentLobby = lobby;

            ConnectToServer(LobbyOwnerID);
        }

        private void SteamMatchmaking_OnLobbyMemberLeave(Lobby lobby, Friend friend)
        {
            OnLobbyMemberLeave?.Invoke(lobby, friend);

            if (friend.Id == LobbyOwnerID && !IsServer)
                Leave();
        }

        private void SteamUser_OnValidateAuthTicketResponse(SteamId userSteamID, SteamId gameOwnerID, AuthResponse response)
        {
            if (!IsServer)
            {
                Debug.Log("Leaving game due to auth ticket response while not server");
                Leave();
            }

            if (userSteamID != SteamID)
                Debug.Log($"Received auth ticket response for {new Friend(userSteamID).Name} ({userSteamID}): {response}");

            if (clients.TryGetValue(userSteamID, out Client connectedClient))
            {
                Debug.Log($"Received auth update for {connectedClient.SteamName}, response was {response}");

                if (response != AuthResponse.OK)
                {
                    // They cancelled ticket
                    connectedClient.Destroy();
                    return;
                }
            }

            try
            {
                if (response == AuthResponse.OK)
                {
                    UserAuthenticated(userSteamID);
                }
                else
                {
                    UserNotAuthenticated(userSteamID, response);
                }
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"Bad auth call with {userSteamID.SteamName()}, response was {response}, disconnecting. " + ex);

                if (!unverifiedSteamIDToConnID.TryGetValue(userSteamID, out uint connID))
                {
                    Debug.LogWarning($"Could not get connection ID from {userSteamID.SteamName()}'s id ({userSteamID}). Shutting down server...");
                    Leave();
                    return;
                }

                Client client = clientsPendingAuth[connID];
                if (client == null)
                {
                    Debug.LogWarning("Null client, shutting down server...");
                    Leave();
                    return;
                }

                client.Destroy();
            }
            
        }

#endregion

#region Unity Functions
        private void Update()
        {
            if (Time.frameCount % 150 == 0)
                DisconnectOldConnections();

            if (!SteamClient.IsValid)
            {
                Debug.LogWarning("Re-initializing steam client...");
                SteamClient.Init(appID, false);
            }

            SteamClient.RunCallbacks();

            try
            {
                if (socketServer != null)
                    socketServer.Receive();

                if (connectionToServer != null)
                    connectionToServer.Receive();
            }
            catch
            {
                Debug.Log("Error receiving data on socket/connection");
            }

            OnUpdate();
        }

        private void OnDisable()
        {
            try
            {
                Leave();
            }
            catch (Exception ex)
            {
                Debug.Log("Caught error leaving while exiting application: " + ex);
            }
            finally
            {
                Debug.Log("Shutting down Steam...");
                SteamClient.Shutdown();
            }

            OnDisabled();
        }
#endregion

#region Lobby
        public static async Task<bool> CreateLobby(uint maxPlayers = 4, SteamLobbyPrivacyMode mode = SteamLobbyPrivacyMode.FriendsOnly, bool joinable = true)
        {
            Lobby? lobby =  await SteamMatchmaking.CreateLobbyAsync((int)maxPlayers);
            if (lobby.HasValue)
            {
                CurrentLobby = lobby.Value;
                //ServerID = CurrentLobby.Owner.Id;
                SetLobbyPrivacyMode(CurrentLobby, mode);
                CurrentLobby.SetJoinable(joinable);
                if (FORCE_VERSION_CHECK)
                    CurrentLobby.SetData(LOBBY_SERVER_VERSION, Application.version);

                OnLobbyCreated?.Invoke(CurrentLobby);

                //if (await CurrentLobby.Join() != RoomEnter.Success)
                //{
                //    Debug.Log("Error joining own lobby!");
                //}

                // Creating a lobby already joins it

                return true;
            }
            else
            {
                throw new NullReferenceException("Lobby created but returned null value!");
            }
        }

        public static void SetLobbyPrivacyMode(Lobby lobby, SteamLobbyPrivacyMode mode)
        {
            switch (mode)
            {
                case SteamLobbyPrivacyMode.Public:
                    lobby.SetPublic();
                    break;
                case SteamLobbyPrivacyMode.Private:
                    lobby.SetPrivate();
                    break;
                case SteamLobbyPrivacyMode.Invisible:
                    lobby.SetInvisible();
                    break;
                case SteamLobbyPrivacyMode.FriendsOnly:
                    lobby.SetFriendsOnly();
                    break;
            }
        }

        private static void LeaveCurrentLobby()
        {
            if (CurrentLobby.Id.IsValid)
            {
                CurrentLobby.Leave();
                OnLobbyLeft?.Invoke(CurrentLobby);
                CurrentLobby = default;
            }
        }

        /// <summary>
        /// Only change before hosting a server.
        /// </summary>
        /// <param name="maxPlayers"></param>
        public static void SetMaxPlayers(uint maxPlayers)
        {
            if (IsServer)
            {
                Debug.Log("Cannot change maxPlayers while mid-game!");
                return;
            }

            instance.maxPlayers = maxPlayers;
        }

        public static void SetCurrentLobbyPrivacy(SteamLobbyPrivacyMode mode)
        {
            SetLobbyPrivacyMode(CurrentLobby, mode);
        }
#endregion

#region Server
        /// <summary>
        /// Hosts a game.
        /// </summary>
        /// <param name="mode"></param>
        /// <param name="joinable"></param>
        public static async void Host(SteamLobbyPrivacyMode mode = SteamLobbyPrivacyMode.FriendsOnly, bool joinable = true)
        {
            Leave();

            CreateSteamSocketServer();
            ConnectToServer(SteamID);

            SteamNetworkingUtils.Timeout = 2000;

            if (!await CreateLobby(instance.maxPlayers, mode, joinable))
            {
                Debug.Log("Error starting server: Lobby not created successfully.");
                Leave();
                return;
            }
        }

        /// <summary>
        /// Leaves the server and the steam lobby. Stops the server if you are the host.
        /// </summary>
        public static void Leave(int serverCloseReason = 0)
        {
            if (!IsServer && !ConnectedToServer)
            {
                Debug.Log("Skipping Leave() call as not currently connected or hosting a server.");
                ResetValues();
                return;
            }

            try
            {
                if (IsServer)
                    StopServer(serverCloseReason);

                KickAllClients();
                // Moved out as client-side clients exist now

                // TODO: THIS LINE OF CODE IS REALLY SLOW FOR SOME REASON, like 900ms when leaving server
                // IDK if i can do anything about it though
                connectionToServer?.Close();
                connectionToServer = null;

                LeaveCurrentLobby();
                ResetValues();
            }
            catch (Exception ex)
            {
                Debug.LogError("Error while trying to leave server! " + ex);
            }
        }


        private static void StopServer(int serverCloseReason)
        {
            //KickAllClients();
            InternalServerSend.SendServerClose(serverCloseReason);

            socketServer?.Close();
            socketServer = null;
        }

        private static void KickAllClients()
        {
            foreach (Client client in clients.Values.ToList())
            {
                client.Destroy();
            }

            foreach (Client client in clientsPendingAuth.Values.ToList())
                client.Destroy();

            if (socketServer != null)
            {
                foreach (Connection connection in socketServer.Connected)
                    connection.Close();
                // Close all connections just in case
            }

            clients.Clear();
            clientsPendingAuth.Clear();

            clientIDToSteamID.Clear();
            connIDToSteamID.Clear();
            unverifiedSteamIDToConnID.Clear();
            // Clear mapping tables as well
        }

        private static void ResetValues()
        {
            clients.Clear();
            clientsPendingAuth.Clear();
            clientIDToSteamID.Clear();
            connIDToSteamID.Clear();
            unverifiedSteamIDToConnID.Clear();

            CurrentLobby = default;
            connectionToServer?.Close();
            connectionToServer = null;
            socketServer?.Close();
            socketServer = null;

            // VVV was above server closing, moved because i can
            currentAuthTicket?.Cancel();
            currentAuthTicket = null;

            FirstConnectionID = 0;

            NetworkID.ResetNetIDs();
            DestroyAllRuntimeNetworkIDs();
        }
#endregion

#region Send
        public static bool SendMessageToServer(Message message)
        {
            if (!ConnectedToServer) return false;

            if (message == null)
            {
                Debug.LogWarning("Cannot send null message!");
                return false;
            }

            try
            {
                // Convert string/byte[] message into IntPtr data type for efficient message send / garbage management
                int sizeOfMessage = message.WrittenLength;
                IntPtr intPtrMessage = System.Runtime.InteropServices.Marshal.AllocHGlobal(sizeOfMessage);
                System.Runtime.InteropServices.Marshal.Copy(message.Bytes, 0, intPtrMessage, sizeOfMessage);
                Result success = connectionToServer.Connection.SendMessage(intPtrMessage, sizeOfMessage, message.SendType);
                if (success == Result.OK)
                {
                    System.Runtime.InteropServices.Marshal.FreeHGlobal(intPtrMessage); // Free up memory at pointer
                    NetStats.OnPacketSent(sizeOfMessage);
                    return true;
                }
                else
                {
                    // RETRY
                    Result retry = connectionToServer.Connection.SendMessage(intPtrMessage, sizeOfMessage, message.SendType);
                    System.Runtime.InteropServices.Marshal.FreeHGlobal(intPtrMessage); // Free up memory at pointer
                    if (retry == Result.OK)
                    {
                        NetStats.OnPacketSent(sizeOfMessage);
                        return true;
                    }
                    return false;
                }
            }
            catch (Exception ex)
            {
                Debug.Log("Unable to send message to socket server: " + ex);
                return false;
            }
        }

        public static bool SendMessageToClient(SteamId id, Message message)
        {
            if (message == null)
            {
                Debug.LogWarning("Cannot send null message!");
                return false;
            }

            if (!IsServer)
            {
                Debug.LogWarning("Cannot send messages to clients as this machine is not currently a server!");
                return false;
            }

            if (!clients.ContainsKey(id))
            {
                Debug.LogWarning($"Tried to send message to {new Friend(id).Name}, but they weren't in the clients dict!");
                return false;
            }

            try
            {
                // Convert string/byte[] message into IntPtr data type for efficient message send / garbage management
                int sizeOfMessage = message.WrittenLength;
                IntPtr intPtrMessage = System.Runtime.InteropServices.Marshal.AllocHGlobal(sizeOfMessage);
                System.Runtime.InteropServices.Marshal.Copy(message.Bytes, 0, intPtrMessage, sizeOfMessage);

                Connection conn = clients[id].connection;

                bool success = SendDataToClient(intPtrMessage, conn, sizeOfMessage, message.SendType);

                System.Runtime.InteropServices.Marshal.FreeHGlobal(intPtrMessage); // Free up memory at pointer

                return success;
            }
            catch (Exception ex)
            {
                Debug.Log($"Unable to send message to {new Friend(id).Name}: " + ex);
                return false;
            }
        }

        public static bool SendMessageToAllClients(Message message)
        {
            if (message == null)
            {
                Debug.LogWarning("Cannot send null message!");
                return false;
            }

            if (!IsServer)
            {
                Debug.LogWarning("Cannot send messages to clients as this machine is not currently a server!");
                return false;
            }

            try
            {
                // Convert string/byte[] message into IntPtr data type for efficient message send / garbage management
                int sizeOfMessage = message.WrittenLength;

                IntPtr intPtrMessage = System.Runtime.InteropServices.Marshal.AllocHGlobal(sizeOfMessage);

                System.Runtime.InteropServices.Marshal.Copy(message.Bytes, 0, intPtrMessage, sizeOfMessage);

                bool success = true;

                foreach (Client client in clients.Values)
                {
                    if (SendDataToClient(intPtrMessage, client.connection, sizeOfMessage, message.SendType) == false)
                        success = false;
                }

                System.Runtime.InteropServices.Marshal.FreeHGlobal(intPtrMessage); // Free up memory at pointer

                return success;
            }
            catch (Exception ex)
            {
                Debug.Log($"Unable to send message to clients: " + ex);
                return false;
            }
        }

        public static bool SendMessageToAllClients(SteamId except, Message message)
        {
            if (message == null)
            {
                Debug.LogWarning("Cannot send null message!");
                return false;
            }

            if (!IsServer)
            {
                Debug.LogWarning("Cannot send messages to clients as this machine is not currently a server!");
                return false;
            }

            try
            {
                // Convert string/byte[] message into IntPtr data type for efficient message send / garbage management
                int sizeOfMessage = message.WrittenLength;
                IntPtr intPtrMessage = System.Runtime.InteropServices.Marshal.AllocHGlobal(sizeOfMessage);
                System.Runtime.InteropServices.Marshal.Copy(message.Bytes, 0, intPtrMessage, sizeOfMessage);

                bool success = true;

                foreach (Client client in clients.Values)
                {
                    if (client.SteamID == except) continue;

                    if (SendDataToClient(intPtrMessage, client.connection, sizeOfMessage, message.SendType) == false)
                        success = false;
                }

                System.Runtime.InteropServices.Marshal.FreeHGlobal(intPtrMessage); // Free up memory at pointer

                return success;
            }
            catch (Exception ex)
            {
                Debug.Log($"Unable to send message to clients: " + ex);
                return false;
            }
        }

        private static bool SendDataToClient(IntPtr data, Connection conn, int size, SendType sendType)
        {
            // Big mistake was in here, data was being freed after sending to one client before sending to all others

            Result success = conn.SendMessage(data, size, sendType);
            if (success == Result.OK)
            {
                //System.Runtime.InteropServices.Marshal.FreeHGlobal(data); // Free up memory at pointer
                NetStats.OnPacketSent(size);
                return true;
            }
            else
            {
                // RETRY
                Result retry = conn.SendMessage(data, size, sendType);
                //System.Runtime.InteropServices.Marshal.FreeHGlobal(data); // Free up memory at pointer
                if (retry == Result.OK)
                {
                    NetStats.OnPacketSent(size);
                    return true;
                }

                Debug.Log($"Could not send message to conn {conn.Id} after 2 attempts.");
                return false;
            }
        }
#endregion

#region Handle
        private static void HandleDataFromClient(Message message, SteamId fromClient)
        {
            NetStats.OnPacketReceived(message.WrittenLength);

            if (!clients.ContainsKey(fromClient))
            {
                Debug.LogWarning($"Received message from {new Friend(fromClient).Name}, but they are not in the clients dictionary!");
                return;
            }

            ushort id = message.GetUShort();

            if (!clients[fromClient].sceneLoaded && id != (ushort)InternalClientMessageIDs.SCENE_LOADED)
            {
                Debug.Log($"Received message from {clients[fromClient].SteamName}, but they have not loaded the next scene yet.");
                return;
            }

            if (Message.IsInternalMessage(id))
            {
                OnInternalMessageFromClient?.Invoke(id, fromClient, message);
                if (internalMessagesFromClientCallbacks.ContainsKey(id))
                {
                    internalMessagesFromClientCallbacks[id]?.Invoke(fromClient, message);
                }
            }
            else
            {
                OnMessageFromClient?.Invoke(id, fromClient, message);
                if (messagesFromClientCallbacks.ContainsKey(id))
                {
                    messagesFromClientCallbacks[id]?.Invoke(fromClient, message);
                }
            }
        }

        private static void HandleDataFromServer(Message message)
        {
            NetStats.OnPacketReceived(message.WrittenLength);

            ushort id = message.GetUShort();

            //Debug.Log("Received packet from server with ID " + (InternalServerMessageIDs)id);

            if (Message.IsInternalMessage(id))
            {
                if (id == (ushort)InternalServerMessageIDs.REQUEST_AUTH)
                {
                    SendAuthToServer();
                    return;
                }

                OnInternalMessageFromServer?.Invoke(id, message);

                //Debug.Log("Count of packet handlers from server: " + internalMessagesFromServerCallbacks.Count);

                if (internalMessagesFromServerCallbacks.ContainsKey(id))
                {
                    //Debug.Log("Found suitable handler for server packet of ID " + (InternalServerMessageIDs)id);
                    internalMessagesFromServerCallbacks[id]?.Invoke(message);
                }
            }
            else
            {
                OnMessageFromServer?.Invoke(id, message);
                if (messagesFromServerCallbacks.ContainsKey(id))
                {
                    messagesFromServerCallbacks[id]?.Invoke(message);
                }
            }
        }
#endregion

#region Message Callback Registration

#region Normal Messages
        /// <summary>
        /// Invokes the <paramref name="callback"/> on the server when a message from a client of ID <paramref name="messageID"/> is received.
        /// Subscribe to OnMessageFromClient to get notified when any message is received, not only a message of ID <paramref name="messageID"/>.
        /// </summary>
        /// <param name="messageID">The message ID that will be listened for.</param>
        /// <param name="callback">The callback to invoke when a message with ID <paramref name="messageID"/> is received.</param>
        public static void AddHandler_FromClient(ushort messageID, ClientMessageCallback callback)
        {
            if (Message.IsInternalMessage(messageID))
            {
                Debug.LogWarning($"Tried to register a ClientMessageCallback with ID of {messageID}, but that ID is used internally!");
                return;
            }

            if (!messagesFromClientCallbacks.ContainsKey(messageID)) messagesFromClientCallbacks.Add(messageID, callback);
            else messagesFromClientCallbacks[messageID] += callback;
        }

        /// <summary>
        /// Invokes the <paramref name="callback"/> on the client when a message from the server of ID <paramref name="messageID"/> is received.
        /// Subscribe to OnMessageFromServer to get notified when any message is received, not only a message of ID <paramref name="messageID"/>.
        /// </summary>
        /// <param name="messageID">The message ID that will be listened for.</param>
        /// <param name="callback">The callback to invoke when a message with ID <paramref name="messageID"/> is received.</param>
        public static void AddHandler_FromServer(ushort messageID, MessageCallback callback)
        {
            if (Message.IsInternalMessage(messageID))
            {
                Debug.LogWarning($"Tried to register a ServerMessageCallback with ID of {messageID}, but that ID is used internally!");
                return;
            }

            if (!messagesFromServerCallbacks.ContainsKey(messageID)) messagesFromServerCallbacks.Add(messageID, callback);
            else messagesFromServerCallbacks[messageID] += callback;
        }
#endregion

#region Internal messages
        /// <summary>
        /// Invokes the <paramref name="callback"/> on the server when an internal message from a client of ID <paramref name="messageID"/> is received.
        /// Subscribe to OnInternalMessageFromClient to get notified when any internal message is received, not only an internal message of ID <paramref name="messageID"/>.
        /// </summary>
        /// <param name="messageID">The message ID that will be listened for.</param>
        /// <param name="callback">The callback to invoke when an internal message with ID <paramref name="messageID"/> is received.</param>
        internal static void AddInternalHandler_FromClient(InternalClientMessageIDs messageID, ClientMessageCallback callback)
        {
            if (!Message.IsInternalMessage((ushort)messageID))
            {
                Debug.LogWarning($"Tried to register an internal ClientMessageCallback with ID of {messageID}, but that ID is not used internally!");
                return;
            }

            if (!internalMessagesFromClientCallbacks.ContainsKey((ushort)messageID)) internalMessagesFromClientCallbacks.Add((ushort)messageID, callback);
            else internalMessagesFromClientCallbacks[(ushort)messageID] += callback;
        }

        /// <summary>
        /// Invokes the <paramref name="callback"/> on the client when an internal message from the server of ID <paramref name="messageID"/> is received.
        /// Subscribe to OnInternalMessageFromServer to get notified when any internal message is received, not only an internal message of ID <paramref name="messageID"/>.
        /// </summary>
        /// <param name="messageID">The message ID that will be listened for.</param>
        /// <param name="callback">The callback to invoke when an internal message with ID <paramref name="messageID"/> is received.</param>
        internal static void AddInternalHandler_FromServer(InternalServerMessageIDs messageID, MessageCallback callback)
        {
            ushort id = (ushort)messageID;

            if (!Message.IsInternalMessage(id))
            {
                Debug.LogWarning($"Tried to register an internal ServerMessageCallback with ID of {messageID}, but that ID is not used internally!");
                return;
            }

            if (!internalMessagesFromServerCallbacks.ContainsKey(id)) internalMessagesFromServerCallbacks.Add(id, callback);
            else internalMessagesFromServerCallbacks[id] += callback;
        }
#endregion

#endregion

#region Message Callback Deregistration

#region Normal Messages
        public static void RemoveHandler_FromClient(ushort messageID, ClientMessageCallback callback)
        {
            if (Message.IsInternalMessage(messageID))
            {
                Debug.LogWarning($"Tried to deregister a ClientMessageCallback with ID of {messageID}, but that ID is used internally!");
                return;
            }

            if (messagesFromClientCallbacks.ContainsKey(messageID)) messagesFromClientCallbacks[messageID] -= callback;
        }

        public static void RemoveHandler_FromServer(ushort messageID, MessageCallback callback)
        {
            if (Message.IsInternalMessage(messageID))
            {
                Debug.LogWarning($"Tried to deregister a ServerMessageCallback with ID of {messageID}, but that ID is used internally!");
                return;
            }

            if (messagesFromServerCallbacks.ContainsKey(messageID)) messagesFromServerCallbacks[messageID] -= callback;
        }
#endregion

#region Internal Messages
        internal static void RemoveInternalHandler_FromClient(InternalClientMessageIDs messageID, ClientMessageCallback callback)
        {
            if (!Message.IsInternalMessage((ushort)messageID))
            {
                Debug.LogWarning($"Tried to deregister an internal ClientMessageCallback with ID of {messageID}, but that ID is not used internally!");
                return;
            }

            if (internalMessagesFromClientCallbacks.ContainsKey((ushort)messageID)) internalMessagesFromClientCallbacks[(ushort)messageID] -= callback;
        }

        internal static void RemoveInternalHandler_FromServer(InternalServerMessageIDs messageID, MessageCallback callback)
        {
            if (!Message.IsInternalMessage((ushort)messageID))
            {
                Debug.LogWarning($"Tried to deregister an internal ServerMessageCallback with ID of {messageID}, but that ID is not used internally!");
                return;
            }

            if (internalMessagesFromServerCallbacks.ContainsKey((ushort)messageID)) internalMessagesFromServerCallbacks[(ushort)messageID] -= callback;
        }
#endregion

#endregion

        //public static void DisconnectClient(SteamId clientID)
        //{
        //    if (!IsServer)
        //    {
        //        Debug.LogWarning("Tried to disconnect " + new Friend(clientID) + ", but this client is not the server!");
        //        return;
        //    }
        //
        //    if (!clients.ContainsKey(clientID))
        //    {
        //        if (serverShuttingDown && SteamID == clientID) return;
        //
        //        Debug.LogWarning("Tried to disconnect client with ID " + clientID + ", but clients dictionary does not contain that ID! (May be duplicate call)");
        //        if (SteamID == clientID) Debug.Log("^^^ This ID was your ID, may happen when closing the server.");
        //    }
        //
        //    InternalServerMessages.SendClientDisconnected(clientID);
        //
        //    OnClientDisconnect?.Invoke(clientID);
        //
        //    if (clients.TryGetValue(clientID, out Client client))
        //        client.Destroy();
        //
        //    SteamNetworking.CloseP2PSessionWithUser(clientID);
        //}

        //private void AcceptP2P(SteamId otherID)
        //{
        //    try
        //    {
        //        SteamNetworking.AcceptP2PSessionWithUser(otherID);
        //    }
        //    catch
        //    {
        //        Debug.Log("Unable to accept P2P Session with user " + otherID);
        //    }
        //}

        public static void OpenSteamOverlayLobbyInvite()
        {
            if (CurrentLobby.Id.IsValid)
                SteamFriends.OpenGameInviteOverlay(CurrentLobby.Id);
            else
                Debug.LogWarning("Tried to open overlay for a lobby invite, but no lobby has been joined!");
        }

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

        public static List<Lobby> GetFriendLobbies()
        {
            List<Lobby> lobbies = new List<Lobby>();

            foreach (Friend friend in SteamFriends.GetFriends())
            {
                if (friend.IsPlayingThisGame && friend.GameInfo.HasValue)
                {
                    Lobby? lobby = friend.GameInfo.Value.Lobby;

                    if (lobby.HasValue && lobby.Value.Id.IsValid)
                    {
                        if (CurrentLobby.Id != lobby.Value.Id)
                            lobbies.Add(lobby.Value);
                    }
                }
            }

            return lobbies;
        }


        public static void LoadScene(string sceneName)
        {
            if (!IsServer)
            {
                Debug.LogWarning($"Tried to load scene {sceneName}, but this client is not the server!");
                return;
            }

            //var scene = UnityEngine.SceneManagement.SceneManager.GetSceneByName(sceneName);
            //
            //if (!scene.IsValid())
            //{
            //    Debug.LogError($"Tried to load scene {sceneName}, but that scene does not exist!");
            //    return;
            //}

            int index = UnityEngine.SceneManagement.SceneUtility.GetBuildIndexByScenePath($"Assets/Scenes/{sceneName}.unity");

            if (index == -1)
            {
                Debug.LogError($"Tried to load scene {sceneName}, but that scene does not exist!");
                return;
            }

            foreach (Client client in clients.Values)
            {
                client.sceneLoaded = false;
            }

            NetworkID.ResetNetIDs();
            //DestroyAllRuntimeNetworkIDs();

            instance.suppressNetworkIDDestroyMessages = true;

            UnityEngine.SceneManagement.SceneManager.LoadScene(index);

            instance.suppressNetworkIDDestroyMessages = false;

            InternalServerSend.SendChangeScene(index);
        }

        public static bool AllClientsLoadedInScene()
        {
            foreach (Client client in clients.Values)
            {
                if (!client.sceneLoaded) return false;
            }

            return true;
        }

        internal static void ClientSceneLoaded(SteamId id)
        {
            Client client = clients[id];

            client.sceneLoaded = true;
            client.OnSceneFinishedLoading();
            OnClientSceneLoaded?.Invoke(id);

            if (AllClientsLoadedInScene())
            {
                OnAllClientsSceneLoaded?.Invoke();
            }

            if (IsServer)
                InternalServerSend.SendClientLoaded(client.clientID);
        }

        public static void ChangeLobbyPrivacy(SteamLobbyPrivacyMode mode)
        {
            if (CurrentLobby.IsOwnedBy(SteamID))
            {
                SetLobbyPrivacyMode(CurrentLobby, mode);
            }
        }

        public static void ChangeLobbyJoinability(bool joinable)
        {
            if (CurrentLobby.IsOwnedBy(SteamID))
            {
                CurrentLobby.SetJoinable(joinable);
            }
        }


#region Objects

        internal static void SpawnObject(NetworkID networkID)
        {
            if (!IsServer) return;
        
            InternalServerSend.SendNetworkIDSpawn(networkID);
        }
        
        internal static void SpawnObject(NetworkID networkID, SteamId onlyTo)
        {
            if (!IsServer) return;
        
            InternalServerSend.SendNetworkIDSpawn(networkID, onlyTo);
        }
        
        internal static void DestroyObject(NetworkID networkID)
        {
            // Called in NetworkIDs OnDestroy(), no need to destroy it here, just send the destroy to clients
            if (!IsServer) return;

            if (instance.suppressNetworkIDDestroyMessages) return;
            // May backfire

            InternalServerSend.SendNetworkIDDestroy(networkID);
        }

        public static void TryRegisterSpawnablePrefab(GameObject obj)
        {
            if (obj == null)
            {
                Debug.LogWarning("Tried to register a spawnable prefab, but the passed GameObject was null!");
                return;
            }
        
            if (!obj.TryGetComponent(out NetworkID networkID))
            {
                Debug.LogWarning($"Tried to register a spawnable prefab ({obj.name}), but that object has no NetworkID component!");
                return;
            }
        
            if (networkID.assetID == Guid.Empty)
            {
                Debug.LogWarning($"Tried to register a spawnable prefab ({obj.name}), but that object's NetworkID has no assetID! Is it a prefab?");
                return;
            }
        
            if (registeredPrefabs.ContainsKey(networkID.assetID))
            {
                Debug.Log($"A passed in prefab ({obj.name}) has the same assetID as a registered prefab ({registeredPrefabs[networkID.assetID].name}). Overwriting...");
            }
        
            registeredPrefabs[networkID.assetID] = obj;
        }

        private static void DestroyAllRuntimeNetworkIDs()
        {
            // Used when disconnecting or server shutdown
            // Destroy messages not sent
        
            //if (IsServer) return;
        
            uint[] netIDsToDestroy = new uint[NetworkID.networkIDs.Count];
            int numNetworkIDsToDestroy = 0;
        
            foreach (NetworkID networkID in NetworkID.networkIDs.Values)
            {
                if (networkID != null && networkID.sceneID == 0)
                {
                    netIDsToDestroy[numNetworkIDsToDestroy++] = networkID.netID;
                }
            }

            instance.suppressNetworkIDDestroyMessages = true;

            for (uint i = 0; i < numNetworkIDsToDestroy; i++)
            {
                NetworkID networkID = NetworkID.networkIDs[netIDsToDestroy[i]];
                if (networkID != null)
                {
                    Debug.Log("Destroying NetworkID " + networkID.name);
                    Destroy(networkID.gameObject);
                    NetworkID.networkIDs.Remove(netIDsToDestroy[i]);
                }
            }

            instance.suppressNetworkIDDestroyMessages = false;

            NetworkID.ResetNetIDs();
        }
#endregion

#region Socket Methods

        internal static void HandleDataFromClient(Connection connection, NetIdentity identity, IntPtr data, int size)
        {
            if (!IsServer) return;

            if (!connIDToSteamID.ContainsKey(connection.Id))
            {
                if (clientsPendingAuth.ContainsKey(connection.Id))
                {
                    try
                    {
                        Message message = Message.Create(data, size);

                        HandleDataFromUnauthedClient(message, connection);
                    }
                    catch (Exception ex)
                    {
                        Debug.Log($"Unable to process message from unauthed client! " + ex);
                    }
                }
                else
                {
                    Debug.Log($"Attempting to handle data from {new Friend(identity.SteamId).Name}, but cannot find that ID in any dictionary!");
                    connection.Close();
                }

                return;
            }

            SteamId clientID = connIDToSteamID[connection.Id];

            try
            {
                Message message = Message.Create(data, size);

                HandleDataFromClient(message, clientID);
            }
            catch (Exception ex)
            {
                Debug.Log($"Unable to process message from {new Friend(clientID).Name}! " + ex);
            }
        }

        internal static void HandleDataFromServer(IntPtr data, int size)
        {
            try
            {
                Message message = Message.Create(data, size);

                HandleDataFromServer(message);
            }
            catch (Exception e)
            {
                Debug.Log("Unable to process message from server: " + e);
            }
        }

        private static void CreateSteamSocketServer()
        {
            socketServer = SteamNetworkingSockets.CreateRelaySocket<SteamSocketManager>(STEAM_VIRTUAL_PORT);
            //connectionToServer = SteamNetworkingSockets.ConnectRelay<SteamConnectionManager>(SteamID);

            //Debug.Log($"Attempting connection to local server...");
        }

        private static void ConnectToServer(SteamId serverID)
        {
            Debug.Log($"Attempting connection to {new Friend(serverID).Name} ({serverID})...");
            connectionToServer = SteamNetworkingSockets.ConnectRelay<SteamConnectionManager>(serverID, STEAM_VIRTUAL_PORT);
        }

        internal static void OnConnectionConnected(Connection connection, ConnectionInfo data)
        {
            //Debug.Log("Incoming Connection: " + data.Identity.ToString());

            if (!instance.CanJoinServer(connection, data))
            {
                connection.Close();
                Debug.Log("Player connection attempt failed CanJoinServer() check. Disconnecting.");
                return;
            }

            if (clients.Count + clientsPendingAuth.Count >= MaxPlayers)
            {
                connection.Close();
                Debug.Log("Player managed to join full game. Disconnecting.");
                return;
            }

            Debug.Log("Client connected to server. Awaiting ready message.");

            Client client = instance.GetClient();
            client.OnCreate(connection, data.Identity.SteamId);

            if (clientsPendingAuth.Count == 1)
                FirstConnectionID = connection.Id;
            //Client.Create(connection, data.Identity.SteamId);
        }

        internal static void OnConnectionDisconnected(Connection connection, ConnectionInfo data)
        {
            uint connID = connection.Id;

            if (connIDToSteamID.ContainsKey(connID))
            {
                Client client = clients[connIDToSteamID[connID]];
                if (client != null)
                    client.Destroy();

                if (clientsPendingAuth.ContainsKey(connID)) clientsPendingAuth[connID].Destroy();
            }
        }


        internal static void OnConnConnectedToServer(ConnectionInfo info)
        {
            Message message = Message.CreateInternal(SendType.Reliable, (ushort)InternalClientMessageIDs.CONNECTED);
            //try
            //{
            //    instance.AddSpawnData(message);
            //}
            //catch (Exception ex)
            //{
            //    Debug.LogWarning("Error adding client spawn data! " + ex);
            //}
            SendMessageToServer(message);
        }

        #endregion

        #region Client Stuff
        private static async void SendAuthToServer()
        {
            if (IsServer)
            {
                Debug.Log("Skipping auth request for this user...");

                unverifiedSteamIDToConnID[SteamID] = FirstConnectionID;

                UserAuthenticated(SteamID);
                return;
            }

            Message message = Message.CreateInternal(SendType.Reliable, (ushort)InternalClientMessageIDs.AUTH_TICKET);
        
            currentAuthTicket = await SteamUser.GetAuthSessionTicketAsync();
        
            if (currentAuthTicket == null)
            {
                Debug.LogError("Could not generate valid auth ticket.");
                return;
            }
        
            Debug.Log("Sending auth ticket to server...");
            message.Add(SteamID).Add(currentAuthTicket.Data.Length).Add(currentAuthTicket.Data);
        
            SendMessageToServer(message);
        }


        private static void HandleDataFromUnauthedClient(Message receievedMessage, Connection connection)
        {
            if (!IsServer) return;
        
            NetStats.OnPacketReceived(receievedMessage.WrittenLength);
        
            ushort id = receievedMessage.GetUShort();
            if (!clientsPendingAuth.TryGetValue(connection.Id, out Client client))
            {
                Debug.LogWarning("Could not find client in the unAuthed dict!");
                connection.Close();
                return;
            }
        
            if (Message.IsInternalMessage(id))
            {
                if (id == (ushort)InternalClientMessageIDs.CONNECTED)
                {
                    client.connected = true;

                    //try
                    //{
                    //    client.GetSpawnData(receievedMessage);
                    //}
                    //catch (Exception ex)
                    //{
                    //    Debug.LogWarning("Error getting client spawn data! " + ex);
                    //}

                    Message message = Message.CreateInternal(SendType.Reliable, (ushort)InternalServerMessageIDs.REQUEST_AUTH);
        
                    int sizeOfMessage = message.WrittenLength;
                    IntPtr intPtrMessage = System.Runtime.InteropServices.Marshal.AllocHGlobal(sizeOfMessage);
                    System.Runtime.InteropServices.Marshal.Copy(message.Bytes, 0, intPtrMessage, sizeOfMessage);
        
                    Debug.Log("Sending auth request to client...");
                    SendDataToClient(intPtrMessage, client.connection, sizeOfMessage, SendType.Reliable);
                }
        
                else if (id == (ushort)InternalClientMessageIDs.AUTH_TICKET)
                {
                    ReceivedAuthenticationDataFromClient(client, receievedMessage);
                }
            }
        }

        private static void ReceivedAuthenticationDataFromClient(Client client, Message message)
        {
            if (!IsServer) return;

            SteamId steamId = message.GetSteamId();

            if (steamId == 0)
            {
                Debug.Log("Received SteamID was zero.");
                client.Destroy();
                return;
            }

            if (unverifiedSteamIDToConnID.ContainsKey(steamId))
            {
                Debug.LogWarning($"Found pre-existing auth mapping for {steamId.SteamName()}!");
                //client.Destroy();
                //return;

                // May be crap/spoofed steam ID
            }

            unverifiedSteamIDToConnID[steamId] = client.connection.Id;

            if (steamId != SteamID)
                Debug.Log($"Received auth ticket, supposedly from {steamId.SteamName()}. Message length: {message.UnreadLength + Util.LONG_LENGTH}");

            try
            {
                int length = message.GetInt();
                byte[] authBytes = message.GetByteArray(length);

                if (authBytes == null || authBytes.Length == 0)
                {
                    Debug.Log($"Removing {steamId.SteamName()} for invalid auth data");
                    client.Destroy();
                    return;
                }

                BeginAuthResult result = SteamUser.BeginAuthSession(authBytes, steamId);
                if (steamId != SteamID)
                    Debug.Log($"Starting auth session for {steamId.SteamName()}, initial result is " + result);
            }
            catch (Exception)
            {
                Debug.LogWarning("Failure starting auth session with " + steamId.SteamName());
                client.Destroy();
            }
        }
        
        private static void UserAuthenticated(SteamId id)
        {
            if (!IsServer) return;

            Debug.Log("Successfully authenticated " + id.SteamName());

            if (!unverifiedSteamIDToConnID.TryGetValue(id, out uint connID))
            {
                Debug.LogWarning($"Could not get connection ID from {id.SteamName()}'s id ({id})");
                return;
            }

            if (!clientsPendingAuth.TryGetValue(connID, out Client client))
            {
                Debug.LogWarning("Null client in auth for " + id.SteamName());
                return;
            }

            try
            {
                client.OnAuthorized(id);
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"Error authorizing {id.SteamName()}: " + ex);
                client.Destroy();
            }


            try
            {
                if (id == SteamID)
                    OnConnectedToServer?.Invoke(); // Called for host

                OnClientConnected?.Invoke(client);
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"Error executing post-auth events for {id.SteamName()}: " + ex);
            }


            // Connect user
        }
        
        private static void UserNotAuthenticated(SteamId id, AuthResponse response)
        {
            if (!IsServer) return;

            Debug.Log($"Received bad authentication for {id.SteamName()}: {response}");

            uint connID = unverifiedSteamIDToConnID[id];
            Client client = clientsPendingAuth[connID];
            client.Destroy();
            // Disconnect user
        }


        internal static void OnClientConnected_Message(Client client)
        {
            // Called client-side
            OnClientConnected?.Invoke(client);
        }

        internal static void OnClientDestroyed(Client client)
        {
            // Passed client is authenticated already
            OnClientDisconnected?.Invoke(client);

            if (client.SteamID == SteamID)
            {
                OnDisconnectedFromServer?.Invoke();
                Leave();
            }
        }

        internal static void OnThisClientConnectedToServer()
        {
            OnConnectedToServer?.Invoke();
        }


        internal static byte GetFreeClientID()
        {
            for (byte i = 0; i < MaxPlayers; i++)
            {
                if (!clientIDToSteamID.ContainsKey(i))
                    return i;
            }

            return byte.MaxValue;
        }


        private void DisconnectOldConnections()
        {
            List<uint> invalidIDs = clientsPendingAuth.Where(pair => Time.realtimeSinceStartup -
                pair.Value.TimeCreated > UNAUTHENTICATED_CLIENT_TIMEOUT)
                         .Select(pair => pair.Key)
                         .ToList();

            foreach (uint connID in invalidIDs)
            {
                Client client = clientsPendingAuth[connID];
                Debug.Log($"Removing unauthenticated connection {client.connection.Id}, supposed SteamID {new Friend(client.SteamID)} ({client.SteamID})");
                client.Destroy();
            }
        }

        #endregion

        #region Inspector Methods

        [ContextMenu("Host Server")]
        public void Inspector_HostServer()
        {
            Host();
        }

        [ContextMenu("Log Clients")]
        public void Inspector_LogClients()
        {
            System.Text.StringBuilder builder = new System.Text.StringBuilder();

            builder.AppendLine("Connected to server: " + ConnectedToServer);

            if (ConnectedToServer)
            {
                Type customClientType = GetClient().GetType();

                builder.AppendLine("Authenticated clients: ");
                foreach (Client client in clients.Values)
                {
                    builder.AppendLine("\n  -" + client.SteamName + ": Is " + customClientType.Name + "? " + (client.GetType() == customClientType));
                }

                builder.AppendLine();

                builder.AppendLine("ConnID -> SteamID mapping table: ");
                foreach (KeyValuePair<uint, SteamId> id in connIDToSteamID)
                {
                    builder.AppendLine($"  -{id.Key} -> {id.Value.SteamName()}");
                }

                builder.AppendLine();

                builder.AppendLine("ClientID -> SteamID mapping table: ");
                foreach (KeyValuePair<byte, SteamId> id in clientIDToSteamID)
                {
                    builder.AppendLine($"  -{id.Key} -> {id.Value.SteamName()}");
                }



                builder.AppendLine();
                builder.AppendLine();

                builder.AppendLine("Clients pending authentication: ");
                foreach (Client client in clientsPendingAuth.Values)
                {
                    builder.AppendLine("\n  -" + client.connection.Id);
                }

                builder.AppendLine();
                builder.AppendLine("Supposed SteamID -> ConnID mapping table: ");
                foreach (KeyValuePair<SteamId, uint> id in unverifiedSteamIDToConnID)
                {
                    builder.AppendLine($"  -{id.Key.SteamName()} -> {id.Value}");
                }
            }

            Debug.Log(builder.ToString());

            builder.Clear();
        }

        [ContextMenu("Log Lobby")]
        public void Inspector_LogLobby()
        {
            System.Text.StringBuilder builder = new System.Text.StringBuilder();

            builder.AppendLine("Connected to lobby: " + CurrentLobby.Id.IsValid);

            if (CurrentLobby.Id.IsValid)
            {
                builder.AppendLine($"Lobby ID: {CurrentLobby.Id}");
                builder.AppendLine($"Lobby Owner: {CurrentLobby.Owner.Name} ({CurrentLobby.Owner.Id})");
            }

            Debug.Log(builder.ToString());

            builder.Clear();
        }

        [ContextMenu("Log Friend Lobbies")]
        public void Inspector_LogFriendLobbies()
        {
            System.Text.StringBuilder builder = new System.Text.StringBuilder();

            List<Lobby> lobbies = GetFriendLobbies();

            builder.AppendLine("Num lobbies found: " + lobbies.Count);

            foreach (Lobby lobby in lobbies)
            {
                builder.AppendLine($"--- Lobby Owner: {lobby.Owner.Name} - Number of members: {lobby.MemberCount}");
            }

            Debug.Log(builder.ToString());
        }

        [ContextMenu("Send test message to server")]
        public void Inspector_SendTestMessageToServer()
        {
            InternalClientSend.SendClientMessageTest($"Hello from {SteamName}!");
        }

        [ContextMenu("Send test message to clients")]
        public void Inspector_SendTestMessageToClients()
        {
            InternalServerSend.SendServerMessageTest($"Hello from the server, {SteamName}!");
        }

        #endregion

        /// <summary>
        /// Used for using a custom client type. By default, returns new Client(). Override with new Player(), or whatever you are using.
        /// </summary>
        /// <returns></returns>
        protected internal virtual Client GetClient()
        {
            return new Client();
        }

        /// <summary>
        /// Called when this user tries to join <paramref name="lobby"/> via the Steam friends list.
        /// </summary>
        /// <param name="lobby"></param>
        /// <returns></returns>
        protected virtual bool ShouldJoinGame(Lobby lobby)
        {
            return true;
        }

        protected virtual bool CanJoinServer(Connection connection, ConnectionInfo data)
        {
            return true;
        }

        protected virtual void OnAwake() { }

        protected virtual void OnUpdate() { }

        protected virtual void OnDisabled() { }

        protected internal virtual void ChangeToScene(int buildIndex) { }


        #region GetClient Methods
        /// <summary>
        /// Attempts to get the client of type <typeparamref name="T"/> with the SteamID <paramref name="id"/>
        /// </summary>
        /// <typeparam name="T">The type of client to get (if you have defined a custom type)</typeparam>
        /// <param name="id">The SteamID of the client</param>
        /// <returns></returns>
        public static T GetClient<T>(SteamId id) where T : Client
        {
            if (!clients.TryGetValue(id, out Client client))
            {
                Debug.LogWarning($"Could not get client {id.SteamName()} ({id}), as they were not present in the dictionary.");
                return null;
            }

            if (!(client is T custom))
            {
                Debug.LogWarning($"The client \"{client.SteamName}\" was not of type {typeof(T).Name}.");
                return null;
            }

            return custom;

            //return GetTryCastClient<T>(client);
        }

        /// <summary>
        /// Attempts to get the client of type <typeparamref name="T"/> with the given <paramref name="clientID"/>
        /// </summary>
        /// <typeparam name="T">The type of client to get (if you have defined a custom type)</typeparam>
        /// <param name="clientID">The client ID of the client</param>
        /// <returns></returns>
        public static T GetClient<T>(byte clientID) where T : Client
        {
            if (!clientIDToSteamID.TryGetValue(clientID, out SteamId steamID))
            {
                Debug.LogWarning($"Could not get client (clientID {clientID}), as they were not present in the dictionary.");
                return null;
            }

            return GetClient<T>(steamID);
        }

        /// <summary>
        /// Attempts to get the client of type <typeparamref name="T"/> with the given <paramref name="connID"/>
        /// </summary>
        /// <typeparam name="T">The type of client to get (if you have defined a custom type)</typeparam>
        /// <param name="connID">The connection ID of the client</param>
        /// <returns></returns>
        public static T GetClient<T>(uint connID) where T : Client
        {
            if (!connIDToSteamID.TryGetValue(connID, out SteamId steamID))
            {
                Debug.LogWarning($"Could not get client (connection ID {connID}), as they were not present in the dictionary.");
                return null;
            }

            return GetClient<T>(steamID);
        }

        public static List<Client> GetAllClients()
        {
            return clients.Values.ToList();
        }

        public static List<T> GetAllClients<T>() where T : Client
        {
            List<T> validClients = new List<T>(clients.Count);
            foreach (Client client in clients.Values)
            {
                if (client is T value)
                    validClients.Add(value);
                else
                    Debug.LogWarning("Client in dictionary was not " + typeof(T).Name);
            }

            return validClients;
        }

        public static Client GetLocalClient()
        {
            foreach (Client client in clients.Values)
            {
                if (client.SteamID == SteamID) return client;
            }

            Debug.LogWarning("Couldn't get local client!");
            return null;
        }

        public static T GetLocalClient<T>() where T : Client
        {
            Client client = GetLocalClient();

            if (client != null && client is T outClient) return outClient;

            else
            {
                Debug.LogWarning("Could get local " + typeof(T).Name);
                return null;
            }
        }
        #endregion

        #region TryGetClient Methods
        /// <summary>
        /// Attempts to get the client of type <typeparamref name="T"/> with the SteamID <paramref name="id"/>
        /// </summary>
        /// <typeparam name="T">The type of client to get (if you have defined a custom type)</typeparam>
        /// <param name="id">The SteamID of the client</param>
        /// <returns></returns>
        public static bool TryGetClient<T>(SteamId id, out T outClient) where T : Client
        {
            if (!clients.TryGetValue(id, out Client client))
            {
                Debug.LogWarning($"Could not get client {id.SteamName()} ({id}), as they were not present in the dictionary.");
                outClient = null;
                return false;
            }

            if (!(client is T custom))
            {
                Debug.LogWarning($"The client \"{client.SteamName}\" was not of type {typeof(T).Name}.");
                outClient = null;
                return false;
            }

            outClient = custom;
            return true;

            //return GetTryCastClient<T>(client);
        }

        /// <summary>
        /// Attempts to get the client of type <typeparamref name="T"/> with the given <paramref name="clientID"/>
        /// </summary>
        /// <typeparam name="T">The type of client to get (if you have defined a custom type)</typeparam>
        /// <param name="clientID">The client ID of the client</param>
        /// <returns></returns>
        public static bool TryGetClient<T>(byte clientID, out T outClient) where T : Client
        {
            if (!clientIDToSteamID.TryGetValue(clientID, out SteamId steamID))
            {
                Debug.LogWarning($"Could not get client (clientID {clientID}), as they were not present in the dictionary.");
                outClient = null;
                return false;
            }

            return TryGetClient(steamID, out outClient);
        }

        /// <summary>
        /// Attempts to get the client of type <typeparamref name="T"/> with the given <paramref name="connID"/>
        /// </summary>
        /// <typeparam name="T">The type of client to get (if you have defined a custom type)</typeparam>
        /// <param name="connID">The connection ID of the client</param>
        /// <returns></returns>
        public static bool TryGetClient<T>(uint connID, out T outClient) where T : Client
        {
            if (!connIDToSteamID.TryGetValue(connID, out SteamId steamID))
            {
                Debug.LogWarning($"Could not get client (connection ID {connID}), as they were not present in the dictionary.");
                outClient = null;
                return false;
            }

            return TryGetClient(steamID, out outClient);
        }
        #endregion

        #region GetClient Vanilla Methods
        /// <summary>
        /// Attempts to get the client with the SteamID <paramref name="id"/>
        /// </summary>
        /// <param name="id">The SteamID of the client</param>
        /// <returns></returns>
        public static Client GetClient(SteamId id)
        {
            if (!clients.TryGetValue(id, out Client client))
            {
                Debug.LogWarning($"Could not get client {id.SteamName()} ({id}), as they were not present in the dictionary.");
                return null;
            }

            return client;

            //return GetTryCastClient<T>(client);
        }

        /// <summary>
        /// Attempts to get the client with the given <paramref name="clientID"/>
        /// </summary>
        /// <param name="clientID">The client ID of the client</param>
        /// <returns></returns>
        public static Client GetClient(byte clientID)
        {
            if (!clientIDToSteamID.TryGetValue(clientID, out SteamId steamID))
            {
                Debug.LogWarning($"Could not get client (clientID {clientID}), as they were not present in the dictionary.");
                return null;
            }

            return GetClient(steamID);
        }

        /// <summary>
        /// Attempts to get the client with the given <paramref name="connID"/>
        /// </summary>
        /// <param name="connID">The connection ID of the client</param>
        /// <returns></returns>
        public static Client GetClient(uint connID)
        {
            if (!connIDToSteamID.TryGetValue(connID, out SteamId steamID))
            {
                Debug.LogWarning($"Could not get client (connection ID {connID}), as they were not present in the dictionary.");
                return null;
            }

            return GetClient(steamID);
        }
        #endregion

        internal static void ConnectionClosed()
        {
            OnConnectionClosed?.Invoke();
        }

        private void CheckForCommandLineJoins()
        {
            CancelInvoke();
            Invoke(nameof(CheckForArgsDelayed), 0.5f);
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
                    SteamFriends_OnGameLobbyJoinRequested(lobby, 0);
                }
            }
        }

        //private static T GetTryCastClient<T>(Client client) where T : Client
        //{
        //    if (!(client is T custom))
        //    {
        //        Debug.LogWarning($"The client \"{client.SteamName}\" was not of type {typeof(T).Name}.");
        //        return null;
        //    }
        //
        //    return custom;
        //}
    }

    public enum SteamLobbyPrivacyMode
    {
        Public,
        Private,
        FriendsOnly,
        Invisible,
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
    */
}
