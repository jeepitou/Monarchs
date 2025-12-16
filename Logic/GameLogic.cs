using System.Collections.Generic;
using System.Linq;
using Monarchs.Ability;
using Monarchs.Ability.Target;
using Monarchs.Api;
using Monarchs.Initiative;
using Monarchs.Logic.AbilitySystem;
using TcgEngine;

namespace Monarchs.Logic
{
    /// <summary>
    /// Main coordinator class that executes and resolves game rules and logic using specialized managers
    /// </summary>
    public class GameLogic : BaseGameLogic
    {
        private readonly CardManager _cardManager;
        private readonly TurnManager _turnManager;
        private readonly BoardLogic _boardLogic;
        private readonly CombatManager _combatManager;

        public GameLogic(bool isInstant) : base(isInstant)
        {
            _cardManager = new CardManager(null, isInstant);
            _turnManager = new TurnManager(null, this, isInstant);
            _boardLogic = new BoardLogic(null, this, isInstant);
            _combatManager = new CombatManager(null, isInstant);
            
            _cardManager.Initialize(this);
            _combatManager.Initialize(this);
        }

        public GameLogic(Game game, bool isInstant = false) : base(game, isInstant)
        {
            _cardManager = new CardManager(game, isInstant);
            _turnManager = new TurnManager(game, this, isInstant);
            _boardLogic = new BoardLogic(game, this, isInstant);
            _combatManager = new CombatManager(game, isInstant);
            
            _cardManager.Initialize(this);
            _combatManager.Initialize(this);
            
            ChooseFirstPlayer();
            SubscribeToEvents();
        }

        private void SubscribeToEvents()
        {
            _abilityLogicSystem.onAbilityStart += OnAbilityStart;
            _abilityLogicSystem.onAbilityEnd += OnAbilityEnd;
            _abilityLogicSystem.onAbilityTarget += OnAbilityTarget;
            _abilityLogicSystem.onAbilityTargetMultiple += OnAbilityTargetMultiple;
            
            _abilityLogicSystem.onSelectCasterPlayCard += PlayCard;
            _abilityLogicSystem.onSelectorSelect += OnSelectorSelect;
            _abilityLogicSystem.onSelectorStart += OnSelectorStart;

            _abilityLogicSystem.onTrapResolved += OnTrapResolved;
            _abilityLogicSystem.onTrapTriggered += OnTrapTriggered;
        }
        
        public void DisconnectPlayer()
        {
            _game.SetState(GameState.PlayerDisconnected);
        }
        
        public void ReconnectPlayer()
        {
            _game.SetState(_game.PreviousState);
        }

        private void OnSelectorStart()
        {
            onSelectorStart?.Invoke();
        }

        private void OnSelectorSelect()
        {
            onSelectorSelect?.Invoke();
        }

        private void OnTrapResolved(Card trap, Card triggerer)
        {
            onTrapResolved?.Invoke(trap, triggerer);
        }

        private void OnAbilityStart(AbilityData ability, Card card)
        {
            onAbilityStart?.Invoke(ability, card);
        }
        
        private void OnAbilityTarget(AbilityArgs args, bool selectTarget)
        {
            onAbilityTarget?.Invoke(args, selectTarget);
        }

        private void OnAbilityTargetMultiple(AbilityArgs args, List<Slot> slots)
        {
            onAbilityTargetMultiple?.Invoke(args, slots);
        }

        private void OnTrapTriggered(Card trap, Card triggerer)
        {
            onTrapTrigger?.Invoke(trap, triggerer);
        }
        
        public override void SetData(Game game)
        {
            base.SetData(game);
            _cardManager.SetData(game);
            _turnManager.SetData(game);
            _boardLogic.SetData(game);
            _combatManager.SetData(game);
        }

        public override void Update(float delta)
        {
            base.Update(delta);
            _cardManager.Update(delta);
            _turnManager.Update(delta);
            _boardLogic.Update(delta);
            _combatManager.Update(delta);
        }

        #region Turn & Round Management

        /// <summary>
        /// Chooses the first player at random or sets a fixed first player
        /// </summary>
        public void ChooseFirstPlayer()
        {
            _turnManager.ChooseFirstPlayer();
        }

        /// <summary>
        /// Initializes and starts the game
        /// </summary>
        public virtual void StartGame()
        {
            _turnManager.StartGame(_cardManager);
        }

        /// <summary>
        /// Begins the mulligan phase
        /// </summary>
        public virtual void StartMulligan()
        {
            _turnManager.StartMulligan();
        }

        /// <summary>
        /// Processes the mulligan choice for a player
        /// </summary>
        public virtual void MulliganCards(string[] mulliganedCards, int playerId)
        {
            _turnManager.MulliganCards(mulliganedCards, playerId, _cardManager);
        }

        /// <summary>
        /// Starts a new round
        /// </summary>
        public virtual void StartRound()
        {
            _turnManager.StartRound(_cardManager);
        }

        /// <summary>
        /// Starts a new turn
        /// </summary>
        public virtual void StartTurn()
        {
            _turnManager.StartTurn(_cardManager);
        }

        /// <summary>
        /// Moves to the next turn
        /// </summary>
        public virtual void StartNextTurn()
        {
            _turnManager.StartNextTurn(_cardManager);
        }

        /// <summary>
        /// Moves to the next round
        /// </summary>
        public virtual void StartNextRound()
        {
            _turnManager.StartNextRound(_cardManager);
        }

        /// <summary>
        /// Begins the play phase
        /// </summary>
        public virtual void StartPlayPhase()
        {
            _turnManager.StartPlayPhase(_cardManager);
        }

        /// <summary>
        /// Ends the current turn
        /// </summary>
        public virtual void EndTurn()
        {
            _turnManager.EndTurn(_cardManager, _boardLogic);
        }

        /// <summary>
        /// Ends the current round
        /// </summary>
        public virtual void EndRound()
        {
            _turnManager.EndRound(_cardManager);
        }

        /// <summary>
        /// Ends the game with a winner
        /// </summary>
        public virtual void EndGame(int winner)
        {
            _game.winnerPlayer = winner;
            _turnManager.EndGame(winner);
        }

        #endregion

        #region Board Operations

        /// <summary>
        /// Sets up the initial board state
        /// </summary>
        public virtual void SetupBoard()
        {
            _boardLogic.SetupBoard();
        }

        /// <summary>
        /// Calculates possible moves for all pieces on the board
        /// </summary>
        public virtual void CalculatePiecesMove()
        {
            _boardLogic.CalculatePiecesMove();
        }

        /// <summary>
        /// Moves a card to another slot
        /// </summary>
        public virtual void MoveCard(Card card, Slot slot, bool skipCost = false, bool exhaust = true)
        {
            _boardLogic.MoveCard(card, slot, skipCost, exhaust);
        }

        /// <summary>
        /// Forces a card to move to a slot without normal move checks
        /// </summary>
        public virtual void ForceMoveCard(Card card, Slot slot, bool skipCost = true, bool exhaust = true)
        {
            _boardLogic.ForceMoveCard(card, slot, skipCost, exhaust);
        }

        /// <summary>
        /// Moves a card to a trap slot
        /// </summary>
        public virtual void MoveCardToTrap(Card card, Slot slot)
        {
            _boardLogic.MoveCardToTrap(card, slot);
        }

        /// <summary>
        /// Adds a status effect to a slot
        /// </summary>
        public virtual void AddSlotStatus(Slot slot, SlotStatusData slotStatusData, bool showOnBoard = true, Card triggerer = null)
        {
            _boardLogic.AddSlotStatus(slot, slotStatusData, showOnBoard, triggerer);
        }

        #endregion

        #region Card Operations

        /// <summary>
        /// Sets up a player's deck using a predefined deck data
        /// </summary>
        public virtual void SetPlayerDeck(int playerID, DeckData deck)
        {
            _cardManager.SetPlayerDeck(playerID, deck);
        }

        /// <summary>
        /// Sets up a player's deck using a custom deck from save file or database
        /// </summary>
        public virtual void SetPlayerDeck(int playerID, UserDeckData deck)
        {
            _cardManager.SetPlayerDeck(playerID, deck);
        }

        /// <summary>
        /// Sets up a player's deck using card IDs
        /// </summary>
        public virtual void SetPlayerDeck(int playerID, string deckID, string monarch, string champion, string[] cards)
        {
            _cardManager.SetPlayerDeck(playerID, deckID, monarch, champion, cards);
        }

        /// <summary>
        /// Draws cards from a player's deck
        /// </summary>
        public virtual void DrawCard(int playerID, int nb = 1)
        {
            _cardManager.DrawCard(playerID, nb);
        }
        
        /// <summary>
        /// Makes a player draw a card and discards it
        /// </summary>
        public virtual void DrawDiscardCard(int playerID, int nb = 1)
        {
            _cardManager.DrawDiscardCard(playerID, nb);
        }

        /// <summary>
        /// Plays a card onto the board
        /// </summary>
        public virtual void PlayCard(Card card, Slot slot, bool skipCost = false)
        {
            Player player = _game.GetPlayer(card.playerID);
            if (_game.CanPlayCardOnSlot(card, slot, skipCost, true))
            {
                Card target = _game.GetSlotCard(slot);
                Card currentCardTurn = _game.GetCurrentCardTurn()[0];
                bool isBoundSpell = currentCardTurn.CardData.boundSpells.Contains(card.CardData);
                bool casterAlreadySelected = _game.selectorCasterUID != "";
                
                Card caster;

                if (casterAlreadySelected)
                {
                    caster = _game.GetCard(_game.selectorCasterUID);
                }
                else
                {
                    caster = _abilityLogicSystem.ValidatePossibleCastersPlayCard(slot, card, null);
                    if (caster == null)
                    {
                        return;
                    }
                }
                
                bool manaToSpendSelected = _game.selectorManaType != PlayerMana.ManaType.None;
                if ((caster.GetPieceType() != PieceType.Monarch && !isBoundSpell && GameplayData.Get().InterventionCastByNonMonarchCostsMana) && !skipCost && !manaToSpendSelected)
                {
                    _abilityLogicSystem.RequestChooseMana(card, slot, caster);
                    return;
                }

                if (card.NeedsToSelectTargetBeforePlay() && _game.lastTarget == null && _game.selectorTargets.Count == 0)
                {
                    card.slot = slot;
                    _game.selectorTargets.Add(slot);
                    _game.selector = card.GetOnPlaySelectorType();
                    _game.selectorCastedCardUID = card.uid;
                    _game.selectorCasterUID = caster.uid;
                    _game.selectorCardUID = card.uid;
                    _game.selectorPlayer = card.playerID;
                    _game.selectorAbilityID = card.GetAbility(trigger:AbilityTrigger.OnPlay, getSelectorOnly:true).id;
                    onSelectorStart?.Invoke();
                    return;
                }
                
                caster.playedCardThisRound = true;
                card.wasPlayedThisTurn = true;
                
                // Pay cost
                if (!skipCost)
                    player.PayMana(card, _game.selectorManaType);

                _game.selectorManaType = PlayerMana.ManaType.None;
                
                // Play card
                player.RemoveCardFromAllGroups(card);
                card.Clear();
                if (card.parentCard != null)
                {
                    player.RemoveCardFromAllGroups(card.parentCard);
                    foreach (var childCard in card.parentCard.childCard)
                    {
                        player.RemoveCardFromAllGroups(childCard.Value);
                        player.cards_all.Remove(childCard.Value.uid);
                    }
                    player.cards_all.Add(card.uid, card);
                }

                // Add to board
                CardData cardData = card.CardData;
                if (cardData.IsBoardCard())
                {
                    player.cards_board.Add(card);
                    card.slot = slot;
                    int cohortSize = card.cohortSize;
                    card.SetCard(cardData, card.VariantData);      // Reset all stats to default
                    card.cohortSize = cohortSize;
                    card.exhausted = true; // Can't attack first turn
                    _game.initiativeManager.AddCard(card);
                }
                else if (cardData.IsTrap())
                {
                    player.cards_trap.Add(card);
                    card.slot = slot; 
                }
                else
                {
                    player.cards_discard.Add(card);
                    card.slot = slot; // Save slot in case spell has PlayTarget
                }

                // History
                if (!cardData.IsTrap())
                {
                    _game.history.AddCardPlayedHistory(player.playerID, caster, card, target, caster.slot,
                        _game.selectorTargets.Count >= 2
                            ? Slot.GetSlotsOfTargets(_game.selectorTargets)
                            : new List<Slot> { slot });
                }

                // Update ongoing effects
                _game.lastPlayed = card;
                _abilityLogicSystem.UpdateOngoingEffect();

                _boardLogic.CalculatePiecesMove();
                onCardPlayed?.Invoke(card, slot);

                // Trigger abilities
                _abilityLogicSystem.TriggerTrapsOnSpecificSlot(AbilityTrigger.OnMoveOnSpecificSquare, card, slot);

                _abilityLogicSystem.TriggerCardAbilityType(AbilityTrigger.OnPlay, card, caster);
                _abilityLogicSystem.TriggerCardAbilityType(AbilityTrigger.IntoTheFray, card, card);
                _abilityLogicSystem.TriggerAllAbilityWithTriggerType(AbilityTrigger.OnPlayOther, card);

                _boardLogic.CalculatePiecesMove();
                _resolveQueue.ResolveAll(0.3f);
            }
        }

        /// <summary>
        /// Summons a card to the board
        /// </summary>
        public virtual void SummonPiece(Card card, Slot slot)
        {
            _cardManager.SummonPiece(card, slot);
        }

        /// <summary>
        /// Creates a copy of an existing card and summons it
        /// </summary>
        public virtual Card SummonCopy(int playerID, Card copy, Slot slot)
        {
            return _cardManager.SummonCopy(playerID, copy, slot);
        }

        /// <summary>
        /// Creates a copy of an existing card in hand
        /// </summary>
        public virtual Card SummonCopyHand(int playerID, Card copy)
        {
            return _cardManager.SummonCopyHand(playerID, copy);
        }

        /// <summary>
        /// Creates a new card and sends it to the board
        /// </summary>
        public virtual Card SummonCard(int playerID, CardData cardData, VariantData variant, Slot slot, bool isSummoningCohort=false, string cohortUid = "", bool usingGameHelper=false)
        {
            return _cardManager.SummonCard(playerID, cardData, variant, slot, isSummoningCohort, cohortUid, usingGameHelper);
        }

        /// <summary>
        /// Creates a new card and sends it to hand
        /// </summary>
        public virtual Card SummonCardHand(int playerID, CardData cardData, VariantData variant, string cohortUid = "")
        {
            return _cardManager.SummonCardHand(playerID, cardData, variant, cohortUid);
        }

        /// <summary>
        /// Creates a new card and adds it to the deck
        /// </summary>
        public virtual Card SummonCardDeck(int playerID, CardData cardData, VariantData variant)
        {
            return _cardManager.SummonCardDeck(playerID, cardData, variant);
        }

        /// <summary>
        /// Transforms a card into another one
        /// </summary>
        public virtual Card TransformCard(Card card, CardData cardToTransformTo)
        {
            return _cardManager.TransformCard(card, cardToTransformTo);
        }

        /// <summary>
        /// Changes the owner of a card
        /// </summary>
        public virtual void ChangeOwner(Card card, Player newOwner)
        {
            _cardManager.ChangeOwner(card, newOwner);
        }
        
        public virtual void ChangeMovementScheme(Card card, MovementScheme newScheme)
        {
            _cardManager.ChangeMovementScheme(card, newScheme);
        }

        /// <summary>
        /// Resurrects a card from discard pile
        /// </summary>
        public virtual void ResurrectCard(Card card, Slot slot, int newOwnerId)
        {
            _cardManager.ResurrectCard(card, slot, newOwnerId);
        }

        /// <summary>
        /// Heals a card by reducing damage
        /// </summary>
        public virtual void HealCard(Card target, int value)
        {
            _cardManager.HealCard(target, value);
        }

        /// <summary>
        /// Damages a card without an attacker
        /// </summary>
        public virtual void DamageCard(Card target, int value, bool triggersDyingWish = true)
        {
            _cardManager.DamageCard(target, value, triggersDyingWish);
        }

        /// <summary>
        /// Damages a card from an attacker
        /// </summary>
        public virtual void DamageCard(Card attacker, Card target, int value, bool armorPenetrating = false, bool isRetaliation = false, bool triggersDyingWish = true)
        {
            _cardManager.DamageCard(attacker, target, value, armorPenetrating, isRetaliation, triggersDyingWish);
        }

        /// <summary>
        /// Directly kills a card with an attacker
        /// </summary>
        public virtual void KillCard(Card attacker, Card target, bool triggersDyingWish = true)
        {
            _cardManager.KillCard(attacker, target, triggersDyingWish);
        }

        /// <summary>
        /// Sends a card to discard
        /// </summary>
        public virtual void DiscardCard(Card card, bool triggersDyingWish = true)
        {
            _cardManager.DiscardCard(card, triggersDyingWish);
        }

        /// <summary>
        /// Shuffles a deck of cards
        /// </summary>
        public virtual void ShuffleDeck(List<Card> cards)
        {
            _cardManager.ShuffleDeck(cards);
        }

        #endregion

        #region Combat Operations

        /// <summary>
        /// Processes an attack from one card to a target slot
        /// </summary>
        public virtual void AttackTarget(Card attacker, Slot targetSlot, bool skipCost = false, bool rangedAttack = false, bool canAttackAlly = false)
        {
            _combatManager.AttackTarget(attacker, targetSlot, _cardManager, _boardLogic, skipCost, rangedAttack, canAttackAlly);
        }

        /// <summary>
        /// Redirects an attack to a new target
        /// </summary>
        public virtual void RedirectAttack(Card attacker, Card newTarget)
        {
            _combatManager.RedirectAttack(attacker, newTarget);
        }

        /// <summary>
        /// Requests the player to select a ranged attacker for a target slot
        /// </summary>
        public virtual void RequestRangeAttackerChoice(List<Card> possibleAttackers, Slot targetSlot)
        {
            _combatManager.RequestRangeAttackerChoice(possibleAttackers, targetSlot);
        }

        /// <summary>
        /// Selects a ranged attacker for an attack
        /// </summary>
        public virtual void SelectRangeAttacker(Card attacker)
        {
            _combatManager.SelectRangeAttacker(attacker);
        }

        /// <summary>
        /// Rolls a random value between min and max
        /// </summary>
        public virtual int RollRandomValue(int min, int max)
        {
            return _combatManager.RollRandomValue(min, max);
        }

        /// <summary>
        /// Rolls a random value using a dice with specified number of sides
        /// </summary>
        public int RollRandomValue(int dice)
        {
            return _combatManager.RollRandomValue(dice);
        }

        #endregion

        #region Ability and Selection

        /// <summary>
        /// Casts an ability from a card
        /// </summary>
        public virtual void CastAbility(Card castedCard, AbilityData ability)
        {
            if (_game.CanCastAbility(castedCard, ability))
            {
                castedCard.RemoveStatus(StatusType.Stealth);
                _abilityLogicSystem.TriggerCardAbility(ability, castedCard);
                _resolveQueue.ResolveAll();
            }
        }

        /// <summary>
        /// Adds ability cast to history
        /// </summary>
        public virtual void AddAbilityHistory(AbilityArgs args, List<ITargetable> targets)
        {
            int playerId = args.caster.playerID;
            List<Slot> slotTargets = new List<Slot>();
            if (targets != null)
            {
                foreach (var target in targets)
                {
                    if (target != null)
                    {
                        slotTargets.Add(target.GetSlot());
                    }
                }
            }
            
            _game.history.AddAbilityCastHistory(playerId, args.caster, args.ability, args.caster.slot, slotTargets);
        }

        /// <summary>
        /// Selects a mana type for payment
        /// </summary>
        public void SelectManaType(PlayerMana.ManaType manaType)
        {
            _abilityLogicSystem.ReceiveSelectManaType(manaType);
        }

        /// <summary>
        /// Selects a card as target
        /// </summary>
        public void SelectCard(Card target)
        {
            _abilityLogicSystem.SelectTarget(target);
        }

        /// <summary>
        /// Selects a player as target
        /// </summary>
        public void SelectPlayer(Player target)
        {
            _abilityLogicSystem.SelectTarget(target);
        }

        /// <summary>
        /// Selects a slot as target
        /// </summary>
        public void SelectSlot(Slot target)
        {
            _abilityLogicSystem.SelectTarget(target);
        }

        /// <summary>
        /// Selects a choice option
        /// </summary>
        public void SelectChoice(int choice)
        {
            _abilityLogicSystem.SelectChoice(choice);
        }

        /// <summary>
        /// Selects a card as caster
        /// </summary>
        public void SelectCaster(Card caster)
        {
            _abilityLogicSystem.SelectCaster(caster);
        }
        
        /// <summary>
        /// Skips the current selection
        /// </summary>
        public void SkipSelection()
        {
            _abilityLogicSystem.SkipSelection();
        }

        /// <summary>
        /// Cancels the current selection
        /// </summary>
        public void CancelSelection()
        {
            _abilityLogicSystem.CancelSelection();
        }

        /// <summary>
        /// Progress to the next step/phase
        /// </summary>
        public virtual void NextStep()
        {
            if (_game.selector != SelectorType.None)
            {
                _abilityLogicSystem.CancelSelection();
            }
            else if (_game.State == GameState.Play)
            {
                EndTurn();
            }
        }

        /// <summary>
        /// Handles the end of an ability
        /// </summary>
        protected virtual void OnAbilityEnd(AbilityArgs args)
        {
            // Add to played
            _game.abilityPlayed.Add(args.ability.id);
            
            _game.selector = SelectorType.None;
            _game.selectorAbilityID = "";
            _game.selectorCardUID = "";
            _game.selectorCasterUID = "";

            // Pay cost
            if (args.ability.trigger == AbilityTrigger.Activate ||
                args.ability.trigger == AbilityTrigger.None ||
                args.ability.trigger == AbilityTrigger.OnAfterMove)
            {
                args.caster.exhausted = args.caster.exhausted || args.ability.exhaust;
                EndTurnAutomaticallyWhenExhausted();
            }

            // Recalculate and clear
            _abilityLogicSystem.UpdateOngoingEffect();
            _cardManager.ValidateIfAnyBoardCardsAreDead();

            if (args.ability.trigger == AbilityTrigger.OnMoveOnSpecificSquare)
            {
                string cohortUid = args.caster.CohortUid;
                if (_game.GetBoardCardsOfCohort(cohortUid).Count == 0 && 
                    _game.initiativeManager.LastPieceTurnDied && 
                    _game.initiativeManager.LastDied.cohortUid == cohortUid)
                {
                    if (GameplayData.Get().InitiativeType != InitiativeManager.InitiativeType.AllPiecesEveryTurn)
                    {
                        _resolveQueue.AddCallback(EndTurn);
                    }
                }
            }
            
            CheckForWinner();
            _boardLogic.CalculatePiecesMove();
            
            _resolveQueue.ResolveAll();
            onAbilityEnd?.Invoke(args);
        }

        public void CheckForWinner()
        {
            int countAlive = 0;
            Player alive = null;
            foreach (Player player in _game.players)
            {
                if (!player.IsDead())
                {
                    alive = player;
                    countAlive++;
                }
            }

            if (countAlive == 0)
            {
                EndGame(-1); // Everyone is dead, Draw
            }
            else if (countAlive == 1)
            {
                EndGame(alive.playerID); // Player win
            }
        }

        /// <summary>
        /// Automatically ends turn when all pieces of a cohort are exhausted
        /// </summary>
        public void EndTurnAutomaticallyWhenExhausted()
        {
            if (GameplayData.Get().InitiativeType == InitiativeManager.InitiativeType.AllPiecesEveryTurn)
            {
                return;
            }
            
            if (_game.IsAllCardTurnExhausted())
            {
                _resolveQueue.AddCallback(EndTurn);
                return;
            }
        }

        /// <summary>
        /// Adds mana of a specified type
        /// </summary>
        public void AddMana(PlayerMana.ManaType manaType)
        {
            _game.selector = SelectorType.None;
            _game.GetPlayer(_game.selectorPlayer).playerMana.AddMana(manaType);
            _game.GetCard(_game.selectorCasterUID).exhausted = true;
        }

        /// <summary>
        /// Updates all ongoing effects in the game
        /// </summary>
        public virtual void UpdateOngoingEffects()
        {
            _abilityLogicSystem.UpdateOngoingEffect();
        }

        #endregion
    }
}