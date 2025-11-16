using System.Collections;
using System.Collections.Generic;
using Monarchs.Ability;
using Monarchs.Logic;
using UnityEngine;

namespace TcgEngine
{
    //Adds slot around the target, situated below range (Used in fireball damage)

    [CreateAssetMenu(fileName = "filter", menuName = "TcgEngine/Filter/FilterRainOfArrowsKing")]
    public class FilterRainOfArrowsKing : FilterData
    {
        public FilterAddRangeSpecificMovement rookFilter;
        public FilterAddRangeSpecificMovement bishopFilter;
        public int range = 1; //Number of first targets selected

        public GameObject cross_FX;
        public GameObject X_FX;

        public override List<Card> FilterTargets(Game data, AbilityData ability, Card caster, List<Card> source, List<Card> dest)
        {
            if (Slot.IsInStraightLine(source[0].slot, caster.slot))
            {
                ability.vfxIndex = 0;
                return bishopFilter.FilterTargets(data, ability, caster, source, dest);
            }
            
            if (Slot.IsInDiagonal(source[0].slot, caster.slot))
            {
                ability.vfxIndex = 1;
                return rookFilter.FilterTargets(data, ability, caster, source, dest);
            }

            Debug.LogError("FilterRainOfArrowsKing used on illegal square.");
            return source;
        }

        public override List<Player> FilterTargets(Game data, AbilityData ability, Card caster, List<Player> source, List<Player> dest)
        {
            Debug.LogError("This Filter FilterAddRange shouldn't be used on players.");
            return dest;
        }

        public override List<Slot> FilterTargets(Game data, AbilityData ability, Card caster, List<Slot> source, List<Slot> dest)
        {
            if (Slot.IsInStraightLine(source[0], caster.slot))
            {
                ability.vfxIndex = 0;
                return bishopFilter.FilterTargets(data, ability, caster, source, dest);
            }
            
            if (Slot.IsInDiagonal(source[0], caster.slot))
            {
                ability.vfxIndex = 1;
                return rookFilter.FilterTargets(data, ability, caster, source, dest);
            }

            Debug.LogError("FilterRainOfArrowsKing used on illegal square.");
            return source;
        }
    }
}
