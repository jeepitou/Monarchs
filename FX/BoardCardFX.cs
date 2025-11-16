using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using Monarchs;
using Monarchs.Ability;
using Monarchs.Client;
using Monarchs.FX.MovementFX;
using Monarchs.Logic;
using Monarchs.Tools;
using UnityEngine;

namespace TcgEngine.FX
{
    /// <summary>
    /// All FX/anims related to a card on the board
    /// </summary>

    public class BoardCardFX : MonoBehaviour
    {
        public Material killMat;
        public string killMatFade = "noise_fade";
        
        public GameObject whitePieceDestroyFX;
        public GameObject blackPieceDestroyFX;

        private static MovementFX _movementFX;
        private BoardCard _boardCard;

        private ParticleSystem _exhaustedFX = null;

        private Dictionary<StatusType, GameObject> _statusFXList = new Dictionary<StatusType, GameObject>();
        private Dictionary<string, GameObject> _traitFXList = new Dictionary<string, GameObject>(); //string is trait id

        void Awake()
        {
            _boardCard = GetComponent<BoardCard>();
            _boardCard.onKill += OnKill;

            GameClient.Get().onCardMoved += OnMove;
            GameClient.Get().onAttackStart += OnAttack;
 
            GameClient.Get().onTrapResolve += OnTrapResolve;
            
            if (_movementFX == null)
            {
                _movementFX = new MovementFX();
            }
        }

        private void OnTrapResolve(Card trap, Card triggerer)
        {
            if (triggerer.uid == _boardCard.GetCardUID() && triggerer.playerID == GameClient.Get().GetPlayerID())
            {
                GameClient.Get().animationManager.AddToQueue(OnTrapResolveCoroutine(trap, triggerer), gameObject);
            }
        }

        private IEnumerator OnTrapResolveCoroutine(Card trap, Card triggerer)
        {
            GameObject trapFX = trap.CardData.trapTriggeredFX;
            if (trapFX == null)
            {
                trapFX = AssetData.Get().trap_triggered_fx;
            }
            
            FXResult result = new FXResult();
            yield return FXTool.DoFX(trapFX, BoardSlot.Get(trap.slot).transform.position, result);
            
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
                (StatusType type) => StatusData.Get(type)?.status_fx
            );

            // For trait FX
            UpdateFX(
                card.traits.Select(t => t.id),
                _traitFXList,
                (string traitId) => TraitData.Get(traitId)?.fx
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

            //Spawn Audio
            AudioClip audio = cardData?.spawnAudio != null ? cardData.spawnAudio : AssetData.Get().card_spawn_audio;
            AudioTool.Get().PlaySFX("card_spawn", audio);

            //Spawn FX
            GameObject spawnFX = cardData.spawnFX != null ? cardData.spawnFX : AssetData.Get().card_spawn_fx;
            GameClient.Get().animationManager.AddToQueue(DoSpawnFX(spawnFX), gameObject);

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
                if (cardData.idleFX != null)
                {
                    GameObject fx = Instantiate(cardData.idleFX, transform);
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
            GameObject deathFX = cardData.deathFX != null ? cardData.deathFX : AssetData.Get().card_destroy_fx;
            FXResult result = new FXResult();
            yield return FXTool.DoFX(deathFX, transform.position, result);
            
            if (result.fxObject != null)
            {
                AnimationQueueElement animElement = result.fxObject.GetComponent<AnimationQueueElement>();
                if (animElement != null)
                {
                    yield return animElement.AnimationCoroutine();
                }
            }

            //Death audio
            AudioClip audio = cardData?.deathAudio != null ? cardData.deathAudio : AssetData.Get().card_destroy_audio;
            AudioTool.Get().PlaySFX("card_spawn", audio);

            Destroy(gameObject);
        }
		
		private void FadeSetVal(SpriteRenderer render, float val)
        {
            render.material = killMat;
            render.material.SetFloat(killMatFade, val);
        }

        private void FadeKill(SpriteRenderer render, float val, float duration)
        {
            //AnimMatFX anim = AnimMatFX.Create(render.gameObject, render.material);
            //anim.SetFloat(kill_mat_fade, val, duration);
        }

        private void OnMove(Card card, Slot slot)
        {
            if (card != _boardCard.GetCard())
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
            
            BoardCard boardTarget = (BoardCard)BoardCard.Get(target.uid);
            if (boardTarget != null)
            {
                bool killed = target.GetHP() - damage <= 0;
                yield return _movementFX.GetMovementFX(_boardCard).ChargeInto(_boardCard, boardTarget, killed);
                _boardCard.UpdateHP(card, gdata);
                boardTarget.UpdateHP(target, gdata);
                GameObject prefab = cardData.damageFX ? cardData.damageFX : AssetData.Get().card_damage_fx;
                AudioClip impactAudio = cardData.damageAudio ? cardData.damageAudio : AssetData.Get().card_damage_audio;
                GameClient.Get().animationManager.AddToQueue(DoDamageFX(prefab, boardTarget.transform.position), gameObject);
                AudioTool.Get().PlaySFX("card_hit", impactAudio);

                if (killed)
                {
                    GameObject g;
                    if (target.playerID == GameClient.GetGameData().firstPlayer)
                    {
                        g = Instantiate(whitePieceDestroyFX, transform.position, Quaternion.identity);
                    }
                    else
                    {
                        g = Instantiate(blackPieceDestroyFX, transform.position, Quaternion.identity);
                    }
                    
                    Vector3 dir = (BoardSlot.Get(target.slot).transform.position - BoardSlot.Get(attacker.slot).transform.position).normalized;
                    g.transform.LookAt(transform.position + dir);
                }

                //Attack FX and Audio
                GameObject fx = cardData.attackFX != null ? cardData.attackFX : AssetData.Get().card_attack_fx;
                GameClient.Get().animationManager.AddToQueue(DoAttackFx(fx), gameObject);
                AudioClip audio = cardData?.attackAudio != null ? cardData.attackAudio : AssetData.Get().card_attack_audio;
                AudioTool.Get().PlaySFX("card_attack", audio);
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
