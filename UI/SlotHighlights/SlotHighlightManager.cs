using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Monarchs.Client;
using Monarchs.Logic;
using TcgEngine.Client;
using UnityEngine;

namespace Monarchs
{
    public class SlotHighlightManager
    {
        private List<SlotHighlight> _highlights = new List<SlotHighlight>();
        public BoardSlot LastHoveredSlot { get; private set; }
        public Card DraggedHandCard { get; private set; }
        public Card DraggedBoardCard { get; private set; }
        
        public Action<BoardSlot, Card> onSlotHovered;
        public Action<Card> onInitiativeCardHovered;
        public Action<Card> onDraggedHandCardChanged;
        public Action<Card> onDraggedBoardCardChanged;
        
        public SlotHighlightManager()
        {
            CreateAllHighlightClasses();
            Subscribe();
            Debug.Log("SlotHighlightManager created");
        }
        
        public void Subscribe()
        {
            BoardInputManager.Instance.OnHover += OnHover;
            BoardInputManager.Instance.OnInitiativeCardHover += OnHoverInitiativeCard;
            PlayerControls.OnStartDragCard += OnStartDragCard;
            HandCard.OnChangeDraggedCard += OnChangeDraggedHandCard;
            BoardInputManager.Instance.OnClickRelease += OnClickRelease;
        }

        private void OnClickRelease(BoardSlot tile, Card card)
        {
            DraggedBoardCard = null;
            onDraggedBoardCardChanged?.Invoke(null);
        }

        private void OnStartDragCard(Card card)
        {
            DraggedBoardCard = card;
            onDraggedBoardCardChanged?.Invoke(card);
        }

        private void OnChangeDraggedHandCard(Card card)
        {
            DraggedHandCard = card;
            onDraggedHandCardChanged?.Invoke(card);
        }

        private void OnHover(BoardSlot slot, Card card)
        {
            LastHoveredSlot = slot;
            onSlotHovered?.Invoke(slot, card);
        }
        
        private void OnHoverInitiativeCard(Card card)
        {
            onInitiativeCardHovered?.Invoke(card);
        }
        
        public void Unsubscribe()
        {
            foreach (var highlight in _highlights)
            {
                highlight.Unsubscribe();
            }
            
            BoardInputManager.Instance.OnHover -= OnHover;
            PlayerControls.OnSelectBoardCard -= OnStartDragCard;
            HandCard.OnChangeDraggedCard -= OnChangeDraggedHandCard;
        }

        private void CreateAllHighlightClasses()
        {
            List<Type> allSubTypes = new List<Type>();
            
            foreach(var assem in AppDomain.CurrentDomain.GetAssemblies())
            {
                var subTypes = assem.GetTypes().Where(x => x.BaseType == typeof(SlotHighlight));

                allSubTypes.AddRange(subTypes);
            }
            
            foreach (var type in allSubTypes)
            {
                _highlights.Add(Activator.CreateInstance(type, this) as SlotHighlight);
            }
        }
        
    }
}