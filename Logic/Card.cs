using System;
using System.Collections.Generic;
using System.Linq;
using Monarchs.Ability;
using Monarchs.Ability.Target;
using Monarchs.Logic.AbilitySystem;
using Sirenix.Utilities;
using TcgEngine;
using TcgEngine.Client;
using UnityEngine;
using UnityEngine.Serialization;
using Random = System.Random;
using ChessTCG.Logic;

namespace Monarchs.Logic
{
    //Represent the current state of a card during the game (data only)

    [System.Serializable]
    public class Card: ITargetable, ICard
    {
        public string cardID;
        public string uid;
        public int playerID;
        public string variantID;
        public int coordinateX;
        public int coordinateY;
        
        public Slot slot;
        public Slot previousSlot;
        public bool exhausted;
        public bool playedCardThisRound = false;
        public int damage = 0;

        public PlayerMana.ManaType mana;
        public int attack;
        public int hp = 0;

        public int manaOngoing = 0;
        public int attackOngoing = 0;
        public int hpOngoing = 0;
        public int moveRangeOnGoing = 0;
        public int attackRangeOnGoing = 0;
        public bool hasMovedThisGame = false;
        public int numberOfMoveThisTurn = 0;
        public int remainingMuster;
        public bool promoted = false;
        public bool wasPlayedThisTurn = false;
        public int cohortSize = 1;
        public string CohortUid { get; private set; }
        public int numberOfCohortUnitDied = 0;
        public Card parentCard = null;
        public Dictionary<PieceType, Card> childCard;

        public List<CardTrait> traits = new List<CardTrait>();
        public List<CardTrait> ongoingTraits = new List<CardTrait>();

        public List<CardStatus> status = new List<CardStatus>();
        private List<CardStatus> _justRemovedStatus = new List<CardStatus>();
        public List<CardStatus> ongoingStatus = new List<CardStatus>();

        [HideInInspector] public DeployChoices deployChoice;
        public bool wasOnBoard = false; // Used to know if a card in the discard pile was killed or discarded from hand.
        public int roundNumberWhenItDied = 0;
        public int turnNumberWhenItDied = 0;
        public bool hasAttacked = false;
        public bool canRetaliate = true;
        public bool cohortSummon = false;
        public List<string> addedAbilitiesSinceInPlayID = new List<string>();
        public List<string> addedSubtypesSinceInPlayID = new List<string>();
        [System.NonSerialized] private CardData _data = null;
        [System.NonSerialized] private VariantData _variantData = null;
        [System.NonSerialized] private int _hash = 0;
        

        public Vector2S GetCoordinates()
        {
            return new Vector2S(slot.x, slot.y);
        }

        public void SetCohortUID(string cohortUID)
        {
            CohortUid = cohortUID;
        }
        
        public Card(string cardID, string uid, string cohortUid, int playerID) { this.cardID = cardID; this.uid = uid; this.playerID = playerID;
            this.CohortUid = cohortUid;
        }
        
        public Card(string cardID, string uid, int playerID) { this.cardID = cardID; this.uid = uid; this.playerID = playerID;
            this.CohortUid = GameTool.GenerateRandomID(11, 15);
        }

        protected bool Equals(Card other)
        {
            return uid == other.uid;
        }

        public virtual bool NeedsToSelectTargetBeforePlay()
        {
            foreach (var ability in CardData.GetAllCurrentAbilities())
            {
                if (ability.trigger == AbilityTrigger.OnPlay)
                {
                    if (ability.targetType == AbilityTargetType.CardSelector ||
                        ability.targetType == AbilityTargetType.ChoiceSelector ||
                        ability.targetType == AbilityTargetType.SelectTarget ||
                        ability.targetType == AbilityTargetType.SelectMultipleTarget ||
                        ability.targetType == AbilityTargetType.PlayTargetAndSelectMultiplesTarget)
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        public virtual SelectorType GetOnPlaySelectorType()
        {
            foreach (var ability in CardData.GetAllCurrentAbilities())
            {
                if (ability.trigger == AbilityTrigger.OnPlay)
                {
                    if (ability.targetType == AbilityTargetType.CardSelector)
                    {
                        return SelectorType.SelectorCard;
                    }
                    if (ability.targetType == AbilityTargetType.ChoiceSelector)
                    {
                        return SelectorType.SelectorChoice;
                    }
                    if (ability.targetType == AbilityTargetType.SelectTarget)
                    {
                        return SelectorType.SelectTarget;
                    }

                    if (ability.targetType == AbilityTargetType.SelectMultipleTarget)
                    {
                        return SelectorType.SelectMultipleTarget;
                    }

                    if (ability.targetType == AbilityTargetType.PlayTargetAndSelectMultiplesTarget)
                    {
                        return SelectorType.SelectMultipleTarget;
                    }
                }
            }
            return SelectorType.None;
        }


        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((Card) obj);
        }

        public override int GetHashCode()
        {
            return (uid != null ? uid.GetHashCode() : 0);
        }
        
        

        public virtual List<Card> GetCurrentPieceTurnThatCanTarget(Game data, string cohortUID, Slot target, bool skipCost = false)
        {
            if ((CardData.IsRequireTarget() && !Targetable.IsValid(target)))
            {
                return new List<Card>();
            }
            
            List<Card> result = new List<Card>();

            var possibleCasters = data.GetPossibleCastersForCard(this);
            if (!CardData.IsRequireTarget())
            {
                return possibleCasters;
            }
            else
            {
                foreach (var ability in GetAllCurrentAbilities())
                {
                    foreach (var card in possibleCasters)
                    {
                        int targetNumber = ability.MultipleTargetToSelect ? 1 : -1;
                        if (ability.AreTargetConditionsMet(data, new AbilityArgs(){ability= ability, caster = card, target = target}, false, targetNumber))
                        {
                            result.Add(card);
                        }
                    }
                }
                
            }
            return result;
        }

        public bool isAlive()
        {
            return GetHP() > 0;
        }
        public virtual void Refresh() { exhausted = false; }
        public virtual void ClearOngoing() { ongoingStatus.Clear(); ongoingTraits.Clear(); attackOngoing = 0; hpOngoing = 0;
            manaOngoing = 0; attackRangeOnGoing=0; moveRangeOnGoing=0;}
        public virtual void Clear()
        {
            ClearOngoing(); Refresh(); damage = 0; status.Clear(); hasMovedThisGame = false; numberOfMoveThisTurn = 0; playedCardThisRound = false;
            numberOfCohortUnitDied = 0;
        }

        public Slot GetSlot()
        {
            return slot;
        }

        public int GetCohortSize()
        {
            return cohortSize;
        }

        public virtual int GetInitiative() { return GetCardData().initiative;}

        public virtual int GetAttack()
        {
            return Math.Max(attack + attackOngoing, 1);
        }

        public virtual int GetMaxAttackRange()
        {
            int tempAttackRange = GetStatus(StatusType.AttackRange) != null ? GetStatus(StatusType.AttackRange).value : 0;
            
            return Math.Max(CardData.GetMaxAttackRange() + tempAttackRange + attackRangeOnGoing, 1);
        }

        public SubtypeData[] GetSubtypes()
        {
            List<SubtypeData> subtypes = new List<SubtypeData>();
            subtypes.AddRange(CardData.subtypes);
            foreach (var subtypeID in addedSubtypesSinceInPlayID)
            {
                subtypes.Add(SubtypeData.Get(subtypeID));
            }
            
            return subtypes.ToArray();
        }

        public PlayerMana.ManaType GetManaCost()
        {
            return mana;
        }

        public bool HasArmorPenetration()
        {
            return HasStat("armor_penetration");
        }
        
        public int GetMinAttackRange()
        {
            return CardData.GetMinAttackRange();
        }

        public virtual CardData GetCardData()
        {
            return CardData;
        }

        public AbilityData[] GetAllCurrentAbilities()
        {
            List<AbilityData> abilities = new List<AbilityData>();
            if (!HasStatus(StatusType.LostAbilities))
            {
                abilities.AddRange(CardData.GetAllCurrentAbilities());
            }
            
            foreach (var abilityID in addedAbilitiesSinceInPlayID)
            {
                abilities.Add(AbilityData.Get(abilityID));
            }

            return abilities.ToArray();
        }

        public virtual int GetArmor()
        {
            int tempArmor = 0;
            if (HasStatus(StatusType.Armor))
            {
                tempArmor = GetStatusValue(StatusType.Armor);
            }

            int permanentArmor = 0;
            if (HasStat("armor"))
            {
                permanentArmor = GetStatValue("armor");
            }

            return tempArmor + permanentArmor;
        }

        public bool OwnedByFirstPlayer(Game game)
        {
            return playerID == game.firstPlayer;
        }
        
        
        public void AddValueToAttack(int value)
        {
            attack = attack + value;
            
        }

        public virtual int GetHP() { return Mathf.Max(hp + hpOngoing - damage, 0); }
        public virtual int GetHPMax() { return Mathf.Max(hp + hpOngoing, 0); }
        public virtual PlayerMana.ManaType GetMana() { return mana; } //Will have to modify if we want to have abilities that can modify mana cost of card

        public virtual int GetMoveRange()
        {
            int tempMoveRange = GetStatus(StatusType.MoveRange) == null ? 0: GetStatus(StatusType.MoveRange).value;
            int permanentMoveRange = moveRangeOnGoing;
            
            if (promoted)
            {
                return Math.Max(CardData.promoteMoveRange + tempMoveRange + permanentMoveRange,1);
            }

            return Math.Max(CardData.moveRange + tempMoveRange + permanentMoveRange,1);
        }

        public virtual bool IsMonarch()
        {
            return HasTrait("king");
        }

        public virtual bool CanBeTargeted()
        {
            return !(HasStatus(StatusType.Stealth) || HasStatus(StatusType.SpellImmunity));
        }

        public int GetPlayerId()
        {
            return playerID;
        }

        public PieceType GetPieceType()
        {
            if (CardData.type == PieceType.Pawn && promoted)
            {
                MovementPieceTypeLink pieceTypeLink = GameplayData.Get().MovementPieceTypeLink;
                return pieceTypeLink.GetType(this);
            }
            return CardData.type;
        }

        public virtual void SetCard(CardData cardData, VariantData variantData)
        {
            _data = cardData;
            cardID = cardData.id;
            variantID = variantData.id;
            _variantData = variantData;
            attack = cardData.attack;
            hp = cardData.hp;
            mana = cardData.manaCost;
            hasMovedThisGame = false;
            cohortSize = cardData.cohortSize;
            deployChoice = cardData.deployType;
            remainingMuster = cardData.musterTime;

            SetTraits(cardData);
        }
        
        public bool CanJump()
        {
            return GetPieceType() == PieceType.Knight || 
                   HasTrait("incorporeal");
        }
        
        public virtual bool IsTrapOnMovementPath(Card triggerCard, Slot finalSlot)
        {
            if (finalSlot == slot)
            {
                return true;
            }

            if (triggerCard.slot == slot)
            {
                return true;
            }

            if (triggerCard.CanJump())
            {
                return false;
            }

            return Slot.IsBetween(triggerCard.slot, finalSlot, slot);
        }

        public void ExhaustAfterMove(bool exhaust=true)
        {
            if (HasStatus(StatusType.Charge))
            {
                RemoveStatus(StatusType.Charge);
                return;
            }
            
            bool isHitAndRun = HasTrait("hit_and_run");

            if (isHitAndRun && numberOfMoveThisTurn < 2 || !exhaust)
            {
                return;
            }
            
            exhausted = true;
        }
        
        public void ExhaustAfterAttack(bool rangedAttack)
        {
            if (HasStatus(StatusType.Charge))
            {
                RemoveStatus(StatusType.Charge);
            }
            
            bool isShootOnTheMoveAttack = rangedAttack && HasTrait("shoot_on_the_move");
            bool isHitAndRun = HasTrait("hit_and_run") && numberOfMoveThisTurn < 2;

            if (isHitAndRun || isShootOnTheMoveAttack)
            {
                return;
            }

            exhausted = true;
        }

        public bool AreAllPiecesOfCohortExhausted(Game game)
        {
            foreach (var card in game.GetBoardCardsOfCohort(CohortUid))
            {
                if (!card.exhausted)
                {
                    return false;
                }
            }

            return true;
        }

        public void CreateChildCards()
        {
            childCard = new Dictionary<PieceType, Card>();
            if (CardData.bishopSpell != null)
            {
                childCard[PieceType.Bishop] = Create(CardData.bishopSpell, VariantData.GetDefault(), playerID);
            }
            if (CardData.pawnSpell != null)
            {
                childCard[PieceType.Pawn] = Create(CardData.pawnSpell, VariantData.GetDefault(), playerID);
            }
            if (CardData.rookSpell != null)
            {
                childCard[PieceType.Rook] = Create(CardData.rookSpell, VariantData.GetDefault(), playerID);
            }
            if (CardData.monarchSpell != null)
            {
                childCard[PieceType.Monarch] = Create(CardData.monarchSpell, VariantData.GetDefault(), playerID);
            }
            if (CardData.championSpell != null)
            {
                childCard[PieceType.Champion] = Create(CardData.championSpell, VariantData.GetDefault(), playerID);
            }
            if (CardData.knightSpell != null)
            {
                childCard[PieceType.Knight] = Create(CardData.knightSpell, VariantData.GetDefault(), playerID);
            }
        }
        
        public Card GetCardCorrespondingWithPieceType(PieceType type)
        {
            if (childCard.Keys.Contains(type))
            {
                Card spell = childCard[type];
                if (spell != null)
                {
                    return spell;
                }
            }
            return null;
        }

        public void Promote()
        {
            promoted = true;
        }

        public MovementScheme GetCurrentMovementScheme()
        {
            if (promoted)
            {
                return CardData.promoteMovementScheme;
            }

            return CardData.MovementScheme;
        }

        public Vector2S[] GetLegalMoves(Game game, bool canTargetAlly = false)
        {
            return CardData.MovementScheme.GetLegalMoves(
                slot.GetCoordinate(), 
                GetMoveRange(), 
                CanJump(), 
                playerID,
                game,
                canTargetAlly);
        }

        public Vector2S[] GetLegalMovesFromSlot(Slot _slot, Game game, bool canTargetAlly = false)
        {
            return CardData.MovementScheme.GetLegalMoves(
                _slot.GetCoordinate(), 
                GetMoveRange(), 
                CanJump(), 
                playerID,
                game,
                canTargetAlly);
        }

        public void SetTraits(CardData cardData)
        {
            traits.Clear();
            foreach (TraitData trait in cardData.traits)
                SetTrait(trait.id, 0);
            if (cardData.stats != null)
            {
                foreach (TraitStat stat in cardData.stats)
                    SetTrait(stat.trait.id, stat.value);
            }
        }
        
        //------ Custom Traits/Stats ---------

        public void SetTrait(string id, int value)
        {
            traits.SetTrait(id, value);
        }

        public void AddTrait(string id, int value)
        {
            traits.AddTrait(id, value);
        }

        public void AddOngoingTrait(string id, int value)
        {
            ongoingTraits.AddTrait(id, value);
        }

        public void RemoveTrait(string id)
        {
            traits.RemoveTrait(id);
        }

        public CardTrait GetTrait(string id)
        {
            foreach (CardTrait trait in traits)
            {
                if (trait.id == id)
                    return trait;
            }
            return null;
        }

        public CardTrait GetOngoingTrait(string id)
        {
            foreach (CardTrait trait in ongoingTraits)
            {
                if (trait.id == id)
                    return trait;
            }
            return null;
        }

        public int GetTraitValue(TraitData trait)
        {
            if (trait != null)
                return GetTraitValue(trait.id);
            return 0;
        }

        public virtual int GetTraitValue(string id)
        {
            int val = 0;
            CardTrait stat1 = GetTrait(id);
            CardTrait stat2 = GetOngoingTrait(id);
            if (stat1 != null)
                val += stat1.value;
            if (stat2 != null)
                val += stat2.value;
            return val;
        }
        
        public virtual bool IsTeamTraitAndType(GuildData guild, TraitData trait, SubtypeData subtype, CardType type)
        {
            bool is_type = CardData.cardType == type || type == CardType.None;
            bool is_team = CardData.guild == guild || guild == null;
            bool is_trait = trait == null || traits.Exists(t => t.id == trait.id) || ongoingTraits.Exists(t => t.id == trait.id);
            bool is_subtype = subtype == null || GetSubtypes().Contains(subtype);
            return (is_type && is_team && is_trait && is_subtype);
        }

        public List<CardTrait> GetAllTraits()
        {
            List<CardTrait> allTraits = new List<CardTrait>();
            allTraits.AddRange(traits);
            allTraits.AddRange(ongoingTraits);
            return allTraits;
        }
        
        //Alternate names since traits/stats are stored in same var
        public void SetStat(string id, int value) => SetTrait(id, value);
        public void AddStat(string id, int value) => AddTrait(id, value);
        public void AddOngoingStat(string id, int value) => AddOngoingTrait(id, value);
        public void RemoveStat(string id) => RemoveTrait(id);
        public int GetStatValue(TraitData trait) => GetTraitValue(trait);
        public int GetStatValue(string id) => GetTraitValue(id);
        public bool HasStat(TraitData trait) => HasTrait(trait);
        public bool HasStat(string id) => HasTrait(id);
        public List<CardTrait> GetAllStats() => GetAllTraits();

        //------  Status Effects ---------

        public void AddStatus(StatusData status, int value, int duration)
        {
            if (status != null)
                AddStatus(status.effect, value, duration);
        }

        public void AddOngoingStatus(StatusData status, int value)
        {
            if (status != null)
                AddOngoingStatus(status.effect, value);
        }

        public void RemoveCharge()
        {
            if (status.Exists(s => s.type == StatusType.Charge) || ongoingStatus.Exists(s => s.type == StatusType.Charge))
            {
                RemoveStatus(StatusType.Charge);
            }
        }

        public int GetRetaliationDamage()
        {
            if (!canRetaliate)
                return 0;
            int retaliationDamage = 0;
            if (traits.Exists(t => t.id == "retaliate") || ongoingTraits.Exists(t => t.id == "retaliate"))
            {
                retaliationDamage = GetAttack();
            }
            if (traits.Exists(t => t.id == "retaliation_damage") || ongoingTraits.Exists(t => t.id == "retaliation_damage"))
            {
                retaliationDamage += GetTraitValue("retaliation_damage");
            }
            if (traits.Exists(t => t.id == "no_retaliation") || ongoingTraits.Exists(t => t.id == "no_retaliation"))
            {
                retaliationDamage = 0;
            }
            return retaliationDamage;
        }

        public void TurnRetaliationOff()
        {
            if (!(traits.Exists(t => t.id == "unlimited_retaliations") || ongoingTraits.Exists(t => t.id == "unlimited_retaliations")))
            {
                canRetaliate = false;
            }
        }

        public void AddStatus(StatusType type, int value, int duration, Card statusApplier = null, bool removeOnApplierTurn = false)
        {
            status.AddStatus(type, value, duration);
        }

        public void AddOngoingStatus(StatusType type, int value)
        {
            ongoingStatus.AddStatus(type, value, 0);
        }

        public void RemoveStatus(StatusType type)
        {
            status.RemoveStatus(type);
        }

        public CardStatus GetStatus(StatusType type)
        {
            return status.GetStatus(type);
        }

        public CardStatus GetOngoingStatus(StatusType type)
        {
            return ongoingStatus.GetStatus(type);
        }

        public bool HasStatus(StatusType type)
        {
            return status.Exists(s => s.type == type) || ongoingStatus.Exists(s => s.type == type);
        }
        
        public bool HasTrait(string id)
        {
            return traits.Exists(t => t.id == id) || ongoingTraits.Exists(t => t.id == id);
        }
        
        public bool HasTrait(TraitData trait)
        {
            if (trait == null)
                return false;
            return HasTrait(trait.id);
        }

        public virtual int GetStatusValue(StatusType type)
        {
            CardStatus status1 = GetStatus(type);
            CardStatus status2 = GetOngoingStatus(type);
            int v1 = status1 != null ? status1.value : 0;
            int v2 = status2 != null ? status2.value : 0;
            return v1 + v2;
        }

        public virtual void ReduceStatusDurations(List<Card> currentTurns)
        {
            for (int i = status.Count - 1; i >= 0; i--)
            {
                if (!status[i].permanent)
                {
                    if ((status[i].isRemovedOnApplierTurn && CheckForUIDInCardList(currentTurns, status[i].applierUID)!=null) ||
                        (!status[i].isRemovedOnApplierTurn) && currentTurns.Contains(this))
                    {
                        status[i].duration -= 1;
                        if (status[i].duration <= 0)
                        {
                            _justRemovedStatus.Add(status[i]);
                            status.RemoveAt(i);
                        }
                    }
                    else
                    {
                        if (status[i].duration == -1) //Used for ongoing status, to verify more often if they lost it
                        {
                            _justRemovedStatus.Add(status[i]);
                            status.RemoveAt(i);
                        }
                    }
                }
            }
        }
        
        public virtual void TriggerOnRemoveStatusEffects(GameLogic logic)
        {
            foreach (var status in _justRemovedStatus)
            {
                status.OnRemoveEffect(this, logic);
            }
            _justRemovedStatus.Clear();
        }

        private Card CheckForUIDInCardList(List<Card> cardList, string uid)
        {
            foreach (var card in cardList)
            {
                if (card.uid == uid)
                {
                    return card;
                }
            }

            return null;
        }
        
        //----- Abilities ------------

        public AbilityData GetAbility(
            string id = null,
            AbilityTrigger? trigger = null,
            AbilityTargetType? targetType = null,
            bool getSelectorOnly = false)
        {
            var abilityList = GetAllCurrentAbilities().Where(a =>
                (id == null || a.id == id) &&
                (!trigger.HasValue || a.trigger == trigger.Value) &&
                (!targetType.HasValue || a.targetType == targetType.Value) &&
                (!getSelectorOnly || a.IsSelector())
            ).ToList();
            
            return abilityList.Count > 0 ? abilityList[0] : null;
        }
        
        public List<AbilityData> GetAbilities(
            string id = null,
            AbilityTrigger? trigger = null,
            AbilityTargetType? targetType = null)
        {
            return GetAllCurrentAbilities().Where(a =>
                (id == null || a.id == id) &&
                (!trigger.HasValue || a.trigger == trigger.Value) &&
                (!targetType.HasValue || a.targetType == targetType.Value)
            ).ToList();
        }
        
        public bool HasAbility(
            string id = null,
            AbilityTrigger? trigger = null,
            AbilityTargetType? targetType = null,
            bool selectorOnly = false)
        {
            return GetAllCurrentAbilities().Any(a =>
                (id == null || a.id == id) &&
                (!trigger.HasValue || a.trigger == trigger.Value) &&
                (!targetType.HasValue || a.targetType == targetType.Value) &&
                (!selectorOnly || a.IsSelector())
            );
        }

        //---- Action Check ---------

        public virtual bool CanAttack(bool skipCost = false)
        {
            if (status.Exists(s => s.type == StatusType.Stunned) || status.Exists(s => s.type == StatusType.Disarmed) || ongoingStatus.Exists(s => s.type == StatusType.Stunned) || ongoingStatus.Exists(s => s.type == StatusType.Disarmed))
                return false;
            bool isHitAndRun = traits.Exists(t => t.id == "hit_and_run") || ongoingTraits.Exists(t => t.id == "hit_and_run");
            if (!skipCost && ((exhausted & !isHitAndRun) || hasAttacked))
                return false; //no more action
            return true;
        }

        public virtual bool CanMove(bool skipCost = false, bool isMeleeAttack=false)
        {
            if (status.Exists(s => s.type == StatusType.Stunned) || ongoingStatus.Exists(s => s.type == StatusType.Stunned))
                return false;
            if ((status.Exists(s => s.type == StatusType.Immobilize) || ongoingStatus.Exists(s => s.type == StatusType.Immobilize)) && !isMeleeAttack)
            {
                return false;
            }
            bool isAttackOnTheMove = (traits.Exists(t => t.id == "shoot_on_the_move") || ongoingTraits.Exists(t => t.id == "shoot_on_the_move")) && numberOfMoveThisTurn==0;
            bool isHitAndRun = (traits.Exists(t => t.id == "hit_and_run") || ongoingTraits.Exists(t => t.id == "hit_and_run")) && numberOfMoveThisTurn<2;
            if (!skipCost && exhausted && !isAttackOnTheMove && !isHitAndRun)
            {
                return false; //no more action
            }
            return true; 
        }

        public virtual bool CanDoActivatedAbilities()
        {
            if (exhausted)
                return false;
            if (status.Exists(s => s.type == StatusType.Stunned) || ongoingStatus.Exists(s => s.type == StatusType.Stunned))
                return false;
            if (status.Exists(s => s.type == StatusType.Silenced) || ongoingStatus.Exists(s => s.type == StatusType.Silenced))
                return false;
            return true;
        }

        public virtual bool CanDoAbilities()
        {
            if (status.Exists(s => s.type == StatusType.Silenced) || ongoingStatus.Exists(s => s.type == StatusType.Silenced))
                return false;
            return true;
        }

        public virtual bool CanDoAnyAction()
        {
            return CanAttack() || CanMove() || CanDoActivatedAbilities();
        }

        //----------------

        public virtual CardData CardData 
        { 
            get { 
                if(_data == null || _data.id != cardID)
                    _data = CardData.Get(cardID); //Optimization, store for future use
                return _data;
            } 
        }

        public VariantData VariantData
        {
            get
            {
                if (_variantData == null || _variantData.id != variantID)
                    _variantData = VariantData.Get(variantID); //Optimization, store for future use
                return _variantData;
            }
        }

        public CardData Data => CardData; //Alternate name

        public int Hash
        {
            get {
                if (_hash == 0)
                    _hash = Mathf.Abs(uid.GetHashCode()); //Optimization, store for future use
                return _hash;
            }
        }

        public static Card Create(CardData cardData, VariantData variantData, int playerID)
        {
            return Create(cardData, variantData, playerID, GameTool.GenerateRandomID(11, 15), GameTool.GenerateRandomID(11, 15));
        }
        
        public static Card Create(CardData cardData, VariantData variantData, int playerID, string uid)
        {
            Card card = new Card(cardData.id, uid, GameTool.GenerateRandomID(11, 15), playerID);
            card.SetCard(cardData, variantData);
            return card;
        }
        
        public static Card CreateInCohort(CardData cardData, VariantData variantData, int playerID, string cohortUid)
        {
            Card card = new Card(cardData.id, GameTool.GenerateRandomID(11, 15), cohortUid, playerID);
            card.SetCard(cardData, variantData);
            return card;
        }

        public void GenerateNewCohortUID()
        {
            CohortUid = GameTool.GenerateRandomID(11, 15);
        }

        public static Card Create(CardData cardData, VariantData variantData, int playerID, string uid, string cohortUid)
        {
            Card card = new Card(cardData.id, uid, cohortUid, playerID);
            card.SetCard(cardData, variantData);
            return card;
        }

        public static Card CloneNew(Card source)
        {
            Card card = new Card(source.cardID, source.uid, source.CohortUid, source.playerID);
            Clone(source, card);
            return card;
        }

        //Clone all card variables into another var, used mostly by the AI when building a prediction tree
        public static void Clone(Card source, Card dest)
        {
            dest.cardID = source.cardID;
            dest.uid = source.uid;
            dest.CohortUid = source.CohortUid;
            dest.playerID = source.playerID;

            dest.variantID = source.variantID;
            dest.slot = source.slot;
            dest.exhausted = source.exhausted;
            dest.damage = source.damage;
            dest.hasMovedThisGame = source.hasMovedThisGame;
            dest.numberOfMoveThisTurn = source.numberOfMoveThisTurn;

            dest.attack = source.attack;
            dest.hp = source.hp;
            dest.mana = source.mana;

            dest.manaOngoing = source.manaOngoing;
            dest.attackOngoing = source.attackOngoing;
            dest.hpOngoing = source.hpOngoing;
            dest.moveRangeOnGoing = source.moveRangeOnGoing;
            dest.attackRangeOnGoing = source.attackRangeOnGoing;
            dest.remainingMuster = source.remainingMuster;

            dest.deployChoice = source.deployChoice;

            if (source.CardData.differentSpellPerMovement)
            {
                Card.CloneChildCards(source, dest);
            }

            source.traits.CloneList(dest.traits);
            source.ongoingTraits.CloneList(dest.ongoingTraits);
            source.status.CloneList(dest.status);
            source.ongoingStatus.CloneList(dest.ongoingStatus);
        }

        private static void CloneChildCards(Card source, Card dest)
        {
            dest.childCard = new Dictionary<PieceType, Card>();
            foreach (var childCard in source.childCard)
            {
                dest.childCard[childCard.Key] = Card.CloneNew(childCard.Value);
            }
        }

        //Clone a var that could be null
        public static void CloneNull(Card source, ref Card dest)
        {
            //Source is null
            if (source == null)
            {
                dest = null;
                return;
            }

            //Dest is null
            if (dest == null)
            {
                dest = CloneNew(source);
                return;
            }

            //Both arent null, just clone
            Clone(source, dest);
        }

        //Clone dictionary completely
        public static void CloneDict(Dictionary<string, Card> source, Dictionary<string, Card> dest)
        {
            foreach (KeyValuePair<string, Card> pair in source)
            {
                bool valid = dest.TryGetValue(pair.Key, out Card val);
                if (valid)
                    Clone(pair.Value, val);
                else
                    dest[pair.Key] = CloneNew(pair.Value);
            }
        }

        //Clone list by keeping references from ref_dict
        public static void CloneListRef(Dictionary<string, Card> refDict, List<Card> source, List<Card> dest)
        {
            for (int i = 0; i < source.Count; i++)
            {
                Card sourceCard = source[i];
                bool valid = refDict.TryGetValue(sourceCard.uid, out Card refCard);
                if (valid)
                {
                    if (i < dest.Count)
                        dest[i] = refCard;
                    else
                        dest.Add(refCard);
                }
            }

            if(dest.Count > source.Count)
                dest.RemoveRange(source.Count, dest.Count - source.Count);
        }

        public void ResetCohortSize()
        {
            cohortSize = CardData.cohortSize;
        }

        public void AddAbility(AbilityData ability)
        {
            addedAbilitiesSinceInPlayID.Add(ability.id);
        }
        
        public virtual bool SpawnsACardOnSlotWhenInDies(GameLogic logic, Slot targetSlot, Slot slotToVerify)
        {
            AbilityArgs args = new AbilityArgs(){ability = null, caster = this, target = targetSlot};
            foreach (var ability in GetAllCurrentAbilities())
            {
                args.ability = ability;
                if (ability.SpawnsACardOnSlotWhenInDies(logic, args, slotToVerify))
                {
                    return true;
                }
            }
            return false;
        }
    }

    [System.Serializable]
    public class CardStatus : ChessTCG.Logic.IClonable<CardStatus>
    {
        public StatusType type;
        public int value;
        public int duration = 1;
        public bool permanent = true;
        public string applierUID;
        public bool isRemovedOnApplierTurn = false;

        [System.NonSerialized]
        private StatusData _data = null;

        public CardStatus(StatusType type, int value, int duration, Card applier = null, bool isRemovedOnApplierTurn = false)
        {
            this.type = type;
            this.value = value;
            this.duration = duration;
            this.permanent = (duration == 0);
            this.applierUID = applier?.uid;
            this.isRemovedOnApplierTurn = isRemovedOnApplierTurn;
        }

        public StatusData StatusData { 
            get
            {
                if (_data == null || _data.effect != type)
                    _data = StatusData.Get(type);
                return _data;
            }
        }

        public void OnRemoveEffect(Card card, GameLogic logic)
        {
            if (type == StatusType.MindControlled)
            {
                int oppositeOwner = card.playerID == 0 ? 1 : 0;
                logic.ChangeOwner(card, logic.Game.GetPlayer(oppositeOwner));
            }
        }

        public StatusData Data => StatusData; //Alternate name

        public static CardStatus CloneNew(CardStatus copy)
        {
            CardStatus status = new CardStatus(copy.type, copy.value, copy.duration);
            status.permanent = copy.permanent;
            return status;
        }

        public static void Clone(CardStatus source, CardStatus dest)
        {
            dest.type = source.type;
            dest.value = source.value;
            dest.duration = source.duration;
            dest.permanent = source.permanent;
        }

        // IClonable implementation
        CardStatus ChessTCG.Logic.IClonable<CardStatus>.CloneNew(CardStatus source) => CloneNew(source);
        void ChessTCG.Logic.IClonable<CardStatus>.Clone(CardStatus source, CardStatus dest) => Clone(source, dest);
    }

    [System.Serializable]
    public class CardTrait : ChessTCG.Logic.IClonable<CardTrait>
    {
        public string id;
        public int value;

        [System.NonSerialized]
        private TraitData _data = null;

        public CardTrait(string id, int value)
        {
            this.id = id;
            this.value = value;
        }

        public CardTrait(TraitData trait, int value)
        {
            this.id = trait.id;
            this.value = value;
        }

        public TraitData TraitData
        {
            get
            {
                if (_data == null || _data.id != id)
                    _data = TraitData.Get(id);
                return _data;
            }
        }

        public TraitData Data => TraitData; //Alternate name

        // IClonable implementation
        CardTrait ChessTCG.Logic.IClonable<CardTrait>.CloneNew(CardTrait source) => new CardTrait(source.id, source.value);
        void ChessTCG.Logic.IClonable<CardTrait>.Clone(CardTrait source, CardTrait dest)
        {
            dest.id = source.id;
            dest.value = source.value;
        }
    }
}
