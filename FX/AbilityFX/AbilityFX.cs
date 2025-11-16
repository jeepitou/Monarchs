using System.Collections;
using System.Collections.Generic;
using Monarchs.Logic.AbilitySystem;
using UnityEngine;

namespace Monarchs
{
    public abstract class AbilityFX : MonoBehaviour
    {
        protected AbilityArgs _abilityArgs;

        public void SetAbilityData(AbilityArgs abilityArgs)
        {
            _abilityArgs = abilityArgs;
            DoFX();
        }

        public void Destroy()
        {
            Destroy(gameObject);   
        }

        public abstract void DoFX();

        public abstract IEnumerator FXEnumerator();
    }
}
