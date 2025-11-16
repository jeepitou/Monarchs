using TcgEngine.Client;
using System.Collections;
using System.Collections.Generic;
using Monarchs.Board;
using Monarchs.Client;
using Monarchs.Logic;
using UnityEngine;
using TcgEngine;

namespace TcgEngine.FX
{
    /// <summary>
    /// Line FX that appear when dragin a board card to attack, top part of the fx
    /// </summary>

    public class MouseLineTopFX : MonoBehaviour
    {
        public GameObject fx;
        
        void Update()
        {
            PlayerControls controls = PlayerControls.Get();
            BoardElement bcard = controls.GetDragged();

            bool visible = false;
            if (bcard != null)
            {
                Game data = GameClient.GetGameData();
                Card card = bcard.GetCard();
                Player player = GameClient.Get().GetPlayer();

                if (data.IsPlayerActionTurn(player) && card.CanDoAnyAction()) 
                {
                    visible = true;
                }
            }

            HandCard drag = HandCard.GetDrag();
            if (drag != null)
            {
                visible = drag.GetCardData().IsRequireTarget();
            }
            
            if (BoardInputManager.Instance.GetLastHoveredSlot() == null)
            {
                visible = false;
            }

            if (fx.activeSelf != visible)
                fx.SetActive(visible);

            if (visible)
            {
                Vector3 dest = BoardInputManager.Instance.GetMousePositionOnBoard();
                transform.position = dest;
            }
        }
    }
}
