using System.Collections.Generic;
using System.Linq;
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
            dest.AddRange(source.Where(card => card.uid != caster.uid));
            return dest;
        }

        public override List<Slot> FilterTargets(Game data, AbilityData ability, Card caster, List<Slot> source, List<Slot> dest)
        {
            Slot casterSlot = caster.slot;
            dest.AddRange(source.Where(slot => slot != casterSlot));
            return dest;
        }
    }
}
