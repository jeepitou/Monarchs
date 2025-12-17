using System.Collections.Generic;
using System.Linq;
using Monarchs.Logic;
using TcgEngine;
using TcgEngine.Server;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Events;

namespace Monarchs.GameServer
{
    /// <summary>
    /// Top-level server script, that manages new connection, and assign players to the right match
    /// Will also receive game actions and send them to the appropirate game
    /// Can contain multiple games at once (GameServer)
    /// </summary>

    public class ServerManager : MonoBehaviour
    {
        [Header("API")]
        public string apiUsername;
        public string apiPassword;

        private Dictionary<ulong, ClientData> _clientList = new ();  //List of clients
        private Dictionary<string, ClientData> _userList = new ();
        protected Dictionary<string, GameServer> _gameList = new (); //List of games
        protected Dictionary<string, GameServer> _disconnectedGames = new (); //first string is UserID of disconnected player
        private List<string> _gameRemoveList = new ();
        private bool _tryLogin;

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
            foreach (KeyValuePair<string, GameServer> pair in _gameList)
            {
                GameServer gserver = pair.Value;
                gserver.Update();

                if (gserver.IsGameExpired())
                    _gameRemoveList.Add(pair.Key);
            }

            foreach (string key in _gameRemoveList)
            {
                RemoveGameFromDisconnectedGames(_gameList[key]);
                
                _gameList.Remove(key);
                

                if (ServerMatchmaker.Get())
                    ServerMatchmaker.Get().EndMatch(key);
            }
            _gameRemoveList.Clear();
        }
        
        void RemoveGameFromDisconnectedGames(GameServer game)
        {
            if (game == null)
                return;

            if (_disconnectedGames.ContainsValue(game))
            {
                var keysToRemove = _disconnectedGames
                    .Where(pair => pair.Value == game)
                    .Select(pair => pair.Key)
                    .ToList();

                foreach (var k in keysToRemove)
                {
                    _disconnectedGames.Remove(k);
                }
            }
        }


        protected virtual async void Login()
        {
            await Authenticator.Get().Login(apiUsername, apiPassword);
            AfterLogin();
        }
         
        protected virtual void AfterLogin()
        {
            bool success = Authenticator.Get().IsConnected();
            int permission = Authenticator.Get().GetPermission();
            string api = Authenticator.Get().IsApi() ? "API" : "Test";
            Debug.Log(api + " authentication: " + success + " (" + permission + ")");

            //If auto-refresh fail, login again
            if (!success && !_tryLogin)
            {
                _tryLogin = true;
                TimeTool.WaitFor(5f, () =>
                {
                    Login();
                });
            }
        }

        protected virtual void OnClientConnected(ulong clientID)
        {
            ClientData iclient = new ClientData(clientID);
            _clientList[clientID] = iclient;
        }

        protected virtual void OnClientDisconnected(ulong clientID)
        {
            ClientData iclient = GetClient(clientID);
            _clientList.Remove(clientID);
            ReceiveDisconnectPlayer(iclient);
            if (_userList.ContainsValue(iclient))
            {
                _userList.Remove(iclient.userID); //Remove from user list
            }
        }

        protected virtual void ReceiveConnectPlayer(ulong clientID, FastBufferReader reader)
        {
            ClientData iclient = GetClient(clientID);
            reader.ReadNetworkSerializable(out MsgPlayerConnect msg);

            if (iclient != null && msg != null)
            {
                if (string.IsNullOrWhiteSpace(msg.username))
                    return;

                if (string.IsNullOrWhiteSpace(msg.game_uid))
                    return;

                Debug.Log("Client " + clientID + " connected to game: " + msg.game_uid);


                if (msg.observer)
                    ConnectObserverToGame(iclient, msg.user_id, msg.username, msg.game_uid);
                else
                    ConnectPlayerToGame(iclient, msg.user_id, msg.username, msg.game_uid, msg.nb_players);

                GameServer gserver = GetGame(iclient.gameUID);
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
        
        protected virtual void ReceivePlayerInfo(ulong clientID, FastBufferReader reader)
        {
            ClientData iclient = GetClient(clientID);
            reader.ReadNetworkSerializable(out MsgPlayerConnect msg);

            if (iclient != null && msg != null)
            {
                if (string.IsNullOrWhiteSpace(msg.user_id))
                    return;
                
                iclient.userID = msg.user_id;
                Debug.Log("Client " + clientID + " user_id: " + iclient.userID);
                
                if (_userList.ContainsKey(msg.user_id))
                {
                    //If user already exists, update client id
                    ClientData existingClient = _userList[msg.user_id];
                    if (existingClient.clientID != iclient.clientID)
                    {
                        //Disconnect old client
                        ForceDisconnectClient(existingClient);
                    }
                }
                _userList[msg.user_id] = iclient; //Add to user list
                
                ValidateIfPlayerWasDisonnectedFromGame(iclient);
            }
        }

        protected virtual void ValidateIfPlayerWasDisonnectedFromGame(ClientData client)
        {
            _disconnectedGames.TryGetValue(client.userID, out GameServer gserver);
            if (gserver != null)
            {
                if (gserver.IsGameOver())
                    return;
                
                Messaging.SendObject("reconnected", client.clientID, new MsgPlayerConnect()
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

                Messaging.SendObject("force_disconnect", client.clientID, msg, NetworkDelivery.Reliable);
                
                //Remove from game if connected
                ReceiveDisconnectPlayer(client);
                
                //Remove from client list
                if (_clientList.ContainsKey(client.clientID))
                {
                    _clientList.Remove(client.clientID);
                }
                //Remove from user list if exists
                if (_userList.ContainsKey(client.userID))
                {
                    _userList.Remove(client.userID);
                }
                Debug.Log("Client " + client.clientID + " disconnected forcefully.");
            }
        }

        protected virtual void ReceiveDisconnectPlayer(ClientData iclient)
        {
            if (iclient == null)
                return;

            GameServer gserver = GetGame(iclient.gameUID);
            if (gserver != null)
            {
                
                gserver.RemoveClient(iclient);
                _disconnectedGames[iclient.userID] = gserver; //Store game for later use
                
                gserver.OnPlayerDisconnected(iclient);
            }
        }

        protected virtual void ReceiveGameAction(ulong clientID, FastBufferReader reader)
        {
            ClientData client = GetClient(clientID);
            if (client != null)
            {
                GameServer gserver = GetGame(client.gameUID);
                if (gserver != null && gserver.IsPlayer(client))
                    gserver.ReceiveAction(clientID, reader);
            }
        }

        //Player wants to connect to game_uid
        protected virtual void ConnectPlayerToGame(ClientData client, string userID, string username, string gameUID, int nbPlayers)
        {
            //Get or Create game
            GameServer gserver = GetGame(gameUID);

            if (gserver == null)
                gserver = CreateGame(gameUID, nbPlayers);

            bool canConnect = gserver.IsPlayer(client) || gserver.CountPlayers() < gserver.nbPlayers;
            if (canConnect)
            {
                //Add player to game
                client.gameUID = gserver.gameUID;
                client.userID = userID;
                client.username = username;
                gserver.AddClient(client);

                int playerID = gserver.AddPlayer(client);
                Player player = gserver.GetGameData().GetPlayer(playerID);
                if (player != null)
                {
                    player.username = username;
                    player.connected = true;
                }

                //Return request
                MsgAfterConnected msgData = new MsgAfterConnected();
                msgData.success = true;
                msgData.playerID = playerID;
                msgData.game_data = gserver.GetGameData();
                SendToClient(client.clientID, GameAction.Connected, msgData, NetworkDelivery.ReliableFragmentedSequenced);
            }
        }

        //Player wants to connect to game_uid as observer
        protected virtual void ConnectObserverToGame(ClientData iclient, string userID, string username, string gameUID)
        {
            GameServer gserver = GetGame(gameUID);
            if (gserver != null && iclient != null)
            {
                //Add player to game
                iclient.gameUID = gserver.gameUID;
                iclient.userID = userID;
                iclient.username = username;
                gserver.AddClient(iclient);

                //Return request
                MsgAfterConnected msgData = new MsgAfterConnected();
                msgData.success = true;
                msgData.playerID = -1;
                msgData.game_data = gserver.GetGameData();
                SendToClient(iclient.clientID, GameAction.Connected, msgData, NetworkDelivery.ReliableFragmentedSequenced);
            }
        }
        
        public void SendToClient(ulong clientID, ushort gameAction, INetworkSerializable data, NetworkDelivery delivery)
        {
            FastBufferWriter writer = new FastBufferWriter(128, Unity.Collections.Allocator.Temp, TcgNetwork.MsgSizeMax);
            writer.WriteValueSafe(gameAction);
            writer.WriteNetworkSerializable(data);
            Messaging.Send("refresh", clientID, writer, delivery);
            writer.Dispose();
        }

        public void SendMsgToClient(ushort clientID, string msg)
        {
            FastBufferWriter writer = new FastBufferWriter(128, Unity.Collections.Allocator.Temp, TcgNetwork.MsgSizeMax);
            writer.WriteValueSafe(GameAction.ServerMessage);
            writer.WriteValueSafe(msg);
            Messaging.Send("refresh", clientID, writer, NetworkDelivery.Reliable);
            writer.Dispose();
        }

        public virtual GameServer CreateGame(string uid, int nbPlayers)
        {
            GameServer game = new GameServer(uid, nbPlayers, true);
            _gameList[game.gameUID] = game;
            return game;
        }

        public void RemoveGame(string gameID)
        {
            _gameList.Remove(gameID);
        }

        public GameServer GetGame(string gameUID)
        {
            if (string.IsNullOrEmpty(gameUID))
                return null;
            if (_gameList.ContainsKey(gameUID))
                return _gameList[gameUID];
            return null;
        }

        public ClientData GetClient(ulong clientID)
        {
            if (_clientList.ContainsKey(clientID))
                return _clientList[clientID];
            return null;
        }

        public ClientData GetClientByUser(string username)
        {
            foreach (KeyValuePair<ulong, ClientData> pair in _clientList)
            {
                if (pair.Value.username == username)
                    return pair.Value;
            }
            return null;
        }

        public ulong ServerID => TcgNetwork.Get().ServerID;
        public NetworkMessaging Messaging => TcgNetwork.Get().Messaging; 
    }

    public class ClientData
    {
        public ulong clientID; //index of the connection
        public string userID; //Player user_id, in auth system
        public string username; //Player username
        public string gameUID; //Unique id for the game

        public ClientData(ulong id) { clientID = id; }
        public bool IsInGame() { return !string.IsNullOrEmpty(gameUID); }
    }

    public class CommandEvent
    {
        public ushort tag;
        public UnityAction<ClientData, SerializedData> callback;
    }
}