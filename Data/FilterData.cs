using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Monarchs;
using Monarchs.Ability;
using Monarchs.Ability.Target;
using Monarchs.Logic;
using UnityEngine;

namespace TcgEngine
{
    /// <summary>
    /// Base class for target filters
    /// Let you filter targets after they have already been picked by conditions but before effects are applied
    /// </summary>

    public class FilterData : ScriptableObject
    {
        public virtual List<ITargetable> FilterTargets(Game game, AbilityData ability, Card caster, List<ITargetable> source, List<ITargetable> dest)
        {
            if (source[0] is Card)
            {
                List<Card> sourceCard = source.OfType<Card>().ToList();
                List<Card> destCard = dest.OfType<Card>().ToList();
                return (List<ITargetable>)FilterTargets(game, ability, caster, sourceCard, destCard).ConvertAll(x => (ITargetable)x);
            }
            else if (source[0] is Player)
            {
                List<Player> sourcePlayer = source.OfType<Player>().ToList();
                List<Player> destPlayer = dest.OfType<Player>().ToList();
                return (List<ITargetable>)FilterTargets(game, ability, caster, sourcePlayer, destPlayer).ConvertAll(x => (ITargetable)x);
            }
            else if (source[0] is Slot)
            {
                List<Slot> sourceSlot = source.OfType<Slot>().ToList();
                List<Slot> destSlot = dest.OfType<Slot>().ToList();
                return (List<ITargetable>)FilterTargets(game, ability, caster, sourceSlot, destSlot).ConvertAll(x => (ITargetable)x);
            }
            else if (source[0] is CardData)
            {
                List<CardData> sourceCard = source.OfType<CardData>().ToList();
                List<CardData> destCard = dest.OfType<CardData>().ToList();
                return (List<ITargetable>)FilterTargets(game, ability, caster, sourceCard, destCard).ConvertAll(x => (ITargetable)x);
            }
            else
            {
                throw new ArgumentException("Unsupported target type");
            }
        }
        
        public virtual List<Card> FilterTargets(Game data, AbilityData ability, Card caster, List<Card> source, List<Card> dest)
        {
            return source; //Override this, filter targeting card
        }

        public virtual List<Player> FilterTargets(Game data, AbilityData ability, Card caster, List<Player> source, List<Player> dest)
        {
            return source; //Override this, filter targeting player
        }

        public virtual List<Slot> FilterTargets(Game data, AbilityData ability, Card caster, List<Slot> source, List<Slot> dest)
        {
            return source; //Override this, filter targeting slot
        }

        public virtual List<CardData> FilterTargets(Game data, AbilityData ability, Card caster, List<CardData> source, List<CardData> dest)
        {
            return source; //Override this, for filters that create new cards
        }
    }
}
