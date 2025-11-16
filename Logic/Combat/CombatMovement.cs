namespace Monarchs.Logic
{
    public class CombatMovement
    {
        public Slot GetKnightFallbackSquare()
        {
            return Slot.None;
        }
        
        public void MoveCardBeforeAttack(Card attacker, Slot fallbackSquare)
        {
            // Logic to move the attacker to the fallback square before attacking
        }
        
        public void MoveCardAfterAttack(Card attacker, Slot originalSquare)
        {
            // Logic to move the attacker back to the original square after attacking
        }
        
        
    }
}