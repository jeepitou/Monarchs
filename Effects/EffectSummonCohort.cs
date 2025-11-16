using System.Collections;
using System.Collections.Generic;
using Monarchs.Ability;
using Monarchs.Logic;
using Monarchs.Logic.AbilitySystem;
using UnityEngine;

namespace TcgEngine
{

    /// <summary>
    /// This effect needs to be on any card that has a cohortSize greater than 1.
    /// </summary>
    [CreateAssetMenu(fileName = "effect", menuName = "TcgEngine/Effect/SummonCohort", order = 10)]
    public class EffectSummonCohort : EffectData
    {
        public override void DoEffectSlotTarget(GameLogic logic, AbilityArgs args)
        {
            if (!args.castedCard.cohortSummon)
            {
                logic.SummonCard(args.castedCard.playerID, args.castedCard.CardData, args.castedCard.VariantData, args.SlotTarget, true, args.castedCard.CohortUid);
            }
        }
    }
}