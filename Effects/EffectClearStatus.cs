using System.Collections;
using System.Collections.Generic;
using Monarchs.Ability;
using Monarchs.Logic;
using Monarchs.Logic.AbilitySystem;
using UnityEngine;

namespace TcgEngine
{
    /// <summary>
    /// Effect that removes a status,
    /// Will remove all status if the public field is empty
    /// </summary>

    [CreateAssetMenu(fileName = "effect", menuName = "TcgEngine/Effect/ClearStatus", order = 10)]
    public class EffectClearStatus : EffectData
    {
        public StatusData status;

        public override void DoEffectPlayerTarget(GameLogic logic, AbilityArgs args)
        {
            if (status != null)
                args.PlayerTarget.RemoveStatus(status.effect);
            else
                args.PlayerTarget.status_effects.Clear();
        }

        public override void DoEffectCardTarget(GameLogic logic, AbilityArgs args)
        {
            if (status != null)
                args.CardTarget.RemoveStatus(status.effect);
            else
                args.CardTarget.status.Clear();
        }
    }
}