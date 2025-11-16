using System.Collections;
using System.Collections.Generic;
using Monarchs.Client;
using Monarchs.Logic;
using UnityEngine;
using TcgEngine.Client;

namespace TcgEngine.FX
{
    /// <summary>
    /// The crosshair target that appears when targeting with a spell
    /// </summary>

    public class AimTargetFX : MonoBehaviour
    {
        public GameObject fx;

        void Start()
        {

        }

        void Update()
        {
            bool visible = false;
            HandCard hcard = HandCard.GetDrag();
            if (hcard != null)
            {
                Card caster = hcard.GetCard();
                if (caster.CardData.IsRequireTarget())
                    visible = true;
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
