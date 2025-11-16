using System.Collections;
using System.Collections.Generic;
using Monarchs.Ability;
using Monarchs.Logic;
using Sirenix.Utilities;
using UnityEngine;

namespace TcgEngine
{
    //Adds slot around the target, situated below range (Used in fireball damage)

    [CreateAssetMenu(fileName = "filter", menuName = "TcgEngine/Filter/FilterAddRangeSpecificMovement")]
    public class FilterAddRangeSpecificMovement : FilterData
    {
        public MovementScheme movementScheme;
        public int range = 1; //Number of first targets selected
        public bool includeEnemy = false;
        public bool includeAlly = false;

        public override List<Card> FilterTargets(Game data, AbilityData ability, Card caster, List<Card> source, List<Card> dest)
        {
            foreach (var card in source)
            {
                Slot slot = card.slot;
                foreach (var slotCoordinate in movementScheme.GetAllSquaresOnMovementScheme(slot.GetCoordinate(), range, data, caster.playerID))
                {
                    dest.Add(data.GetSlotCard(Slot.Get(slotCoordinate.x, slotCoordinate.y)));
                }
            }
            dest.AddRange(source);
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
                foreach (var slotCoordinate in movementScheme.GetAllSquaresOnMovementScheme(slot.GetCoordinate(), range, data, caster.playerID))
                {
                    Slot targetSlot = Slot.Get(slotCoordinate.x, slotCoordinate.y);

                    if (ValidateOwner(data, caster, targetSlot))
                    {
                        dest.Add(targetSlot);
                    }
                }

                if (ValidateOwner(data, caster, slot))
                {
                    dest.Add(slot);
                }
            }
            
            return dest;
        }

        bool ValidateOwner(Game game, Card caster, Slot slot)
        {
            Card card = game.GetSlotCard(slot);
            
            if (card != null)
            {
                bool isAllyAndInclude = card.playerID == caster.playerID && includeAlly;
                bool isEnemyAndInclude = card.playerID != caster.playerID && includeEnemy;
                if (isAllyAndInclude || isEnemyAndInclude)
                {
                    return true;
                }
            }
            else
            {
                return true;
            }

            return false;
        }
    }
}
