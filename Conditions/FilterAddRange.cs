using System.Collections;
using System.Collections.Generic;
using Monarchs.Ability;
using Monarchs.Logic;
using UnityEngine;

namespace TcgEngine
{
    //Adds slot around the target, situated below range (Used in fireball damage)

    [CreateAssetMenu(fileName = "filter", menuName = "TcgEngine/Filter/AddRange", order = 10)]
    public class FilterAddRange : FilterData
    {
        public int range = 1; //Number of first targets selected

        public override List<Card> FilterTargets(Game data, AbilityData ability, Card caster, List<Card> source, List<Card> dest)
        {
            foreach (var card in source)
            {
                Slot slot = card.slot;
                foreach (var slotInRange in slot.GetSlotsInRange(range))
                {
                    dest.Add(data.GetSlotCard(slotInRange));
                    
                }
            }
            return dest;
        }

        public override List<Player> FilterTargets(Game data, AbilityData ability, Card caster, List<Player> source, List<Player> dest)
        {
            Debug.LogError("This Filter FilterAddRange shouldn't be used on players.");
            return dest;
        }

        public override List<Slot> FilterTargets(Game data, AbilityData ability, Card caster, List<Slot> source, List<Slot> dest)
        {
            foreach (var slot in source)
            {
                dest.AddRange(slot.GetSlotsInRange(range));
            }

            return dest;
        }
    }
}
