using Monarchs.Client;
using UnityEngine;

namespace Monarchs.FX
{
    /// <summary>
    /// Rotate FX to face camera
    /// </summary>

    public class FaceFX : MonoBehaviour
    {
        void Start()
        {
            GameCamera cam = GameCamera.Get();
            if (cam != null)
            {
                Vector3 forward = cam.transform.forward;
                transform.rotation = Quaternion.LookRotation(forward);
            }
        }
    }
}
