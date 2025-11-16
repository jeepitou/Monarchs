using System.Collections;
using DG.Tweening;
using Monarchs.Client;
using Monarchs.Logic;
using TcgEngine.FX;
using UnityEngine;

namespace Monarchs
{
    public class TrollPranksterVFX : AbilityFX
    {
        public float grabSpeed;
        public float aimSpeed;
        public float aimDuration;
        public float aimThrowBack;
        public float throwSpeed;
        public GameObject impactPrefab;
        private GameObject _thrownObject;
        private GameObject _trollObject;
        private GameObject _targetObject;
        private const float OFFSET_ABOVE_PIECE = 0.3f;

        private void OnDestroy()
        {
            DOTween.Kill(gameObject);
        }

        public override void DoFX()
        {
            Game game = GameClient.GetGameData();
            Card targetCard = game.GetSlotCard(GameBoardFX.Get().targetSlots[1]);
            Card thrownCard = game.GetSlotCard(GameBoardFX.Get().targetSlots[0]);
            _trollObject = BoardCard.Get(_abilityArgs.caster.uid).gameObject;
            
            _targetObject = BoardCard.Get(targetCard.uid).gameObject;
            _thrownObject = BoardCard.Get(thrownCard.uid).gameObject;
        }

        public override IEnumerator FXEnumerator()
        {
            float grabDuration = Vector3.Distance(_thrownObject.transform.position, _trollObject.transform.position) / grabSpeed;
            float throwDuration = Vector3.Distance(_thrownObject.transform.position, _targetObject.transform.position) / throwSpeed;
            Vector3 throwDirection = (_targetObject.transform.position - _thrownObject.transform.position).normalized;
            float throwBackDuration = aimThrowBack / aimSpeed;
            
            _thrownObject.GetComponent<BoardCard>().SetMovingInCoroutine(true);
            _thrownObject.transform.DOMove(_trollObject.transform.position + Vector3.up*OFFSET_ABOVE_PIECE, grabDuration).OnComplete(() =>
            {
                _thrownObject.transform.DOMove(_trollObject.transform.position + Vector3.up*OFFSET_ABOVE_PIECE - aimThrowBack * throwDirection,
                    throwBackDuration).SetEase(Ease.Linear);
                    
                
            });
            yield return new WaitForSeconds(grabDuration + throwBackDuration + aimDuration);
            
            _thrownObject.transform.DOMove(_targetObject.transform.position - throwDirection*0.5f + Vector3.up*OFFSET_ABOVE_PIECE, throwDuration).SetEase(Ease.Linear);
            yield return new WaitForSeconds(throwDuration);
            Instantiate(impactPrefab, _targetObject.transform.position + Vector3.up*OFFSET_ABOVE_PIECE, Quaternion.identity);
            _thrownObject.GetComponent<BoardCard>().SetMovingInCoroutine(false);
        }
    }
}