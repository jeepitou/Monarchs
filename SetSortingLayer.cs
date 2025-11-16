using UnityEngine;

namespace Monarchs
{
    [RequireComponent(typeof(Canvas))]
    public class SetSortingLayer : MonoBehaviour
    {
        public string sortingLayerName = "UI";
        void Start()
        {
            Canvas canvas = GetComponent<Canvas>();
            canvas.sortingLayerName = sortingLayerName;
        }
    }
}
