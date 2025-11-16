using TcgEngine;

namespace Monarchs.Logic
{
    public class CombatDamage
    {
        public static int GetDamage(Card attacker, Slot targetSlot)
        {
            //Count attack damage
            int damage = attacker.GetAttack();

            // If attacker is a range unit in melee range, we halve its damage.
            if (attacker.GetMaxAttackRange() > 1 && attacker.slot.GetDistanceTo(targetSlot) <= 1 && !attacker.HasTrait("no_melee_penalty"))
            {
                damage = (int)System.Math.Ceiling(((decimal)damage / 2));
            }

            if (attacker.HasStatus(StatusType.Sabotage))
            {
                return 0;
            }
            return damage;
        }
    }
}