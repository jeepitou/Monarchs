using System.Collections;
using System.Collections.Generic;
using Monarchs.Ability;
using Monarchs.Client;
using Monarchs.Logic;
using TcgEngine.FX;
using UnityEngine;

namespace Monarchs
{
    public class AbilityFXMultipleTarget : MonoBehaviour
    {
        public AbilityFXTarget target;
        protected GameObject GetTargetGameObject()
        {
            GameBoardFX gameBoardFX = GameBoardFX.Get();

            if (target == AbilityFXTarget.Caster)
            {
                Card caster = gameBoardFX.abilityArgs.caster;
                if (caster == null)
                {
                    return null;
                }

                return BoardCard.Get(caster.uid).gameObject; 
            }
            else if (target == AbilityFXTarget.ALly_Monarch)
            {
                Card caster = gameBoardFX.abilityArgs.caster;
                if (caster == null)
                {
                    return null;
                }

                Player player = GameClient.GetGameData().GetPlayer(caster.playerID);
                Card monarch = player.king;
                if (monarch == null)
                {
                    return null;
                }

                return BoardCard.Get(monarch.uid).gameObject; 
            }
            else 
            {
                int index = AbilityData.GetTargetIndex((EffectTarget)target);
                Slot slot = gameBoardFX.targetSlots[index];
                Card card = GameClient.GetGameData().GetSlotCard(slot);
                if (card == null)
                {
                    return BoardSlot.Get(slot).gameObject;
                }
                else
                {
                    return BoardCard.Get(card.uid).gameObject;
                }
            }
        }

        
        public enum AbilityFXTarget {
            
            Target_1 = EffectTarget.Target_1,
            Target_2 = EffectTarget.Target_2,
            Target_3 = EffectTarget.Target_3,
            Target_4 = EffectTarget.Target_4,
            Target_5 = EffectTarget.Target_5,
            Caster,
            ALly_Monarch
        }
    }
}
