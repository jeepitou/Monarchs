using System.Collections;
using System.Collections.Generic;
using Monarchs.Ability;
using Monarchs.Logic;
using UnityEngine;

namespace TcgEngine
{
    /// <summary>
    ///  This filter will remove the king from the target list.
    /// </summary>
    [CreateAssetMenu(fileName = "filter", menuName = "TcgEngine/Filter/RemoveKing", order = 10)]
    public class FilterRemoveKing : FilterData
    {
        public override List<Card> FilterTargets(Game data, AbilityData ability, Card caster, List<Card> source, List<Card> dest)
        {
            foreach (var card in source)
            {
                if (card.GetPieceType() != PieceType.Monarch)
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
                if (card != null && card.GetPieceType() != PieceType.Monarch)
                {
                    dest.Add(slot);
                }

                if (card == null)
                {
                    dest.Add(slot);
                }
            }

            return dest;
        }
    }
}
