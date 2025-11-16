using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;
using Monarchs.GameServer;
using Monarchs.Logic;
using UnityEngine.Events;
using Unity.Netcode;

namespace TcgEngine.Server
{
    /// <summary>
    /// Top-level server script, that manages new connection, and assign players to the right match
    /// Will also receive game actions and send them to the appropirate game
    /// Can contain multiple games at once (GameServer)
    /// </summary>

    public class ServerManager : MonoBehaviour
    {
        [Header("API")]
        public string api_username;
        public string api_password;

        private Dictionary<ulong, ClientData> client_list = new Dictionary<ulong, ClientData>();  //List of clients
        private Dictionary<string, ClientData> userList = new Dictionary<string, ClientData>();
        protected Dictionary<string, GameServer> game_list = new Dictionary<string, GameServer>(); //List of games
        protected Dictionary<string, GameServer> disconnected_games = new Dictionary<string, GameServer>(); //first string is UserID of disconnected player
        private List<string> game_remove_list = new List<string>();
        private bool try_login = false;

        protected virtual void Awake()
        {
            Application.runInBackground = true;
            Application.targetFrameRate = 200; //Limit server frame rate to prevent using 100% cpu
        }

        protected virtual void Start()
        {
            TcgNetwork network = TcgNetwork.Get();
            network.onClientJoin += OnClientConnected;
            network.onClientQuit += OnClientDisconnected;
            Messaging.ListenMsg("connect", ReceiveConnectPlayer);
            Messaging.ListenMsg("send_player_info", ReceivePlayerInfo);
            Messaging.ListenMsg("action", ReceiveGameAction);

            if (!network.IsActive())
            {
                network.StartServer(NetworkData.Get().port);
            }

            Login();
        }

        protected virtual void Update()
        {
            //Update games and Destroy games with no players
            foreach (KeyValuePair<string, GameServer> pair in game_list)
            {
                GameServer gserver = pair.Value;
                gserver.Update();

                if (gserver.IsGameExpired())
                    game_remove_list.Add(pair.Key);
            }

            foreach (string key in game_remove_list)
            {
                RemoveGameFromDisconnectedGames(game_list[key]);
                
                game_list.Remove(key);
                

                if (ServerMatchmaker.Get())
                    ServerMatchmaker.Get().EndMatch(key);
            }
            game_remove_list.Clear();
        }
        
        void RemoveGameFromDisconnectedGames(GameServer game)
        {
            if (game == null)
                return;

            if (disconnected_games.ContainsValue(game))
            {
                var keysToRemove = disconnected_games
                    .Where(pair => pair.Value == game)
                    .Select(pair => pair.Key)
                    .ToList();

                foreach (var k in keysToRemove)
                {
                    disconnected_games.Remove(k);
                }
            }
        }


        protected virtual async void Login()
        {
            await Authenticator.Get().Login(api_username, api_password);
            AfterLogin();
        }
         
        protected virtual void AfterLogin()
        {
            bool success = Authenticator.Get().IsConnected();
            int permission = Authenticator.Get().GetPermission();
            string api = Authenticator.Get().IsApi() ? "API" : "Test";
            Debug.Log(api + " authentication: " + success + " (" + permission + ")");

            //If auto-refresh fail, login again
            if (!success && !try_login)
            {
                try_login = true;
                TimeTool.WaitFor(5f, () =>
                {
                    Login();
                });
            }
        }

        protected virtual void OnClientConnected(ulong client_id)
        {
            ClientData iclient = new ClientData(client_id);
            client_list[client_id] = iclient;
        }

        protected virtual void OnClientDisconnected(ulong client_id)
        {
            ClientData iclient = GetClient(client_id);
            client_list.Remove(client_id);
            ReceiveDisconnectPlayer(iclient);
            if (userList.ContainsValue(iclient))
            {
                userList.Remove(iclient.user_id); //Remove from user list
            }
        }

        protected virtual void ReceiveConnectPlayer(ulong client_id, FastBufferReader reader)
        {
            ClientData iclient = GetClient(client_id);
            reader.ReadNetworkSerializable(out MsgPlayerConnect msg);

            if (iclient != null && msg != null)
            {
                if (string.IsNullOrWhiteSpace(msg.username))
                    return;

                if (string.IsNullOrWhiteSpace(msg.game_uid))
                    return;

                Debug.Log("Client " + client_id + " connected to game: " + msg.game_uid);


                if (msg.observer)
                    ConnectObserverToGame(iclient, msg.user_id, msg.username, msg.game_uid);
                else
                    ConnectPlayerToGame(iclient, msg.user_id, msg.username, msg.game_uid, msg.nb_players);

                GameServer gserver = GetGame(iclient.game_uid);
                if (gserver != null)
                {
                    if (gserver.GetGameData().State == GameState.PlayerDisconnected && !gserver.IsGameOver())
                    {
                        gserver.OnPlayerReconnected(iclient);
                    }
                    else
                    {
                        gserver.RefreshAll();
                    }
                }
                    
            }
        }
        
        protected virtual void ReceivePlayerInfo(ulong client_id, FastBufferReader reader)
        {
            ClientData iclient = GetClient(client_id);
            reader.ReadNetworkSerializable(out MsgPlayerConnect msg);

            if (iclient != null && msg != null)
            {
                if (string.IsNullOrWhiteSpace(msg.user_id))
                    return;

                
                
                iclient.user_id = msg.user_id;
                Debug.Log("Client " + client_id + " user_id: " + iclient.user_id);
                
                if (userList.ContainsKey(msg.user_id))
                {
                    //If user already exists, update client id
                    ClientData existingClient = userList[msg.user_id];
                    if (existingClient.client_id != iclient.client_id)
                    {
                        //Disconnect old client
                        ForceDisconnectClient(existingClient);
                    }
                }
                userList[msg.user_id] = iclient; //Add to user list
                
                ValidateIfPlayerWasDisonnectedFromGame(iclient);
            }
        }

        protected virtual void ValidateIfPlayerWasDisonnectedFromGame(ClientData client)
        {
            disconnected_games.TryGetValue(client.user_id, out GameServer gserver);
            if (gserver != null)
            {
                if (gserver.IsGameOver())
                    return;
                
                Messaging.SendObject("reconnected", client.client_id, new MsgPlayerConnect()
                {
                    game_uid = gserver.gameUID,
                    user_id = "",
                    username = "",
                }, NetworkDelivery.Reliable);
            }
        }
        
        protected void ForceDisconnectClient(ClientData client)
        {
            if (client != null)
            {
                //Send disconnect message to client
                MsgInt msg = new MsgInt();
                msg.value = -1; //Force disconnect

                Messaging.SendObject("force_disconnect", client.client_id, msg, NetworkDelivery.Reliable);
                
                //Remove from game if connected
                ReceiveDisconnectPlayer(client);
                
                //Remove from client list
                if (client_list.ContainsKey(client.client_id))
                {
                    client_list.Remove(client.client_id);
                }
                //Remove from user list if exists
                if (userList.ContainsKey(client.user_id))
                {
                    userList.Remove(client.user_id);
                }
                Debug.Log("Client " + client.client_id + " disconnected forcefully.");
            }
        }

        protected virtual void ReceiveDisconnectPlayer(ClientData iclient)
        {
            if (iclient == null)
                return;

            GameServer gserver = GetGame(iclient.game_uid);
            if (gserver != null)
            {
                
                gserver.RemoveClient(iclient);
                disconnected_games[iclient.user_id] = gserver; //Store game for later use
                
                gserver.OnPlayerDisconnected(iclient);
            }
        }

        protected virtual void ReceiveGameAction(ulong client_id, FastBufferReader reader)
        {
            ClientData client = GetClient(client_id);
            if (client != null)
            {
                GameServer gserver = GetGame(client.game_uid);
                if (gserver != null && gserver.IsPlayer(client))
                    gserver.ReceiveAction(client_id, reader);
            }
        }

        //Player wants to connect to game_uid
        protected virtual void ConnectPlayerToGame(ClientData client, string user_id, string username, string game_uid, int nb_players)
        {
            //Get or Create game
            GameServer gserver = GetGame(game_uid);

            if (gserver == null)
                gserver = CreateGame(game_uid, nb_players);

            bool can_connect = gserver.IsPlayer(client) || gserver.CountPlayers() < gserver.nbPlayers;
            if (gserver != null && can_connect)
            {
                //Add player to game
                client.game_uid = gserver.gameUID;
                client.user_id = user_id;
                client.username = username;
                gserver.AddClient(client);

                int player_id = gserver.AddPlayer(client);
                Player player = gserver.GetGameData().GetPlayer(player_id);
                if (player != null)
                {
                    player.username = username;
                    player.connected = true;
                }

                //Return request
                MsgAfterConnected msg_data = new MsgAfterConnected();
                msg_data.success = true;
                msg_data.playerID = player_id;
                msg_data.game_data = gserver.GetGameData();
                SendToClient(client.client_id, GameAction.Connected, msg_data, NetworkDelivery.ReliableFragmentedSequenced);
            }
        }

        //Player wants to connect to game_uid as observer
        protected virtual void ConnectObserverToGame(ClientData iclient, string user_id, string username, string game_uid)
        {
            GameServer gserver = GetGame(game_uid);
            if (gserver != null && iclient != null)
            {
                //Add player to game
                iclient.game_uid = gserver.gameUID;
                iclient.user_id = user_id;
                iclient.username = username;
                gserver.AddClient(iclient);

                //Return request
                MsgAfterConnected msg_data = new MsgAfterConnected();
                msg_data.success = true;
                msg_data.playerID = -1;
                msg_data.game_data = gserver.GetGameData();
                SendToClient(iclient.client_id, GameAction.Connected, msg_data, NetworkDelivery.ReliableFragmentedSequenced);
            }
        }
        
        public void SendToClient(ulong client_id, ushort tag, INetworkSerializable data, NetworkDelivery delivery)
        {
            FastBufferWriter writer = new FastBufferWriter(128, Unity.Collections.Allocator.Temp, TcgNetwork.MsgSizeMax);
            writer.WriteValueSafe(tag);
            writer.WriteNetworkSerializable(data);
            Messaging.Send("refresh", client_id, writer, delivery);
            writer.Dispose();
        }

        public void SendMsgToClient(ushort client_id, string msg)
        {
            FastBufferWriter writer = new FastBufferWriter(128, Unity.Collections.Allocator.Temp, TcgNetwork.MsgSizeMax);
            writer.WriteValueSafe(GameAction.ServerMessage);
            writer.WriteValueSafe(msg);
            Messaging.Send("refresh", client_id, writer, NetworkDelivery.Reliable);
            writer.Dispose();
        }

        public virtual GameServer CreateGame(string uid, int nb_players)
        {
            GameServer game = new GameServer(uid, nb_players, true);
            game_list[game.gameUID] = game;
            return game;
        }

        public void RemoveGame(string game_id)
        {
            game_list.Remove(game_id);
        }

        public GameServer GetGame(string game_uid)
        {
            if (string.IsNullOrEmpty(game_uid))
                return null;
            if (game_list.ContainsKey(game_uid))
                return game_list[game_uid];
            return null;
        }

        public ClientData GetClient(ulong client_id)
        {
            if (client_list.ContainsKey(client_id))
                return client_list[client_id];
            return null;
        }

        public ClientData GetClientByUser(string username)
        {
            foreach (KeyValuePair<ulong, ClientData> pair in client_list)
            {
                if (pair.Value.username == username)
                    return pair.Value;
            }
            return null;
        }

        public ulong ServerID { get { return TcgNetwork.Get().ServerID; } }
        public NetworkMessaging Messaging { get { return TcgNetwork.Get().Messaging; } }
    }

    public class ClientData
    {
        public ulong client_id; //index of the connection
        public string user_id; //Player user_id, in auth system
        public string username; //Player username
        public string game_uid; //Unique id for the game

        public ClientData(ulong id) { client_id = id; }
        public bool IsInGame() { return !string.IsNullOrEmpty(game_uid); }
    }

    public class CommandEvent
    {
        public ushort tag;
        public UnityAction<ClientData, SerializedData> callback;
    }
}