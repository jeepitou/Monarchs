using System;
using System.Collections;
using System.Collections.Generic;
using Monarchs;
using Monarchs.Ability;
using Monarchs.Ability.Target;
using Monarchs.Logic;
using Monarchs.Logic.AbilitySystem;
using UnityEngine;

namespace TcgEngine
{
    /// <summary>
    /// Base class for all ability effects, override the IsConditionMet function
    /// </summary>
    
    public class EffectData : ScriptableObject
    {
        /// <summary>
        /// Effect used by slot status
        /// </summary>
        /// <param name="logic"></param>
        /// <param name="caster">Card that is walking on the slotStatus</param>
        /// <param name="slotWithStatus">Board slot with status</param>
        /// <param name="destinationSlot">Board slot that is the end of the movement, used to calculate new end slot</param>
        public virtual void DoEffect(GameLogic logic, SlotStatusData slotStatusData, Card caster, Slot slotWithStatus, Slot destinationSlot)
        {
            //Server side gameplay logic
        }

        public virtual void DoEffect(GameLogic logic, AbilityArgs args)
        {
            if (args.target == null)
            {
                DoEffectNoTarget(logic, args);
            }
            else if (args.target is Card)
            {
                DoEffectCardTarget(logic, args);
            }
            else if (args.target is Player)
            {
                DoEffectPlayerTarget(logic, args);
            }
            else if (args.target is Slot)
            {
                DoEffectSlotTarget(logic, args);
            }
            else if (args.target is CardData)
            {
                DoEffectCardDataTarget(logic, args);
            }
            else
            {
                throw new ArgumentException("Unsupported target type");
            }
        }
        
        public virtual void DoOngoingEffect(GameLogic logic, AbilityArgs args)
        {
            
            if (args.target is Card)
            {
                DoOngoingEffectCardTarget(logic, args);
            }
            else if (args.target is Slot)
            {
                DoOngoingEffectSlotTarget(logic, args);
            }
            else if (args.target is Player)
            {
                DoOngoingEffectPlayerTarget(logic, args);
            }
        }
        
        public virtual void DoEffectNoTarget(GameLogic logic, AbilityArgs args)
        {
            //Server side gameplay logic
        }

        public virtual void DoEffectCardTarget(GameLogic logic, AbilityArgs args)
        {
            //Server side gameplay logic
        }

        public virtual void DoEffectPlayerTarget(GameLogic logic, AbilityArgs args)
        {
            //Server side gameplay logic
        }

        public virtual void DoEffectSlotTarget(GameLogic logic, AbilityArgs args)
        {
            Card card = logic.Game.GetSlotCard(args.SlotTarget);

            if (card != null)
            {
                args.target = card;
                DoEffectCardTarget(logic, args);
            }
        }

        public virtual void DoEffectCardDataTarget(GameLogic logic, AbilityArgs args)
        {
            //Server side gameplay logic
        }

        public virtual void DoOngoingEffectCardTarget(GameLogic logic, AbilityArgs args)
        {
            //Ongoing effect only
        }
        
        public virtual void DoOngoingEffectSlotTarget(GameLogic logic, AbilityArgs args)
        {
            Card card = logic.Game.GetSlotCard(args.SlotTarget);
            if (card != null)
            {
                args.target = card;
                DoOngoingEffectCardTarget(logic, args);
            }
        }

        public virtual void DoOngoingEffectPlayerTarget(GameLogic logic, AbilityArgs args)
        {
            //Ongoing effect only
        }

        public virtual int GetAiValue(AbilityData ability)
        {
            return 0; //Helps the AI know if this is a positive or negative ability effect (return 1, 0 or -1)
        }
        
        public virtual bool SlotIsEmptyAfterEffect(GameLogic logic, AbilityArgs args, Slot slot)
        {
            if (args.target == null)
            {
                return true;
            }
            else if (args.target is Card)
            {
                return SlotIsEmptyAfterEffectCardTarget(logic, args, slot);
            }
            else if (args.target is Player)
            {
                return SlotIsEmptyAfterEffectPlayerTarget(logic, args, slot);
            }
            else if (args.target is Slot)
            {
                return SlotIsEmptyAfterEffectSlotTarget(logic, args, slot);
            }

            return true;
        }
        
        protected virtual bool SlotIsEmptyAfterEffectCardTarget(GameLogic logic, AbilityArgs args, Slot slot)
        {
            return true; //Override in effects that remove cards from slots
        }
        
        protected virtual bool SlotIsEmptyAfterEffectSlotTarget(GameLogic logic, AbilityArgs args, Slot slot)
        {
            return true; //Override in effects that remove cards from slots
        }
        
        protected virtual bool SlotIsEmptyAfterEffectPlayerTarget(GameLogic logic, AbilityArgs args, Slot slot)
        {
            return true; //Override in effects that remove cards from slots
        }
    }
}