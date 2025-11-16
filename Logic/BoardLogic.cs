using System.Collections.Generic;
using System.Linq;
using Monarchs.Ability;
using Monarchs.Initiative;
using Monarchs.Logic.AbilitySystem;
using TcgEngine;
using UnityEngine;
using UnityEngine.Events;

namespace Monarchs.Logic
{
    /// <summary>
    /// Manages board state, slot operations, and movement
    /// </summary>
    public class BoardLogic
    {
        private readonly GameLogic _gameLogic;
        private Game _game;
        private readonly bool _isInstant;
        private readonly ResolveQueue _resolveQueue;
        private readonly SlotStatusTrigger _slotStatusTrigger;
        private AbilityLogicSystem _abilityLogicSystem => _gameLogic?._abilityLogicSystem;
        
        public BoardLogic(Game game, GameLogic gameLogic, bool isInstant = false)
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
        /// Sets up the initial board state based on GameplayData
        /// </summary>
        public virtual void SetupBoard()
        {
            Debug.Log("Server is setting up board.");
            BoardSetupPosition boardSetupPosition = GameplayData.Get().BoardSetupPosition;

            foreach (var piece in boardSetupPosition.piecesOnSquare)
            {
                AddCardBeforeGame(piece.piece, piece.firstPlayer, Slot.Get(piece.x, piece.y));
            }

            _game.initiativeManager.ResetCurrentTurnIndex();
            CalculatePiecesMove();
            _gameLogic.onBoardSetup?.Invoke();
        }

        /// <summary>
        /// Adds a card to the board before the game starts
        /// </summary>
        protected virtual void AddCardBeforeGame(CardData cardData, bool firstPlayer, Slot slot)
        {
            int playerID = firstPlayer ? _game.firstPlayer : _game.SecondPlayerId();
            Card card = Card.Create(cardData, VariantData.GetDefault(), playerID);
            card.slot = slot;
            
            Player player = _game.GetPlayer(playerID);
            player.cards_all[card.uid] = card;
            player.cards_board.Add(card);
            if (card.HasTrait("king"))
            {
                if (player.king != null)
                {
                    Debug.LogError("Player is trying to add a second king");
                }
                else
                {
                    player.king = card;
                }
            }
            
            _game.initiativeManager.AddCard(card, true);
            _resolveQueue.ResolveAll(0f);
        }

        /// <summary>
        /// Calculates possible moves for all pieces on the board
        /// </summary>
        public virtual void CalculatePiecesMove()
        {
            _game.pieceMovesList = new Dictionary<string, PieceMoves>();
            foreach (var player in _game.players)
            {
                foreach (var card in player.cards_board)
                {
                    PieceMoves pieceMove = new PieceMoves
                    {
                        possibleMoves = card.GetLegalMoves(_game)
                    };
                    
                    pieceMove.possibleMoves = _game.intimidateManager.FilterIntimidate(_game, player.playerID,
                        pieceMove.possibleMoves.ToList()).ToArray();
                    pieceMove.realDestination = new Dictionary<Vector2S, Vector2S>();

                    foreach (var status in _game.slotStatusList)
                    {
                        if (pieceMove.possibleMoves.Contains(status.slot.GetCoordinate()))
                        {
                            Slot slot = status.SlotStatusData.GetModifiedDestination(_gameLogic, card, status.slot);
                            if (slot != status.slot)
                            {
                                pieceMove.realDestination[status.slot.GetCoordinate()] = slot.GetCoordinate();
                            }
                        }
                    }
                    _game.pieceMovesList[card.uid] = pieceMove;
                }
            }
        }

        /// <summary>
        /// Moves a card to another slot
        /// </summary>
        public virtual void MoveCard(Card card, Slot slot, bool skipCost = false, bool exhaust = true)
        {
            if (_game.CanMoveCard(card, slot, skipCost))
            {
                card.previousSlot = card.slot;
                Slot initialSlot = card.slot;
                slot = _game.GetModifiedDestination(card, card.slot, slot);
                _abilityLogicSystem.TriggerCardAbilityType(AbilityTrigger.OnBeforeMove, card);
                _game.lastMoveDestination = slot;
                
                if (_abilityLogicSystem.TriggerTrapsOnMovePath(AbilityTrigger.OnMoveOnSpecificSquare, card, slot))
                {
                    return;
                }
                
                RefreshSlotStatusOnMovePath(card, card.slot, slot);
                card.slot = slot;
                
                _game.history.AddMoveHistory(card.playerID, card, initialSlot, slot);
                
                _abilityLogicSystem.UpdateOngoingEffect();
                
                // Check if a pawn has reached the end of the line
                if (card.CardData.IsPawn)
                {
                    if (card.playerID == _game.firstPlayer && slot.y == 7 ||
                        card.playerID != _game.firstPlayer && slot.y == 0)
                    {
                        _abilityLogicSystem.TriggerCardAbilityType(AbilityTrigger.EndOfTheLine, card, card);
                    }
                }
                
                // End turn
                if (!skipCost)
                {
                    card.hasMovedThisGame = true;
                    card.numberOfMoveThisTurn += 1;
                    card.ExhaustAfterMove(exhaust);
                }
                
                CalculatePiecesMove();

                _gameLogic.onCardMoved?.Invoke(card, slot);
                _abilityLogicSystem.TriggerCardAbilityType(AbilityTrigger.OnAfterMove, card);
                _slotStatusTrigger.TriggerAllSlotStatusOnMovePath(_gameLogic, _game, initialSlot, slot, card);
                
                bool isHitAndRun = card.HasTrait("hit_and_run") && card.numberOfMoveThisTurn < 2;


                bool hasAfterMoveAbilityWithSelector = card.HasAbility(trigger:AbilityTrigger.OnAfterMove, selectorOnly:true);
                
                if (_game.IsAllCardTurnExhausted() && !isHitAndRun && !hasAfterMoveAbilityWithSelector && GameplayData.Get().InitiativeType != InitiativeManager.InitiativeType.AllPiecesEveryTurn)
                {
                    _resolveQueue.AddCallback(_gameLogic.EndTurn); // This needs to be hooked to EndTurn in main GameLogic
                }

                _resolveQueue.ResolveAll(0f);
            }
        }

        /// <summary>
        /// Forces a card to move to a slot without normal move checks
        /// </summary>
        public virtual void ForceMoveCard(Card card, Slot slot, bool skipCost = true, bool exhaust = true)
        {
            card.previousSlot = card.slot;
            if (_game.GetSlotCard(slot) != null)
            {
                return;
            }
            
            _abilityLogicSystem.TriggerCardAbilityType(AbilityTrigger.OnBeforeMove, card);
            
            if (_abilityLogicSystem.TriggerTrapsOnMovePath(AbilityTrigger.OnMoveOnSpecificSquare, card, slot))
            {
                return;
            }
            
            card.slot = slot;
            card.ExhaustAfterMove(exhaust);
            
            _abilityLogicSystem.UpdateOngoingEffect();

            _gameLogic.onCardMoved?.Invoke(card, slot);
            _abilityLogicSystem.TriggerCardAbilityType(AbilityTrigger.OnAfterMove, card);
            CalculatePiecesMove();
            _resolveQueue.ResolveAll(0.2f);
        }

        /// <summary>
        /// Moves a card to a trap slot
        /// </summary>
        public virtual void MoveCardToTrap(Card card, Slot slot)
        {
            card.previousSlot = card.slot;
            card.slot = slot;
            card.ExhaustAfterMove();
            _gameLogic.onCardMoved?.Invoke(card, slot);
        }

        /// <summary>
        /// Refreshes slot statuses along a movement path
        /// </summary>
        public virtual void RefreshSlotStatusOnMovePath(Card card, Slot startSlot, Slot destinationSlot)
        {
            if (card.GetPieceType() == PieceType.Knight)
            {
                RefreshSlotStatus(destinationSlot);
            }
            else
            {
                List<Slot> slotInBetweenMove = Slot.GetSlotInBetween(startSlot, destinationSlot);
                foreach (var slot in slotInBetweenMove)
                {
                    RefreshSlotStatus(slot);
                }
                RefreshSlotStatus(destinationSlot);
            }
        }

        /// <summary>
        /// Refreshes a slot's status
        /// </summary>
        public virtual void RefreshSlotStatus(Slot slot)
        {
            SlotStatus slotStatus = _game.GetSlotStatus(slot);

            if (slotStatus != null)
            {
                if (slotStatus.SlotStatusData.duration == SlotStatusDuration.ActivateOnce)
                {
                    _game.slotStatusList.Remove(slotStatus);
                }
            }
        }

        /// <summary>
        /// Adds a status effect to a slot
        /// </summary>
        public virtual void AddSlotStatus(Slot slot, SlotStatusData slotStatusData, bool showOnBoard = true, Card triggerer = null)
        {
            if (_game.GetSlotStatus(slot) != null)
            {
                _game.slotStatusList.Remove(_game.GetSlotStatus(slot));
            }
            SlotStatus slotStatus = new SlotStatus(slotStatusData, slot, showOnBoard, triggerer);
            _game.slotStatusList.Add(slotStatus);
            _slotStatusTrigger.TriggerSlotStatus(_gameLogic, _game, SlotStatusTriggerType.OnSlotStatusSpawn, slotStatus);
        }

        /// <summary>
        /// Reduces the duration of all slot status effects
        /// </summary>
        public void ReduceSlotStatusDurations()
        {
            List<SlotStatus> statusesToRemove = new List<SlotStatus>();

            foreach (var status in _game.slotStatusList)
            {
                status.ReduceDuration();
                if (status.turnsLeft <= 0 && status.SlotStatusData.duration == SlotStatusDuration.Temporary)
                {
                    statusesToRemove.Add(status);
                }
            }
            
            foreach (var status in statusesToRemove)
            {
                _game.slotStatusList.Remove(status);
            }
        }
    }
}