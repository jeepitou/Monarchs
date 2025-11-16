using System.Collections;
using System.Collections.Generic;
using MoreMountains.Feedbacks;
using UnityEngine;

namespace Monarchs
{
    public class DamageFXManager : MonoBehaviour
    {
        private static DamageFXManager _instance;
        public static DamageFXManager Get()
        {
            if (_instance == null)
            {
                _instance = FindObjectOfType<DamageFXManager>();
            }
            return _instance;
        }

        public void ShowDamage(Vector3 position, string damage)
        {
            MMF_Player player = GetComponent<MMF_Player>();
            MMF_FloatingText floatingText = player.GetFeedbackOfType<MMF_FloatingText>();
            floatingText.Value = damage;

            if (damage[0] == '+')
            {
                
            }
            
            player.PlayFeedbacks(position + Vector3.forward*0.5f);
            
        }
            
    }
}
