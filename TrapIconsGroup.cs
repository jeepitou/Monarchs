using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using Monarchs.Animations;
using Monarchs.Client;
using Monarchs.Logic;
using Monarchs.Tools;
using Sirenix.OdinInspector;
using TcgEngine;
using UnityEngine;
using UnityEngine.UI;
using TrapTriggerConst = Monarchs.Animations.AnimConstants.TrapTriggered;
using TrapPlayedConst = Monarchs.Animations.AnimConstants.TrapPlayed;

namespace Monarchs
{
    public class TrapIconsGroup : MonoBehaviour
    {
        [Required]public GameObject trapPlayedGameObject;
        [Required]public GameObject trapIconPrefab;
        [Required]public GameObject trapTriggeredGameObject;

        private bool _initialized;
        private readonly List<GameObject> _trapIcons = new ();
        
        public void Start()
        {
            GameClient.Get().onCardPlayed += OnCardPlayed;
            GameClient.Get().animationManager.onTrapTriggerIconAndShowCard += OnTrapTriggered;
        }

        private void AddTrapIconsForAlreadyPlayed()
        {
            GameClient.Get().GetOpponentPlayer().cards_trap.ForEach((a) =>
            {
                AddTrapIconWithoutAnimation();
            });
    }

        void Update()
        {
            if (GameClient.GetGameData()!=null && !_initialized)
            {
                AddTrapIconsForAlreadyPlayed(); // If it's a reconnect and there is trap on the board
                _initialized = true;
            }
            LayoutRebuilder.ForceRebuildLayoutImmediate(GetComponent<RectTransform>());
        }
        
        private void OnCardPlayed(Card card, Slot slot)
        {
            bool isOpponentTrap = card.CardData.cardType == CardType.Trap && card.playerID != GameClient.Get().GetPlayerID();
            if (isOpponentTrap)
            {
                GameClient.Get().animationManager.AddToQueue(AddTrapIconWithAnimation(), gameObject);
            }
        }

        private void OnTrapTriggered(Card trap, Card triggerer)
        {
            GameClient.Get().animationManager.AddToQueue(OnTrapTriggerEnumerator(trap), gameObject);
        }
        
        private IEnumerator OnTrapTriggerEnumerator(Card trap)
        {
            if (trap.playerID != GameClient.Get().GetPlayerID())
            {
                RemoveTrapIconAnimation();
            }
            
            yield return new WaitForSeconds(TrapTriggerConst.TRAP_TRIGGER_DELAY_BEFORE_SHOWING_CARD);
            ShowTrapCard(trap);
            yield return new WaitForSeconds(TrapTriggerConst.TRAP_CARD_SHOW_DURATION);
            trapTriggeredGameObject.GetComponent<CanvasGroup>().DOFade(0, TrapTriggerConst.TRAP_CARD_FADE_OUT_DURATION);
            yield return new WaitForSeconds(TrapTriggerConst.TRAP_CARD_FADE_OUT_DURATION);
            yield return new WaitForSeconds(TrapTriggerConst.DELAY_AFTER_TRAP_CARD_HIDDEN_BEFORE_RESOLVING_TRAP);
        }

        private void RemoveTrapIconAnimation()
        {
            RectTransform rectTransform = _trapIcons[^1].transform.GetChild(0).GetComponent<RectTransform>();
            rectTransform.DOScale(Vector3.one * TrapTriggerConst.TRAP_ICON_REMOVE_INITIAL_SCALE_STRENGTH, TrapTriggerConst.TRAP_ICON_REMOVE_INITIAL_SCALE_DURATION);
            rectTransform.DOShakePosition(TrapTriggerConst.TRAP_ICON_REMOVE_SHAKE_DURATION, TrapTriggerConst.TRAP_ICON_REMOVE_SHAKE_STRENGTH, 20).OnComplete(()=>
            {
                _trapIcons[^1].GetComponent<CanvasGroup>().DOFade(0, TrapTriggerConst.TRAP_ICON_REMOVE_FADE_OUT_DURATION);
                rectTransform.DOLocalMoveY(rectTransform.localPosition.y - TrapTriggerConst.TRAP_ICON_REMOVE_MOVE_DISTANCE, TrapTriggerConst.TRAP_ICON_REMOVE_MOVE_DURATION);
                rectTransform.DOScale(Vector3.one*TrapTriggerConst.TRAP_ICON_REMOVE_SCALE_DURING_FADE_OUT_STRENGTH, TrapTriggerConst.TRAP_ICON_REMOVE_SCALE_DURING_FADE_OUT_DURATION).OnComplete(() =>
                {
                    Destroy(_trapIcons[^1]);
                    _trapIcons.RemoveAt(_trapIcons.Count - 1);
                });
            });
        }

        private void ShowTrapCard(Card trap)
        {
            trapTriggeredGameObject.GetComponent<HandCardUIManager>().SetCard(trap);
            trapTriggeredGameObject.GetComponent<CanvasGroup>().DOFade(1, AnimConstants.TrapTriggered.TRAP_CARD_FADE_IN_DURATION);
        }
        
        private void AddTrapIconWithoutAnimation()
        {
            GameObject trapIcon = Instantiate(trapIconPrefab, transform);
            trapIcon.GetComponent<CanvasGroup>().alpha = 1;
            LayoutRebuilder.ForceRebuildLayoutImmediate(GetComponent<RectTransform>());
            _trapIcons.Add(trapIcon);
            
        }
        
        private IEnumerator AddTrapIconWithAnimation()
        {
            float trapInitialScale = trapPlayedGameObject.GetComponent<RectTransform>().localScale.x;
            trapPlayedGameObject.SetActive(true);
            yield return new WaitForSeconds(TrapPlayedConst.TRAP_PLAYED_TEXT_SHOW_TIME);
            trapPlayedGameObject.GetComponent<CanvasGroup>().DOFade(0, TrapPlayedConst.TRAP_PLAYED_TEXT_FADE_OUT_TIME);
            trapPlayedGameObject.GetComponent<RectTransform>().DOScale(Vector3.zero, TrapPlayedConst.TRAP_PLAYED_SCALE_DOWN_TIME).OnComplete(() =>
            {
                ResetTrapPlayedTextProperties(trapInitialScale);
            });
            
            GameObject trapIcon = Instantiate(trapIconPrefab, transform);
            LayoutRebuilder.ForceRebuildLayoutImmediate(GetComponent<RectTransform>());
            RectTransform rectTransform = trapIcon.transform.GetChild(0).GetComponent<RectTransform>();
            
            RectTransformTools.MoveRectTransformOnTopOfAnotherRectTransform(rectTransform, trapPlayedGameObject.GetComponent<RectTransform>());
            
            trapIcon.GetComponent<CanvasGroup>().DOFade(1, TrapPlayedConst.TRAP_ICON_FADE_IN_DURATION).OnComplete(() =>
            {
                rectTransform.DOLocalMove(Vector3.zero, TrapPlayedConst.TRAP_ICON_MOVE_DURATION).SetEase(Ease.InOutBack);
            });
            
            _trapIcons.Add(trapIcon);
        }
        
        private void ResetTrapPlayedTextProperties(float initialScale)
        {
            trapPlayedGameObject.SetActive(false);
            trapPlayedGameObject.GetComponent<CanvasGroup>().alpha = 1;
            trapPlayedGameObject.GetComponent<RectTransform>().localScale = new Vector3(initialScale, initialScale, initialScale);
        }
    }
}
