using System.Collections;
using System.Collections.Generic;
using Monarchs.Ability;
using Monarchs.Logic;
using Monarchs.Logic.AbilitySystem;
using Sirenix.OdinInspector;
using UnityEngine;

namespace TcgEngine
{
    /// <summary>
    /// Effect that adds or removes basic card/player stats such as hp, attack, mana
    /// </summary>

    [CreateAssetMenu(fileName = "effect", menuName = "TcgEngine/Effect/AddStat", order = 10)]
    public class EffectAddStat : EffectData
    {
        public EffectStatType type;
        public EffectStatOperation operation = EffectStatOperation.Add;
        private bool isMana => type == EffectStatType.Mana;

        [ShowIf("isMana")]
        public PlayerMana.ManaType manaType;

        public override void DoEffectPlayerTarget(GameLogic logic, AbilityArgs args)
        {
            if (type == EffectStatType.Mana)
            {
                ((Player)args.target).playerMana.AddMana(manaType);
            }
        }

        public override void DoEffectCardTarget(GameLogic logic, AbilityArgs args)
        {
            Card target = (Card) args.target;

            if (type == EffectStatType.Attack)
            {
                target.attack = GetValueAfterOperation(target.attack, args.ability.value);
            }

            if (type == EffectStatType.HP)
            {
                target.hp = GetValueAfterOperation(target.hp, args.ability.value);
            }
            
            if (type == EffectStatType.Mana)
                logic.Game.GetPlayer(target.playerID).playerMana.AddMana(manaType);
        }

        public override void DoOngoingEffectCardTarget(GameLogic logic, AbilityArgs args)
        {
            Card target = (Card) args.target;
            if (type == EffectStatType.Attack)
                target.attackOngoing =  GetValueAfterOperation(target.attackOngoing, args.ability.value);
            if (type == EffectStatType.HP)
                target.hpOngoing = GetValueAfterOperation(target.hpOngoing, args.ability.value);
            if (type == EffectStatType.Mana)
                target.manaOngoing = GetValueAfterOperation(target.manaOngoing, args.ability.value);
            if (type == EffectStatType.MoveRange)
                target.moveRangeOnGoing = GetValueAfterOperation(target.moveRangeOnGoing, args.ability.value);
            if (type == EffectStatType.AttackRange)
                target.attackRangeOnGoing = GetValueAfterOperation(target.attackRangeOnGoing, args.ability.value);
        }
        
        private int GetValueAfterOperation(int original, int value)
        {
            switch (operation)
            {
                case EffectStatOperation.Add:
                    return original + value;
                case EffectStatOperation.Remove:
                    return original - value;
                case EffectStatOperation.Set:
                    return value;
                case EffectStatOperation.Multiply:
                    return original * value;
                case EffectStatOperation.Divide:
                    return Mathf.Max(1, Mathf.RoundToInt((float)original/value));
                default:
                    return original;
            }
        }

        public override int GetAiValue(AbilityData ability)
        {
            if (type == EffectStatType.Mana)
                return 0; //Mana unclear, depend of target (good for player, bad for card)

            return Mathf.RoundToInt(Mathf.Sign(ability.value));
        }
    }

    public enum EffectStatType
    {
        None = 0,
        Attack = 10,
        HP = 20,
        Mana = 30,
        MoveRange = 40,
        AttackRange = 50,
    }
    
    public enum EffectStatOperation
    {
        Add = 0,
        Remove = 10,
        Set = 20,
        Multiply = 30,
        Divide = 40,
    }
    
    
}