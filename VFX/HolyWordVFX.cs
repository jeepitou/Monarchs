using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using Monarchs.Client;
using Monarchs.Logic;
using TcgEngine.FX;
using UnityEngine;

namespace Monarchs
{
    public class HolyWordVFX : AbilityFX
    {
        public float nextSlotDelay = 0.1f;
        public float explosionDelay = 0.2f;

        public GameObject holyWordPrefab;
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
            Game game = GameClient.GetGameData();
            casterSlot =game.GetCard(_abilityArgs.caster.uid).slot;
            targetSlot = _abilityArgs.target.GetSlot();
        }

        public override IEnumerator FXEnumerator()
        {
            Vector2S direction = targetSlot.GetCoordinate() - casterSlot.GetCoordinate();
            
            SpawnHolyWordAtSlot(BoardSlot.Get(targetSlot));
            yield return new WaitForSeconds(nextSlotDelay);
            
            
            BoardSlot nextSlot = BoardSlot.Get(targetSlot.GetCoordinate() + direction);
            SpawnHolyWordAtSlot(nextSlot);
            yield return new WaitForSeconds(nextSlotDelay);
            
            nextSlot = BoardSlot.Get(targetSlot.GetCoordinate() + direction + direction);
            SpawnHolyWordAtSlot(nextSlot);
            nextSlot = BoardSlot.Get(targetSlot.GetCoordinate() + direction + direction + direction.GetPerpendicular());
            SpawnHolyWordAtSlot(nextSlot);
            nextSlot = BoardSlot.Get(targetSlot.GetCoordinate() + direction + direction - direction.GetPerpendicular());
            SpawnHolyWordAtSlot(nextSlot);
            yield return new WaitForSeconds(nextSlotDelay);
            
            nextSlot = BoardSlot.Get(targetSlot.GetCoordinate() + direction + direction + direction);
            SpawnHolyWordAtSlot(nextSlot);
            
            yield return new WaitForSeconds(explosionDelay);
            DoExplosion();
        }
        
        private BoardSlot SpawnHolyWordAtSlot(BoardSlot slot)
        {
            if (slot == null)
                return null;
            affectedSlots.Add(slot);
            spawnedHolyWords.Add(Instantiate(holyWordPrefab, slot.transform.position, Quaternion.identity));
            return slot;
        }
        
        private void DoExplosion()
        {
            foreach (var holyWord in spawnedHolyWords)
            {
                Destroy(holyWord);
            }
            foreach (var slot in affectedSlots)
            {
                Instantiate(explosionPrefab, slot.transform.position, Quaternion.identity);
            }
        }
    }
}