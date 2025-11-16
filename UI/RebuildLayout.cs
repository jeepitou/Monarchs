using UnityEngine;
using UnityEngine.UI;

namespace Monarchs
{
    public class RebuildLayout : MonoBehaviour
    {
        private int _elementCount = -1;
        void Update()
        {
            if (_elementCount == -1)
            {
                _elementCount = transform.childCount;
                LayoutRebuilder.ForceRebuildLayoutImmediate(GetComponent<RectTransform>());
            }
            
            if (_elementCount != transform.childCount)
            {
                _elementCount = transform.childCount;
                LayoutRebuilder.ForceRebuildLayoutImmediate(GetComponent<RectTransform>());
            }
            //LayoutRebuilder.ForceRebuildLayoutImmediate(GetComponent<RectTransform>());
        }
    }
}
