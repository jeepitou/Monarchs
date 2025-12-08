using System.Collections;
using System.Collections.Generic;
using Monarchs.Client;
using Monarchs.Logic;
using TcgEngine;
using TcgEngine.UI;
using UnityEngine;
using UnityEngine.Events;

namespace Monarchs.Board
{
    public abstract class BoardElement : MonoBehaviour
    {
        public UnityAction onKill;
        public bool promoted;
        
        
        protected string _cardUID = "";
        protected bool _destroyed;
        protected bool _focus;
        protected float _statusAlphaTarget;
    
        private static readonly List<BoardElement> CardList = new ();
        
        protected virtual void Awake()
        {
            CardList.Add(this);
        }
        
        protected virtual void OnDestroy()
        {
            CardList.Remove(this);
        }
    
        protected Vector3 GetTargetPos()
        {
            return BoardManager.Instance.GetPositionFromCoordinate(new Vector2S(GetSlot().x, GetSlot().y));
        }
    
        public void Kill(bool triggerVFX = true)
        {
            if (!_destroyed)
            {
                _destroyed = true;
                GameClient.Get().animationManager.AddToQueue(KillCoroutine(triggerVFX), gameObject);
                if (onKill != null && triggerVFX)
                {
                    onKill.Invoke();
                }
            }
        }

        public IEnumerator KillCoroutine(bool triggerVFX = true)
        {
            Game data = GameClient.GetGameData();
            Card card = data.GetCard(_cardUID);

            _destroyed = true;
            _statusAlphaTarget = 0f;
            
            card.hp = 0;

            if (!triggerVFX)
            {
                yield return new WaitForSeconds(0.3f);
                Destroy(gameObject);
            }
        }
    
        public bool IsDead()
        {
            return _destroyed;
        }

        private bool IsFocus()
        {
            return _focus;
        }
    
        public virtual void OnMouseEnter()
        {
            if (GameUI.IsUIOpened())
                return;
        
            if (GameTool.IsMobile())
                return;

            _focus = true;
        }
    
        public virtual void OnMouseExit()
        {
            _focus = false;
            _statusAlphaTarget = 0f;
        }

        public virtual void OnMouseDown()
        {
            if (GameUI.IsOverUI())
                return;

            PlayerControls.Get().SelectCard(GetCard());

            if (GameTool.IsMobile())
            {
                _focus = true;
            }
        }

        public string GetCardUID()
        {
            return _cardUID;
        }

        public Card GetCard()
        {
            Game data = GameClient.GetGameData();
            Card card = data.GetCard(_cardUID);
            return card;
        }

        public CardData GetCardData()
        {
            Card card = GetCard();
            if (card != null)
                return CardData.Get(card.cardID);
            return null;
        }

        private Slot GetSlot()
        {
            return GetCard().slot;
        }

        public static BoardElement GetFocus()
        {
            foreach (BoardElement card in CardList)
            {
                if (card.IsFocus())
                    return card;
            }
            return null;
        }

        public static void UnfocusAll()
        {
            foreach (BoardElement card in CardList)
            {
                card._focus = false;
                card._statusAlphaTarget = 0f;
            }
        }

        public static BoardElement Get(string uid)
        {
            foreach (BoardElement card in CardList)
            {
                if (card._cardUID == uid)
                    return card;
            }
            return null;
        }

        public static List<BoardElement> GetAll()
        {
            return CardList;
        }


        public abstract void UpdateUI(Card card, Game data, bool showFX=true);

        public abstract void SetCard(Card card);

    }
}
