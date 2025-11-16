using Monarchs.Ability;
using Monarchs.Logic;
using Monarchs.Logic.AbilitySystem;
using TcgEngine;
using UnityEngine;

namespace Monarchs
{
    [CreateAssetMenu(fileName = "effect", menuName = "TcgEngine/Effect/EffectRaiseLastPieceThatDiedOnSlot", order = 10)]
    public class EffectRaiseLastPieceThatDiedOnSlot : EffectData
    {
        public AbilityData abilityToAddToRaisedPiece;
        public SubtypeData subtypeToAdd;
        public bool removeAllAbilities;
        
        public override void DoEffectSlotTarget(GameLogic logic, AbilityArgs args)
        {
            Card card = logic.Game.GetMostRecentDiedCardOnSlot(args.SlotTarget);

            if (card == null)
            {
                return;
            }

            logic.ResurrectCard(card, card.slot, args.caster.playerID);
            
            if (subtypeToAdd != null)
            {
                card.addedSubtypesSinceInPlayID.Add(subtypeToAdd.id);
            }

            if (removeAllAbilities)
            {
                card.AddStatus(StatusType.LostAbilities,0,0);
            }
            
            if (abilityToAddToRaisedPiece != null)
            {
                card.addedAbilitiesSinceInPlayID.Add(abilityToAddToRaisedPiece.id);

                if (abilityToAddToRaisedPiece.trigger == AbilityTrigger.OnPlay)
                {
                    logic._abilityLogicSystem.TriggerCardAbilityType(AbilityTrigger.OnPlay, new AbilityArgs()
                    {
                        castedCard = card,
                        caster = card
                    });
                }
            }
        }
    }
}
