using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using Monarchs.Client;
using Monarchs.Logic;
using TcgEngine.FX;
using UnityEngine;
using UnityEngine.Events;

namespace Monarchs
{
    public class FireballVFX : AbilityFX
    {
        public float explosionDelay = 0.2f;
        public GameObject fireballPrefab;
        public GameObject explosionPrefab;
        public Slot casterSlot;
        public Slot targetSlot;
        
        private List<BoardSlot> affectedSlots = new List<BoardSlot>();
        private List<GameObject> spawnedHolyWords = new List<GameObject>();
        

        private void OnDestroy()
        {
            DOTween.Kill(gameObject);
        }

        public override void DoFX()
        {
            targetSlot = _abilityArgs.target.GetSlot();
        }

        public override IEnumerator FXEnumerator()
        {
            BoardSlot casterSlot = BoardSlot.Get(_abilityArgs.caster.slot);
            GameObject fireball = Instantiate(fireballPrefab, casterSlot.transform.position, Quaternion.identity);
            Projectile proj = fireball.GetComponent<Projectile>();
            
            proj.SetTargetPosition(BoardSlot.Get(targetSlot).transform.position + Vector3.up * 0.1f);
            proj.onImpact += OnImpact;
            float duration = (BoardSlot.Get(targetSlot).transform.position - casterSlot.transform.position).magnitude / proj.speed;
            yield return new WaitForSeconds(duration + explosionDelay);
        }
        
        void OnImpact()
        {
            foreach (var slot in targetSlot.GetSlotsInRange(1))
            {
                SpawnFireBall(explosionPrefab, BoardSlot.Get(slot));
            }
        }
        
        private void SpawnFireBall(GameObject prefab, BoardSlot slot)
        {
            if (slot == null)
                return;
            Instantiate(prefab, slot.transform.position, Quaternion.identity);
        }
    }
}