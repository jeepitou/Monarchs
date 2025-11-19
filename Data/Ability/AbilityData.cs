using System;
using System.Collections.Generic;
using System.Linq;
using Ability.Target;
using Monarchs.Ability.Target;
using Monarchs.Logic;
using Monarchs.Logic.AbilitySystem;
using Sirenix.OdinInspector;
using TcgEngine;
using UnityEngine;

namespace Monarchs.Ability
{
    /// <summary>
    /// Defines all ability data
    /// </summary>

    [CreateAssetMenu(fileName = "ability", menuName = "TcgEngine/AbilityData", order = 4)]
    public class AbilityData : ScriptableObject
    {
        public string id;

        [Header("Trigger")]
        public AbilityTrigger trigger;             //WHEN does the ability trigger?
        public ConditionData[] conditions_trigger; //Condition checked on the card triggering the ability (usually the caster)

        [Header("Target")]
        public AbilityTargetType targetType;               //WHO is targeted?

        public bool MultipleTargetToSelect => targetType == AbilityTargetType.SelectMultipleTarget ||
                                               targetType == AbilityTargetType.PlayTargetAndSelectMultiplesTarget;
        [ShowIf("MultipleTargetToSelect")]
        public bool useCohortSizeForQuantityOfTargets = false;
        private bool _showQuantityOfTarget => MultipleTargetToSelect && !useCohortSizeForQuantityOfTargets;
        
        [SerializeField][ShowIf("_showQuantityOfTarget")]
        private int _quantityOfTargets = 1;
        
        [HideIf("MultipleTargetToSelect")]
        public ConditionData[] conditions_target;  //Condition checked on the target to know if its a valid taget
        [HideIf("MultipleTargetToSelect")]
        public FilterData[] filters_target;  //Condition checked on the target to know if its a valid taget

        [ShowIf("MultipleTargetToSelect")]
        public TargetSpecifications[] targetSpecifications;

        [Header("Effect")]
        [HideIf("MultipleTargetToSelect")]
        public EffectData[] effects;              //WHAT this does?
        [ShowIf("MultipleTargetToSelect")]
        public EffectSpecifications[] effect_specifications;
        [HideIf("MultipleTargetToSelect")]
        public StatusData[] status;               //Status added by this ability  
        [ShowIf("MultipleTargetToSelect")]
        public StatusSpecifications[] status_specifications;
        public int value;                         //Value passed to the effect (deal X damage)
        public int value2;                        // Second value passed to the effect (min/max damage)
        public int duration;                      //Duration passed to the effect (usually for status, 0=permanent)

        [Header("Chain/Choices")]
        public AbilityData[] chain_abilities;    //Abilities that will be triggered after this one

        [Header("Activated Ability")]
        public PlayerMana.ManaType mana_cost;                   //Mana cost for  activated abilities
        public bool exhaust;                    //Action cost for activated abilities
        public bool ignoreExhaustWhenCasting = false; 

        [Header("FX")]
        public FXData[] board_fx;
        public FXData[] caster_fx;
        public FXData[] selectTargetFx;
        public FXData[] target_fx;
        public int vfxIndex = 0;
        public AudioClip cast_audio;
        public AudioClip target_audio;
        public bool charge_target;

        [Header("Text")]
        public string title;
        [TextArea(5, 7)]
        public string desc;
        public bool DontShowInHistory = false;
        public bool DontShowOnPreview = false;
        public bool DontShowToEnemyWhenPlayed = false;

        public Sprite icon;

        public static List<AbilityData> ability_list = new List<AbilityData>();

        private static AbilityTargetExtractor _abilityTargetExtractor = new();
        public bool displayDeathHighlights;
        [ShowIf("displayDeathHighlights")]
        public ConditionData[] deathHighlightsConditions;

        public static void Load(string folder = "")
        {
            if (ability_list.Count == 0)
                ability_list.AddRange(Resources.LoadAll<AbilityData>(folder));
        }

        public string GetTitle()
        {
            return title;
        }
        
        void OnValidate()
        {
            if (targetSpecifications?.Length != _quantityOfTargets)
            {
                Array.Resize(ref targetSpecifications, _quantityOfTargets);
            }
        }

        public string GetDesc()
        {
            return desc;
        }

        public string GetDesc(CardData card)
        {
            string dsc = desc;
            dsc = dsc.Replace("<name>", card.title);
            dsc = dsc.Replace("<value>", value.ToString());
            dsc = dsc.Replace("<duration>", duration.ToString());
            return dsc;
        }

        public int GetQuantityOfTargets(Card card)
        {
            if (!MultipleTargetToSelect)
            {
                return 0;
            }
            
            if (useCohortSizeForQuantityOfTargets)
            {
                return card.cohortSize;
            }

            return _quantityOfTargets;
        }

        public virtual bool AreTriggerConditionsMet(Game data, AbilityArgs args)
        {
            if (args.target == null)
            {
                args.target = args.ability.targetType == AbilityTargetType.PlayTarget
                    ? args.castedCard.slot
                    : args.caster;
            }

            foreach (ConditionData cond in conditions_trigger)
            {
                if (cond != null)
                {
                    if (!cond.IsTriggerConditionMetNoTarget(data, args))
                        return false;
                    // if (!cond.IsTargetConditionMet(data, args))
                    //     return false;
                }
            }
            return true;
        }

        public bool AreTargetConditionsMet(Game data, AbilityArgs args, bool triggeringTrap = false,
            int targetNumber = -1, bool validateInput = false)
        {
            if (triggeringTrap)
            {
                return true;
            }
            
            return _abilityTargetExtractor.AreTargetConditionsMet(data, args, targetNumber, validateInput);
        }

        public virtual bool CanTarget(Game data, AbilityArgs args, int targetNumber = -1, bool validateInput=false)
        {
            if (args.ability.targetType == AbilityTargetType.PlayTargetAndSelectMultiplesTarget && targetNumber == -1)
            {
                targetNumber = 1;
            }
            
            if (!args.target.CanBeTargeted())
            {
                return false;
            }

            return _abilityTargetExtractor.AreTargetConditionsMet(data, args, targetNumber);
        }
        
        public List<Slot> GetAllSlotsThatCanBeTargeted(List<Card> possibleCasters, Game data, AbilityArgs args, int targetNumber = -1)
        {
            List<Slot> result = new List<Slot>();
            foreach (var slot in Slot.GetAll())
            {
                args.target = slot;
                if (PossibleCastersCanTarget(possibleCasters, data, args, false, targetNumber))
                {
                    result.Add(slot);
                }
            }

            return result;
        }
        
        public bool PossibleCastersCanTarget(List<Card> possibleCasters, Game data, AbilityArgs args, bool verifyIfCohortPlayedCard = false, int targetNumber = -1, bool validateInput=false)
        {
            if (args.ability.targetType == AbilityTargetType.PlayTargetAndSelectMultiplesTarget && targetNumber == -1)
            {
                targetNumber = 1;
            }
            
            if (!args.target.CanBeTargeted())
            {
                return false;
            }

            if (args.caster == null)
            {
                args.caster = data.GetCurrentCardTurn()[0];
            }
            
            foreach (var possibleCaster in possibleCasters)
            {
                args.caster = possibleCaster;
                if (AreTargetConditionsMet(data, args, false, targetNumber))
                {
                    return true;
                }
            }

            return false; 
        }
        
        public virtual List<Card> GetPiecesOfCohortThatCanTarget(Game data, AbilityArgs args)
        {
            List<Card> result = new List<Card>();

            foreach (var cohortCard in data.GetBoardCardsOfCohort(args.caster.CohortUid))
            {
                if (!cohortCard.exhausted || args.ability.trigger == AbilityTrigger.OnPlay )
                {
                    args.caster = cohortCard;
                    if (AreTargetConditionsMet(data, args))
                    {
                        result.Add(cohortCard);
                    }
                }
            }

            return result;
        }
        
        public virtual List<Card> GetPiecesOfCohortThatCanCast(Game data, Card caster)
        {
            List<Card> result = new List<Card>();

            foreach (var cohortCard in data.GetBoardCardsOfCohort(caster.CohortUid))
            {
                if (!cohortCard.exhausted && cohortCard.CanDoAbilities())
                {
                    result.Add(cohortCard);
                }
            }

            return result;
        }

        //Check if destination array has the target after being filtered, used to support filters in CardSelector
        public virtual bool IsCardSelectionValid(Game data, Card caster, Card target, ListSwap<ITargetable> cardArray = null)
        {
            foreach (var cohortCard in data.GetBoardCardsOfCohort(caster.CohortUid))
            {
                if (!cohortCard.exhausted)
                {
                    List<Card> targets = GetCardTargets(data, cohortCard, cardArray);
                    if (targets.Contains(target))
                    {
                        return true;
                    }
                }
            }
            
            return false ; //Card is still in array after filtering
        }

        public virtual void DoEffects(GameLogic logic, AbilityArgs args)
        {
            if (targetType == AbilityTargetType.SelectMultipleTarget ||
                targetType == AbilityTargetType.PlayTargetAndSelectMultiplesTarget)
            {
                DoEffectsMultipleTargets(logic, args);
                return;
            }
            
            
            foreach(EffectData effect in effects)
                effect?.DoEffect(logic, args);

            if (args.target is Card)
            {
                foreach(StatusData stat in status)
                    args.CardTarget.AddStatus(stat, value, duration);
            }
        }

        public virtual void DoEffectsMultipleTargets(GameLogic logic, AbilityArgs args) //TODO Take filter into account
        {
            Game game = logic.Game;
            foreach (var effect in effect_specifications)
            {
                var targets = GetMultipleTargets(game, effect.target);
                foreach (var target in targets)
                {
                    args.target = target;
                    effect.effect.DoEffect(logic, args);
                }
            }
            AddStatusMultipleTargets(logic, args);
        }

        public virtual void AddStatusMultipleTargets(GameLogic logic, AbilityArgs args) //TODO Take filter into account
        {
            Game game = logic.Game;
            foreach (var status in status_specifications)
            {
                var targets = GetMultipleTargets(game, status.target);
                foreach (var target in targets)
                {
                    if (target is Card)
                    {
                        args.CardTarget.AddStatus(status.status, value, duration);
                    }
                }
            }
        }

        public static int GetTargetIndex(EffectTarget target)
        {
            if (target.HasFlag(EffectTarget.Target_1) )
            {
                return 0;
            }
            if (target.HasFlag(EffectTarget.Target_2) )
            {
                return 1;
            }
            if (target.HasFlag(EffectTarget.Target_3) )
            {
                return 2;
            }
            if (target.HasFlag(EffectTarget.Target_4) )
            {
                return 3;
            }
            if (target.HasFlag(EffectTarget.Target_5) )
            {
                return 4;
            }
            return 0;
        }

        public static List<ITargetable> GetMultipleTargets(Game game, EffectTarget target)
        {
            int count = game.selectorTargets.Count;
            List<ITargetable> result = new List<ITargetable>();
            if (target.HasFlag(EffectTarget.Target_1) && count >= 1)
            {
                result.Add(game.selectorTargets[0]);
            }
            if (target.HasFlag(EffectTarget.Target_2) && count >= 2)
            {
                result.Add(game.selectorTargets[1]);
            }
            if (target.HasFlag(EffectTarget.Target_3) && count >= 3)
            {
                result.Add(game.selectorTargets[2] );
            }
            if (target.HasFlag(EffectTarget.Target_4) && count >= 4)
            {
                result.Add(game.selectorTargets[3]);
            }
            if (target.HasFlag(EffectTarget.Target_5) && count >= 5)
            {
                result.Add(game.selectorTargets[4]);
            }
            return result;
        }

        public virtual void DoOngoingEffects(GameLogic logic, AbilityArgs args)
        {

            foreach (EffectData effect in effects)
                effect?.DoOngoingEffect(logic, args);
            

            if (args.target is Card)
            {
                foreach(StatusData stat in status)
                    args.CardTarget.AddStatus(stat.effect, value, duration, args.castedCard);
            }
        }

        public virtual bool HasEffect<T>() where T : EffectData
        {
            foreach (EffectData eff in effects)
            {
                if (eff != null && eff is T)
                    return true;
            }
            return false;
        }

        public virtual bool HasStatus(StatusType type)
        {
            foreach (StatusData sta in status)
            {
                if (sta != null && sta.effect == type)
                    return true;
            }
            return false;
        }

        public virtual bool SpawnsACardOnSlotWhenInDies(GameLogic logic, AbilityArgs args, Slot slot)
        {
            if (trigger != AbilityTrigger.OnDeath)
            {
                return false;
            }
            
            foreach (EffectData eff in effects)
            {
                if (eff.SlotIsEmptyAfterEffect(logic, args, slot) == false)
                {
                    return true;
                }
            }
            return false;
        }

        public virtual List<ITargetable> GetTargets(Game data, Card caster, ITargetable target=null, ListSwap<ITargetable> memoryArray = null, bool dontVerifyPlayTarget=false)
        {
            if (targetType == AbilityTargetType.PlayTarget)
            {
                return GetPlayTargets(data, caster, target, memoryArray, dontVerifyPlayTarget);
            }

            AbilityArgs args = new AbilityArgs() {ability = this, caster = caster};
            return _abilityTargetExtractor.GetAllTargets(data, args, memoryArray);
        }
        
        public virtual List<ITargetable> GetPlayTargets(Game data, Card caster, ITargetable target, ListSwap<ITargetable> memoryArray = null, bool dontVerifyPlayTarget=false)
        {
            AbilityArgs args = new AbilityArgs() {ability = this, caster = caster, target = target};
            return _abilityTargetExtractor.GetAllSelectTargets(data, args, memoryArray, dontVerifyPlayTarget);
        }
        
        public virtual List<Card> GetCardTargets(Game data, Card caster, ListSwap<ITargetable> memoryArray = null)
        {
            var targetableList = GetTargets(data, caster, null, memoryArray);
            return targetableList.OfType<Card>().ToList();
        }
        
        public virtual List<Card> GetCardPlayTargets(Game data, Card caster, Card targetCard, ListSwap<ITargetable> memoryArray = null)
        {
            var targetableList = GetPlayTargets(data, caster, targetCard, memoryArray);
            return targetableList.OfType<Card>().ToList();
        }
        public virtual List<Player> GetPlayerTargets(Game data, Card caster, ListSwap<ITargetable> memoryArray = null)
        {
            var targetableList = GetTargets(data, caster, null, memoryArray);
            return targetableList.OfType<Player>().ToList();
        }
        
        public virtual List<Slot> GetSlotTargets(Game data, Card caster, ListSwap<ITargetable> memoryArray = null)
        {
            var targetableList = GetTargets(data, caster, null, memoryArray);
            return targetableList.OfType<Slot>().ToList();
        }
        
        public virtual List<Slot> GetSlotPlayTargets(Game data, Card caster, Slot targetSlot, ListSwap<ITargetable> memoryArray = null)
        {
            var targetableList = GetPlayTargets(data, caster, targetSlot, memoryArray);
            return targetableList.OfType<Slot>().ToList();
        }

        public virtual List<CardData> GetCardDataTargets(Game data, Card caster, ListSwap<ITargetable> memoryArray = null)
        {
            var targetableList = GetTargets(data, caster, null, memoryArray);
            return targetableList.OfType<CardData>().ToList();
        }

        public virtual bool IsSelector()
        {
            return targetType == AbilityTargetType.SelectTarget ||
                   targetType == AbilityTargetType.CardSelector ||
                   targetType == AbilityTargetType.ChoiceSelector ||
                   targetType == AbilityTargetType.SelectMultipleTarget ||
                   targetType == AbilityTargetType.PlayTargetAndSelectMultiplesTarget ||
                   targetType == AbilityTargetType.SelectManaType;
        }

        public static AbilityData Get(string id)
        {
            foreach (AbilityData ability in GetAll())
            {
                if (ability.id == id)
                    return ability;
            }
            return null;
        }

        public static List<AbilityData> GetAll()
        {
            return ability_list;
        }
        
        //AI has additional restrictions based on if the effect is positive or not
        public bool CanAiTarget(Game data, AbilityArgs args)
        {
            return CanTarget(data, args) && CanAiTarget(args.caster.playerID, args.target.GetPlayerId());
        }

        public bool CanAiTarget(int caster_pid, int target_pid)
        {
            if (target_pid == -1)
            {
                return true; // (Targets slot or CardData)
            }
            int ai_value = GetAiValue();
            if (ai_value > 0 && caster_pid != target_pid)
                return false; //Positive effect, dont target others
            if (ai_value < 0 && caster_pid == target_pid)
                return false; //Negative effect, dont target self
            return true;
        }
        
        public int GetAiValue()
        {
            int total = 0;
            foreach (EffectData eff in effects)
                total += eff != null ? eff.GetAiValue(this) : 0;
            foreach (StatusData astatus in status)
                total += astatus != null ? astatus.hvalue : 0;
            foreach (AbilityData ability in chain_abilities)
                total += ability != null ? ability.GetAiValue() : 0;
            return total;
        }
    }
    
    [Serializable]
    public struct FXData
    {
        public GameObject FX;
        public FXTarget Target;
        public EffectTarget targetNumber;
    }

    [Serializable]
    public struct TargetSpecifications
    {
        [TextArea(2, 4)]
        public string promptText;
        public ConditionData[] conditions;  //Condition checked on the target to know if its a valid taget
        public FilterData[] filters;  //Condition checked on the target to know if its a valid taget
        public bool optional;
    }

    [Serializable]
    public struct EffectSpecifications
    {
        public EffectData effect;
        public EffectTarget target;
    
    }

    [Serializable]
    public struct StatusSpecifications
    {
        public StatusData status;
        public EffectTarget target;
    }

    [Flags]
    public enum EffectTarget
    {
        None = 0,
        Target_1 = 1 << 0,
        Target_2 = 1 << 1,
        Target_3 = 1 << 2,
        Target_4 = 1 << 3,
        Target_5 = 1 << 4,
        All = Target_1 | Target_2 | Target_3 | Target_4 | Target_5
    }

    [Serializable]
    public enum FXTarget
    {
        Piece = 1,
        Slot =2
    }
}
