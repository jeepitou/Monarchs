using UnityEngine;

namespace Monarchs.Tools
{
    public static class RectTransformTools
    {
        public static void MoveRectTransformOnTopOfAnotherRectTransform(RectTransform rectTransformToMove, RectTransform rectTransformToMoveOnTopOf)
        {
            Camera camera = Camera.main;
            if (camera == null)
            {
                Debug.LogError("No main camera found");
                return;
            }
            
            Vector3 screenPoint = camera.WorldToScreenPoint(rectTransformToMoveOnTopOf.transform.position);
            RectTransform parentRectTransform = rectTransformToMove.parent.GetComponent<RectTransform>();
            
            RectTransformUtility.ScreenPointToLocalPointInRectangle(parentRectTransform,
                screenPoint, 
                Camera.main, 
                out Vector2 localPosition);
            
            rectTransformToMove.localPosition = new Vector3(localPosition.x, localPosition.y, rectTransformToMove.localPosition.z);
        }
    }
}
