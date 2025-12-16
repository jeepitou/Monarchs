using System.Collections.Generic;
using Monarchs.Client;
using Monarchs.Logic;
using TcgEngine;
using Unity.Netcode;
using UnityEngine;

namespace Monarchs.GameServer
{
    /// <summary>
    /// Local server running on the client to play in solo mode against AI
    /// Contains only one GameServer
    /// </summary>

    public class ServerManagerLocal : MonoBehaviour
    {
        protected GameServer _server;

        protected Dictionary<ulong, ClientData> _clientList = new ();  //List of clients

        protected virtual void Start()
        {
            if (GameClient.GameSettings.IsHost())
            {
                StartServer(); //Start local server if not playing online
            }
        }

        protected virtual void StartServer()
        {
            TcgNetwork network = TcgNetwork.Get();
            network.onClientJoin += OnClientJoin;
            network.onClientQuit += OnClientQuit;
            network.Messaging.ListenMsg("connect", ReceiveConnectPlayer);
            network.Messaging.ListenMsg("action", ReceiveGameAction);

            _clientList[network.ServerID] = new ClientData(network.ServerID); //Add yourself
            _server = new GameServer(GameClient.GameSettings.game_uid, GameClient.GameSettings.nb_players, false);
        }

        protected virtual void OnDestroy()
        {
            TcgNetwork network = TcgNetwork.Get();
            if (network != null)
            {
                network.onClientJoin -= OnClientJoin;
                network.onClientQuit -= OnClientQuit;
                network.Messaging.UnListenMsg("connect");
                network.Messaging.UnListenMsg("action");
            }
        }

        protected virtual void OnClientJoin(ulong clientID)
        {
            _clientList[clientID] = new ClientData(clientID);
        }

        protected virtual void OnClientQuit(ulong clientID)
        {
            TcgNetwork network = TcgNetwork.Get();
            ClientData client = GetClient(network.ClientID);
            _server?.RemoveClient(client);
            _clientList.Remove(network.ClientID);
        }

        protected virtual void Update()
        {
            if (_server != null)
                _server.Update();
        }

        protected virtual void ReceiveConnectPlayer(ulong clientID, FastBufferReader reader)
        {
            //ClientData iclient = GetClient(client_id);
            reader.ReadNetworkSerializable(out MsgPlayerConnect msg);

            if (msg != null)
            {
                if (string.IsNullOrWhiteSpace(msg.username))
                    return;

                if (string.IsNullOrWhiteSpace(msg.game_uid))
                    return;

                ClientData client = GetClient(clientID);
                if (client == null)
                    return;

                bool canConnect = _server.IsPlayer(client) || _server.CountPlayers() < _server.nbPlayers;
                if (canConnect)
                {
                    client.gameUID = msg.game_uid;
                    client.userID = msg.user_id;
                    client.username = msg.username;
                    _server.AddClient(client);

                    int playerID = _server.AddPlayer(client);
                    Player player = _server.GetGameData().GetPlayer(playerID);
                    if (player != null)
                    {
                        player.username = msg.username;
                        player.connected = true;
                    }

                    //Return request
                    MsgAfterConnected msgData = new MsgAfterConnected();
                    msgData.success = true;
                    msgData.playerID = playerID;
                    msgData.game_data = _server.GetGameData();
                    SendToClient(clientID, GameAction.Connected, msgData, NetworkDelivery.ReliableFragmentedSequenced);
                }
            }
        }

        protected virtual void ReceiveGameAction(ulong clientID, FastBufferReader reader)
        {
            _server.ReceiveAction(clientID, reader);
        }

        public void SendToClient(ulong clientID, ushort gameAction, INetworkSerializable data, NetworkDelivery delivery)
        {
            FastBufferWriter writer = new FastBufferWriter(128, Unity.Collections.Allocator.Temp, TcgNetwork.MsgSizeMax);
            writer.WriteValueSafe(gameAction);
            writer.WriteNetworkSerializable(data);
            Messaging.Send("refresh", clientID, writer, delivery);
            writer.Dispose();
        }

        public ClientData GetClient(ulong clientID)
        {
            if (_clientList.ContainsKey(clientID))
                return _clientList[clientID];
            return null;
        }

        public ulong ServerID => TcgNetwork.Get().ServerID; 
        public NetworkMessaging Messaging => TcgNetwork.Get().Messaging;
    }
}
