using System.Collections;
using System.Collections.Generic;
using Monarchs.Client;
using Monarchs.Logic;
using UnityEngine;

namespace Monarchs
{
    public class DyingWishFX : AbilityFX
    {
        public float secondsToWait = 2f;
        public override void DoFX()
        {
            
        }

        public override IEnumerator FXEnumerator()
        {
            Card triggerer = (Card)_abilityArgs.triggerer;

            BoardCard boardCard = (BoardCard)BoardCard.Get(triggerer.uid);

            if (boardCard != null)
            {
                yield return boardCard.KillCoroutine(false);
            }

            GetComponent<Animator>().SetTrigger("StartAnimation");
            yield return new WaitForSeconds(secondsToWait);
        }
    }
}
