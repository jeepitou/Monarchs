using Monarchs.Logic;
using TMPro;
using UnityEngine;

namespace Monarchs
{
    public class HistoryCoordinate : MonoBehaviour
    {
        public TextMeshProUGUI text;

        public void SetCoordinates(Slot slot)
        {
            text.text = slot.GetCoordinateString();
        }
    }
}
