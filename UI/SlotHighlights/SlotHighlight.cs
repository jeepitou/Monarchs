using System.Collections;
using System.Collections.Generic;
using Monarchs.Client;
using TcgEngine.Client;
using UnityEngine;

namespace Monarchs
{
    public abstract class SlotHighlight
    {
        protected SlotHighlightManager manager;
        protected SlotHighlightTypes highlightType;
        
        public SlotHighlight(SlotHighlightManager manager)
        {
            this.manager = manager;
            SetHighlightType();
        }
        
        public abstract void Unsubscribe();
        protected abstract void SetHighlightType();
        
        protected virtual void SetStatus(BoardSlot tile, bool newStatus)
        {
            if (tile != null)
            {
                tile.SetHighlightStatus(highlightType, newStatus);
            }
        }
        
        protected virtual void SetStatus(List<BoardSlot> tiles, bool newStatus)
        {
            foreach (BoardSlot tile in tiles)
            {
                SetStatus(tile, newStatus);
            }
        }
        
        protected bool IsYourTurn => GameClient.Get().IsYourTurn();
        
    }
}
