using System;
using System.Collections.Generic;
using Monarchs.Ability;
using Monarchs.Ability.Target;
using Monarchs.Logic.AbilitySystem;
using TcgEngine;
using UnityEngine;

namespace Monarchs.Logic
{
    /// <summary>
    /// Handles turn and round progression in the game
    /// </summary>
    public class TurnManager
    {
        private GameLogic _gameLogic;
        private Game _game;
        private readonly bool _isInstant;
        private readonly ResolveQueue _resolveQueue;
        private readonly SlotStatusTrigger _slotStatusTrigger;
        private AbilityLogicSystem _abilityLogicSystem => _gameLogic?._abilityLogicSystem;
        private readonly System.Random _random = new System.Random();
        
        public TurnManager(Game game, GameLogic gameLogic, bool isInstant = false)
        {
            _gameLogic = gameLogic;
            _game = game;
            _isInstant = isInstant;
            _resolveQueue = new ResolveQueue(game, isInstant);
            _slotStatusTrigger = new SlotStatusTrigger();
        }

        public void SetData(Game game)
        {
            _game = game;
            _resolveQueue.SetData(game);
        }

        public void Update(float delta)
        {
            _resolveQueue.Update(delta);
        }
        
        /// <summary>
        /// Chooses the first player at random or sets a fixed first player
        /// </summary>
        public void ChooseFirstPlayer()
        {
            //Uncomment this line to enable random first player selection
            //_game.firstPlayer = _random.NextDouble() < 0.5 ? 0 : 1;
            
            // Currently set to player 1 for deterministic gameplay
            _game.firstPlayer = 1;
            _game.CurrentPlayer = 1;
        }
        
        /// <summary>
        /// Initializes and starts the game, setting up initial state and drawing starting cards
        /// </summary>
        public virtual void StartGame(CardManager cardManager)
        {
            if (_game.State == GameState.GameEnded)
                return;

            // Setup game state
            _game.SetState(GameState.Starting);
            _game.roundCount = 1;
            _game.turnCount = 1;

            // Initialize each player's starting hand
            foreach (Player player in _game.players)
            {
                int nbCardToDraw = GameplayData.Get().cards_start;
                cardManager.DrawCard(player.playerID, nbCardToDraw);
            }

            // Trigger game start event
            _gameLogic.onGameStart?.Invoke();

            // Move to mulligan phase
            StartMulligan();
        }

        /// <summary>
        /// Begins the mulligan phase where players can choose to replace cards
        /// </summary>
        public virtual void StartMulligan()
        {
            _game.SetState(GameState.Mulligan);
            _game.mulliganTimer = GameplayData.Get().mulligan_duration;
            
            _gameLogic.onMulliganStart?.Invoke(-1);
        }

        /// <summary>
        /// Processes the mulligan choice for a player
        /// </summary>
        public virtual void MulliganCards(string[] mulliganedCards, int playerId, CardManager cardManager)
        {
            Player player = _game.GetPlayer(playerId);
            if (player.submittedMulligan) 
                return;
            
            foreach (var cardUID in mulliganedCards)
            {
                Card card = _game.GetCard(cardUID);
                Card newCard = player.cards_deck[0];
                player.cards_deck.RemoveAt(0);
                
                int index = player.cards_hand.IndexOf(card);
                player.cards_hand[index] = newCard;
                player.cards_deck.Add(card);
            }
            
            cardManager.ShuffleDeck(player.cards_deck);
            player.submittedMulligan = true;
            
            _gameLogic.onMulliganStart?.Invoke(playerId);
            _resolveQueue.ResolveAll(0.2f);
            
            if (_game.BothPlayersSubmittedMulligan())
            {
                StartRound(cardManager);
            }
        }

        private void ClearRoundData()
        {
            _game.selector = SelectorType.None;
            _game.selectorAbilityID = "";
            _game.selectorCardUID = "";
            _game.selectorCasterUID = "";
            _game.selectorTargets = new List<ITargetable>();
            _resolveQueue.Clear();
            _game.lastPlayed = null;
            _game.lastKilled = null;
            _game.lastTarget = null;
            _game.abilityTriggerer = null;
            _game.abilityPlayed.Clear();
            _game.cardsAttacked.Clear();
        }

        /// <summary>
        /// Starts a new round, drawing cards and refreshing units
        /// </summary>
        public virtual void StartRound(CardManager cardManager)
        {
            if (_game.State == GameState.GameEnded)
                return;

            ClearRoundData();
            
            _game.SetState(GameState.StartRound);
            
            // Don't trigger round start event for the first round
            if (_game.roundCount != 1)
            {
                _gameLogic.onRoundStart?.Invoke();
            }

            foreach (var player in _game.players)
            {
                // Draw cards for the new round
                cardManager.DrawCard(player.playerID, GameplayData.Get().cards_per_turn);
                player.playerMana.AddGeneratingMana();
                
                // Refresh Cards and process status effects
                for (int i = player.cards_board.Count - 1; i >= 0; i--)
                {
                    Card card = player.cards_board[i];
                    card.playedCardThisRound = false;
                    card.canRetaliate = true;
                    card.numberOfMoveThisTurn = 0;
                    card.hasAttacked = false;

                    if(!card.HasStatus(StatusType.Sleep))
                        card.Refresh();

                    if (card.HasStatus(StatusType.Poisoned))
                        cardManager.DamageCard(card, card.GetStatusValue(StatusType.Poisoned));
                }
                
                _abilityLogicSystem.TriggerAllAbilityWithTriggerTypeOfPlayer(player, AbilityTrigger.StartOfRound);
            }
            
            // Initialize turn timer
            _game.turnTimer = GameplayData.Get().turn_duration;

            _resolveQueue.AddCallback(() => StartTurn(cardManager));
            _resolveQueue.ResolveAll(0.2f);
        }

        /// <summary>
        /// Starts a new turn
        /// </summary>
        public virtual void StartTurn(CardManager cardManager)
        {
            if (_game.State == GameState.GameEnded)
                return;

            ClearRoundData();
            _game.SetState(GameState.StartTurn);

            if (!(_game.roundCount == 1 && _game.turnCount == 1)) //To avoid calling event at start of game
            {
                _gameLogic.onTurnStart?.Invoke();
                _slotStatusTrigger.TriggerAllSlotStatus(_gameLogic, _game, SlotStatusTriggerType.OnTurnStart);
            }

            foreach (var player in _game.players)
            {
                _abilityLogicSystem.TriggerAllAbilityWithTriggerTypeOfPlayer(player, AbilityTrigger.StartOfTurn);
            }
            
            _game.turnTimer = GameplayData.Get().turn_duration;
            
            Player activePlayer = _game.GetActivePlayer();
            if (activePlayer == null)
            {
                _resolveQueue.AddCallback(() => StartNextTurn(cardManager));
                return;
            }
            
            _resolveQueue.AddCallback(() => StartPlayPhase(cardManager));
            _resolveQueue.ResolveAll(0.2f);
        }

        /// <summary>
        /// Moves to the next turn
        /// </summary>
        public virtual void StartNextTurn(CardManager cardManager)
        {
            if (_game.State == GameState.GameEnded)
                return;

            _game.initiativeManager.NextTurn();
            _game.CurrentPlayer = _game.CurrentPlayer + 1 == 2 ? 0 : 1; // Toggle between player 0 and 1
            _game.turnCount++;
            
            if (_game.initiativeManager.IsRoundOver() || _game.CurrentPlayer == _game.firstPlayer)
            {
                StartNextRound(cardManager);
                return;
            }
            
            _gameLogic.CheckForWinner();
            StartTurn(cardManager);
        }

        /// <summary>
        /// Moves to the next round
        /// </summary>
        public virtual void StartNextRound(CardManager cardManager)
        {
            if (_game.State == GameState.GameEnded)
                return;

            _game.roundCount++;
            _game.turnCount = 0;
            _game.initiativeManager.NextRound(_game);

            _gameLogic.CheckForWinner();
            StartRound(cardManager);
        }

        /// <summary>
        /// Begins the play phase where players can take actions
        /// </summary>
        public virtual void StartPlayPhase(CardManager cardManager)
        {
            if (_game.State == GameState.GameEnded)
                return;

            _game.SetState(GameState.Play);
            
            // Handle special case for units with Rabies status
            foreach (var card in _game.GetCurrentCardTurn())
            {
                if (card.HasStatus(StatusType.Rabies))
                {
                    RabiesAI rabiesAI = new RabiesAI();
                    rabiesAI.DoTurn(null, _game, card); // TODO: Fix this reference to use the main GameLogic
                }
            }
            
            _gameLogic.onTurnPlay?.Invoke();
        }

        /// <summary>
        /// Ends the current turn, processes end of turn effects
        /// </summary>
        public virtual void EndTurn(CardManager cardManager, BoardLogic boardManager)
        {
            if (_game.State != GameState.Play)
                return;
            
            // Reset selector state
            _game.selector = SelectorType.None;
            _game.lastAbilityDone = "";
            _game.SetState(GameState.EndTurn);
            
            // Trigger end of turn abilities for all players
            foreach (var player in _game.players)
            {
                _abilityLogicSystem.TriggerAllAbilityWithTriggerTypeOfPlayer(player, AbilityTrigger.EndOfTurn);
            }
            
            
            List<Card> currentCardTurn = _game.GetCurrentCardTurn();
            
            
            // Process statuses and charges for all cards
            foreach (var player in _game.players)
            {
                foreach (var card in player.cards_board)
                {
                    card.RemoveCharge();
                    card.ReduceStatusDurations(currentCardTurn);
                    card.wasPlayedThisTurn = false;
                }
            }
            
            // Process "on remove status" effects
            foreach (var player in _game.players)
            {
                for (int i = player.cards_board.Count - 1; i >= 0; i--)
                {
                    player.cards_board[i].TriggerOnRemoveStatusEffects(_gameLogic);
                }
            }

            // Process slot status movements
            foreach (var slotStatus in _game.slotStatusList)
            {
                slotStatus.DoMovement(_gameLogic, currentCardTurn);
            }

            // Reduce slot status durations at the end of a full round
            if (_game.initiativeManager.GetCurrentTurnIndex() == 
                _game.initiativeManager.GetInitiativeOrder().Count - 1)
            {
                boardManager.ReduceSlotStatusDurations();
            }
            
            _abilityLogicSystem.UpdateOngoingEffect();
            
            // Trigger end of turn abilities for current card
            foreach (var card in currentCardTurn)
            {
                if (card.isAlive())
                {
                    _abilityLogicSystem.TriggerCardAbilityType(AbilityTrigger.EndOfCasterTurn, card, card);
                }
            }
            
            // Update calculations
            boardManager.CalculatePiecesMove();
            
            cardManager.ValidateIfAnyBoardCardsAreDead();

            // Queue next turn
            _resolveQueue.AddCallback(() => StartNextTurn(cardManager));
            _resolveQueue.ResolveAll(0.2f);
        }

        /// <summary>
        /// Ends the current round
        /// </summary>
        public virtual void EndRound(CardManager cardManager)
        {
            if (_game.State != GameState.Play)
                return;

            _game.selector = SelectorType.None;
            _game.SetState(GameState.EndRound);

            // Trigger end of round abilities
            Player player = _game.GetActivePlayer();
            _abilityLogicSystem.TriggerAllAbilityWithTriggerTypeOfPlayer(player, AbilityTrigger.EndOfRound);
            _gameLogic.onRoundEnd?.Invoke();

            // Queue next round
            _resolveQueue.AddCallback(() => StartNextRound(cardManager));
            _resolveQueue.ResolveAll(0.2f);
        }

        public virtual void EndGame(int winner)
        {
            if (_game.State != GameState.GameEnded)
            {
                _game.SetState(GameState.GameEnded);
                Player player = _game.GetPlayer(winner);
                _gameLogic.onGameEnd?.Invoke(player);
            }
        }
    }
}