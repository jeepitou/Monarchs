using System.Collections;
using UnityEngine;

namespace Monarchs
{
    public class ManaVFX : AbilityFX
    {
        public GameObject lightManaPrefab;
        public GameObject darkManaPrefab;
        public GameObject fireManaPrefab;
        public GameObject waterManaPrefab;
        public GameObject earthManaPrefab;
        public GameObject airManaPrefab;
        public override void DoFX()
        {
            if (_abilityArgs.manaType == PlayerMana.ManaType.Light)
            {
                Instantiate(lightManaPrefab, transform.position, Quaternion.identity, transform);
            }
            else if (_abilityArgs.manaType == PlayerMana.ManaType.Dark)
            {
                Instantiate(darkManaPrefab, transform.position, Quaternion.identity, transform);
            }
            else if (_abilityArgs.manaType == PlayerMana.ManaType.Fire)
            {
                Instantiate(fireManaPrefab, transform.position, Quaternion.identity, transform);
            }
            else if (_abilityArgs.manaType == PlayerMana.ManaType.Water)
            {
                Instantiate(waterManaPrefab, transform.position, Quaternion.identity, transform);
            }
            else if (_abilityArgs.manaType == PlayerMana.ManaType.Earth)
            {
                Instantiate(earthManaPrefab, transform.position, Quaternion.identity, transform);
            }
            else if (_abilityArgs.manaType == PlayerMana.ManaType.Air)
            {
                Instantiate(airManaPrefab, transform.position, Quaternion.identity, transform);
            }
        }

        public override IEnumerator FXEnumerator()
        {
            yield return null;
        }
    }
}
