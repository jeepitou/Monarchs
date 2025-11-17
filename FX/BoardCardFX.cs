using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Monarchs.Board;
using Monarchs.Client;
using Monarchs.Logic;
using Monarchs.Tools;
using TcgEngine;
using UnityEngine;

namespace Monarchs.FX
{
    public class BoardCardFX : MonoBehaviour
    {
        
        public GameObject whitePieceDestroyFX;
        public GameObject blackPieceDestroyFX;

        private static MovementFX.MovementFX _movementFX;
        private BoardCard _boardCard;

        private ParticleSystem _exhaustedFX;

        private Dictionary<StatusType, GameObject> _statusFXList = new ();
        private Dictionary<string, GameObject> _traitFXList = new (); //string is trait id

        private void Awake()
        {
            _boardCard = GetComponent<BoardCard>();
            _boardCard.onKill += OnKill;

            GameClient.Get().onCardMoved += OnMove;
            GameClient.Get().onAttackStart += OnAttack;
 
            GameClient.Get().onTrapResolve += OnTrapResolve;
            
            if (_movementFX == null)
            {
                _movementFX = new MovementFX.MovementFX();
            }
        }

        private void OnTrapResolve(Card trap, Card triggerer)
        {
            if (triggerer.uid == _boardCard.GetCardUID() && triggerer.playerID == GameClient.Get().GetPlayerID())
            {
                GameClient.Get().animationManager.AddToQueue(OnTrapResolveCoroutine(trap), gameObject);
            }
        }

        private IEnumerator OnTrapResolveCoroutine(Card trap)
        {
            FXResult result = new FXResult();
            yield return FXTool.DoFX(trap.CardData.TrapTriggeredFX, BoardSlot.Get(trap.slot).transform.position, result);
            
            if (result.fxObject != null)
            {
                AnimationQueueElement animElement = result.fxObject.GetComponent<AnimationQueueElement>();
                if (animElement != null)
                {
                    yield return animElement.AnimationCoroutine();
                }
            }
                
            AudioTool.Get().PlaySFX("trap", AssetData.Get().trap_played_audio);
            yield return new WaitForSeconds(0.5f);
        }

        void Start()
        {
            OnSpawn();
        }

        private void OnDestroy()
        {
            GameClient.Get().onCardMoved -= OnMove;
            GameClient.Get().onTrapResolve -= OnTrapResolve;
            GameClient.Get().onAttackStart -= OnAttack;
        }

        void Update()
        {
            if (!GameClient.Get().IsReady())
                return;

            Card card = _boardCard.GetCard();

            // For status FX
            UpdateFX(
                card.status.Select(s => s.type),
                _statusFXList,
                (type) => StatusData.Get(type)?.status_fx
            );

            // For trait FX
            UpdateFX(
                card.traits.Select(t => t.id),
                _traitFXList,
                (traitId) => TraitData.Get(traitId)?.fx
            );

            //Exhausted add/remove
            if (_exhaustedFX != null && !_exhaustedFX.isPlaying && card.exhausted)
                _exhaustedFX.Play();
            if (_exhaustedFX != null && _exhaustedFX.isPlaying && !card.exhausted)
                _exhaustedFX.Stop();
        }

        private void OnSpawn()
        {
            CardData cardData = _boardCard.GetCardData();
            
            AudioTool.Get().PlaySFX("card_spawn", cardData?.SpawnAudio);
            GameClient.Get().animationManager.AddToQueue(DoSpawnFX(cardData?.SpawnFX), gameObject);

            //Exhausted fx
            if (AssetData.Get().card_exhausted_fx != null)
            {
                GameObject efx = Instantiate(AssetData.Get().card_exhausted_fx, transform);
                efx.transform.localPosition = Vector3.zero;
                _exhaustedFX = efx.GetComponent<ParticleSystem>();
            }

            //Idle status
            TimeTool.WaitFor(1f, () =>
            {
                if (cardData?.IdleFX != null)
                {
                    GameObject fx = Instantiate(cardData.IdleFX, transform);
                    fx.transform.localPosition = Vector3.zero;
                }
            });
        }
        
        private IEnumerator DoSpawnFX(GameObject spawnFX)
        {
            FXResult result = new FXResult();
            yield return FXTool.DoFX(spawnFX, transform.position, result);
            
            if (result.fxObject != null)
            {
                AnimationQueueElement animElement = result.fxObject.GetComponent<AnimationQueueElement>();
                if (animElement != null)
                {
                    yield return animElement.AnimationCoroutine();
                }
            }
        }
        
        public IEnumerator ShowDamageFX(int value)
        {
            DamageFXManager.Get().ShowDamage(transform.position, GetDamageString(value));
            Debug.Log("SHOWING DAMAGE FX: " + value);
            yield return new WaitForSeconds(0.5f);
        }

        public string GetDamageString(int value)
        {
            string valueString = "";
            if (value > 0)
            {
                valueString += "+";
            }

            valueString += value.ToString();
            
            return valueString;
        }


        private void OnKill()
        {
            GameClient.Get().animationManager.AddToQueue(KillRoutine(), gameObject);
        }

        private IEnumerator KillRoutine()
        {
            CardData cardData = _boardCard.GetCardData();

            //Death FX
            FXResult result = new FXResult();
            yield return FXTool.DoFX(cardData.DeathFX, transform.position, result);
            
            if (result.fxObject != null)
            {
                AnimationQueueElement animElement = result.fxObject.GetComponent<AnimationQueueElement>();
                if (animElement != null)
                {
                    yield return animElement.AnimationCoroutine();
                }
            }
            
            AudioTool.Get().PlaySFX("card_spawn", cardData.DeathAudio);

            Destroy(gameObject);
        }

        private void OnMove(Card card, Slot slot)
        {
            if (card.uid != _boardCard.GetCard().uid)
                return;
            
            AudioTool.Get().PlaySFX("card_move", AssetData.Get().card_move_audio);
            
            GameClient.Get().animationManager.AddToQueue(_movementFX.GetMovementFX(_boardCard).DoMove(_boardCard, slot), gameObject);
        }

        private void OnAttack(Card attacker, Card target, int damage)
        {
            if (attacker.uid == _boardCard.GetCard().uid)
            {
                GameClient.Get().animationManager.AddToQueue(OnAttackCoroutine(attacker, target, damage), gameObject);
            }
        }

        private IEnumerator OnAttackCoroutine(Card attacker, Card target, int damage)
        {
            Game gdata = GameClient.GetGameData();
            Card card = _boardCard.GetCard();
            CardData cardData = _boardCard.GetCardData();
            if (target == null)
            {
                target = gdata.lastAttackedCard;
            }
            
            BoardCard boardTarget = (BoardCard)BoardElement.Get(target.uid);
            if (boardTarget != null)
            {
                bool killed = target.GetHP() - damage <= 0;
                yield return _movementFX.GetMovementFX(_boardCard).ChargeInto(_boardCard, boardTarget, killed);
                _boardCard.UpdateHP(card, gdata);
                boardTarget.UpdateHP(target, gdata);
                GameClient.Get().animationManager.AddToQueue(DoDamageFX(cardData.DamageFX, boardTarget.transform.position), gameObject);
                AudioTool.Get().PlaySFX("card_hit", cardData.DamageAudio);
                
                if (killed)
                {
                    GameObject fxToUse = target.playerID == GameClient.GetGameData().firstPlayer ? whitePieceDestroyFX : blackPieceDestroyFX;
                    fxToUse = Instantiate(fxToUse, transform.position, Quaternion.identity);
                    
                    Vector3 dir = (BoardSlot.Get(target.slot).transform.position - BoardSlot.Get(attacker.slot).transform.position).normalized;
                    fxToUse.transform.LookAt(transform.position + dir);
                }
                
                GameClient.Get().animationManager.AddToQueue(DoAttackFx(cardData.AttackFX), gameObject);
                AudioTool.Get().PlaySFX("card_attack", cardData.AttackAudio);
            }
        }
        
        private IEnumerator DoAttackFx(GameObject fxPrefab)
        {
            FXResult result = new FXResult();
            yield return FXTool.DoSnapFX(fxPrefab, transform, result);
            
            if (result.fxObject != null)
            {
                AnimationQueueElement animElement = result.fxObject.GetComponent<AnimationQueueElement>();
                if (animElement != null)
                {
                    yield return animElement.AnimationCoroutine();
                }
            }
        }
        
        private IEnumerator DoDamageFX(GameObject prefab, Vector3 position)
        {
            FXResult result = new FXResult();
            yield return FXTool.DoFX(prefab, position, result);
            
            if (result.fxObject != null)
            {
                AnimationQueueElement animElement = result.fxObject.GetComponent<AnimationQueueElement>();
                if (animElement != null)
                {
                    yield return animElement.AnimationCoroutine();
                }
            }
        }
        
        private void UpdateFX<T>(
            IEnumerable<T> currentEffects,
            Dictionary<T, GameObject> fxDict,
            Func<T, GameObject> getFXPrefab)
        {
            // Add new FX
            foreach (var effect in currentEffects)
            {
                GameObject fxPrefab = getFXPrefab(effect);
                if (fxPrefab != null && !fxDict.ContainsKey(effect))
                {
                    GameObject fx = Instantiate(fxPrefab, transform);
                    fx.transform.localPosition = Vector3.zero;
                    fxDict[effect] = fx;
                }
            }

            // Remove FX no longer present
            List<T> removeList = new List<T>();
            foreach (var pair in fxDict)
            {
                if (!currentEffects.Contains(pair.Key))
                {
                    removeList.Add(pair.Key);
                    Destroy(pair.Value);
                }
            }
            foreach (var effect in removeList)
                fxDict.Remove(effect);
        }
    }
}
