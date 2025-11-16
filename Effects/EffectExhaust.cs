using System.Collections;
using System.Collections.Generic;
using Monarchs.Ability;
using Monarchs.Logic;
using Monarchs.Logic.AbilitySystem;
using UnityEngine;

namespace TcgEngine
{
    /// <summary>
    /// Effect that exhaust or unexhaust a card (means it can no longer perform actions or will be able to perform another action)
    /// </summary>

    [CreateAssetMenu(fileName = "effect", menuName = "TcgEngine/Effect/Exhaust", order = 10)]
    public class EffectExhaust : EffectData
    {
        public bool exhausted;
        public bool affectCohort;
        public bool affectOnlyTriggerer;

        public override void DoEffectCardTarget(GameLogic logic, AbilityArgs args)
        {
            if (affectCohort)
            {
                var cohortCards = logic.Game.GetBoardCardsOfCohort(args.CardTarget.CohortUid);
                foreach (var card in cohortCards)
                {
                    card.exhausted = exhausted;
                }
            }
            else if (affectOnlyTriggerer)
            {
                if (args.CardTarget.GetSlot() == args.triggerer.GetSlot())
                {
                    args.CardTarget.exhausted = exhausted;
                }
            }
            else
            {
                args.CardTarget.exhausted = exhausted;
            }
        }

        public override int GetAiValue(AbilityData ability)
        {
            return exhausted ? -1 : 1;
        }
    }
}