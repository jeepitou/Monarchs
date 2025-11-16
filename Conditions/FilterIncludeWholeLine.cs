using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Monarchs.Ability;
using Monarchs.Ability.Target;
using Monarchs.Logic;
using UnityEngine;

namespace TcgEngine
{
    /// <summary>
    /// This filter will add all the slot on a row or a column to the target list
    /// </summary>
    [CreateAssetMenu(fileName = "filter", menuName = "TcgEngine/Filter/IncludeWholeLine", order = 10)]
    public class FilterIncludeWholeLine : FilterData
    {
        public BoardLine lineToCheck;

        public override List<ITargetable> FilterTargets(Game game, AbilityData ability, Card caster, List<ITargetable> source, List<ITargetable> dest)
        {
            if (source[0] is Card)
            {
                List<Slot> sourceSlot = Slot.GetSlotsOfTargets(source);
                List<Slot> destSlot = Slot.GetSlotsOfTargets(dest);
                return (List<ITargetable>)FilterTargets(game, ability, caster, sourceSlot, destSlot).ConvertAll(x => (ITargetable)x);
            }
            else if (source[0] is Slot)
            {
                List<Slot> sourceSlot = source.OfType<Slot>().ToList();
                List<Slot> destSlot = dest.OfType<Slot>().ToList();
                return (List<ITargetable>)FilterTargets(game, ability, caster, sourceSlot, destSlot).ConvertAll(x => (ITargetable)x);
            }
            else
            {
                throw new ArgumentException("Unsupported target type");
            }
        }

        public override List<Slot> FilterTargets(Game data, AbilityData ability, Card caster, List<Slot> source, List<Slot> dest)
        {
            if (lineToCheck == BoardLine.Row)
            {
                for (int x = 0; x <= 7; x++)
                {
                    dest.Add(Slot.Get(x, source[0].y));
                }
            }
            else
            {
                for (int y = 0; y <= 7; y++)
                {
                    dest.Add(Slot.Get(source[0].x, y));
                }
            }
            return dest;
        }
    }

    public enum BoardLine
    {
        Row,
        Column
    }
}
