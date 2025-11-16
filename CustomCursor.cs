using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Monarchs.Client;
using Monarchs.Logic;
using TcgEngine;
using TcgEngine.Client;
using UnityEngine;
using UnityEngine.UI;

namespace Monarchs
{
    public class CustomCursor : MonoBehaviour
    {
        public Texture2D defaultCursor;
        public Texture2D draggingCursor;
        public Texture2D hoveringRangedAttackCursor;
        public Texture2D hoveringMeleeAttackCursor;
        public Texture2D hoveringIllegalMoveCursor;
        void Start()
        {
            // this sets the base cursor as invisible
            //Cursor.visible = false;
            SetCursor(CursorImage.Default);
        }

        public void SetCursor(CursorImage cursor)
        {
            Cursor.SetCursor(GetCursorImage(cursor), Vector2.zero, CursorMode.Auto);
        }

        private void Update()
        {
            if (!GameClient.Get().IsReady())
            {
                return;
            }
            
            Card pieceDragged = PlayerControls.Get().GetDragged()?.GetCard();

            bool isYourTurn = GameClient.Get().IsYourTurn();
            Game game = GameClient.GetGameData();
            Slot? hoveredSlot = BoardInputManager.Instance.GetLastHoveredSlot()?.GetSlot();
            Vector2S hoveredSlotCoordinate = new Vector2S(-1, -1);
            Card hoveredPiece = null;
            if (hoveredSlot != null)
            {
                hoveredSlotCoordinate = ((Slot)hoveredSlot).GetCoordinate();
                hoveredPiece = game.GetSlotCard((Slot)hoveredSlot);
            }
            
            if (!GameplayData.Get().DragAndDropRangedUnitWillDoARangedAttack && game.GetLegalRangedAttacks().Contains(hoveredSlotCoordinate) && pieceDragged == null)
            {
                SetCursor(CursorImage.HoveringRangedAttack);
                return;
            }

            if (pieceDragged != null && pieceDragged.CanMove())
            {
                if (hoveredPiece != null)
                {
                    if (GameplayData.Get().DragAndDropRangedUnitWillDoARangedAttack && game.CanRangeAttackTarget(pieceDragged, hoveredPiece))
                    {
                        SetCursor(CursorImage.HoveringRangedAttack);
                        return;
                    }
                    if (game.CanAttackTarget(pieceDragged, hoveredPiece))
                    {
                        SetCursor(CursorImage.HoveringMeleeAttack);
                        return;
                    }
                }
                else if ((hoveredSlot != null && game.GetCardLegalMove(pieceDragged).Contains(hoveredSlotCoordinate) && isYourTurn))
                {
                    SetCursor(CursorImage.HoveringLegalMove);
                    return;
                }
                else if (hoveredSlot != null && isYourTurn)
                {
                    SetCursor(CursorImage.HoveringIllegalMove);
                    return;
                }
            }

            if (Input.GetMouseButton(0))
            {
                SetCursor(CursorImage.Dragging);
                return;
            }
            SetCursor(CursorImage.Default);
        }

        private Texture2D GetCursorImage(CursorImage cursor)
        {
            if (cursor == CursorImage.Default)
            {
                return defaultCursor;
            }
            else if (cursor == CursorImage.Dragging)
            {
                return draggingCursor;
            }
            else if (cursor == CursorImage.HoveringMeleeAttack)
            {
                return hoveringMeleeAttackCursor;
            }
            else if (cursor == CursorImage.HoveringRangedAttack)
            {
                return hoveringRangedAttackCursor;
            }
            else if (cursor == CursorImage.HoveringIllegalMove)
            {
                return hoveringIllegalMoveCursor;
            }

            return defaultCursor;
        }

        public enum CursorImage
        {
            Default,
            Dragging,
            HoveringRangedAttack,
            HoveringMeleeAttack,
            HoveringLegalMove,
            HoveringIllegalMove
        }
    }
}
