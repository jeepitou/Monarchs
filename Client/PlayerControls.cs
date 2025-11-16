using System;
using System.Linq;
using Monarchs.Board;
using Monarchs.Logic;
using TcgEngine;
using TcgEngine.UI;
using UnityEngine;
using UnityEngine.Events;

namespace Monarchs.Client
{
    /// <summary>
    /// Script that contain main controls for clicking on cards, attacking, activating abilities
    /// Holds the currently selected card and will send action to GameClient on click release
    /// </summary>

    public class PlayerControls : MonoBehaviour
    {
        public static UnityAction<Card> OnSelectBoardCard;
        public static UnityAction<Card> OnStartDragCard;
        public TextTooltip tooltip;
        private bool hasAiPlayer = false;
        private bool checkedAiPlayer = false;
        private BoardElement _lastClickedCard;
        private BoardElement _draggedCard;

        private static PlayerControls _instance;

        void Awake()
        {
            _instance = this;
            BoardInputManager.Instance.OnClickRelease += OnSlotClickRelease;
            BoardInputManager.Instance.OnClick += OnClick;
            
        }

        private void Start()
        {
            GameClient.Get().onRefreshAll += () =>
            {
                SetSelectedCard(_lastClickedCard); //To refresh exhausted state
            };
        }

        private void OnClick(BoardSlot slot, Card card)
        {
            if (GameClient.GetGameData().selector != SelectorType.None)
            {
                return;
            }
            
            if ((card == null || slot == null) 
                && AbilityButton.hoveredAbilityButton == null)
            {
                SetSelectedCard(null);
            }
            
        }
        
        void Update()
        {
            if (!GameClient.Get().IsReady())
                return;

            if (!checkedAiPlayer)
            {
                hasAiPlayer = GameClient.GetGameData().HasAIPlayer();
                checkedAiPlayer = true;
            }

            if (Input.GetMouseButtonDown(1))
            {
                SetSelectedCard(null);
                UnselectAll();
            }
                    

            if (GameClient.GetGameData().selector != SelectorType.None)
            {
                if (Input.GetMouseButtonUp(1))
                {
                    GameClient.Get().CancelSelection();
                }
            }

            bool yourturn = GameClient.Get().IsYourTurn();
            bool canFinishTurn = yourturn || hasAiPlayer;

            if (Input.GetKeyDown(KeyCode.Space) && canFinishTurn)
            {
                GameClient.Get().EndTurn();
            }
        }
        
        public void SelectCard(Card card)
        {
            if (GameClient.GetGameData().selector == SelectorType.None)
            {
                SetSelectedCard(BoardElement.Get(card.uid));
            }
            
            int playerID = GameClient.Get().GetPlayerID();
            bool isYourTurn = GameClient.Get().IsYourTurn();
            Game gdata = GameClient.GetGameData();
            bool isSelectTarget = gdata.selector == SelectorType.SelectTarget || gdata.selector == SelectorType.SelectMultipleTarget;

            if (isSelectTarget && playerID == gdata.selectorPlayer)
            {
                //Target selector, select this card
                GameClient.Get().SelectCard(card);
            }
            else if ((gdata.selector == SelectorType.SelectCaster || gdata.selector == SelectorType.SelectRangeAttacker) && playerID == gdata.selectorPlayer)
            {
                Card caster = card;
                if (gdata.selectorPotentialCasters.Contains(caster.uid))
                {
                    GameClient.Get().SelectCaster(caster);
                }
            }
            else if (gdata.State == GameState.Play && gdata.selector == SelectorType.None && isYourTurn)
            {
                bool selectedCardTurn = gdata.GetCurrentCardTurn().Any(c => c.uid == _lastClickedCard.GetCard().uid);
                bool playedCardThisRound = gdata.GetCard(_lastClickedCard.GetCard().uid).playedCardThisRound;
                
                if (selectedCardTurn && !playedCardThisRound && !_lastClickedCard.GetCard().exhausted && !GameplayData.Get().canPlayCardAfterMove)
                {
                    tooltip.ShowTooltip("You won't be able to play a card with this piece after moving it");
                }

            }
            SetDraggedCard(BoardElement.Get(card.uid));
        }

        private void ReleaseClick()
        {
            tooltip.HideTooltip();
            bool isYourTurn = GameClient.Get().IsYourTurn();
            Game gdata = GameClient.GetGameData();

            if (!isYourTurn)
            {
                BoardSlot lastHoveredSlot = BoardInputManager.Instance.GetLastHoveredSlot();
                if (_draggedCard != null && lastHoveredSlot != BoardSlot.Get(_draggedCard.GetCard().slot))
                {
                    ShowNotPieceTurn();
                }
                return;
            }
            
            AbilityButton ability = AbilityButton.hoveredAbilityButton;
            if (ability != null && ability.IsVisible())
            {
                ability.OnClick();
                return;
            }
            
            if (_draggedCard == null)
            {
                return;
            }

            bool draggedCardTurn = gdata.GetCurrentCardTurn().Any(c => c.uid == _draggedCard.GetCard().uid);
            BoardSlot targetSlot = BoardInputManager.Instance.GetLastHoveredSlot();
            Card cardTarget = targetSlot ? gdata.GetSlotCard(targetSlot.GetSlot()) : null;
            if (!draggedCardTurn)
            {
                if (_draggedCard.GetCard().uid == cardTarget?.uid && !GameplayData.Get().DragAndDropRangedUnitWillDoARangedAttack)
                {
                    GameClient.Get().RangeAttackTarget(null, cardTarget);
                }
                else
                {
                    ShowNotPieceTurn();
                }
                return;
            }
            
            if (cardTarget != null && cardTarget.uid != _draggedCard.GetCardUID())
            {
                if (_draggedCard.GetCard().exhausted)
                    WarningText.ShowExhausted();
                else
                    GameClient.Get().AttackTarget(_draggedCard.GetCard(), cardTarget);
            }
            else if (targetSlot != null)
            {
                if (_draggedCard.GetCard().exhausted)
                    WarningText.ShowExhausted();
                else
                    GameClient.Get().Move(_draggedCard.GetCard(), targetSlot.GetSlot());
            }
        }

        void ShowNotPieceTurn()
        {
            if (_lastClickedCard.GetCard().playerID == GameClient.Get()?.GetPlayerID())
            {
                WarningText.ShowNotThisPieceTurn();
            }
            else
            {
                WarningText.ShowCantPlayEnemyPiece();
            }
        }

        public void OnSlotClickRelease(BoardSlot slot, Card card)
        {
            if (_draggedCard == null && AbilityButton.hoveredAbilityButton == null)
                return;
            
            if (_draggedCard != null && (_draggedCard.GetCard().uid == card?.uid)) // Clicked a slot without dragging
            {
                SetDraggedCard(null);
                return;
            }
            
            ReleaseClick();
            SetDraggedCard(null);
        }

        public void UnselectAll()
        {
            SetDraggedCard(null);
            HandCard.UnselectAll();
        }
        
        private void SetSelectedCard(BoardElement boardCard)
        {
            _lastClickedCard = boardCard;
            OnSelectBoardCard?.Invoke(boardCard?.GetCard());
        }
        
        private void SetDraggedCard(BoardElement boardCard)
        {
            _draggedCard = boardCard;
            OnStartDragCard?.Invoke(boardCard?.GetCard());
        }
        
        public BoardElement GetDragged()
        {
            return _draggedCard;
        }

        public static PlayerControls Get()
        {
            return _instance;
        }
    }
}