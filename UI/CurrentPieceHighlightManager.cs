using System;
using DG.Tweening;
using Monarchs.Ability;
using Monarchs.Client;
using Monarchs.Logic;
using UnityEngine;
using UnityEngine.UI;
using Outline = UI.Outline;

namespace Monarchs.UI
{
    public class CurrentPieceHighlightManager : MonoBehaviour
    {
        public GameObject currentTurnArrowPrefab;
        public Outline outline;

        public float outlineWidth;
        public float selectedOutlineWidth;
        public float otherPieceSelectedOutlineWidth;
        public float animationTime;

        public ColorSO allyColorSO;
        public ColorSO enemyColorSO;
        public ColorSO selectedColorSO;
        public ColorSO enemySelectedColorSO;

        private Card _card;
        private bool _selected = false;
        private bool _appeared;
        private Tweener _outlineWidthTweener;
        private Tweener _outlineColorTweener;
        private GameObject _currentTurnArrow;
        private Image _currentTurnArrowImage;

        private void Start()
        {
            PlayerControls.OnSelectBoardCard += OnSelectBoardCard;
        }

        private void OnSelectBoardCard(Card card)
        {
            if (card!=null && _card?.uid == card.uid && !card.exhausted)
            {
                _selected = true;
                Color color = card.playerID == GameClient.Get().GetPlayerID() ? selectedColorSO.color : enemySelectedColorSO.color;
                SetOutlineColor(color);
            }
            else if (_selected)
            {
                _selected = false;
                UpdateOutline(GameClient.GetGameData(), _card);
            }
            if (card != null && !_selected)
            {
                RefreshOutline(true);
            }
            else
            {
                RefreshOutline(false);
            }
        }

        

        public void UpdateOutline(Game game, Card card)
        {
            if (_selected && _appeared)
            {
                return;
            }
            
            _card = card;
            if (card == null || card.exhausted)
            {
                RemoveOutline();
                return;
            }

            if (game.GetCurrentCardTurn().Contains(card))
            {
                ActivateOutline();
                bool hasAbility = card.CardData.HasAbility(AbilityTrigger.Activate);
                
                Color color = card.playerID == GameClient.Get().GetPlayerID() ? allyColorSO.color : enemyColorSO.color;
                
                SetOutlineColor(color, hasAbility);
            }
            else
            {
                RemoveOutline();
                _outlineColorTweener?.Kill();
            }
        }

        private void RemoveOutline()
        {
            _outlineWidthTweener.Kill();
            if (!_appeared)
                return;

            _appeared = false;
            DOVirtual.Float(outlineWidth, 0, animationTime/2, SetOutlineWidth).onComplete = () =>
            {
                outline.enabled = false;
            };
        }

        private void ActivateOutline()
        {
            if (_appeared)
                return;
            
            _appeared = true;
            
            float width = _selected ? selectedOutlineWidth : outlineWidth;
            SetOutlineWidth(0);
            outline.enabled = true;
            DOVirtual.Float(0, width, animationTime, SetOutlineWidth).onComplete = () =>
            {
                _outlineWidthTweener = DOVirtual.Float(width, .8f * width, animationTime / 2, SetOutlineWidth)
                    .SetLoops(-1, LoopType.Yoyo);
            };
            
        }
        
        private void RefreshOutline(bool otherPieceSelected)
        {
            _outlineWidthTweener.Kill();
            float width;
            if (otherPieceSelected && !_selected)
            {
                width = otherPieceSelectedOutlineWidth;
            }
            else
            {
                width = _selected ? selectedOutlineWidth : outlineWidth;
            }
            
            DOVirtual.Float(outline.OutlineWidth, width, animationTime / 2, SetOutlineWidth).onComplete = () =>
            {
                _outlineWidthTweener = DOVirtual.Float(width, .8f * width, animationTime / 2, SetOutlineWidth)
                    .SetLoops(-1, LoopType.Yoyo);
            };
        }

        private void SetOutlineColor(Color color, bool hasAbility = false)
        {
            outline.OutlineColor = color;
        }

        private void SetOutlineWidth(float width)
        {
            outline.OutlineWidth = width;
        }
        
        public void UnSelectCard(Game game, Card card)
        {
            UpdateOutline(game, card);
        }
    }
}
