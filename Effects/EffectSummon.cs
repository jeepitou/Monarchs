using System.Collections;
using System.Collections.Generic;
using Monarchs;
using Monarchs.Ability;
using Monarchs.Logic;
using Monarchs.Logic.AbilitySystem;
using UnityEngine;

namespace TcgEngine
{
    //Effect to Summon an entirely new card (not in anyones deck)
    //And places it on the board

    [CreateAssetMenu(fileName = "effect", menuName = "TcgEngine/Effect/Summon", order = 10)]
    public class EffectSummon : EffectData
    {
        public CardData summon;

        public override void DoEffectPlayerTarget(GameLogic logic, AbilityArgs args)
        {
            logic.SummonCardHand(args.PlayerTarget.playerID, summon, args.caster.VariantData); //Summon to hand
        }

        public override void DoEffectCardTarget(GameLogic logic, AbilityArgs args)
        {
            logic.SummonCard(args.caster.playerID, summon, args.caster.VariantData, args.CardTarget.slot); //Assumes the target has just been killed, so the slot is empty
        }

        public override void DoEffectSlotTarget(GameLogic logic, AbilityArgs args)
        {
            logic.SummonCard(args.caster.playerID, summon, args.caster.VariantData, args.SlotTarget);
        }

        public override void DoEffectCardDataTarget(GameLogic logic, AbilityArgs args)
        {
            logic.SummonCardHand(args.caster.playerID, args.CardDataTarget, args.caster.VariantData);
        }
        
        protected override bool SlotIsEmptyAfterEffectCardTarget(GameLogic logic, AbilityArgs args, Slot slot)
        {
            if (args.CardTarget != null && args.CardTarget.slot == slot)
                return false; //This effect will summon a card into this slot, so it won't be empty
            return true;
        }
        
        protected override bool SlotIsEmptyAfterEffectSlotTarget(GameLogic logic, AbilityArgs args, Slot slot)
        {
            if (args.SlotTarget == slot)
                return false; //This effect will summon a card into this slot, so it won't be empty
            return true;
        }
    }
}