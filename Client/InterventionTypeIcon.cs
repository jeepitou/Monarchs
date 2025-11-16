using UnityEngine;
using UnityEngine.UI;

namespace Monarchs.Client
{
    public class InterventionTypeIcon : MonoBehaviour
    {
        [SerializeField] private Image image;
        [SerializeField] private Sprite hexSprite;
        [SerializeField] private Sprite strikeSprite;
        [SerializeField] private Sprite buffSprite;
        [SerializeField] private Sprite trapSprite;

        public void SetIcon(InterventionType type)
        {
            if (type == InterventionType.Hex)
            {
                image.sprite = hexSprite;
            }
            else if (type == InterventionType.Strike)
            {
                image.sprite = strikeSprite;
            }
            else if (type == InterventionType.Buff)
            {
                image.sprite = buffSprite;
            }
        }

        public void SetTrapIcon()
        {
            image.sprite = trapSprite;
        }
    }
}
