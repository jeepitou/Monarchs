using Monarchs.Logic;

namespace Monarchs.Ability.Target
{
    public interface ITargetable
    {
        bool CanBeTargeted();
        int GetPlayerId();
        Slot GetSlot();
    }

    public static class Targetable
    {
        public static bool IsValid(ITargetable target)
        {
            if (target is Slot)
            {
                return ((Slot) target).IsValid();
            }
            else
            {
                return target != null;
            }
        }
    }
}