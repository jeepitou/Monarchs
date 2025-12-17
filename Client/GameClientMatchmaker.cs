using Monarchs.Api;
using TcgEngine;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Events;

namespace Monarchs.Client
{
    /// <summary>
    /// Main client script for the matchmaker
    /// Will send requests to server and receive a response when a matchmaking succeed or fail
    /// </summary>

    public class GameClientMatchmaker : MonoBehaviour
    {
        public UnityAction<MatchmakingResult> onMatchingComplete;
        public UnityAction<MatchmakingList> onMatchmakingList;
        public UnityAction<MatchList> onMatchList;

        private bool _matchmaking;
        private float _timer;
        private float _matchTimer;
        private string _matchmakingGroup;
        private int _matchmakingPlayers;
        private UnityAction<bool> _connectCallback;

        private static GameClientMatchmaker _instance;

        void Awake()
        {
            _instance = this;
        }

        private void Start()
        {
            TcgNetwork.Get().onConnect += OnConnect;
            TcgNetwork.Get().onDisconnect += OnDisconnect;
            Messaging.ListenMsg("matchmaking", ReceiveMatchmaking);
            Messaging.ListenMsg("matchmaking_list", ReceiveMatchmakingList);
            Messaging.ListenMsg("match_list", ReceiveMatchList);
        }

        private void OnDestroy()
        {
            //Disconnect(); //Disconnect when switching scene

            if (TcgNetwork.Get() != null)
            {
                TcgNetwork.Get().onConnect -= OnConnect;
                TcgNetwork.Get().onDisconnect -= OnDisconnect;
                Messaging.UnListenMsg("matchmaking");
                Messaging.UnListenMsg("matchmaking_list");
                Messaging.UnListenMsg("match_list");
            }
        }

        void Update()
        {
            if (_matchmaking)
            {
                _timer += Time.deltaTime;
                _matchTimer += Time.deltaTime;

                //Send periodic request
                if (IsConnected() && _timer > 2f)
                {
                    _timer = 0f;
                    SendMatchRequest(true, _matchmakingGroup, _matchmakingPlayers);
                }

                //Disconnected, stop
                if (!IsConnected() && !IsConnecting() && _timer > 5f)
                {
                    StopMatchmaking();
                }
            }
        }

        public void StartMatchmaking(string group, int nbPlayers)
        {
            if (_matchmaking)
                StopMatchmaking();

            Debug.Log("Start Matchmaking!");
            _matchmakingGroup = group;
            _matchmakingPlayers = nbPlayers;
            _matchmaking = true;
            _matchTimer = 0f;
            _timer = 0f;

            Connect(NetworkData.Get().url, NetworkData.Get().port, (success) =>
            {
                if (success)
                {
                    SendMatchRequest(false, group, nbPlayers);
                }
                else
                {
                    StopMatchmaking();
                }
            });
        }

        public void StopMatchmaking()
        {
            if (_matchmaking)
            {
                Debug.Log("Stop Matchmaking!");
                onMatchingComplete?.Invoke(null);
                _matchmakingGroup = "";
                _matchmakingPlayers = 0;
                _matchmaking = false;
            }
        }

        public void RefreshMatchmakingList()
        {
            Connect(NetworkData.Get().url, NetworkData.Get().port, (success) =>
            {
                if(success)
                    SendMatchmakingListRequest();
            });
        }

        public void RefreshMatchList(string username)
        {
            Connect(NetworkData.Get().url, NetworkData.Get().port, (success) =>
            {
                if (success)
                    SendMatchListRequest(username);
            });
        }

        public void Connect(string url, ushort port, UnityAction<bool> callback=null)
        {
            //Must be logged in to API to connect
            if(!Authenticator.Get().IsSignedIn())
            {
                callback?.Invoke(false);
                return;
            }

            //Check if already connected
            if (IsConnected() || IsConnecting())
            {
                callback?.Invoke(IsConnected());
                return;
            }

            _connectCallback = callback;
            TcgNetwork.Get().StartClient(url, port);
        }

        public void Disconnect()
        {
            //TcgNetwork.Get()?.Disconnect();
        }

        private void OnConnect()
        {
            Debug.Log("Connected to server!");
            _connectCallback?.Invoke(true);
            _connectCallback = null;
        }

        private void OnDisconnect()
        {
            StopMatchmaking(); //Stop if currently running
            _connectCallback?.Invoke(false);
            _connectCallback = null;
            _matchmaking = false;
        }

        private void SendMatchRequest(bool refresh, string group, int nbPlayers)
        {
            MsgMatchmaking msgMatch = new MsgMatchmaking();
            UserData udata = Authenticator.Get().GetUserData();
            msgMatch.user_id = Authenticator.Get().GetUserId();
            msgMatch.username = Authenticator.Get().GetUsername();
            msgMatch.group = group;
            msgMatch.players = nbPlayers;
            msgMatch.elo = udata.elo;
            msgMatch.time = _matchTimer;
            msgMatch.refresh = refresh;
            Messaging.SendObject("matchmaking", ServerID, msgMatch, NetworkDelivery.Reliable);
        }

        private void SendMatchmakingListRequest()
        {
            MsgMatchmakingList msgMatch = new MsgMatchmakingList();
            msgMatch.username = ""; //Return all users
            Messaging.SendObject("matchmaking_list", ServerID, msgMatch, NetworkDelivery.Reliable);
        }

        private void SendMatchListRequest(string username)
        {
            MsgMatchmakingList msgMatch = new MsgMatchmakingList();
            msgMatch.username = username;
            Messaging.SendObject("match_list", ServerID, msgMatch, NetworkDelivery.Reliable);
        }

        private void ReceiveMatchmaking(ulong clientID, FastBufferReader reader)
        {
            reader.ReadNetworkSerializable(out MatchmakingResult msg);

            if (IsConnected() && _matchmaking && _matchmakingGroup == msg.group)
            {
                _matchmaking = !msg.success; //Stop matchmaking if success
                onMatchingComplete?.Invoke(msg);
            }
        }

        private void ReceiveMatchmakingList(ulong clientID, FastBufferReader reader)
        {
            reader.ReadNetworkSerializable(out MatchmakingList list);
            onMatchmakingList?.Invoke(list);
        }

        private void ReceiveMatchList(ulong clientID, FastBufferReader reader)
        {
            reader.ReadNetworkSerializable(out MatchList list);
            onMatchList?.Invoke(list);
        }

        public bool IsMatchmaking()
        {
            return _matchmaking;
        }

        public string GetGroup()
        {
            return _matchmakingGroup;
        }

        public int GetNbPlayers()
        {
            return _matchmakingPlayers;
        }

        public float GetTimer()
        {
            return _matchTimer;
        }

        public bool IsConnected()
        {
            return TcgNetwork.Get().IsConnected();
        }

        public bool IsConnecting()
        {
            return TcgNetwork.Get().IsConnecting();
        }

        public ulong ServerID => TcgNetwork.Get().ServerID;
        public NetworkMessaging Messaging => TcgNetwork.Get().Messaging;

        public static GameClientMatchmaker Get()
        {
            return _instance;
        }
    }

}