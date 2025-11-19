using System;
using System.Collections;
using System.Collections.Generic;
using System.Data.SqlTypes;
using System.Threading.Tasks;
using Monarchs.Ability;
using Monarchs.Animations;
using Monarchs.Logic;
using TcgEngine;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Events;

namespace Monarchs.Client
{
    /// <summary>
    /// Main script for the client-side of the game, should be in game scene only
    /// Will connect to server, then connect to the game on that server (with uid) and then will send game settings
    /// During the game, will send all actions performed by the player and receive game refreshes
    /// </summary>

    public class GameClient : MonoBehaviour
    {
        //--- These settings are set in the menu scene and when the game start will be sent to server

        public AnimationManager animationManager;
        public static GameSettings GameSettings = GameSettings.Default;
        public static PlayerSettings PlayerSettings = PlayerSettings.Default;
        public static readonly PlayerSettings AISettings = PlayerSettings.DefaultAI;
        public static string ObserveUser = null; //Which user should it observe, null if not an obs

        public UnityAction onConnectServer;
        public UnityAction onConnectGame;
        public UnityAction<int> onPlayerDisconnected; //Called when a player disconnects
        public UnityAction onPlayerReconnected;
        public UnityAction<int> onPlayerReady;
        public UnityAction<int> onMulligan;
        public UnityAction onGameStart;
        public UnityAction<int> onGameEnd;              //winner playerID
        public UnityAction<Card> onNewTurn;      
        public UnityAction<Card> onNewRound;              //current playerID
        public UnityAction<Card, Slot> onCardPlayed;
        public UnityAction<Card, Slot> onCardMoved;
        public UnityAction<Slot> onCardSummoned;
        public UnityAction<Card> onCardTransformed;
        public UnityAction<Card> onCardDiscarded;
        public UnityAction<int> onCardDraw;
        public UnityAction<int> onValueRolled;

        public UnityAction<AbilityData, Card> onAbilityStart;
        public UnityAction<AbilityData, Card, PlayerMana.ManaType> onAbilitySelectMana;
        public UnityAction<AbilityData, Card, Card, bool> onAbilityTargetCard;      //Ability, Caster, Target
        public UnityAction<AbilityData, Card, Player> onAbilityTargetPlayer;
        public UnityAction<AbilityData, Card, List<Slot>> onAbilityTargetMultiple;
        public UnityAction<AbilityData, Card, Slot, bool> onAbilityTargetSlot;
        public UnityAction<AbilityData, Card> onAbilityEnd;
        public UnityAction<string> onAbilitySummonedCardToHand; // Event triggered when a card is summoned to hand via ability
        public UnityAction<Card, Card> onTrapTrigger;    //Secret, Triggerer
        public UnityAction<Card, Card> onTrapResolve;    //Secret, Triggerer

        public UnityAction<Card, Card, int> onAttackStart;   //Attacker, Defender
        public UnityAction<Card, Card, int> onAttackEnd;     //Attacker, Defender
        public UnityAction<Card, Card, Slot, int> onRangeAttackStart;   //Attacker, Defender
        public UnityAction<Card, Card, Slot, int> onRangeAttackEnd;     //Attacker, Defender
        
        public UnityAction<Card> onHandCardHoveredByOpponent;
        public UnityAction<Slot> onBoardSlotHoveredByOpponent;
        public UnityAction<AbilityData, Card> onAbilityHoveredByOpponent;

        public UnityAction<int, string> onChatMsg;  //playerID, msg
        public UnityAction< string> onServerMsg;  //msg
        public UnityAction onRefreshAll;

        private int _playerID; //Player playing on this device;
        protected Game _gameData;

        private bool _observerMode;
        private int _observePlayerID;
        private float _timer;


        private readonly Dictionary<ushort, RefreshEvent> _registeredCommands = new ();

        protected static GameClient Instance;
        

        protected virtual void Awake()
        {
            Instance = this;
            Application.targetFrameRate = 120;
        }

        protected virtual void Start()
        {
            RegisterRefresh(GameAction.Connected, OnConnectedToGame);
            RegisterRefresh(GameAction.Disconnected, OnPlayerDisconnected);
            RegisterRefresh(GameAction.Reconnected, OnPlayerReconnected);
            RegisterRefresh(GameAction.PlayerReady, OnPlayerReady);
            RegisterRefresh(GameAction.GameStart, OnGameStart);
            RegisterRefresh(GameAction.GameEnd, OnGameEnd);
            RegisterRefresh(GameAction.NewTurn, OnNewTurn);
            RegisterRefresh(GameAction.NewRound, OnNewRound);
            RegisterRefresh(GameAction.CardPlayed, OnCardPlayed);
            RegisterRefresh(GameAction.CardMoved, OnCardMoved);
            RegisterRefresh(GameAction.CardSummoned, OnCardSummoned);
            RegisterRefresh(GameAction.CardTransformed, OnCardTransformed);
            RegisterRefresh(GameAction.CardDiscarded, OnCardDiscarded);
            RegisterRefresh(GameAction.CardDrawn, OnCardDraw);
            RegisterRefresh(GameAction.ValueRolled, OnValueRolled);
            RegisterRefresh(GameAction.Mulligan, OnMulligan);

            RegisterRefresh(GameAction.AttackStart, OnAttackStart);
            RegisterRefresh(GameAction.AttackEnd, OnAttackEnd);
            RegisterRefresh(GameAction.RangeAttackStart, OnRangeAttackStart);
            RegisterRefresh(GameAction.RangeAttackEnd, OnRangeAttackEnd);

            RegisterRefresh(GameAction.AbilityTrigger, OnAbilityTrigger);
            RegisterRefresh(GameAction.AbilitySelectMana, OnAbilitySelectMana);
            RegisterRefresh(GameAction.AbilityTargetCard, OnAbilityTargetCard);
            RegisterRefresh(GameAction.AbilityTargetPlayer, OnAbilityTargetPlayer);
            RegisterRefresh(GameAction.AbilityTargetSlot, OnAbilityTargetSlot);
            RegisterRefresh(GameAction.AbilityTargetMultiple, OnAbilityTargetMultiple);
            RegisterRefresh(GameAction.AbilityEnd, OnAbilityAfter);
            RegisterRefresh(GameAction.AbilitySummonedCardToHand, OnAbilitySummonedCardToHand);

            RegisterRefresh(GameAction.TrapTriggered, OnTrapTrigger);
            RegisterRefresh(GameAction.TrapResolved, OnTrapResolve);

            RegisterRefresh(GameAction.ChatMessage, OnChat);
            RegisterRefresh(GameAction.ServerMessage, OnServerMsg);
            RegisterRefresh(GameAction.RefreshAll, OnRefreshAll);
            
            RegisterRefresh(GameAction.HandCardHoveredByOpponent, OnHandCardHovered);
            RegisterRefresh(GameAction.BoardSlotHoveredByOpponent, OnBoardSlotHovered);
            RegisterRefresh(GameAction.AbilityHovered, OnAbilityHovered);

            TcgNetwork.Get().onConnect += OnConnectServer;
            TcgNetwork.Get().Messaging.ListenMsg("refresh", OnReceiveRefresh);
            
            ConnectToGame(GameSettings.game_uid);
            ConnectToAPI();
            ConnectToServer();
            onConnectServer?.Invoke();
        }

        private void OnPlayerDisconnected(SerializedData msg)
        {
            //TcgNetwork class is taking care of disconnecting the correct player (since we need it in each scene)
            MsgClientID msgClientID = msg.Get<MsgClientID>();
            onPlayerDisconnected?.Invoke(msgClientID.secondsUntilForfeit);
        }
        
        private void OnPlayerReconnected(SerializedData msg)
        {
            onPlayerReconnected?.Invoke();
        }

        private void OnAbilitySummonedCardToHand(SerializedData msg)
        {
            animationManager.AddToQueue(OnAbilitySummonedCardToHandCoroutine(msg.Get<MsgCard>().cardUID), gameObject);
        }

        private IEnumerator OnAbilitySummonedCardToHandCoroutine(string uid)
        {
            onAbilitySummonedCardToHand?.Invoke(uid);
            yield return null;
        }

        private void OnMulligan(SerializedData msg)
        {
            onMulligan?.Invoke(msg.Get<MsgPlayer>().playerID);
        }

        protected virtual void OnDestroy()
        {
            TcgNetwork.Get().onConnect -= OnConnectServer;
            TcgNetwork.Get().Messaging.UnListenMsg("refresh");
        }

        protected virtual void Update()
        {
            //Exit game scene if it cannot connect after a while
            if (_gameData == null || _gameData.State == GameState.Connecting || _gameData.State == GameState.Starting)
            {
                _timer += Time.deltaTime;
                if (!GameSettings.IsHost() && _timer > 10f)
                {
                    SceneNav.GoTo("Menu");
                }
            }
        }

        //--------------------

        public virtual void ConnectToAPI()
        {
            //Should already be connected to API from the menu
            //If not connected, start in test mode (this means game scene was launched directly from Unity)
            if (!Authenticator.Get().IsSignedIn())
            {
                Authenticator.Get().LoginTest("Player");

                PlayerSettings.deck = new PlayerDeckSettings(GameplayData.Get().test_deck);
                AISettings.deck = new PlayerDeckSettings(GameplayData.Get().test_deck_ai);
                AISettings.ai_level = GameplayData.Get().ai_level;
            }
            
            //Set avatar, cardback based on your api data
            UserData udata = Authenticator.Get().UserData;
            if (udata != null)
            {
                PlayerSettings.avatar = udata.GetAvatar();
                PlayerSettings.cardback = udata.GetCardback();
            }
        }

        public virtual async void ConnectToServer()
        {
            try
            {
                await Task.Yield(); //Wait for initialization to finish

                if (TcgNetwork.Get().IsActive())
                    return; // Already connected

                if (GameSettings.IsHost())
                    TcgNetwork.Get().StartHost(NetworkData.Get().port);
                else
                    TcgNetwork.Get().StartClient(GameSettings.GetUrl(), NetworkData.Get().port);
            }
            catch (Exception e)
            {
                Debug.LogError($"Error connecting to server: {e}");
            }
        }

        public virtual async void ConnectToGame(string uid)
        {
            try
            {
                await Task.Yield(); //Wait for initialization to finish

                if (!TcgNetwork.Get().IsActive())
                    return; //Not connected to server

                MsgPlayerConnect msgPlayerConnect = new MsgPlayerConnect
                {
                    user_id = Authenticator.Get().UserID,
                    username = Authenticator.Get().Username,
                    game_uid = uid,
                    nb_players = GameSettings.nb_players,
                    observer = GameSettings.game_type == GameType.Observer
                };

                Messaging.SendObject("connect", ServerID, msgPlayerConnect, NetworkDelivery.Reliable);
            }
            catch (Exception e)
            {
                Debug.LogError($"Error connecting to game: {e}");
            }
        }

        public virtual void SendGameSettings()
        {
            if (GameSettings.IsOffline())
            {
                //Solo mode, send both your settings and AI settings
                SendGameplaySettings(GameSettings);
                SendPlayerSettingsAI(AISettings);
                SendPlayerSettings(PlayerSettings);
            }
            else
            {
                //Online mode, only send your own settings
                SendGameplaySettings(GameSettings);
                SendPlayerSettings(PlayerSettings);
            }
        }

        public virtual void Disconnect()
        {
            TcgNetwork.Get().Disconnect();
        }

        private void RegisterRefresh(ushort refreshTag, UnityAction<SerializedData> callback)
        {
            RefreshEvent refreshEvent = new RefreshEvent
            {
                tag = refreshTag,
                callback = callback
            };
            _registeredCommands.Add(refreshTag, refreshEvent);
        }

        private void OnReceiveRefresh(ulong clientID, FastBufferReader reader)
        {
            reader.ReadValueSafe(out ushort type);
            bool found = _registeredCommands.TryGetValue(type, out RefreshEvent command);
            if (found)
            {
                Debug.Log($"Received refresh: {GameAction.GetString(type)}", this);
                command.callback.Invoke(new SerializedData(reader));
            }
        }

        //--------------------------

        private void SendPlayerSettings(PlayerSettings playerSettings)
        {
            SendAction(GameAction.PlayerSettings, playerSettings);
        }

        private void SendPlayerSettingsAI(PlayerSettings playerSettings)
        {
            SendAction(GameAction.PlayerSettingsAI, playerSettings);
        }

        private void SendGameplaySettings(GameSettings settings)
        {
            SendAction(GameAction.GameSettings, settings);
        }

        public void SendBugReport(string title, string description, string pathToPrintscreen)
        {
            ApiClient.Get().SubmitBug(title, description, pathToPrintscreen);
        }

        public void PlayCard(Card card, Slot slot)
        {
            MsgPlayCard msg = new MsgPlayCard
            {
                cardUID = card.uid,
                slot = slot
            };
            SendAction(GameAction.PlayCard, msg);
        }

        public void AttackTarget(Card card, Card target)
        {
            MsgAttack msg = new MsgAttack
            {
                attackerUID = card.uid,
                slotX = target.slot.x,
                slotY = target.slot.y
            };
            SendAction(GameAction.Attack, msg);
        }

        public void MulliganCards(List<Card> cards)
        {
            string[] cardsUID = new string[cards.Count];
            for (int i = 0; i < cards.Count; i++)
            {
                cardsUID[i] = cards[i].uid;
            }
            
            MsgMulliganDiscarded msg = new MsgMulliganDiscarded
            {
                discardedCards = cardsUID
            };
            SendAction(GameAction.Mulligan, msg);
        }
        
        public void RangeAttackTarget(Card card, Card target)
        {
            MsgAttack msg = new MsgAttack
            {
                attackerUID = card == null ? "" : card.uid,
                slotX = target.slot.x,
                slotY = target.slot.y
            };

            SendAction(GameAction.RangeAttack, msg);
        }
        
        public void RangeAttackTarget(Card card, Slot targetSlot)
        {
            MsgAttack msg = new MsgAttack
            {
                attackerUID = card == null ? "" : card.uid,
                slotX = targetSlot.x,
                slotY = targetSlot.y
            };
            SendAction(GameAction.RangeAttack, msg);
        }

        public void Move(Card card, Slot slot)
        {
            MsgPlayCard msg = new MsgPlayCard
            {
                cardUID = card.uid,
                slot = slot
            };
            SendAction(GameAction.Move, msg);
        }

        public void CastAbility(Card card, AbilityData ability)
        {
            MsgCastAbility msg = new MsgCastAbility
            {
                casterUID = card.uid,
                abilityID = ability.id,
                target_uid = ""
            };
            SendAction(GameAction.CastAbility, msg);
        }
        
        public void ChooseManaType(PlayerMana.ManaType manaType)
        {
            MsgInt msg = new MsgInt
            {
                value = (int)manaType
            };
            SendAction(GameAction.ChooseManaType, msg);
        }

        public void SelectCard(Card card)
        {
            MsgCard msg = new MsgCard
            {
                cardUID = card.uid
            };
            SendAction(GameAction.SelectCard, msg);
        }

        public void SelectPlayer(Player player)
        {
            MsgPlayer msg = new MsgPlayer
            {
                playerID = player.playerID
            };
            SendAction(GameAction.SelectPlayer, msg);
        }

        public void SelectSlot(Slot slot)
        {
            SendAction(GameAction.SelectSlot, slot);
        }
        
        public void SelectCaster(Card caster)
        {
            MsgCard msg = new MsgCard
            {
                cardUID = caster.uid
            };
            SendAction(GameAction.SelectCaster, msg);
        }

        public void SelectChoice(int c)
        {
            MsgInt choice = new MsgInt
            {
                value = c
            };
            SendAction(GameAction.SelectChoice, choice);
        }
        
        public void SkipSelection()
        {
            SendAction(GameAction.SkipSelect);
        }
        public void CancelSelection()
        {
            SendAction(GameAction.CancelSelect);
        }

        public void SendChatMsg(string msg)
        {
            MsgChat chat = new MsgChat
            {
                msg = msg,
                playerID = _playerID
            };
            SendAction(GameAction.ChatMessage, chat);
        }
        
        public void EndTurn()
        {
            SendAction(GameAction.EndTurn);
        }

        public void Resign()
        {
            SendAction(GameAction.Resign);
        }

        public void HoverHandCard(Card card)
        {
            MsgCard msg = new MsgCard
            {
                cardUID = card != null ? card.uid : ""
            };
            SendAction(GameAction.HandCardHoveredByOpponent, msg);
        }
        
        public void HoverSlot(Slot slot)
        {
            MsgSlot msg = new MsgSlot
            {
                slot = slot
            };
            SendAction(GameAction.BoardSlotHoveredByOpponent, msg);
        }
        
        public void HoverAbility(AbilityData ability, Card caster)
        {
            MsgCastAbility msg = new MsgCastAbility
            {
                casterUID = "",
                target_uid = "",
                VFX_Index = 0
            };
            if (ability == null || caster == null)
            {
                msg.abilityID = "";
                msg.casterUID = "";
                SendAction(GameAction.AbilityHovered, msg);
                return;
            }
            msg.abilityID = ability.id;
            msg.casterUID = caster.uid;
            SendAction(GameAction.AbilityHovered, msg);
        }

        public void SetObserverMode(int playerID)
        {
            _observerMode = true;
            _observePlayerID = playerID;
        }

        private void SetObserverMode(string username)
        {
            _observePlayerID = 0; //Default value of observe_user not found

            Game data = GetGameData();
            foreach (Player player in data.players)
            {
                if (player.username == username)
                {
                    _observePlayerID = player.playerID;
                }
            }
        }

        private void SendAction<T>(ushort type, T data) where T : INetworkSerializable
        {
            FastBufferWriter writer = new FastBufferWriter(128, Unity.Collections.Allocator.Temp, TcgNetwork.MsgSizeMax);
            writer.WriteValueSafe(type);
            writer.WriteNetworkSerializable(data);
            Messaging.Send("action", ServerID, writer, NetworkDelivery.Reliable);
            writer.Dispose();
        }

        public void SendAction(ushort type, int data)
        {
            FastBufferWriter writer = new FastBufferWriter(128, Unity.Collections.Allocator.Temp, TcgNetwork.MsgSizeMax);
            writer.WriteValueSafe(type);
            writer.WriteValueSafe(data);
            Messaging.Send("action", ServerID, writer, NetworkDelivery.Reliable);
            writer.Dispose();
        }

        private void SendAction(ushort type)
        {
            FastBufferWriter writer = new FastBufferWriter(128, Unity.Collections.Allocator.Temp, TcgNetwork.MsgSizeMax);
            writer.WriteValueSafe(type);
            Messaging.Send("action", ServerID, writer, NetworkDelivery.Reliable);
            writer.Dispose();
        }

        //--- Receive Refresh ----------------------

        protected virtual void OnConnectServer()
        {
            ConnectToGame(GameSettings.game_uid);
            onConnectServer?.Invoke();
        }

        protected virtual void OnConnectedToGame(SerializedData data)
        {
            MsgAfterConnected msg = data.Get<MsgAfterConnected>();
            _playerID = msg.playerID;
            _gameData = msg.game_data;
            _observerMode = _playerID < 0; //Will usually return -1 if it's an observer

            if (_observerMode)
                SetObserverMode(ObserveUser);

            if (onConnectGame != null)
                onConnectGame.Invoke();

            SendGameSettings();
        }

        protected virtual void OnPlayerReady(SerializedData data)
        {
            MsgInt msg = data.Get<MsgInt>();
            int pid = msg.value;

            if (onPlayerReady != null)
                onPlayerReady.Invoke(pid);
        }

        private void OnGameStart(SerializedData data)
        {
            onGameStart?.Invoke();
        }

        private void OnGameEnd(SerializedData data)
        {
            MsgPlayer msg = data.Get<MsgPlayer>();
            onGameEnd?.Invoke(msg.playerID);
        }

        private void OnNewTurn(SerializedData data)
        {
            MsgCard msg = data.Get<MsgCard>();
            Card card = _gameData.GetCard(msg.cardUID);
            onNewTurn?.Invoke(card);
        }

        private void OnNewRound(SerializedData data)
        {
            //MsgCard msg = data.Get<MsgCard>();
            //Card card = game_data.GetCard(msg.cardUID);
            onNewRound?.Invoke(null);
        }

        private void OnCardPlayed(SerializedData data)
        {
            MsgPlayCard msg = data.Get<MsgPlayCard>();
            Card card = _gameData.GetCard(msg.cardUID);
            onCardPlayed?.Invoke(card, msg.slot);
        }
        
        private void OnCardSummoned(SerializedData data)
        {
            MsgPlayCard msg = data.Get<MsgPlayCard>();
            onCardSummoned?.Invoke(msg.slot);
        }

        private void OnCardMoved(SerializedData data)
        {
            MsgPlayCard msg = data.Get<MsgPlayCard>();
            Card card = _gameData.GetCard(msg.cardUID);
            onCardMoved?.Invoke(card, msg.slot);
            //animationManager.AddToQueue(OnRefreshAllCoroutine(msg));
        }

        private void OnCardTransformed(SerializedData data)
        {
            MsgCard msg = data.Get<MsgCard>();
            
            Card card = _gameData.GetCard(msg.cardUID);
            onCardTransformed?.Invoke(card);
        }

        private void OnCardDiscarded(SerializedData data)
        {
            MsgCard msg = data.Get<MsgCard>();
            Card card = _gameData.GetCard(msg.cardUID);
            onCardDiscarded?.Invoke(card);
        }

        private void OnCardDraw(SerializedData data)
        {
            MsgInt msg = data.Get<MsgInt>();
            onCardDraw?.Invoke(msg.value);
        }

        private void OnValueRolled(SerializedData data)
        {
            MsgInt msg = data.Get<MsgInt>();
            onValueRolled?.Invoke(msg.value);
        }

        private void OnAttackStart(SerializedData data)
        {
            MsgAttack msg = data.Get<MsgAttack>();
            Card attacker = _gameData.GetCard(msg.attackerUID);
            Slot targetSlot = Slot.Get(msg.slotX, msg.slotY);
            Card target = _gameData.GetSlotCard(targetSlot);
            int damage = msg.damage;
            onAttackStart?.Invoke(attacker, target, damage);
        }

        private void OnAttackEnd(SerializedData data)
        {
            MsgAttack msg = data.Get<MsgAttack>();
            Card attacker = _gameData.GetCard(msg.attackerUID);
            Slot targetSlot = Slot.Get(msg.slotX, msg.slotY);
            Card target = _gameData.GetSlotCard(targetSlot);
            int damage = msg.damage;
            onAttackEnd?.Invoke(attacker, target, damage);
        }
        
        private void OnRangeAttackStart(SerializedData data)
        {
            MsgAttack msg = data.Get<MsgAttack>();
            Card attacker = _gameData.GetCard(msg.attackerUID);
            Slot targetSlot = Slot.Get(msg.slotX, msg.slotY);
            Card target = _gameData.GetSlotCard(targetSlot);
            int damage = msg.damage;
            onRangeAttackStart?.Invoke(attacker, target, targetSlot, damage);
        }

        private void OnRangeAttackEnd(SerializedData data)
        {
            MsgAttack msg = data.Get<MsgAttack>();
            Card attacker = _gameData.GetCard(msg.attackerUID);
            Slot targetSlot = Slot.Get(msg.slotX, msg.slotY);
            Card target = _gameData.GetSlotCard(targetSlot);
            int damage = msg.damage;
            onRangeAttackEnd?.Invoke(attacker, target, targetSlot, damage);
        }
        
        private void OnAbilityTrigger(SerializedData data)
        {
            MsgCastAbility msg = data.Get<MsgCastAbility>();
            AbilityData ability = AbilityData.Get(msg.abilityID);
            Card caster = _gameData.GetCard(msg.casterUID);
            Slot slot = Slot.Get(msg.slotX, msg.slotY);
            
            //Update caster slot if it changed (for abilities that move the caster, to have FX at the right place)
            if (caster != null && slot.IsValid())
            {
                caster.slot = slot;
            }
            
            animationManager.AddTriggerAnimationToQueue(ability.trigger, caster);
            onAbilityStart?.Invoke(ability, caster);
        }

        private void OnAbilityTargetMultiple(SerializedData data)
        {
            MsgCastAbilityMultipleTarget msg = data.Get<MsgCastAbilityMultipleTarget>();
            AbilityData ability = AbilityData.Get(msg.abilityID);
            Card caster = _gameData.GetCard(msg.casterUID);
            List<Slot> slots = new List<Slot>{msg.slot1, msg.slot2, msg.slot3, msg.slot4, msg.slot5};
            onAbilityTargetMultiple?.Invoke(ability, caster, slots);
        }

        private void OnAbilitySelectMana(SerializedData data)
        {
            MsgCastAbilityPlayer msg = data.Get<MsgCastAbilityPlayer>();
            AbilityData ability = AbilityData.Get(msg.abilityID);
            Card caster = _gameData.GetCard(msg.casterUID);
            PlayerMana.ManaType manaType = (PlayerMana.ManaType)msg.targetID;
            onAbilitySelectMana(ability, caster, manaType);
        }

        private void OnAbilityTargetCard(SerializedData data)
        {
            MsgCastAbility msg = data.Get<MsgCastAbility>();
            AbilityData ability = AbilityData.Get(msg.abilityID);
            Card caster = _gameData.GetCard(msg.casterUID);
            Card target = _gameData.GetCard(msg.target_uid);
            onAbilityTargetCard?.Invoke(ability, caster, target, msg.isSelectTarget);
        }

        private void OnAbilityTargetPlayer(SerializedData data)
        {
            MsgCastAbilityPlayer msg = data.Get<MsgCastAbilityPlayer>();
            AbilityData ability = AbilityData.Get(msg.abilityID);
            Card caster = _gameData.GetCard(msg.casterUID);
            Player target = _gameData.GetPlayer(msg.targetID);
            onAbilityTargetPlayer?.Invoke(ability, caster, target);
        }

        private void OnAbilityTargetSlot(SerializedData data)
        {
            MsgCastAbilitySlot msg = data.Get<MsgCastAbilitySlot>();
            AbilityData ability = AbilityData.Get(msg.abilityID);
            Card caster = _gameData.GetCard(msg.casterUID);
            onAbilityTargetSlot?.Invoke(ability, caster, msg.slot, msg.isSelectTarget);
        }

        private void OnAbilityAfter(SerializedData data)
        {
            MsgCastAbility msg = data.Get<MsgCastAbility>();
            AbilityData ability = AbilityData.Get(msg.abilityID);
            Card caster = _gameData.GetCard(msg.casterUID);
            onAbilityEnd?.Invoke(ability, caster);
        }

        private void OnTrapTrigger(SerializedData data)
        {
            MsgTrap msg = data.Get<MsgTrap>();
            Card trap = _gameData.GetCard(msg.trapUID);
            Card triggerer = _gameData.GetCard(msg.triggerer_uid);
            onTrapTrigger?.Invoke(trap, triggerer);
        }

        private void OnTrapResolve(SerializedData data)
        {
            MsgTrap msg = data.Get<MsgTrap>();
            Card trap = _gameData.GetCard(msg.trapUID);
            Card triggerer = _gameData.GetCard(msg.triggerer_uid);
            onTrapResolve?.Invoke(trap, triggerer);
        }

        private void OnChat(SerializedData data)
        {
            MsgChat msg = data.Get<MsgChat>();
            onChatMsg?.Invoke(msg.playerID, msg.msg);
        }

        private void OnServerMsg(SerializedData data)
        {
            string msg = data.GetString();
            onServerMsg?.Invoke(msg);
        }

        private void OnRefreshAll(SerializedData data)
        {
            MsgRefreshAll msg = data.Get<MsgRefreshAll>();
            if (_gameData.State == GameState.Starting || _gameData.State == GameState.Connecting || _gameData.State == GameState.Mulligan)
            {
                _gameData = msg.game_data;
                onRefreshAll?.Invoke();
                return;
            }
            animationManager.AddToQueue(OnRefreshAllCoroutine(msg), gameObject);
        }
        
        private void OnHandCardHovered(SerializedData data)
        {
            MsgCard msg = data.Get<MsgCard>();
            Card card = _gameData.GetCard(msg.cardUID);
            onHandCardHoveredByOpponent?.Invoke(card);
        }
        
        private void OnBoardSlotHovered(SerializedData data)
        {
            MsgSlot msg = data.Get<MsgSlot>();
            onBoardSlotHoveredByOpponent?.Invoke(msg.slot);
        }
        
        private void OnAbilityHovered(SerializedData data)
        {
            MsgCastAbility msg = data.Get<MsgCastAbility>();
            AbilityData ability = AbilityData.Get(msg.abilityID);
            Card caster = _gameData.GetCard(msg.casterUID);
            onAbilityHoveredByOpponent?.Invoke(ability, caster);
        }
        
        private IEnumerator OnRefreshAllCoroutine(MsgRefreshAll msg)
        {
            _gameData = msg.game_data;
            onRefreshAll?.Invoke();
            yield return null;
        }

        //--------------------------

        public virtual bool IsReady()
        {
            return _gameData != null && TcgNetwork.Get().IsConnected() && _gameData.State != GameState.Connecting;
        }

        public Player GetPlayer()
        {
            Game gdata = GetGameData();
            return gdata.GetPlayer(GetPlayerID());
        }

        public Player GetOpponentPlayer()
        {
            Game gdata = GetGameData();
            return gdata.GetPlayer(GetOpponentPlayerID());
        }

        public int GetPlayerID()
        {
            if (_observerMode)
                return _observePlayerID;
            return _playerID;
        }

        public int GetOpponentPlayerID()
        {
            return GetPlayerID() == 0 ? 1 : 0;
        }

        public virtual bool IsYourTurn()
        {
            int playerID = GetPlayerID();
            Game gameData = GetGameData();

            if (!IsReady())
                return false;
            return playerID == gameData.CurrentPlayer;
        }

        public bool IsObserveMode()
        {
            return _observerMode;
        }

        public bool IsFirstPlayer()
        {
            return _playerID == _gameData.firstPlayer;
        }

        public static Game GetGameData()
        {
            return Get()._gameData;
        }

        public bool HasEnded()
        {
            return _gameData.HasEnded();
        }

        private void OnApplicationQuit()
        {
            Resign(); //Auto Resign before closing the app. NOTE: doesn't seem to work since the msg don't have time to be sent before it closes
        }

        public bool IsHost => TcgNetwork.Get().IsHost;
        public ulong ServerID => TcgNetwork.Get().ServerID;
        public NetworkMessaging Messaging => TcgNetwork.Get().Messaging;

        public static GameClient Get()
        {
            return Instance;
        }

        public List<Card> GetCurrentPieceTurn()
        {
            return _gameData.GetCurrentCardTurn();
        }

    }

    public class RefreshEvent
    {
        public ushort tag;
        public UnityAction<SerializedData> callback;
    }
}