using System.Collections;
using System.Collections.Generic;
using Monarchs.Ability;
using Monarchs.Logic;
using Sirenix.Utilities;
using UnityEngine;

namespace TcgEngine
{
    //Adds slot around the target, situated below range (Used in fireball damage)

    [CreateAssetMenu(fileName = "filter", menuName = "TcgEngine/Filter/FilterHolyWord")]
    public class FilterHolyWord : FilterData
    {
        public override List<Card> FilterTargets(Game data, AbilityData ability, Card caster, List<Card> source, List<Card> dest)
        {
            dest.AddRange(source);
            foreach (var card in source)
            {
                Vector2S target = card.slot.GetCoordinate();
                Vector2S direction = GetUnitDirection(target, caster.slot.GetCoordinate());
                Vector2S perpendicularDirect = direction.GetPerpendicular();
                
                dest.Add(GetSlotCard(data, target + direction));
                Vector2S center = target + direction + direction;
                dest.Add(GetSlotCard(data, center));
                dest.Add(GetSlotCard(data, center + direction));
                dest.Add(GetSlotCard(data, center + perpendicularDirect));
                dest.Add(GetSlotCard(data, center - perpendicularDirect));
            }
            
            
            return dest;
        }

        public override List<Slot> FilterTargets(Game data, AbilityData ability, Card caster, List<Slot> source,
            List<Slot> dest)
        {
            foreach (var slot in source)
            {
                Vector2S target = slot.GetCoordinate();
                Vector2S direction = GetUnitDirection(target, caster.slot.GetCoordinate());
                Vector2S perpendicularDirect = direction.GetPerpendicular();

                dest.Add(Slot.Get(target + direction));
                Vector2S center = target + direction + direction;
                dest.Add(Slot.Get(center));
                dest.Add(Slot.Get(center + direction));
                dest.Add(Slot.Get(center + perpendicularDirect));
                dest.Add(Slot.Get(center - perpendicularDirect));
            }

            dest.AddRange(source);
            return dest;
        }

        private Vector2S GetUnitDirection(Vector2S from, Vector2S to)
        {
            Vector2S direction = from - to;

            return direction.ToUnit();
        }
        
        private Card GetSlotCard(Game game, Vector2S vector)
        {
            return game.GetSlotCard(Slot.Get(vector.x, vector.y));
        }
    }
}
