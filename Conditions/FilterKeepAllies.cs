using System.Collections;
using System.Collections.Generic;
using Monarchs.Ability;
using Monarchs.Logic;
using UnityEngine;

namespace TcgEngine
{
    /// <summary>
    /// This filter will only keep the allies and the empty slots in the target list.
    /// If isAlly is set to false, it will only keep the enemies and the empty slots.
    /// </summary>
    [CreateAssetMenu(fileName = "filter", menuName = "TcgEngine/Filter/KeepAllies", order = 10)]
    public class FilterKeepAllies : FilterData
    {
        public bool isAlly = true;

        public override List<Card> FilterTargets(Game data, AbilityData ability, Card caster, List<Card> source, List<Card> dest)
        {
            foreach (var card in source)
            {
                if ((card.playerID == caster.playerID) == isAlly)
                {
                    dest.Add(card);
                }
            }
            return dest;
        }

        public override List<Slot> FilterTargets(Game data, AbilityData ability, Card caster, List<Slot> source, List<Slot> dest)
        {
            foreach (var slot in source)
            {
                Card card = data.GetSlotCard(slot);

                if (card != null)
                {
                    if ((card.playerID == caster.playerID) == isAlly)
                    {
                        dest.Add(slot);
                    }
                }
                else
                {
                    dest.Add(slot);
                }
            }

            return dest;
        }
    }
}
