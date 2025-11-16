using System.Collections;
using DG.Tweening;
using Monarchs.Client;
using Monarchs.Logic;
using UnityEngine;

namespace Monarchs
{
    public class ConversionVFX : AbilityFX
    {
        public float liftAmount;
        public float liftSpeed;
        public float breakBeforeSpin;
        public float spinAmount;
        public float spinSpeed;
        
        private GameObject _targetObject;

        private void OnDestroy()
        {
            DOTween.Kill(gameObject);
        }

        public override void DoFX()
        {
            Card targetCard = _abilityArgs.CardTarget;
            _targetObject = BoardCard.Get(targetCard.uid).gameObject;
        }

        public override IEnumerator FXEnumerator()
        {
            
            float liftDuration = Vector3.Distance(_targetObject.transform.position, _targetObject.transform.position + Vector3.up*liftAmount) / liftSpeed;
            float spinDuration = spinAmount / spinSpeed;
            
            _targetObject.GetComponent<BoardCard>().SetMovingInCoroutine(true);
            _targetObject.transform.DOMove(_targetObject.transform.position + Vector3.up * liftAmount, liftDuration);
            
            yield return new WaitForSeconds(liftDuration + breakBeforeSpin);

            _targetObject.transform
                .DORotate(new Vector3(0, 0, 360 * spinAmount), spinDuration, RotateMode.FastBeyond360)
                .SetEase(Ease.InOutCirc);
               
            yield return new WaitForSeconds(spinDuration/2);
            
            _targetObject.GetComponent<PieceOnBoard>().SwapColor();
            
            yield return new WaitForSeconds(spinDuration/2 + breakBeforeSpin);
                
            _targetObject.transform.DOMove(_targetObject.transform.position - Vector3.up * liftAmount,
                        liftDuration);

            yield return new WaitForSeconds(liftDuration);
            
            _targetObject.GetComponent<BoardCard>().SetMovingInCoroutine(false);
        }
    }
}