using System.Collections;
using System.Collections.Generic;
using Monarchs.Ability;
using Monarchs.Logic;
using UnityEngine;

namespace TcgEngine
{
    /// <summary>
    ///  This filter will remove the caster from the target list.
    /// </summary>
    [CreateAssetMenu(fileName = "filter", menuName = "TcgEngine/Filter/RemoveCaster", order = 10)]
    public class FilterRemoveCaster : FilterData
    {
        public override List<Card> FilterTargets(Game data, AbilityData ability, Card caster, List<Card> source, List<Card> dest)
        {
            foreach (var card in source)
            {
                if (card.uid != caster.uid)
                {
                    dest.Add(card);
                }
            }
            return dest;
        }

        public override List<Slot> FilterTargets(Game data, AbilityData ability, Card caster, List<Slot> source, List<Slot> dest)
        {
            Slot casterSlot = caster.slot;
            foreach (var slot in source)
            {
                if (slot != casterSlot)
                {
                    dest.Add(slot);
                }
            }

            return dest;
        }
    }
}
