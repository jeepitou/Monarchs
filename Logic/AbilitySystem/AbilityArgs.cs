using System;
using Monarchs.Ability;
using Monarchs.Ability.Target;
using TcgEngine;

namespace Monarchs.Logic.AbilitySystem
{
    public class AbilityArgs
    {
        public AbilityData ability;
        public Card caster;
        public Card castedCard;
        public ITargetable target;
        public ITargetable triggerer;
        public Card CardTarget => (Card) target;
        public Slot SlotTarget => (Slot) target;
        public CardData CardDataTarget => (CardData) target;
        public Player PlayerTarget => (Player) target;
        public PlayerMana.ManaType manaType;

        public AbilityArgs()
        {
        }

        public AbilityArgs(AbilityData ability, Card caster, Card castedCard, ITargetable target, ITargetable triggerer, PlayerMana.ManaType manaType = PlayerMana.ManaType.None)
        {
            this.ability = ability;
            this.caster = caster;
            this.castedCard = castedCard;
            this.target = target;
            this.triggerer = triggerer;
            this.manaType = manaType;
        }

        public AbilityArgs Clone()
        {
            return new (ability, caster, castedCard, target, triggerer, manaType);
        }
        
        protected bool Equals(AbilityArgs other)
        {
            return Equals(ability, other.ability) && Equals(caster, other.caster) && Equals(castedCard, other.castedCard) && Equals(target, other.target) && Equals(triggerer, other.triggerer) && manaType == other.manaType;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((AbilityArgs) obj);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(ability, caster, castedCard, target, triggerer, manaType);
        }
    }
}