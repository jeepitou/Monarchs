using System;
using System.Collections.Generic;
using Monarchs.Ability;
using Monarchs.Logic;
using Monarchs.Logic.AbilitySystem;
using TcgEngine;
using TcgEngine.AI;
using TcgEngine.Server;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Events;

namespace Monarchs.GameServer
{
    /// <summary>
    /// Represent one game on the server, when playing solo this will be created locally, 
    /// or if online multiple GameServer, one for each match, will be created by the dedicated server
    /// Manage receiving actions, sending refresh, and running AI
    /// </summary>
    
    public class GameServer
    {
        public string gameUID; //Game unique ID
        public int nbPlayers = 2;
        public bool mulliganTimerExpired;

        public const float GAME_EXPIRE_TIME = 30f; //How long for the game to be deleted when no one is connected
        public const float WIN_EXPIRE_TIME = 120f; //How long for a player to be declared winnner if hes the only one connected

        private Game _gameData;
        private GameLogic _gameplay;
        private float _expiration;
        private float _winExpiration;
        private bool _isDedicatedServer;

        private readonly List<ClientData> _players = new ();            //Exclude observers, stays in array when disconnected, only players can send commands
        private readonly List<ClientData> _connectedClients = new ();  //Include obervers, removed from array when disconnected, all clients receive refreshes
        private readonly List<AIPlayer> _aiList = new ();
        
        private readonly Dictionary<ushort, CommandEvent> _registeredCommands = new ();

        public GameServer(string uid, int players, bool online)
        {
            Init(uid, players, online);
        }

        ~GameServer()
        {
            Clear();
        }

        private void Init(string uid, int players, bool online)
        {
            gameUID = uid;
            nbPlayers = Mathf.Max(players, 2);
            _isDedicatedServer = online;
            _gameData = new Game(uid, nbPlayers);
            _gameplay = new GameLogic(_gameData);

            RegisterAllCommands();

            SubscribeToEvents();
        }

        private void SubscribeToEvents()
        {
            _gameplay.onGameStart += OnGameStart;
            _gameplay.onBoardSetup += OnBoardSetup;
            _gameplay.onMulliganStart += OnReceivedMulligan;
            _gameplay.onGameEnd += OnGameEnd;
            _gameplay.onTurnStart += OnTurnStart;
            _gameplay.onRoundStart += OnRoundStart;
            _gameplay.onTurnPlay += OnTurnPlay;
            _gameplay.onTurnEnd += OnTurnEnd;

            _gameplay.onCardPlayed += OnCardPlayed;
            _gameplay.onCardSummoned += OnCardSummoned;
            _gameplay.onCardMoved += OnCardMoved;
            _gameplay.onCardTransformed += OnCardTransformed;
            _gameplay.onCardDiscarded += OnCardDiscarded;
            _gameplay.onCardDrawn += OnCardDraw;
            _gameplay.onRollValue += OnValueRolled;

            _gameplay.onAbilityStart += OnAbilityStart;
            _gameplay.onAbilityTarget += OnAbilityTarget;
            _gameplay.onAbilityTargetMultiple += OnAbilityTargetMultiple;
            _gameplay.onAbilityEnd += OnAbilityEnd;
            _gameplay.onAbilitySummonedCardToHand += OnAbilitySummonedCardToHand;

            _gameplay.onAttackStart += OnAttackStart;
            _gameplay.onAttackEnd += OnAttackEnd;
            _gameplay.onRangeAttackStart += OnRangeAttackStart;
            _gameplay.onRangeAttackEnd += OnRangeAttackEnd;

            _gameplay.onTrapTrigger += OnTrapTriggered;
            _gameplay.onTrapResolved += OnTrapResolved;

            _gameplay.onSelectorStart += OnSelector;
            _gameplay.onSelectorSelect += OnSelector;
        }

        private void OnAbilitySummonedCardToHand(string uid, string id)
        {
            MsgCardWithID msg = new MsgCardWithID
            {
                cardUID = uid,
                cardID = id
            };

                
            SendToAll(GameAction.AbilitySummonedCardToHand, msg, NetworkDelivery.Reliable);
        }

        private void RegisterAllCommands()
        {
            RegisterCommand(GameAction.PlayerSettings, ReceivePlayerSettings);
            RegisterCommand(GameAction.PlayerSettingsAI, ReceivePlayerSettingsAI);
            RegisterCommand(GameAction.GameSettings, ReceiveGameplaySettings);
            RegisterCommand(GameAction.PlayCard, ReceivePlayCard);
            RegisterCommand(GameAction.Attack, ReceiveAttackTarget);
            RegisterCommand(GameAction.RangeAttack, ReceiveRangedAttackTarget);
            RegisterCommand(GameAction.Move, ReceiveMove);
            RegisterCommand(GameAction.CastAbility, ReceiveCastCardAbility);
            RegisterCommand(GameAction.SelectCard, ReceiveSelectCard);
            RegisterCommand(GameAction.SelectPlayer, ReceiveSelectPlayer);
            RegisterCommand(GameAction.SelectSlot, ReceiveSelectSlot);
            RegisterCommand(GameAction.SelectCaster, ReceiveSelectCaster);
            RegisterCommand(GameAction.SelectChoice, ReceiveSelectChoice);
            RegisterCommand(GameAction.SkipSelect, ReceiveSkipSelect);
            RegisterCommand(GameAction.CancelSelect, ReceiveCancelSelection);
            RegisterCommand(GameAction.EndTurn, ReceiveEndTurn);
            RegisterCommand(GameAction.Resign, ReceiveResign);
            RegisterCommand(GameAction.ChatMessage, ReceiveChat);
            RegisterCommand(GameAction.Mulligan, ReceiveMulligan);
            RegisterCommand(GameAction.HandCardHoveredByOpponent, ReceiveHandCardHovered);
            RegisterCommand(GameAction.BoardSlotHoveredByOpponent, ReceiveBoardSlotHovered);
            RegisterCommand(GameAction.AbilityHovered, ReceiveAbilityHovered);
            RegisterCommand(GameAction.ChooseManaType, ChooseManaType);
        }

        private void Clear()
        {
            _gameplay.onGameStart -= OnGameStart;
            _gameplay.onGameEnd -= OnGameEnd;
            _gameplay.onRoundStart -= OnRoundStart;
            _gameplay.onTurnPlay -= OnTurnPlay;
            _gameplay.onTurnEnd -= OnTurnEnd;

            _gameplay.onCardPlayed -= OnCardPlayed;
            _gameplay.onCardSummoned -= OnCardSummoned;
            _gameplay.onCardMoved -= OnCardMoved;
            _gameplay.onCardTransformed -= OnCardTransformed;
            _gameplay.onCardDiscarded -= OnCardDiscarded;
            _gameplay.onCardDrawn -= OnCardDraw;
            _gameplay.onRollValue -= OnValueRolled;

            _gameplay.onAbilityStart -= OnAbilityStart;
            _gameplay.onAbilityTarget -= OnAbilityTarget;
            _gameplay.onAbilityEnd -= OnAbilityEnd;
            _gameplay.onAbilitySummonedCardToHand -= OnAbilitySummonedCardToHand;

            _gameplay.onAttackStart -= OnAttackStart;
            _gameplay.onAttackEnd -= OnAttackEnd;
            _gameplay.onRangeAttackStart -= OnRangeAttackStart;
            _gameplay.onRangeAttackEnd -= OnRangeAttackEnd;

            _gameplay.onTrapTrigger -= OnTrapTriggered;
            _gameplay.onTrapResolved -= OnTrapResolved;

            _gameplay.onSelectorStart -= OnSelector;
            _gameplay.onSelectorSelect -= OnSelector;
        }

        public void Update()
        {
            //Game Expiration if no one is connected or game ended
            int connectedPlayers = CountConnectedClients();
            if (HasGameEnded() || connectedPlayers == 0)
                _expiration += Time.deltaTime;

            //Win expiration if all other players left
            if (connectedPlayers == 1 && HasGameStarted() && !HasGameEnded())
                _winExpiration += Time.deltaTime;
            else
            {
                _winExpiration = 0f; //Reset win expiration if more than one player is connected
            }

            if (_isDedicatedServer && !HasGameEnded() && IsWinExpired())
                EndExpiredGame();

            //Timer during Play phase
            if (_gameData.State == GameState.Play && !_gameplay.IsResolving())
            {
                _gameData.turnTimer -= Time.deltaTime;

                if (_gameData.selector != SelectorType.None)
                {
                    if (_gameData.turnTimer <= 0f)
                    {
                        //Time expired during selection
                        _gameplay.CancelSelection();
                    }
                }
                else if (_gameData.turnTimer <= 0f)
                {
                    //Time expired during turn
                    _gameplay.EndTurn();
                }
            }
            
            //Timer during Mulligan phase
            if (_gameData.State == GameState.Mulligan && !_gameplay.IsResolving())
            {
                _gameData.mulliganTimer -= Time.deltaTime;

                if (_gameData.mulliganTimer <= 0f && !mulliganTimerExpired)
                {
                    //Time expired during mulligan
                    _gameplay.onMulliganStart?.Invoke(-2);
                    mulliganTimerExpired = true;
                }
            }

            //Start Game when ready
            if (_gameData.State == GameState.Connecting)
            {
                bool allConnected = _gameData.AreAllPlayersConnected();
                bool allReady = _gameData.AreAllPlayersReady();
                if (allConnected && allReady)
                {
                    StartGame();
                }
            }

            _gameplay.Update(Time.deltaTime);

            //Update AI
            foreach (AIPlayer ai in _aiList)
            {
                ai.Update();
            }
        }
        
        public bool IsGameOver()
        {
            return _gameplay.IsGameEnded() || IsGameExpired() || IsWinExpired();
        }

        private void StartGame()
        {
            //Setup AI
            bool isAIVsAI = !_isDedicatedServer && GameplayData.Get().ai_vs_ai;
            foreach (Player player in _gameData.players)
            {
                if (player.is_ai || isAIVsAI)
                {
                    AIPlayer aiGameplay = AIPlayer.Create(GameplayData.Get().ai_type, _gameplay, player.playerID, player.ai_level);
                    _aiList.Add(aiGameplay);
                }
            }

            //Start Game
            _gameplay.SetupBoard();
            _gameplay.StartGame();
            
        }

        //End game when it has expired (only one player is still connected)
        private void EndExpiredGame()
        {
            Game gdata = _gameplay.GetGameData();
            foreach (Player player in gdata.players)
            {
                if (player.IsConnected())
                {
                    _gameplay.EndGame(player.playerID);
                    return;
                }
            }
        }

        //------ Receive Actions -------

        private void RegisterCommand(ushort tag, UnityAction<ClientData, SerializedData> callback)
        {
            CommandEvent commandEvent = new CommandEvent
            {
                tag = tag,
                callback = callback
            };
            _registeredCommands.Add(tag, commandEvent);
        }
        
        
        public void ReceiveAction(ulong clientID, FastBufferReader reader)
        {
            reader.ReadValueSafe(out ushort type);
            bool found = _registeredCommands.TryGetValue(type, out CommandEvent command);
            if (found)
            {
                ClientData client = GetClient(clientID);
                if(client != null)
                    command.callback.Invoke(client, new SerializedData(reader));
            }
        }

        public void ReceivePlayerSettings(ClientData client, SerializedData data)
        {
            PlayerSettings msg = data.Get<PlayerSettings>();
            int playerID = GetPlayerID(client);
            if (playerID >= 0 && msg != null)
            {
                SetPlayerSettings(playerID, msg);
            }
        }

        public void ReceivePlayerSettingsAI(ClientData client, SerializedData data)
        {
            PlayerSettings msg = data.Get<PlayerSettings>();
            int playerID = GetPlayerID(client);
            if (playerID >= 0 && msg != null)
            {
                SetPlayerSettingsAI(playerID, msg);
            }
        }

        public void ReceiveGameplaySettings(ClientData client, SerializedData data)
        {
            int playerID = GetPlayerID(client);
            GameSettings settings = data.Get<GameSettings>();
            if (playerID >= 0 && settings != null)
            {
                SetGameSettings(settings);
            }
        }

        public void ReceivePlayCard(ClientData client, SerializedData data)
        {
            MsgPlayCard msg = data.Get<MsgPlayCard>();
            int playerID = GetPlayerID(client);
            if (playerID >= 0 && msg != null)
                PlayAction(playerID, msg.cardUID, msg.slot);
        }
        
        private void ChooseManaType(ClientData client, SerializedData data)
        {
            MsgInt msg = data.Get<MsgInt>();
            PlayerMana.ManaType manaType = (PlayerMana.ManaType) msg.value;
            _gameplay.SelectManaType(manaType);
        }

        public void ReceiveAttackTarget(ClientData client, SerializedData data)
        {
            MsgAttack msg = data.Get<MsgAttack>();
            int playerID = GetPlayerID(client);
            Slot targetSlot = Slot.Get(msg.slotX, msg.slotY);
            if (playerID >= 0)
                AttackAction(playerID, msg.attackerUID, targetSlot, false);
        }
        
        public void ReceiveRangedAttackTarget(ClientData client, SerializedData data)
        {
            MsgAttack msg = data.Get<MsgAttack>();
            int playerID = GetPlayerID(client);
            Slot targetSlot = Slot.Get(msg.slotX, msg.slotY);
            if (playerID >= 0)
                AttackAction(playerID, msg.attackerUID, targetSlot, true);
        }

        public void ReceiveMove(ClientData client, SerializedData data)
        {
            MsgPlayCard msg = data.Get<MsgPlayCard>();
            int playerID = GetPlayerID(client);
            if (playerID >= 0 && msg != null)
                MoveAction(playerID, msg.cardUID, msg.slot);
        }

        public void ReceiveCastCardAbility(ClientData client, SerializedData data)
        {
            MsgCastAbility msg = data.Get<MsgCastAbility>();
            int playerID = GetPlayerID(client);
            if (playerID >= 0 && msg != null)
                CastAbilityAction(playerID, msg.casterUID, msg.abilityID);
        }

        public void ReceiveSelectCard(ClientData client, SerializedData data)
        {
            MsgCard msg = data.Get<MsgCard>();
            int playerID = GetPlayerID(client);
            if (playerID >= 0 && msg != null)
                SelectCardAction(playerID, msg.cardUID);
        }

        public void ReceiveSelectPlayer(ClientData client, SerializedData data)
        {
            MsgPlayer msg = data.Get<MsgPlayer>();
            int playerID = GetPlayerID(client);
            if (playerID >= 0 && msg != null)
                SelectPlayerAction(playerID, msg.playerID);
        }

        public void ReceiveSelectSlot(ClientData client, SerializedData data)
        {
            Slot slot = data.Get<Slot>();
            int playerID = GetPlayerID(client);
            if (playerID >= 0 && slot.IsValid())
                SelectSlotAction(playerID, slot);
        }
        
        public void ReceiveSelectCaster(ClientData client, SerializedData data)
        {
            MsgCard msg = data.Get<MsgCard>();
            Card caster = _gameData.GetCard(msg.cardUID);
            if (caster != null)
                SelectCasterAction(caster);
        }

        public void ReceiveSelectChoice(ClientData client, SerializedData data)
        {
            MsgInt msg = data.Get<MsgInt>();
            int playerID = GetPlayerID(client);
            if (playerID >= 0 && msg != null)
                SelectChoiceAction(playerID, msg.value);
        }
        
        public void ReceiveSkipSelect(ClientData client, SerializedData data)
        {
            int playerID = GetPlayerID(client);
            if (playerID >= 0)
                SkipSelectAction(playerID);
        }
        
        public void ReceiveCancelSelection(ClientData client, SerializedData data)
        {
            int playerID = GetPlayerID(client);
            if (playerID >= 0)
                CancelSelectionAction(playerID);
        }

        public void ReceiveEndTurn(ClientData client, SerializedData data)
        {
            int playerID = GetPlayerID(client);
            
            if (playerID >= 0)
                NextTurnAction(playerID);
            
        }

        public void ReceiveResign(ClientData client, SerializedData data)
        {
            int playerID = GetPlayerID(client);
            if (playerID >= 0)
                ResignAction(playerID);
        }

        public void ReceiveChat(ClientData client, SerializedData data)
        {
            MsgChat msg = data.Get<MsgChat>();
            int playerID = GetPlayerID(client);
            if (playerID >= 0 && msg != null)
            {
                msg.playerID = playerID; //Force player id to sending client to avoid spoofing
                SendToAll(GameAction.ChatMessage, msg, NetworkDelivery.Reliable);
            }
        }
        
        public void ReceiveMulligan(ClientData client, SerializedData data)
        {
            int playerID = GetPlayerID(client);
            if (playerID >= 0)
            {
                MsgMulliganDiscarded msg = data.Get<MsgMulliganDiscarded>();
                if (msg != null)
                {
                    _gameplay.MulliganCards(msg.discardedCards, playerID);
                }
            }
        }
        
        public void ReceiveHandCardHovered(ClientData client, SerializedData data)
        {
            MsgCard msg = data.Get<MsgCard>();
            int playerID = GetPlayerID(client);
            if (playerID >= 0 && msg != null)
            {
                SendToAllExcept(client, GameAction.HandCardHoveredByOpponent, msg, NetworkDelivery.Reliable);
            }
        }
        
        public void ReceiveBoardSlotHovered(ClientData client, SerializedData data)
        {
            MsgSlot msg = data.Get<MsgSlot>();
            int playerID = GetPlayerID(client);
            if (playerID >= 0 && msg != null)
            {
                SendToAllExcept(client, GameAction.BoardSlotHoveredByOpponent, msg, NetworkDelivery.Reliable);
            }
        }
        
        public void ReceiveAbilityHovered(ClientData client, SerializedData data)
        {
            MsgCastAbility msg = data.Get<MsgCastAbility>();
            int playerID = GetPlayerID(client);
            if (playerID >= 0 && msg != null)
            {
                SendToAllExcept(client, GameAction.AbilityHovered, msg, NetworkDelivery.Reliable);
            }
        }

        //--- Setup Commands ------

        public async void SetPlayerDeck(int playerID, string username, PlayerDeckSettings deck)
        {
            try
            {
                Player player = _gameData.GetPlayer(playerID);
                if (player != null && _gameData.State == GameState.Connecting)
                {
                    UserData user = Authenticator.Get().UserData; //Offline game, get local user

                    if(Authenticator.Get().IsApi())
                        user = await ApiClient.Get().LoadUserData(username); //Online game, validate from api

                    //Use user API deck
                    UserDeckData udeck = user?.GetDeck(deck.id);
                    if (user != null && udeck != null)
                    {
                        if (user.IsDeckValid(udeck))
                        {
                            _gameplay.SetPlayerDeck(playerID, udeck);
                            SendPlayerReady(player);
                            return;
                        }
                        else
                        {
                            Debug.Log(user.username + " deck is invalid: " + udeck.title);
                            return;
                        }
                    }

                    //Use premade deck
                    DeckData cdeck = DeckData.Get(deck.id);
                    if (cdeck != null)
                        _gameplay.SetPlayerDeck(playerID, cdeck);

                    //Trust client in test mode
                    else if (Authenticator.Get().IsTest())
                        _gameplay.SetPlayerDeck(playerID, deck.id, deck.hero, deck.cards);

                    //Deck not found
                    else
                        Debug.Log("Player " + playerID + " deck not found: " + deck.id);

                    SendPlayerReady(player);
                }
            }
            catch (Exception e)
            {
                Debug.LogError("Error setting player deck: " + e.Message);
            }
        }

        public void SetPlayerSettings(int playerID, PlayerSettings playerSettings)
        {
            if (_gameData.State != GameState.Connecting)
                return;

            Player player = _gameData.GetPlayer(playerID);
            
            if (player == null)
                return;
            
            if (!player.ready)
            {
                player.avatar = playerSettings.avatar;
                player.cardback = playerSettings.cardback;
                player.is_ai = false;
                player.ready = true;
                SetPlayerDeck(playerID, player.username, playerSettings.deck);
                RefreshAll();
            }
        }

        public void SetPlayerSettingsAI(int playerID, PlayerSettings playerSettings)
        {
            if (_gameData.State != GameState.Connecting)
                return;
            if (_isDedicatedServer)
                return; //No AI allowed online server

            Player player = _gameData.GetOpponentPlayer(playerID);
            if (player == null)
                return;
            
            if (!player.ready)
            {
                player.username = playerSettings.username;
                player.avatar = playerSettings.avatar;
                player.cardback = playerSettings.cardback;
                player.is_ai = true;
                player.ready = true;
                player.ai_level = playerSettings.ai_level;

                SetPlayerDeck(player.playerID, player.username, playerSettings.deck);
                RefreshAll();
            }
        }

        public void SetGameSettings(GameSettings settings)
        {
            if (_gameData.State == GameState.Connecting)
            {
                _gameData.settings = settings;
                RefreshAll();
            }
        }

        //----- Commands from player ------------

        public void PlayAction(int playerID, string cardUID, Slot slot)
        {
            Player player = _gameData.GetPlayer(playerID);
            if (!_gameData.IsPlayerActionTurn(player) || _gameplay.IsResolving())
                return; //Actions cant be performed now (not your turn?)

            Card card = player.GetCard(cardUID);
            if(card != null && card.playerID == player.playerID)
                _gameplay.PlayCard(card, slot);
        }

        public void CastAbilityAction(int playerID, string cardUID, string abilityID)
        {
            Player player = _gameData.GetPlayer(playerID);
            if (_gameData.selector == SelectorType.SelectManaTypeToGenerate )
            {
                _gameData.selector = SelectorType.None;
            }
            
            if (!_gameData.IsPlayerActionTurn(player) || _gameplay.IsResolving())
                return; //Actions cant be performed now (not your turn?)

            Card castedCard = player.GetCard(cardUID);
            AbilityData iability = AbilityData.Get(abilityID);
            if (castedCard != null && castedCard.playerID == player.playerID)
                _gameplay.CastAbility(castedCard, iability);
        }

        public void MoveAction(int playerID, string cardUID, Slot slot)
        {
            Player player = _gameData.GetPlayer(playerID);
            if (!_gameData.IsPlayerActionTurn(player) || _gameplay.IsResolving())
                return; //Actions cant be performed now (not your turn?)

            Card card = player.GetCard(cardUID);
            if (card != null && card.playerID == player.playerID)
                _gameplay.MoveCard(card, slot);
        }

        public void AttackAction(int playerID, string attackerUID, Slot targetSlot, bool rangedAttack)
        {
            Player player = _gameData.GetPlayer(playerID);
            if (player == null)
                return;

            if (!_gameData.IsPlayerActionTurn(player) || _gameplay.IsResolving())
                return; //Actions cant be performed now (not your turn?)

            List<Card> possibleAttacker = new List<Card>();
            if (attackerUID == "" && rangedAttack)
            {
                foreach (var currentTurn in _gameData.GetCurrentCardTurn())
                {
                    if (_gameData.CanRangeAttackTarget(currentTurn, targetSlot))
                    {
                        possibleAttacker.Add(currentTurn);
                    }
                }
            }
            else
            {
                possibleAttacker.Add(player.GetCard(attackerUID));
            }

            if (possibleAttacker.Count == 1 && possibleAttacker[0].playerID == playerID)
            {
                if (GameplayData.Get().DragAndDropRangedUnitWillDoARangedAttack)
                {
                    rangedAttack = _gameData.CanRangeAttackTarget(possibleAttacker[0], targetSlot);
                }
                _gameplay.AttackTarget(possibleAttacker[0], targetSlot, false, rangedAttack);
            }

            if (possibleAttacker.Count > 1)
            {
                _gameplay.RequestRangeAttackerChoice(possibleAttacker, targetSlot);
            }
        }

        public void SelectCardAction(int playerID, string cardUID)
        {
            Player player = _gameData.GetPlayer(playerID);
            if (!_gameData.IsPlayerSelectorTurn(player) || _gameplay.IsResolving())
                return;

            Card target = _gameData.GetCard(cardUID);
            _gameplay.SelectCard(target);
        }

        public void SelectPlayerAction(int playerID, int targetID)
        {
            Player player = _gameData.GetPlayer(playerID);
            if (!_gameData.IsPlayerSelectorTurn(player) || _gameplay.IsResolving())
                return;

            Player target = _gameData.GetPlayer(targetID);
            _gameplay.SelectPlayer(target);
        }

        public void SelectSlotAction(int playerID, Slot slot)
        {
            Player player = _gameData.GetPlayer(playerID);
            if (!_gameData.IsPlayerSelectorTurn(player) || _gameplay.IsResolving())
                return;

            _gameplay.SelectSlot(slot);
        }
        
        public void SelectCasterAction(Card caster)
        {
            if (_gameplay.IsResolving())
                return;

            if (_gameData.selector == SelectorType.SelectCaster)
            {
                _gameplay.SelectCaster(caster);
            }
            else if (_gameData.selector == SelectorType.SelectRangeAttacker)
            {
                _gameplay.SelectRangeAttacker(caster);
            }
            
        }

        public void SelectChoiceAction(int playerID, int choice)
        {
            Player player = _gameData.GetPlayer(playerID);
            if (!_gameData.IsPlayerSelectorTurn(player) || _gameplay.IsResolving())
                return;

            _gameplay.SelectChoice(choice);
        }
        
        public void SkipSelectAction(int playerID)
        {
            Player player = _gameData.GetPlayer(playerID);
            if (!_gameData.IsPlayerSelectorTurn(player) || _gameplay.IsResolving())
                return;

            _gameplay.SkipSelection();
        }

        public void CancelSelectionAction(int playerID)
        {
            Player player = _gameData.GetPlayer(playerID);
            if (!_gameData.IsPlayerSelectorTurn(player) || _gameplay.IsResolving())
                return;

            _gameplay.CancelSelection();
        }

        //Go to next step, or next turn
        public void NextStepAction(int playerID)
        {
            Player player = _gameData.GetPlayer(playerID);

            if (!_gameData.IsPlayerTurn(player))
                return; //Actions cant be performed now (not your turn?)

            if (_gameplay.IsResolving())
                return; //Abilities are being resolved

            //Selection
            _gameplay.NextStep();
        }
        
        public void NextTurnAction(int playerID)
        {
            Player player = _gameData.GetPlayer(playerID);

            if (!_gameData.IsPlayerTurn(player))
            {
                if (_gameData.GetPlayer(_gameData.CurrentPlayer).is_ai)
                {
                    _gameplay.EndTurn(); //This is only for testing
                }
                return; //Actions cant be performed now (not your turn?)
            }
                

            if (_gameplay.IsResolving())
                return; //Abilities are being resolved

            //Selection
            _gameplay.EndTurn();
        }

        public void ResignAction(int playerID)
        {
            if (_gameData.State != GameState.Connecting && _gameData.State != GameState.GameEnded)
            {
                int winner = playerID == 0 ? 1 : 0;
                _gameplay.EndGame(winner);
            }
        }

        //-------------

        public void AddClient(ClientData client)
        {
            if (!_connectedClients.Contains(client))
                _connectedClients.Add(client);
        }

        public void RemoveClient(ClientData client)
        {
            _connectedClients.Remove(client);

            int playerID = GetPlayerID(client);
            Player player = _gameData.GetPlayer(playerID);
            
            if (player != null && player.connected)
            {
                player.connected = false;
                RefreshAll();
            }
        }

        public ClientData GetClient(ulong clientID)
        {
            foreach (ClientData client in _connectedClients)
            {
                if (client.client_id == clientID)
                    return client;
            }
            return null;
        }

        public int AddPlayer(ClientData client)
        {
            if (!_players.Contains(client))
                _players.Add(client);
            return GetPlayerID(client);
        }

        public int GetPlayerID(ClientData client)
        {
            int index = 0;
            foreach (ClientData player in _players)
            {
                if (player.user_id == client.user_id)
                    return index;
                index++;
            }
            return -1;
        }

        public bool IsPlayer(ClientData client)
        {
            int id = GetPlayerID(client);
            return id >= 0;
        }

        public int CountPlayers()
        {
            return _players.Count;
        }

        public int CountConnectedClients()
        {
            int nb = 0;
            Game game = GetGameData();
            foreach (Player player in game.players)
            {
                if (player.IsConnected())
                {
                    nb++;
                }
            }
            return nb;
        }

        public Game GetGameData()
        {
            return _gameplay.GetGameData();
        }

        public bool HasGameStarted()
        {
            return _gameplay.IsGameStarted();
        }

        public bool HasGameEnded()
        {
            return _gameplay.IsGameEnded();
        }

        public bool IsGameExpired()
        {
            return _expiration > GAME_EXPIRE_TIME; //Means that the game expired (everyone left or game ended)
        }

        public bool IsWinExpired()
        {
            return _winExpiration > WIN_EXPIRE_TIME; //Means that only one player is left, and he should win
        }

        private void OnGameStart()
        {
            SendToAll(GameAction.GameStart);
            RefreshAll();

            if (_isDedicatedServer && Authenticator.Get().IsApi())
            {
                //Create Match
                ApiClient.Get().CreateMatch(_gameData);
            }
        }

        private void OnBoardSetup()
        {
            RefreshAll();
        }

        private void OnGameEnd(Player winner)
        {
            MsgPlayer msg = new MsgPlayer
            {
                playerID = winner?.playerID ?? -1
            };
            _gameData.winnerPlayer = msg.playerID;
            SendToAll(GameAction.GameEnd, msg, NetworkDelivery.Reliable);
            RefreshAll();

            if (_isDedicatedServer && Authenticator.Get().IsApi())
            {
                //End Match and give rewards
                ApiClient.Get().EndMatch(_gameData, msg.playerID);
            }
        }
        
        public void OnPlayerDisconnected(ClientData iclient)
        {
            _gameplay.DisconnectPlayer();
            MsgClientID msg = new MsgClientID();
            msg.clientID = iclient.client_id; 
            msg.secondsUntilForfeit = Math.Max(0, (int)Math.Round(WIN_EXPIRE_TIME - _winExpiration));
            SendToAll(GameAction.Disconnected, msg, NetworkDelivery.Reliable);
            RefreshAll();
        }
        
        public void OnPlayerReconnected(ClientData iclient)
        {
            _gameplay.ReconnectPlayer();
            MsgClientID msg = new MsgClientID();
            msg.clientID = iclient.client_id; 
            msg.secondsUntilForfeit = 0;
            SendToAll(GameAction.Reconnected, msg, NetworkDelivery.Reliable);
            RefreshAll();
        }
        
        private void OnTurnStart()
        {
            MsgCard msg = new MsgCard
            {
                cardUID = ""
            };

            if (_gameData.GetCurrentCardTurn().Count != 0)
            {
                msg.cardUID = _gameData.GetCurrentCardTurn()[0].uid;
            }
            RefreshAll();
            SendToAll(GameAction.NewTurn, msg, NetworkDelivery.Reliable);
            
        }

        private void OnRoundStart()
        {
            MsgPlayer msg = new MsgPlayer
            {
                playerID = _gameData.CurrentPlayer
            };
            SendToAll(GameAction.NewRound, msg, NetworkDelivery.Reliable);
            RefreshAll();
        }

        private void OnTurnPlay()
        {
            RefreshAll();
        }

        private void OnTurnEnd()
        {
            RefreshAll();
        }

        private void OnSelector()
        {
            RefreshAll();
        }

        private void OnCardPlayed(Card card, Slot slot)
        {
            MsgPlayCard msg = new MsgPlayCard
            {
                cardUID = card.uid,
                slot = slot
            };
            SendToAll(GameAction.CardPlayed, msg, NetworkDelivery.Reliable);
            RefreshAll();
        }

        private void OnReceivedMulligan(int playerId)
        {
            MsgPlayer msg = new MsgPlayer
            {
                playerID = playerId
            };
            RefreshAll();
            SendToAll(GameAction.Mulligan, msg, NetworkDelivery.Reliable);
        }

        private void OnCardMoved(Card card, Slot slot)
        {
            MsgPlayCard msg = new MsgPlayCard
            {
                cardUID = card.uid,
                slot = slot
            };
            SendToAll(GameAction.CardMoved, msg, NetworkDelivery.Reliable);
            RefreshAll();
        }

        private void OnCardSummoned(Card card, Slot slot)
        {
            MsgPlayCard msg = new MsgPlayCard
            {
                cardUID = card.uid,
                slot = slot
            };
            SendToAll(GameAction.CardSummoned, msg, NetworkDelivery.Reliable);
        }

        private void OnCardTransformed(Card card)
        {
            MsgCard msg = new MsgCard
            {
                cardUID = card.uid
            };
            RefreshAll();
            SendToAll(GameAction.CardTransformed, msg, NetworkDelivery.Reliable);
        }

        private void OnCardDiscarded(Card card)
        {
            MsgCard msg = new MsgCard
            {
                cardUID = card.uid
            };
            SendToAll(GameAction.CardDiscarded, msg, NetworkDelivery.Reliable);
            RefreshAll();
        }

        private void OnCardDraw(int nb)
        {
            MsgInt msg = new MsgInt
            {
                value = nb
            };
            SendToAll(GameAction.CardDrawn, msg, NetworkDelivery.Reliable);
        }

        private void OnValueRolled(int nb)
        {
            MsgInt msg = new MsgInt
            {
                value = nb
            };
            SendToAll(GameAction.ValueRolled, msg, NetworkDelivery.Reliable);
        }

        private void OnAttackStart(Card attacker, Card target, int damage)
        {
            MsgAttack msg = new MsgAttack
            {
                attackerUID = attacker.uid,
                slotX = target.slot.x,
                slotY = target.slot.y,
                damage = damage
            };
            SendToAll(GameAction.AttackStart, msg, NetworkDelivery.Reliable);
        }

        private void OnAttackEnd(Card attacker, Card target, int damage)
        {
            MsgAttack msg = new MsgAttack
            {
                attackerUID = attacker.uid,
                slotX = target.slot.x,
                slotY = target.slot.y,
                damage = damage
            };
            RefreshAll();
            SendToAll(GameAction.AttackEnd, msg, NetworkDelivery.Reliable);
        }

        private void OnRangeAttackStart(Card attacker, Slot targetSlot, int damage)
        {
            MsgAttack msg = new MsgAttack
            {
                attackerUID = attacker.uid,
                slotX = targetSlot.x,
                slotY = targetSlot.y,
                damage = damage
            };
            SendToAll(GameAction.RangeAttackStart, msg, NetworkDelivery.Reliable);
        }

        private void OnRangeAttackEnd(Card attacker, Slot targetSlot, int damage)
        {
            MsgAttack msg = new MsgAttack
            {
                attackerUID = attacker.uid,
                slotX = targetSlot.x,
                slotY = targetSlot.y,
                damage = damage
            };
            SendToAll(GameAction.RangeAttackEnd, msg, NetworkDelivery.Reliable);
            RefreshAll();
        }

        private void OnAbilityStart(AbilityData ability, Card caster)
        {
            MsgCastAbility msg = new MsgCastAbility
            {
                abilityID = ability.id,
                casterUID = ""
            };
            if (caster != null)
            {
                msg.casterUID = caster.uid;
                msg.slotX = caster.slot.x;
                msg.slotY = caster.slot.y;
            }
            msg.target_uid = "";
            RefreshAll(); //Ability trigger should not change the game state,  but we want to update game state in case we get multiple triggers before ending ability
            SendToAll(GameAction.AbilityTrigger, msg, NetworkDelivery.Reliable);
        }

        private void OnAbilityTarget(AbilityArgs args, bool isSelectTarget)
        {
            if (args.ability.targetType == AbilityTargetType.SelectManaType)
            {
                OnAbilitySelectMana(args);
                return;
            }
            if (args.target is Card)
                {
                    OnAbilityTargetCard(args, isSelectTarget);
                }
                else if (args.target is Player)
                {
                    OnAbilityTargetPlayer(args);
                }
                else if (args.target is Slot)
                {
                    OnAbilityTargetSlot(args, isSelectTarget);
                }
        }

        private void OnAbilitySelectMana(AbilityArgs args)
        {
            MsgCastAbilityPlayer msg = new MsgCastAbilityPlayer
            {
                abilityID = args.ability.id,
                casterUID = args.caster.uid,
                targetID = (int)args.manaType
            };
            SendToAll(GameAction.AbilitySelectMana, msg, NetworkDelivery.Reliable);
            RefreshAll();
        }

        private void OnAbilityTargetCard(AbilityArgs args, bool isSelectTarget)
        {
            MsgCastAbility msg = new MsgCastAbility
            {
                abilityID = args.ability.id,
                casterUID = args.caster.uid
            };
            Card target = (Card)args.target;
            msg.target_uid = target != null ? target.uid : "";
            msg.isSelectTarget = isSelectTarget;
            SendToAll(GameAction.AbilityTargetCard, msg, NetworkDelivery.Reliable);
            RefreshAll();
        }

        private void OnAbilityTargetPlayer(AbilityArgs args)
        {
            MsgCastAbilityPlayer msg = new MsgCastAbilityPlayer
            {
                abilityID = args.ability.id,
                casterUID = args.caster.uid
            };
            Player target = (Player) args.target;
            msg.targetID = target?.playerID ?? -1;
            SendToAll(GameAction.AbilityTargetPlayer, msg, NetworkDelivery.Reliable);
            RefreshAll();
        }

        private void OnAbilityTargetSlot(AbilityArgs args, bool isSelectTarget)
        {
            MsgCastAbilitySlot msg = new MsgCastAbilitySlot
            {
                abilityID = args.ability.id,
                casterUID = args.caster.uid
            };
            Slot target = (Slot)args.target;
            msg.slot = target;
            msg.isSelectTarget = isSelectTarget;
            SendToAll(GameAction.AbilityTargetSlot, msg, NetworkDelivery.Reliable);
            RefreshAll();
        }

        private void OnAbilityTargetMultiple(AbilityArgs args, List<Slot> slots)
        {
            MsgCastAbilityMultipleTarget msg = new MsgCastAbilityMultipleTarget
            {
                abilityID = args.ability.id,
                casterUID = args.caster.uid,
            };
            msg.slot1 = slots.Count >= 1 ? slots[0] : Slot.None;
            msg.slot2 = slots.Count >= 2 ? slots[1] : Slot.None;
            msg.slot3 = slots.Count >= 3 ? slots[2] : Slot.None;
            msg.slot4 = slots.Count >= 4 ? slots[3] : Slot.None;
            msg.slot5 = slots.Count >= 5 ? slots[4] : Slot.None;

            SendToAll(GameAction.AbilityTargetMultiple, msg, NetworkDelivery.Reliable);
            RefreshAll();
        }

        private void OnAbilityEnd(AbilityArgs args)
        {
            MsgCastAbility msg = new MsgCastAbility
            {
                abilityID = args.ability.id,
                casterUID = args.caster.uid,
                target_uid = ""
            };
            SendToAll(GameAction.AbilityEnd, msg, NetworkDelivery.Reliable);
            RefreshAll();
        }

        private void OnTrapTriggered(Card trap, Card trigger)
        {
            MsgTrap msg = new MsgTrap
            {
                trapUID = trap.uid,
                triggerer_uid = trigger != null ? trigger.uid : ""
            };
            RefreshAll();
            SendToAll(GameAction.TrapTriggered, msg, NetworkDelivery.Reliable);
        }

        private void OnTrapResolved(Card trap, Card trigger)
        {
            MsgTrap msg = new MsgTrap
            {
                trapUID = trap.uid,
                triggerer_uid = trigger != null ? trigger.uid : ""
            };
            SendToAll(GameAction.TrapResolved, msg, NetworkDelivery.Reliable);
            RefreshAll();
        }

        private void SendPlayerReady(Player player)
        {
            if (player != null && player.IsReady())
            {
                MsgInt msg = new MsgInt
                {
                    value = player.playerID
                };
                SendToAll(GameAction.PlayerReady, msg, NetworkDelivery.Reliable);
            }
        }

        public void RefreshAll()
        {
            MsgRefreshAll msg = new MsgRefreshAll
            {
                game_data = GetGameData()
            };
            SendToAll(GameAction.RefreshAll, msg, NetworkDelivery.ReliableFragmentedSequenced);
        }

        public void SendToAll(ushort tag)
        {
            FastBufferWriter writer = new FastBufferWriter(128, Unity.Collections.Allocator.Temp, TcgNetwork.MsgSizeMax);
            writer.WriteValueSafe(tag);
            foreach (ClientData client in _connectedClients)
            {
                if (client != null)
                {
                    Messaging.Send("refresh", client.client_id, writer, NetworkDelivery.Reliable);
                }
            }
            writer.Dispose();
        }

        public void SendToAll(ushort tag, INetworkSerializable data, NetworkDelivery delivery)
        {
            //Debug.Log($"GameServer.SendToAll {tag}");
            FastBufferWriter writer = new FastBufferWriter(128, Unity.Collections.Allocator.Temp, TcgNetwork.MsgSizeMax);
            writer.WriteValueSafe(tag);
            writer.WriteNetworkSerializable(data);
            foreach (ClientData client in _connectedClients)
            {
                if (client != null)
                {
                    Messaging.Send("refresh", client.client_id, writer, delivery);
                }
            }
            writer.Dispose();
        }
        
        public void SendToClient(ClientData client, ushort tag, INetworkSerializable data, NetworkDelivery delivery)
        {
            FastBufferWriter writer = new FastBufferWriter(128, Unity.Collections.Allocator.Temp, TcgNetwork.MsgSizeMax);
            writer.WriteValueSafe(tag);
            writer.WriteNetworkSerializable(data);
            Messaging.Send("refresh", client.client_id, writer, delivery);
            writer.Dispose();
        }
        
        public void SendToAllExcept(ClientData clientToExcept, ushort tag, INetworkSerializable data, NetworkDelivery delivery)
        {
            FastBufferWriter writer = new FastBufferWriter(128, Unity.Collections.Allocator.Temp, TcgNetwork.MsgSizeMax);
            writer.WriteValueSafe(tag);
            writer.WriteNetworkSerializable(data);
            foreach (ClientData client in _connectedClients)
            {
                if (client != null && client.client_id != clientToExcept.client_id)
                {
                    Messaging.Send("refresh", client.client_id, writer, delivery);
                }
            }
            writer.Dispose();
        }

        public ulong ServerID => TcgNetwork.Get().ServerID;
        public NetworkMessaging Messaging => TcgNetwork.Get().Messaging;

        
    }

}
