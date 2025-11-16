using System;
using System.Collections.Generic;
using System.Linq;
using Monarchs.Abilities;
using Monarchs.Ability;
using Monarchs.Ability.Target;
using Monarchs.Initiative;
using Monarchs.Logic.AbilitySystem;
using TcgEngine;
using UnityEngine;

namespace Monarchs.Logic
{
    //Contains all gameplay state data that is sync across network

    [System.Serializable]
    public class Game
    {
        public int nbPlayers = 2;
        public GameSettings settings;
        public string gameUID;

        //Game state
        public int firstPlayer = 0;
        public int CurrentPlayer
        {
            get
            {
                if (GameplayData.Get().InitiativeType == InitiativeManager.InitiativeType.Initiative)
                {
                    if (GetCurrentCardTurn().Count == 0)
                    {
                        return -1;
                    }
                    return GetCurrentCardTurn()[0].playerID;
                }

                return _currentPlayer;
            }
            set
            {
                _currentPlayer = value;
            }
        }

        public int turnCount = 0;
        public int roundCount = 0;
        public float turnTimer = 0f;
        public float mulliganTimer = 0f;
        public int winnerPlayer = -1;
        private int _currentPlayer = 0;

        public GameState State => _state;
        public GameState PreviousState => _previousState;
        private GameState _state = GameState.Connecting;
        private GameState _previousState = GameState.Connecting;

        //Players
        public Player[] players;
        public Dictionary<string, PieceMoves> pieceMovesList;

        //Selector
        public SelectorType selector { get; 
            set; } = SelectorType.None;
        public int selectorPlayer = 0;
        public string selectorAbilityID = "";
        public string lastAbilityDone = "";
        public string lastAbilityCasterUID = "";
        public string selectorCardUID = "";
        public string selectorCasterUID = "";
        public string selectorCastedCardUID = "";
        public PlayerMana.ManaType selectorManaType = PlayerMana.ManaType.None;
        public List<ITargetable> selectorTargets = new List<ITargetable>();
        public Slot selectorLastTargetSlot = Slot.None;
        public string[] selectorPotentialCasters;

        //Other values
        public Card lastPlayed;
        public Card lastAttackedCard;
        public Slot lastSlotAttacked;
        public ITargetable lastTarget;
        public ITargetable savedTargetForAbility;
        public Card lastKilled;
        public Card abilityTriggerer;
        public Slot lastMoveDestination;
        public int rolledValue;

        public List<SlotStatus> slotStatusList = new List<SlotStatus>();

        public string currentCohortUid;

        public InitiativeManager initiativeManager;
        public Intimidate intimidateManager;
        
        public GameHistory history = new GameHistory();

        //Other arrays 
        public HashSet<string> abilityPlayed = new HashSet<string>();
        public HashSet<string> cardsAttacked = new HashSet<string>();

        public Game()
        {
            this.initiativeManager = new InitiativeManager();
            intimidateManager = new Intimidate();
        }

        public bool HasAIPlayer()
        {
            foreach (var player in players)
            {
                if (player.is_ai)
                {
                    return true;
                }
            }

            return false;
        }
        
        public void SetState(GameState newState)
        {
            if (State == newState)
            {
                return; //No change
            }

            _previousState = State;
            _state = newState;
        }
        
        public Game(string uid, int nb_players)
        {
            this.gameUID = uid;
            this.nbPlayers = nb_players;
            players = new Player[nb_players];
            for (int i = 0; i < nb_players; i++)
                players[i] = new Player(i);
            settings = GameSettings.Default;
            this.initiativeManager = new InitiativeManager();
            intimidateManager = new Intimidate();
        }

        public virtual bool AreAllPlayersReady()
        {
            int ready = 0;
            foreach (Player player in players)
            {
                if (player.IsReady())
                    ready++;
            }
            return ready >= nbPlayers;
        }

        public virtual bool AreAllPlayersConnected()
        {
            int ready = 0;
            foreach (Player player in players)
            {
                if (player.IsConnected())
                    ready++;
            }
            return ready >= nbPlayers;
        }

        public virtual List<Card> GetCurrentCardTurn()
        {
            if (GameplayData.Get().InitiativeType == InitiativeManager.InitiativeType.Initiative)
            {
                return initiativeManager.GetCurrentCardTurn(this);
            }
            else if (GameplayData.Get().InitiativeType == InitiativeManager.InitiativeType.AllPiecesEveryTurn)
            {
                return players[CurrentPlayer].cards_board;
            }

            return new List<Card>();
        }
        
        public virtual bool IsCardTurn(Card card)
        {
            // Check if any card in the current turn has the same UID as the provided card
            return GetCurrentCardTurn().Any(c => c.uid == card.uid);
        }
        
        public virtual bool IsAllCardTurnExhausted()
        {
            foreach (var card in GetCurrentCardTurn())
            {
                if (!card.exhausted && !card.HasStatus(StatusType.Stunned))
                {
                    return false;
                }
            }

            return true;
        }

        //Check if its player's turn
        public virtual bool IsPlayerTurn(Player player)
        {
            return IsPlayerActionTurn(player) || IsPlayerSelectorTurn(player);
        }

        public virtual bool IsPlayerActionTurn(Player player)
        {
            if (GetCurrentCardTurn().Count == 0)
            {
                return false;
            }
            return player != null && GetCurrentCardTurn()[0].playerID == player.playerID
                && State == GameState.Play && selector == SelectorType.None;
        }

        public virtual bool IsPlayerSelectorTurn(Player player)
        {
            if (player != null && selectorPlayer == player.playerID
                               && State == GameState.Play)
            {
                if (selector != SelectorType.None)
                {
                    return true;
                }
                
                Card caster = GetCard(selectorCasterUID);
                if (selectorAbilityID == "activate_choose_mana" && caster.IsMonarch())
                {
                    caster.exhausted = false;
                    return true;
                }
            }

            return false;
        }

        public List<Vector2S> GetLegalMeleeAttacks(Card attacker)
        {
            List<Vector2S> legalMeleeAttacks = new List<Vector2S>();
            MovementScheme movementScheme = attacker.CardData.MovementScheme;
            return movementScheme.GetLegalMeleeAttack(attacker.GetCoordinates(), 
                attacker.GetMoveRange(), 
                attacker.CanJump(), 
                attacker.playerID, 
                this);
        }


        public List<Vector2S> GetLegalRangedAttacks(List<Card> attackers = null, bool showEmptySquare = false)
        {
            if (attackers == null)
            {
                attackers = GetCurrentCardTurn();
            }

            List<Vector2S> legalRangedAttack = new List<Vector2S>();

            foreach (var attacker in attackers)
            {
                if (!attacker.CanAttack() && !showEmptySquare) // showEmptySquare is used for highlights only
                {
                    continue;
                }
                    
                MovementScheme movementScheme = attacker.CardData.MovementScheme;
                bool canAttackGround = attacker.HasTrait("can_range_attack_ground") || showEmptySquare;
                bool indirectFire = attacker.HasTrait("indirect_fire");
            
                legalRangedAttack.AddRange(movementScheme.GetLegalRangedAttack(attacker.GetCoordinates(),
                    attacker.CardData.minAttackRange, attacker.GetMaxAttackRange(),
                    attacker.playerID, this, canAttackGround, indirectFire, canAttackGround));
            }

            return legalRangedAttack;
        }

        public bool CurrentTurnCanRangeAttackTarget(Card target)
        {
            return GetLegalRangedAttacks().Contains(target.GetCoordinates());
        }



        public virtual bool CanPlayCardThisTurn(Card card, bool skipCost = false)
        {
            if (card == null)
            {
                return false;
            }

            var possibleCastersForCard = GetPossibleCastersForCard(card);

            if (possibleCastersForCard.Count == 0)
            {
                return false;
            }
            
            // if (possibleCastersForCard[0].playerID != card.playerID)
            // {
            //     return false;
            // }
            //
            // int exaustedCount = 0;
            // foreach (var currentCard in possibleCastersForCard)
            // {
            //     if ((currentCard.exhausted && !GameplayData.Get().canPlayCardAfterMove) 
            //         || currentCard.HasStatus(StatusType.Stunned) || currentCard.playedCardThisRound || currentCard.HasTrait("undisciplined"))
            //     {
            //         exaustedCount++;
            //     }
            // }
            //
            // if (exaustedCount == possibleCastersForCard.Count)
            // {
            //     return false;
            // }
            //
            // bool boundSpell = possibleCastersForCard[0].CardData.boundSpells.Contains(card.CardData);
            //
            // if (!GameplayData.Get().EveryPieceCanCastInterventions && (!card.CardData.possibleCasters.HasFlag(possibleCastersForCard[0].GetPieceType()) && !boundSpell) && !skipCost) //Current piece can cast
            // {
            //     return false;
            // }

            Player player = GetPlayer(card.playerID);
            // if (!skipCost && !player.CanPayMana(card, (possibleCastersForCard[0].GetPieceType() == PieceType.Monarch || boundSpell)))
            //     return false; //Cant pay mana
            
            
            if (card.playerID != CurrentPlayer)
                return false; //Not this player's turn
            
            if (!player.HasCard(player.cards_hand, card) && !player.HasCard(player.cards_hand, card.parentCard))
                return false; // Card not in hand

            return true;
        }

        public virtual bool CanPlayCardOnAnySlotOnTheBoard(Card card, bool skip_cost = false)
        {
            foreach (var slot in Slot.GetAll())
            {
                if (CanPlayCardOnSlot(card, slot, skip_cost))
                {
                    return true;
                }
            }

            return false;
        }
        
        public List<Slot> GetLegalSlotsToPlayCard(Card card)
        {
            List<Slot> legalSlots = new List<Slot>();
            foreach (var slot in Slot.GetAll())
            {
                if (CanPlayCardOnSlot(card, slot))
                {
                    legalSlots.Add(slot);
                }
            }

            return legalSlots;
        }
        
        //Check if a card is allowed to be played on slot
        public virtual bool CanPlayCardOnSlot(Card card, Slot slot, bool skipCost = false, bool validateInput=false)
        {
            List<Card> currentCardsTurn = GetCurrentCardTurn();
            if (currentCardsTurn.Count == 0)
            {
                return false;
            }

            if (card == null)
            {
                return false;
            }
            
            if (!CanPlayCardThisTurn(card, skipCost))
            {
                Debug.Log($"Cannot play card {card.uid} this turn");
                return false;
            }

            if (card.CardData.IsBoardCard())
            {
                if (!CardCanSpawnOnSlot(card, slot, skipCost))
                {
                    Debug.Log("Card can't spawn on slot");
                    return false;
                }
            }

            if (card.CardData.IsRequireTarget())
            {
                return IsPlayTargetValid(card, slot, false, validateInput); //Check play target on slot
            }
            return true;
        }

        public virtual bool CardCanSpawnOnSlot(Card card, Slot slot, bool skipCost)
        {
            if (!slot.IsValid() || IsCardOnSlot(slot))
                return false;   //Slot already occupied

            if (card.OwnedByFirstPlayer(this) && !DeployPositionOptions.GetDeployPosition(card.deployChoice, this).PositionsWhite.Contains(slot.GetCoordinate()))
            {
                return false;
            }
            
            if (!card.OwnedByFirstPlayer(this) && !DeployPositionOptions.GetDeployPosition(card.deployChoice, this).PositionsBlack.Contains(slot.GetCoordinate()))
            {
                return false;
            }
            return true;
        }

        public Vector2S[] GetCardLegalMove(Card card)
        {
            foreach (var pieceMove in pieceMovesList?.Keys)
            {
                if (pieceMove == card?.uid)
                {
                    return pieceMovesList[pieceMove].possibleMoves;
                }
            }        

            return null;
        }

        public bool SlotIsInCardLegalMove(Card card, Slot slot)
        {
            return GetCardLegalMove(card).Contains(new Vector2S(slot.x, slot.y));
        }
        
        public bool SlotIsInCardRangedAttack(Card card, Slot slot)
        {
            return GetLegalRangedAttacks().Contains(new Vector2S(slot.x, slot.y));
        }
        
        public List<Card> GetCardsOnCoordinates(List<Vector2S> coordinates)
        {
            List<Card> cards = new List<Card>();
            foreach (var coordinate in coordinates)
            {
                Slot slot = Slot.Get(coordinate);
                Card card = GetSlotCard(slot);
                if (card != null)
                {
                    cards.Add(card);
                }
            }

            return cards;
        }

        //Check if a card is allowed to move to slot
        public virtual bool CanMoveCard(Card card, Slot slot, bool skip_cost = false, bool isMeleeAttack=false, bool canAttackAlly = false)
        {
            if (!slot.IsValid())
                return false;
            
            if (!CardCanMoveThisTurn(card, skip_cost, isMeleeAttack))
                return false; //Card cant move this turn    
            
            if (card.slot == slot)
                return false; //Cant move to same slot

            Vector2S[] legalMoves;
            if (canAttackAlly)
            {
                legalMoves = card.GetLegalMoves(this, canAttackAlly);
            }
            else
            {
                legalMoves = GetCardLegalMove(card);
            }
            
            
            if (legalMoves.Contains(slot.GetCoordinate()))
            {
                return true;
            }

            return false;
        }
        
        public virtual bool CardCanMoveThisTurn(Card card, bool skip_cost = false, bool isMeleeAttack=false)
        {
            if (card == null)
            {
                return false;
            }

            if (!IsCardTurn(card))
            {
                return false;
            }

            if (!card.CanMove(skip_cost, isMeleeAttack))
            {
                return false;
            }

            return true;
        }

        public virtual Slot GetModifiedDestination(Card card, Slot initialSlot, Slot destinationSlot)
        {
            try
            {
                Vector2S destinationCoordinate =
                    pieceMovesList[card.uid].realDestination[destinationSlot.GetCoordinate()];
                return Slot.Get(destinationCoordinate.x, destinationCoordinate.y);
            }
            catch (Exception e)
            {
                return destinationSlot;
            }
        }
        
        public int SecondPlayerId()
        {
            return firstPlayer == 0 ? 1 : 0;
        }

        //Check if a card is allowed to attack another one
        public virtual bool CanAttackTarget(Card attacker, Card target, bool skipCost = false, bool canAttackAlly = false)
        {
            if (target == null || attacker == null)
            {
                return false;
            }
            bool attackerDontNeedToMove = attacker.slot.IsInDistance(target.slot, 1);
            if (!CanMoveCard(attacker, target.slot, skipCost, attackerDontNeedToMove, canAttackAlly))
            {
                return false;
            }

            if (attacker.HasTrait("no_melee_attack"))
            {
                return false;
            }
            
            if (attacker.HasStatus(StatusType.Stunned))
            {
                return false;
            }
            
            if (target.HasTrait("fear") && target.GetHP() >= attacker.GetHP())
            {
                return false;
            }
            
            if (attacker == null || target == null)
                return false;

            if (!attacker.CanAttack(skipCost))
                return false; //Card cant attack

            if (attacker.playerID == target.playerID && !canAttackAlly)
                return false; //Cant attack same player

            if (!IsOnBoard(attacker) || !IsOnBoard(target))
                return false; //Cards not on board

            if (!attacker.CardData.IsCharacter() || !target.CardData.IsBoardCard())
                return false; //Only character can attack

            if (target.HasStatus(StatusType.Stealth))
                return false; //Stealth cant be attacked

            if (target.HasStatus(StatusType.Protected) && !attacker.HasStatus(StatusType.Flying))
                return false; //Protected by adjacent card

            return true;
        }

        public virtual bool CanRangeAttackTarget(Card attacker, Card target, bool skipCost = false, bool canAttackAlly = false)
        {
            return CanRangeAttackTarget(attacker, target.slot, skipCost);
        }

        public virtual bool CanRangeAttackTarget(Card attacker, Slot targetSlot, bool skipCost = false, bool canAttackAlly = false)
        {
            bool canAttackGround = attacker.HasTrait("can_range_attack_ground");
            Card target = GetSlotCard(targetSlot);
            
            if (!IsCardTurn(attacker))
                return false;
            
            if (attacker.HasStatus(StatusType.Stunned))
            {
                return false;
            }

            if (attacker == null || (target == null && !canAttackGround))
                return false;

            if (!attacker.CanAttack(skipCost))
                return false; //Card cant attack

            if (!IsOnBoard(attacker))
                return false; //Cards not on board

            if (target != null)
            {
                if (!IsOnBoard(target))
                {
                    return false;
                }
                
                if (attacker.playerID == target.playerID && !canAttackGround && !canAttackAlly)
                    return false; //Cant attack same player, unless it can target ground
                
                if (target.HasStatus(StatusType.Stealth))
                    return false; //Stealth cant be attacked

                if (target.HasStatus(StatusType.Protected))
                    return false; //Protected by adjacent card
                
                if (!target.CardData.IsBoardCard())
                    return false; //Only character can attack
            }

            if (!attacker.CardData.IsCharacter())
                return false; //Only character can attack

            

            if (attacker.GetMaxAttackRange() > 0)
            {
                List<Vector2S> legalMoves;
                bool indirectFire = attacker.HasTrait("indirect_fire");

                legalMoves = attacker.GetCurrentMovementScheme().GetLegalRangedAttack(attacker.slot.GetCoordinate(), 
                    attacker.CardData.minAttackRange, attacker.GetMaxAttackRange(), attacker.playerID, this, canAttackGround, indirectFire, (canAttackGround || canAttackAlly));

                if (!legalMoves.Contains(targetSlot.GetCoordinate()))
                {
                    return false;
                }
            }
            else
            {
                return false;
            }
            
            return true;
        }
        
        public virtual bool CanCastAbility(Card card, AbilityData ability)
        {
            if (ability == null || card == null)
                return false; //This card cant cast

            if (GameplayData.Get().InitiativeType == InitiativeManager.InitiativeType.AllPiecesEveryTurn)
            {
                if (card.exhausted && !ability.ignoreExhaustWhenCasting)
                {
                    return false;
                }
            }
            else
            {
                if (card.AreAllPiecesOfCohortExhausted(this) && !ability.ignoreExhaustWhenCasting)
                {
                    return false;
                }
            }
            
            if (ability.trigger != AbilityTrigger.Activate)
                return false; //Not an activated ability

            Player player = GetPlayer(card.playerID);
            if (!player.CanPayAbility(card, ability))
                return false; //Cant pay for ability

            var args = new AbilityArgs() {ability = ability, caster = card};
            if (!ability.AreTriggerConditionsMet(this, args))
                return false; //Conditions not met

            return true;
        }

        //Check if Player play target is valid, play target is the target when a spell requires to drag directly onto another card
        public virtual bool IsPlayTargetValid(Card castedCard, Player target, bool ai_check = false)
        {
            if (castedCard == null || target == null)
                return false;

            foreach (AbilityData ability in castedCard.GetAllCurrentAbilities())
            {
                if (ability && ability.trigger == AbilityTrigger.OnPlay && ability.targetType == AbilityTargetType.PlayTarget)
                {
                    AbilityArgs args = new AbilityArgs() {ability = ability, castedCard = castedCard, target = target};
                    bool can_target = ai_check ? ability.CanAiTarget(this, args) : ability.CanTarget(this, args);
                    if (!can_target)
                        return false;
                }
            }
            return true;
        }

        //Check if Card play target is valid, play target is the target when a spell requires to drag directly onto another card
        public virtual bool IsPlayTargetValid(Card castedCard, Card target, bool ai_check = false, bool validateInput=false)
        {
            if (castedCard == null || target == null)
                return false;

            foreach (AbilityData ability in castedCard.GetAllCurrentAbilities())
            {
                if (ability && (ability.trigger == AbilityTrigger.OnPlay || ability.trigger == AbilityTrigger.OnMoveOnSpecificSquare) && (
                        ability.targetType == AbilityTargetType.PlayTarget || ability.targetType == AbilityTargetType.PlayTargetAndSelectMultiplesTarget))
                {
                    Card caster = GetCurrentCardTurn()[0];
                    AbilityArgs args = new AbilityArgs() {ability = ability, castedCard = castedCard, target = target, caster = caster};
                    bool can_target = ai_check ? ability.CanAiTarget(this, args) : ability.CanTarget(this, args, -1, validateInput);
                    if (!can_target)
                        return false;
                }
            }
            return true;
        }

        //Check if Slot play target is valid, play target is the target when a spell requires to drag directly onto another card
        public virtual bool IsPlayTargetValid(Card castedCard, Slot target, bool ai_check = false, bool validateInput=false, Card caster=null)
        {
            if (castedCard == null || target == null)
                return false;
            
            Card slot_card = GetSlotCard(target);
            if (slot_card != null)
                return IsPlayTargetValid(castedCard, slot_card, ai_check, validateInput); //Slot has card, check play target on that card

            List<Card> possibleCasters = new List<Card>();
            if (caster == null)
            {
                possibleCasters = GetPossibleCastersForCard(castedCard);
            }
            else
            {
                possibleCasters.Add(caster);
            }
            foreach (AbilityData ability in castedCard.GetAllCurrentAbilities())
            {
                if (ability && (ability.trigger == AbilityTrigger.OnPlay || ability.trigger == AbilityTrigger.OnMoveOnSpecificSquare) && 
                    (ability.targetType == AbilityTargetType.PlayTarget || ability.targetType == AbilityTargetType.PlayTargetAndSelectMultiplesTarget))
                {
                    AbilityArgs args = new AbilityArgs() {ability = ability, castedCard = castedCard, target = target, caster=caster};
                    bool can_target = ai_check ? ability.CanAiTarget(this, args) : ability.PossibleCastersCanTarget(possibleCasters,this, args, true, -1, validateInput);
                    if (!can_target)
                    {
                        Debug.Log($"Ability {ability.id} can't target {target.GetCoordinate()}");
                        return false;
                    }
                }
            }
            return true;
        }
        
        // This checks if any piece that can cast the card is able to do so (this includes mana and if the piece is exhausted)
        public List<Card> GetPossibleCastersForCard(Card card, Slot? targetSlot = null)
        {
            if (card == null)
            {
                return new List<Card>();
            }
            List<Card> possibleCasters = new List<Card>();
            bool monarchCancast = GetPlayer(CurrentPlayer).CanPayMana(card, true);
            bool nonMonarchCanCast = card.CardData.cardType != CardType.Character &&
                                     ((!GameplayData.Get().InterventionCastByNonMonarchCostsMana && monarchCancast) ||
                                     GetPlayer(CurrentPlayer).CanPayMana(card, false)); 
            if (!monarchCancast)
            {
                return possibleCasters;
            }

            Card king = GetPlayer(CurrentPlayer).king;
            if (!nonMonarchCanCast && !king.playedCardThisRound)
            {
                possibleCasters.Add(king);
                return possibleCasters;
            }

            if (!nonMonarchCanCast)
            {
                return possibleCasters;
            }
            
            foreach (var currentCard in GetCurrentCardTurn())
            {
                if ((GameplayData.Get().EveryPieceCanCastInterventions || card.CardData.possibleCasters.HasFlag(currentCard.GetPieceType()) || currentCard.CardData.boundSpells.Contains(card.CardData)) 
                    && !currentCard.playedCardThisRound && 
                    !currentCard.HasTrait("undisciplined") && 
                    (!currentCard.exhausted || GameplayData.Get().canPlayCardAfterMove) &&
                    !currentCard.wasPlayedThisTurn) 
                {
                    possibleCasters.Add(currentCard);
                }
            }
            
            if (targetSlot != null)
            {
                List<Card> possibleCastersWithTarget = new List<Card>();
                foreach (var caster in possibleCasters)
                {
                    if (IsPlayTargetValid(card, targetSlot.Value, false, true, caster: caster))
                    {
                        possibleCastersWithTarget.Add(caster);
                    }
                }

                return possibleCastersWithTarget;
            }

            return possibleCasters;
        }

        public virtual Player GetPlayer(int id)
        {
            if (id >= 0 && id < players.Length)
                return players[id];
            return null;
        }

        public Player GetActivePlayer()
        {
            return GetPlayer(CurrentPlayer);
        }

        public virtual Player GetOpponentPlayer(int id)
        {
            int oid = id == 0 ? 1 : 0;
            return GetPlayer(oid);
        }

        public virtual Card GetCard(string card_uid)
        {
            foreach (Player player in players)
            {
                Card acard = player.GetCard(card_uid);
                if (acard != null)
                    return acard;
            }
            return null;
        }

        public Card GetBoardCard(string card_uid)
        {
            foreach (Player player in players)
            {
                foreach (Card card in player.cards_board)
                {
                    if (card != null && card.uid == card_uid)
                        return card;
                }
                
                foreach (Card card in player.cards_trap)
                {
                    if (card != null && card.uid == card_uid)
                        return card;
                }
            }

            return null;
        }
        
        public virtual List<Card> GetBoardCardsOfCohort(string cohort_uid)
        {
            List<Card> result = new List<Card>();
            if (cohort_uid == "")
            {
                return result;
            }

            foreach (Player player in players)
            {
                foreach (Card card in player.cards_board)
                {
                    if (card != null && card.CohortUid == cohort_uid)
                        result.Add(card); 
                }
            }

            return result;
        }

        public Card GetHandCard(string card_uid)
        {
            foreach (Player player in players)
            {
                foreach (Card card in player.cards_hand)
                {
                    if (card != null && card.uid == card_uid)
                        return card;
                }
            }
            return null;
        }

        public Card GetDeckCard(string card_uid)
        {
            foreach (Player player in players)
            {
                foreach (Card card in player.cards_deck)
                {
                    if (card != null && card.uid == card_uid)
                        return card;
                }
            }
            return null;
        }

        public Card GetDiscardCard(string card_uid)
        {
            foreach (Player player in players)
            {
                foreach (Card card in player.cards_discard)
                {
                    if (card != null && card.uid == card_uid)
                        return card;
                }
            }
            return null;
        }

        public Card GetTempCard(string card_uid)
        {
            foreach (Player player in players)
            {
                foreach (Card card in player.cards_temp)
                {
                    if (card != null && card.uid == card_uid)
                        return card;
                }
            }
            return null;
        }

        public virtual Card GetSlotCard(Slot slot)
        {
            foreach (Player player in players)
            {
                foreach (Card card in player.cards_board)
                {
                    if (card != null && card.slot == slot)
                        return card;
                }
            }
            return null;
        }

        public virtual Card GetSlotTrap(Slot slot)
        {
            foreach (Player player in players)
            {
                foreach (Card card in player.cards_trap)
                {
                    if (card != null && card.slot == slot)
                        return card;
                }
            }
            return null;
        }

        public SlotStatus GetSlotStatus(Slot slot)
        {
            foreach (var status in slotStatusList)
            {
                if (status.slot.GetCoordinate() == slot.GetCoordinate())
                {
                    return status;
                }
            }

            return null;
        }
        
        public SlotStatus GetSlotStatus(string uid)
        {
            foreach (var status in slotStatusList)
            {
                if (status.uid == uid)
                {
                    return status;
                }
            }

            return null;
        }

        public virtual Player GetRandomPlayer(System.Random rand)
        {
            Player player = GetPlayer(rand.NextDouble() < 0.5 ? 1 : 0);
            return player;
        }

        public virtual Card GetRandomBoardCard(System.Random rand)
        {
            Player player = GetRandomPlayer(rand);
            return player.GetRandomCard(player.cards_board, rand);
        }

        public virtual Slot GetRandomSlot(System.Random rand)
        {
            Player player = GetRandomPlayer(rand);
            return Slot.GetRandom(rand);
        }

        public virtual Card GetMostRecentDiedCardOnSlot(Slot slot)
        {
            List<Card> cards = new List<Card>();
            foreach (Player player in players)
            {
                Card card = GetMostRecentDiedCardOnSlotOfPlayer(slot, player);
                if (card != null)
                {
                    cards.Add(card);
                }
            }
            
            if (cards.Count == 0)
            {
                return null;
            }
            
            if (cards.Count == 1)
            {
                return cards[0];
            }
            
            if (cards[0].roundNumberWhenItDied != cards[1].roundNumberWhenItDied)
            {
                return cards[0].roundNumberWhenItDied > cards[1].roundNumberWhenItDied ? cards[0] : cards[1];
            }
            else
            {
                return cards[0].turnNumberWhenItDied > cards[1].turnNumberWhenItDied ? cards[0] : cards[1];
            }
        }

        public virtual List<Card> GetAllDiedCardOnSlot(Slot slot)
        {
            List<Card> cards = new List<Card>();
            foreach (Player player in players)
            {
                foreach (var card in player.cards_discard)
                {
                    if (card.wasOnBoard && card.slot == slot)
                    {
                        cards.Add(card);
                    }
                }
            }

            return cards;
        }

        public virtual Card GetMostRecentDiedCardOnSlotOfPlayer(Slot slot, Player player)
        {
            for (int i=player.cards_discard.Count-1; i>=0 ; i--)
            {
                Card card = player.cards_discard[i];
                if (card != null && card.slot == slot && card.wasOnBoard)
                {
                    return card;
                }
            }

            return null;
        }
        
        

        public virtual bool IsInHand(Card card)
        {
            return card != null && GetHandCard(card.uid) != null;
        }

        public virtual bool IsOnBoard(Card card)
        {
            return card != null && GetBoardCard(card.uid) != null;
        }

        public virtual bool IsInDeck(Card card)
        {
            return card != null && GetDeckCard(card.uid) != null;
        }

        public virtual bool IsInDiscard(Card card)
        {
            return card != null && GetDiscardCard(card.uid) != null;
        }

        public virtual bool IsInTemp(Card card)
        {
            return card != null && GetTempCard(card.uid) != null;
        }

        public virtual bool IsCardOnSlot(Slot slot)
        {
            return GetSlotCard(slot) != null;
        }

        public bool HasStarted()
        {
            return State != GameState.Connecting;
        }

        public bool HasEnded()
        {
            return State == GameState.GameEnded;
        }

        //Same as clone, but also instantiates the variable (much slower)
        public static Game CloneNew(Game source)
        {
            Game game = new Game();
            Clone(source, game);
            return game;
        }

        //Clone all variables into another var, used mostly by the AI when building a prediction tree
        public static void Clone(Game source, Game dest)
        {
            dest.gameUID = source.gameUID;
            dest.nbPlayers = source.nbPlayers;
            dest.settings = source.settings;

            dest.firstPlayer = source.firstPlayer;
            dest.roundCount = source.roundCount;
            dest.turnTimer = source.turnTimer;
            dest._state = source.State;
            dest._previousState = source.PreviousState;
            dest.initiativeManager = source.initiativeManager.Clone();
            dest.intimidateManager = source.intimidateManager.Clone();
            dest.pieceMovesList = new Dictionary<string, PieceMoves>(source.pieceMovesList);
            dest.slotStatusList = new List<SlotStatus>(source.slotStatusList);

            if (dest.players == null)
            {
                dest.players = new Player[source.players.Length];
                for(int i=0; i< source.players.Length; i++)
                    dest.players[i] = new Player(i);
            }

            for (int i = 0; i < source.players.Length; i++)
                Player.Clone(source.players[i], dest.players[i]);

            dest.selector = source.selector;
            dest.selectorPlayer = source.selectorPlayer;
            dest.selectorCasterUID = source.selectorCasterUID;
            dest.selectorAbilityID = source.selectorAbilityID;
            dest.selectorCardUID = source.selectorCardUID;
            dest.selectorCastedCardUID = source.selectorCastedCardUID;
            dest.selectorManaType = source.selectorManaType;
            dest.rolledValue = source.rolledValue;
            dest.history = source.history.Clone();

            //Some values are commented for optimization, you can uncomment if you want more accurate slower AI
            //Card.CloneNull(source.last_played, ref dest.last_played);
            //Card.CloneNull(source.last_killed, ref dest.last_killed);
            //Card.CloneNull(source.last_target, ref dest.last_target);
            Card.CloneNull(source.abilityTriggerer, ref dest.abilityTriggerer);

            //CloneHash(source.ability_played, dest.ability_played);
            //CloneHash(source.cards_attacked, dest.cards_attacked);
        }

        public static void CloneHash(HashSet<string> source, HashSet<string> dest)
        {
            dest.Clear();
            foreach (string str in source)
                dest.Add(str);
        }

        public void DestroyAllOtherCohortCards(Card target)
        {
            Player player = GetPlayer(target.playerID);
            
            DestroyAllOtherCohortCards(target, ref player.cards_deck);
            DestroyAllOtherCohortCards(target, ref player.cards_hand);
            DestroyAllOtherCohortCards(target, ref player.cards_board, true);
            DestroyAllOtherCohortCards(target, ref player.cards_discard);
            DestroyAllOtherCohortCards(target, ref player.cards_temp);
        }
        
        public bool BothPlayersSubmittedMulligan()
        {
            foreach (var player in players)
            {
                if (!player.submittedMulligan && !player.is_ai)
                {
                    return false;
                }
            }

            return true;
        }
        
        private void DestroyAllOtherCohortCards(Card target, ref List<Card> list, bool removeFromInitiative = false)
        {
            List<Card> toRemove = new List<Card>();
            foreach (var card in list)
            {
                if (card.CohortUid == target.CohortUid && card.uid != target.uid)
                {
                    toRemove.Add(card);
                }
            }

            foreach (var card in toRemove)
            {
                list.Remove(card);
                if (removeFromInitiative)
                {
                    initiativeManager.RemoveCard(card, this);
                }
            }
        }
    }

    [System.Serializable]
    public class PieceMoves
    {
        public Vector2S[] possibleMoves;
        public Dictionary<Vector2S, Vector2S> realDestination;
    }

    [System.Serializable]
    public enum GameState
    {
        Connecting = 0, //Players are not connected
        Starting = 1,  //Players are ready and connected, game is setting-up
        Mulligan = 2,  //Players are mulliganing

        StartTurn = 9,
        StartRound = 10, //Start of turn effects
        Play = 20,      //Play step
        EndTurn = 29,
        EndRound = 30,   //End of turn effects

        PlayerDisconnected = 50, //Player disconnected
        GameEnded = 99,
    }

    [System.Serializable]
    public enum SelectorType
    {
        None = 0,
        SelectTarget = 10,
        SelectMultipleTarget = 11,
        SelectorCard = 20,
        SelectorChoice = 30,
        SelectCaster = 40,
        SelectRangeAttacker = 50,
        SelectManaTypeToGenerate = 60,
        SelectManaTypeToSpend = 70,
    }
}

