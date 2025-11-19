using System.Collections.Generic;
using Monarchs.Ability;
using Monarchs.Logic.AbilitySystem;
using TcgEngine;
using UnityEngine;
using UnityEngine.Events;

namespace Monarchs.Logic
{
    /// <summary>
    /// Manages card creation, manipulation, and lifecycle
    /// </summary>
    public class CardManager
    {
        private GameLogic _gameLogic;
        private Game _game;
        private readonly bool _isInstant;
        private readonly ResolveQueue _resolveQueue;
        private readonly System.Random _random = new System.Random();
        private AbilityLogicSystem _abilityLogicSystem => _gameLogic?._abilityLogicSystem;

        public CardManager(Game game, bool isInstant = false)
        {
            _game = game;
            _isInstant = isInstant;
            _resolveQueue = new ResolveQueue(game, isInstant);
        }

        public void Initialize(GameLogic gameLogic)
        {
            _gameLogic = gameLogic;
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

        #region Card Creation and Setup
        
        /// <summary>
        /// Sets up a player's deck using a predefined deck data
        /// </summary>
        public virtual void SetPlayerDeck(int playerID, DeckData deck)
        {
            Player player = _game.GetPlayer(playerID);
            player.cards_all.Clear();
            player.cards_deck.Clear();
            player.deck = deck.id;

            CardData monarchCardData = deck.monarch;

            if (monarchCardData != null)
            {
                if (_game.firstPlayer == playerID)
                {
                    AddCardBeforeGame(monarchCardData, true, GameplayData.Get().whiteMonarchStartSlot);
                }
                else
                {
                    AddCardBeforeGame(monarchCardData, false, GameplayData.Get().blackMonarchStartSlot);
                }
            }

            VariantData variant = VariantData.GetDefault();

            foreach (CardData cardData in deck.cards)
            {
                if (cardData != null)
                {
                    Card card = Card.Create(cardData, variant, player.playerID);
                    player.cards_all[card.uid] = card;
                    player.cards_deck.Add(card);

                    if (cardData.differentSpellPerMovement)
                    {
                        card.CreateChildCards();
                        foreach (var childCard in card.childCard.Values)
                        {
                            childCard.parentCard = card;
                            player.cards_all[childCard.uid] = childCard;
                            player.cards_temp.Add(childCard);
                        }
                    }
                }
            }
            
            // Add champion
            Card champion = Card.Create(deck.champion, variant, player.playerID);
            player.cards_all[champion.uid] = champion;
            player.cards_hand.Add(champion);

            DeckPuzzleData puzzle = deck as DeckPuzzleData;

            // Board cards
            if (puzzle != null)
            {
                foreach (DeckCardSlot deckCardSlot in puzzle.board_cards)
                {
                    Card card = Card.Create(deckCardSlot.card, variant, player.playerID);
                    card.slot = new Slot(deckCardSlot.slot);
                    player.cards_all[card.uid] = card;
                    player.cards_board.Add(card);
                }
            }

            // Shuffle deck
            if (puzzle == null || !puzzle.dont_shuffle_deck)
                ShuffleDeck(player.cards_deck);
            
            _resolveQueue.ResolveAll(0f);
        }

        /// <summary>
        /// Sets up a player's deck using a custom deck from save file or database
        /// </summary>
        public virtual void SetPlayerDeck(int playerID, UserDeckData deck)
        {
            SetPlayerDeck(playerID, deck.tid, deck.hero, deck.cards);
        }

        /// <summary>
        /// Sets up a player's deck using card IDs
        /// </summary>
        public virtual void SetPlayerDeck(int playerID, string deckID, string hero, string[] cards)
        {
            Player player = _game.GetPlayer(playerID);

            player.cards_all.Clear();
            player.cards_deck.Clear();
            player.deck = deckID;

            CardData heroCardData = UserCardData.GetCardData(hero);
            
            if (heroCardData != null && !string.IsNullOrEmpty(hero))
            {
                if (_game.firstPlayer == playerID)
                {
                    AddCardBeforeGame(heroCardData, true, Slot.Get(3, 2));
                }
                else
                {
                    AddCardBeforeGame(heroCardData, false, Slot.Get(4, 5));
                }
            }

            foreach (string tid in cards)
            {
                CardData cardData = UserCardData.GetCardData(tid);
                VariantData variant = UserCardData.GetCardVariant(tid);

                Card card = Card.Create(cardData, variant, player.playerID);
                player.cards_all[card.uid] = card;
                player.cards_deck.Add(card);
            }

            // Shuffle deck
            ShuffleDeck(player.cards_deck);
            _resolveQueue.ResolveAll(0f);
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
        
        #endregion

        #region Card Draw and Deck Operations
        
        /// <summary>
        /// Shuffles a deck of cards
        /// </summary>
        public virtual void ShuffleDeck(List<Card> cards)
        {
            for (int i = 0; i < cards.Count; i++)
            {
                Card temp = cards[i];
                int randomIndex = _random.Next(i, cards.Count);
                cards[i] = cards[randomIndex];
                cards[randomIndex] = temp;
            }
        }

        /// <summary>
        /// Draws cards from a player's deck
        /// </summary>
        public virtual void DrawCard(int playerID, int nb = 1)
        {
            Player player = _game.GetPlayer(playerID);
            for (int i = 0; i < nb; i++)
            {
                if (player.cards_deck.Count > 0 && player.cards_hand.Count < GameplayData.Get().cards_max)
                {
                    Card card = player.cards_deck[0];
                    player.cards_deck.RemoveAt(0);
                    player.cards_hand.Add(card);
                }
            }

            _gameLogic.onCardDrawn?.Invoke(nb);
        }

        /// <summary>
        /// Puts cards from deck into discard
        /// </summary>
        public virtual void DrawDiscardCard(int playerID, int nb = 1)
        {
            Player player = _game.GetPlayer(playerID);
            for (int i = 0; i < nb; i++)
            {
                if (player.cards_deck.Count > 0)
                {
                    Card card = player.cards_deck[0];
                    player.cards_deck.RemoveAt(0);
                    player.cards_deck.Add(card);
                }
            }
        }
        
        #endregion

        #region Card Summoning and Creation
        
        /// <summary>
        /// Summons a card to the board
        /// </summary>
        public virtual void SummonPiece(Card card, Slot slot)
        {
            Player player = _game.GetPlayer(card.playerID);
            
            // Play card
            player.RemoveCardFromAllGroups(card);
            card.Clear();

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

            // Update ongoing effects
            _game.lastPlayed = card;
            _abilityLogicSystem.UpdateOngoingEffect();

            // Trigger abilities
            _abilityLogicSystem.TriggerTrapsOnSpecificSlot(AbilityTrigger.OnMoveOnSpecificSquare, card, slot); // After playing card
            _abilityLogicSystem.TriggerCardAbilityType(AbilityTrigger.OnPlay, card, card);
            _abilityLogicSystem.TriggerAllAbilityWithTriggerType(AbilityTrigger.OnPlayOther, card);

            _gameLogic.onCardSummoned?.Invoke(card, slot);
            _resolveQueue.ResolveAll(0.3f);
        }
        
        /// <summary>
        /// Creates a copy of an existing card and summons it
        /// </summary>
        public virtual Card SummonCopy(int playerID, Card copy, Slot slot)
        {
            CardData cardData = copy.CardData;
            return SummonCard(playerID, cardData, copy.VariantData, slot);
        }

        /// <summary>
        /// Creates a copy of an existing card in hand
        /// </summary>
        public virtual Card SummonCopyHand(int playerID, Card copy)
        {
            CardData cardData = copy.CardData;
            return SummonCardHand(playerID, cardData, copy.VariantData);
        }

        /// <summary>
        /// Creates a new card and sends it to the board
        /// </summary>
        public virtual Card SummonCard(int playerID, CardData cardData, VariantData variant, Slot slot, bool isSummoningCohort=false, string cohortUid = "", bool usingGameHelper=false)
        {
            if (!slot.IsValid())
                return null;

            if (_game.GetSlotCard(slot) != null)
                return null;
            
            Card card = SummonCardHand(playerID, cardData, variant, cohortUid);
            if (usingGameHelper)
            {
                card.cohortSummon = true;
            }
            
            
            if (cohortUid != "")
            {
                card.wasPlayedThisTurn = true;
            }

            if (isSummoningCohort)
            {
                card.cohortSummon = true;
            }
            
            SummonPiece(card, slot);

            return card;
        }

        /// <summary>
        /// Creates a new card and sends it to hand
        /// </summary>
        public virtual Card SummonCardHand(int playerID, CardData cardData, VariantData variant, string cohortUid = "")
        {
            string uid = "s_" + GameTool.GenerateRandomID();
            if (cohortUid == "")
            {
                cohortUid = "s_" + GameTool.GenerateRandomID();
            }
            
            Player player = _game.GetPlayer(playerID);
            Card card = Card.Create(cardData, variant, player.playerID, uid, cohortUid);
            player.cards_all[card.uid] = card;
            player.cards_hand.Add(card);
            _resolveQueue.ResolveAll(0f);
            
            return card;
        }

        /// <summary>
        /// Creates a new card and adds it to the deck
        /// </summary>
        public virtual Card SummonCardDeck(int playerID, CardData cardData, VariantData variant)
        {
            string uid = "s_" + GameTool.GenerateRandomID();
            Player player = _game.GetPlayer(playerID);
            Card card = Card.Create(cardData, variant, player.playerID, uid);
            player.cards_all[card.uid] = card;
            player.cards_deck.Add(card);
            ShuffleDeck(player.cards_deck);
            _resolveQueue.ResolveAll(0f);
            return card;
        }
        
        #endregion

        #region Card Transformation and Owner Change
        
        /// <summary>
        /// Transforms a card into another one
        /// </summary>
        public virtual Card TransformCard(Card card, CardData cardToTransformTo)
        {
            card.SetCard(cardToTransformTo, card.VariantData);
            _gameLogic.onCardTransformed?.Invoke(card);
            return card;
        }

        /// <summary>
        /// Changes the owner of a card
        /// </summary>
        public virtual void ChangeOwner(Card card, Player newOwner)
        {
            if (card.playerID != newOwner.playerID)
            {
                List<Card> cohortCards = _game.GetBoardCardsOfCohort(card.CohortUid);
                if (cohortCards.Count > 1)
                {
                    BreakCohortAfterOwnerChange(card, cohortCards);
                }
                
                Player oldOwner = _game.GetPlayer(card.playerID);
                oldOwner.RemoveCardFromAllGroups(card);
                oldOwner.cards_all.Remove(card.uid);
                newOwner.cards_all[card.uid] = card;
                newOwner.cards_board.Add(card);
                card.playerID = newOwner.playerID;
                
                _gameLogic.onCardTransformed?.Invoke(card);
            }
        }

        /// <summary>
        /// Breaks a cohort when a card changes owner
        /// </summary>
        public virtual void BreakCohortAfterOwnerChange(Card card, List<Card> cohortCards)
        {
            if (_game.initiativeManager.IsCardInInitiativeList(card))
            {
                _game.initiativeManager.RemoveCard(card, _game, true);
                Card otherCard = cohortCards.Find(c => c.uid != card.uid);
                _game.initiativeManager.AddCard(otherCard, true);
                card.GenerateNewCohortUID();
                _game.initiativeManager.AddCard(card, true);
            }
            else
            {
                card.GenerateNewCohortUID();
                _game.initiativeManager.AddCard(card, true);
            }
        }

        /// <summary>
        /// Resurrects a card from discard pile
        /// </summary>
        public virtual void ResurrectCard(Card card, Slot slot, int newOwnerId)
        {
            Player player = _game.GetPlayer(card.playerID);
            player.cards_discard.Remove(card);
            player.cards_board.Add(card);
            card.slot = slot;
            card.Clear();
            card.exhausted = true;
            card.GenerateNewCohortUID();
            
            if (card.playerID != newOwnerId)
            {
                ChangeOwner(card, _game.GetPlayer(newOwnerId));
            }
            
            _game.initiativeManager.AddCard(card);
            _gameLogic.onCardSummoned?.Invoke(card, slot);
        }
        
        #endregion

        #region Card Damage and Health
        
        /// <summary>
        /// Heals a card by reducing damage
        /// </summary>
        public virtual void HealCard(Card target, int value)
        {
            if (target == null)
                return;

            if (target.HasStatus(StatusType.Invincibility))
                return;

            target.damage -= value;
            target.damage = Mathf.Max(target.damage, 0);
        }

        /// <summary>
        /// Damages a card without an attacker
        /// </summary>
        public virtual void DamageCard(Card target, int value, bool triggersDyingWish = true)
        {
            if (target == null)
                return;

            if (target.HasStatus(StatusType.Invincibility))
                return; // Invincible

            if (target.HasStatus(StatusType.SpellImmunity))
                return; // Spell immunity

            // Impending doom
            if (target.HasStatus(StatusType.ImpendingDoom))
            {
                value *= 2;
            }
            
            target.damage += value;

            if (target.GetHP() <= 0)
                DiscardCard(target, triggersDyingWish);
            else
            {
                _abilityLogicSystem.TriggerCardAbilityType(AbilityTrigger.Enrage, target, target);
            }
        }

        /// <summary>
        /// Damages a card from an attacker
        /// </summary>
        public virtual void DamageCard(Card attacker, Card target, int value, bool armorPenetrating = false, bool isRetaliation = false, bool triggersDyingWish = true)
        {
            if (attacker == null || target == null)
                return;

            if (target.HasStatus(StatusType.Invincibility))
                return; // Invincible

            if (target.HasStatus(StatusType.SpellImmunity) && attacker.CardData.cardType != CardType.Character)
                return; // Spell immunity

            // Shell
            bool doubleLife = target.HasStatus(StatusType.Shell);
            if (doubleLife)
            {
                target.RemoveStatus(StatusType.Shell);
                return;
            }

            // Impending doom
            if (target.HasStatus(StatusType.ImpendingDoom))
            {
                value *= 2;
            }
            
            // Armor
            int tempArmor = 0;
            if (target.HasStatus(StatusType.Armor))
            {
                tempArmor = target.GetStatusValue(StatusType.Armor);
            }

            int permanentArmor = 0;
            if (target.HasStat("armor"))
            {
                permanentArmor = target.GetStatValue("armor");
            }
            
            if (!attacker.HasStat("armor_penetration") && !armorPenetrating && value != 0)
                value = Mathf.Max(value - permanentArmor - tempArmor, 1);
            
            target.damage += value;

            // Remove sleep on damage
            target.RemoveStatus(StatusType.Sleep);

            // Deathstrike
            if (value > 0 && attacker.HasStatus(StatusType.DeathStrike) &&
                target.CardData.cardType == CardType.Character && !target.IsMonarch() && !isRetaliation)
            {
                KillCard(attacker, target, triggersDyingWish);
                return;
            }

            // Kill on 0 hp
            if (target.GetHP() <= 0)
            {
                KillCard(attacker, target, triggersDyingWish);
                return;
            }
                
            _abilityLogicSystem.TriggerCardAbilityType(AbilityTrigger.Enrage, target, target);
        }

        /// <summary>
        /// Directly kills a card with an attacker
        /// </summary>
        public virtual void KillCard(Card attacker, Card target, bool triggersDyingWish = true)
        {
            if (attacker == null || target == null)
                return;

            if (!_game.IsOnBoard(target))
                return;

            if (target.HasStatus(StatusType.Invincibility))
                return;

            Player playerAttacker = _game.GetPlayer(attacker.playerID);
            if (attacker.playerID != target.playerID)
                playerAttacker.kill_count++;

            _game.lastKilled = target;
            DiscardCard(target, triggersDyingWish);

            _abilityLogicSystem.TriggerCardAbilityType(AbilityTrigger.OnKill, attacker, target);
        }

        /// <summary>
        /// Sends a card to discard
        /// </summary>
        public virtual void DiscardCard(Card card, bool triggersDyingWish = true)
        {
            if (card == null)
                return;

            if (_game.IsInDiscard(card))
                return; // Already discarded

            

            Player player = _game.GetPlayer(card.playerID);

            if (card.CardData.cardType == CardType.Trap)
            {
                player.RemoveCardFromAllGroups(card);
                player.cards_discard.Add(card);
                _gameLogic.onCardDiscarded?.Invoke(card);
                return;
            }

            bool wasOnBoard = _game.IsOnBoard(card);
            _game.intimidateManager.RemoveCardWithIntimidation(card);

            // Remove card from board and add to discard
            player.RemoveCardFromAllGroups(card);
            
            player.cards_discard.Add(card);
            
            if (wasOnBoard)
            {
                card.numberOfCohortUnitDied++; // We add it since it won't be included in the board cards of cohort
                foreach (var boardCohort in _game.GetBoardCardsOfCohort(card.CohortUid))
                {
                    boardCohort.numberOfCohortUnitDied++;
                }
                
                // Trigger on death abilities
                _game.initiativeManager.RemoveCard(card, _game);
                card.wasOnBoard = true;
                card.roundNumberWhenItDied = _game.roundCount;
                card.turnNumberWhenItDied = _game.turnCount;
                
                _gameLogic.onCardDiscarded?.Invoke(card);
                if (triggersDyingWish && card.CardData.cardType == CardType.Character)
                {
                    _abilityLogicSystem.TriggerCardAbilityType(AbilityTrigger.OnDeath, card, card);
                    _abilityLogicSystem.TriggerAllAbilityWithTriggerType(AbilityTrigger.OnDeathOther, card);
                }
            }
            else
            {
                _gameLogic.onCardDiscarded?.Invoke(card);
            }
        
        }
        
        /// <summary>
        /// Checks if any cards on the board have 0 or less health and discards them
        /// </summary>
        public void ValidateIfAnyBoardCardsAreDead()
        {
            foreach (var player in _game.players)
            {
                for (int i = player.cards_board.Count - 1; i >= 0; i--)
                {
                    Card card = player.cards_board[i];
                    if (card.GetHP() <= 0)
                        DiscardCard(card);
                }
            }
        }
        
        #endregion
    }
}