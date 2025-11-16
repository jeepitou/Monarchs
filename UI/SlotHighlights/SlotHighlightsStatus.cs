using System;
using System.Collections.Generic;

namespace Monarchs
{
    public class SlotHighlightsStatus
    {
        public Dictionary<SlotHighlightTypes, bool> statusDict = new Dictionary<SlotHighlightTypes, bool>();
        
        public SlotHighlightsStatus()
        {
            Init();
        }
        
        public void SetStatus(SlotHighlightTypes type, bool status)
        {
            statusDict[type] = status;
        }

        private void Init()
        {
            foreach (SlotHighlightTypes value in Enum.GetValues(typeof(SlotHighlightTypes)))
            {
                statusDict[value] = false;
            }
        }
    }

    public enum SlotHighlightTypes
    {
        HoveredByPlayer, 
        HoveredByOpponent, 
        IsFallBackSquare, 
        IsSelectorPotentialChoice, 
        IsDraggedHandCardPotentialChoice, 
        IsInSpellAOE, 
        IsInSelfSpell,
        IsInSplashDamageArea, 
        IsLegalMeleeAttack, 
        IsLegalRangedAttackOfHoveredCard,
        IsLegalRangedAttackOfCurrentTurn,
        DraggedHandCardLegalMove,
        HoveredCardLegalMove, 
        HoveredInInitiativeCard,
        RealDestination,
        DraggedCardLegalMove,
        PieceDiedOnSlot,
        IsSelected
    }
    
}