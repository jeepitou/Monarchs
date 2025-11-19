using Monarchs.Ability;
using Monarchs.Client;
using Monarchs.Logic;
using Monarchs.Tools;
using Monarchs.Logic.AbilitySystem;
using UnityEngine;
using System.Collections;
using Monarchs;
using Monarchs.Ability.Target;
using System.Collections.Generic;

namespace TcgEngine.FX
{
    /// <summary>
    /// FX that are not related to any card/player, and appear in the middle of the board
    /// Usually when big abilities are played
    /// </summary>

    public class GameBoardFX : MonoBehaviour
    {
        public AbilityArgs abilityArgs;
        public List<Slot> targetSlots;
        private static GameBoardFX _instance;
        void Start()
        {
            GameClient.Get().onNewTurn += OnNewTurn;
            //GameClient.Get().onCardPlayed += OnPlayCard;
            GameClient.Get().onAbilityStart += OnAbility;
            GameClient.Get().onAbilityTargetSlot += OnAbilityTargetSlot;
            GameClient.Get().onAbilityTargetCard += OnAbilityTargetCard;
            GameClient.Get().onAbilityTargetPlayer += OnAbilityTargetPlayer;
            GameClient.Get().onAbilityTargetMultiple += OnAbilityTargetMultiple;
            GameClient.Get().onAbilitySelectMana += OnAbilitySelectMana;
            GameClient.Get().onTrapTrigger += OnSecret;
            GameClient.Get().onValueRolled += OnRoll;
            GameClient.Get().onAbilityEnd += OnAbilityAfter;

            if (_instance == null)
            {
                _instance = this;
            }
            else
            {
                Destroy(gameObject);
            }
        }

        public static GameBoardFX Get()
        {
            return _instance;
        }

        private void OnDestroy()
        {
            GameClient.Get().onNewTurn -= OnNewTurn;
            //GameClient.Get().onCardPlayed -= OnPlayCard;
            GameClient.Get().onAbilityStart -= OnAbility;
            GameClient.Get().onAbilityTargetSlot -= OnAbilityTargetSlot;
            GameClient.Get().onAbilityTargetCard -= OnAbilityTargetCard;
            GameClient.Get().onAbilityTargetPlayer -= OnAbilityTargetPlayer;
            GameClient.Get().onAbilityTargetMultiple -= OnAbilityTargetMultiple;
            GameClient.Get().onAbilitySelectMana -= OnAbilitySelectMana;
            GameClient.Get().onTrapTrigger -= OnSecret;
            GameClient.Get().onValueRolled -= OnRoll;
            GameClient.Get().onAbilityEnd -= OnAbilityAfter;
        }

        #region Event Handlers
        
        private void OnAbilityTargetPlayer(AbilityData ability, Card caster, Player target)
        {
            if (ability != null && ValidateAbilityFxTarget(ability.caster_fx, ability.vfxIndex, FXTarget.Slot) && ability.caster_fx[ability.vfxIndex].FX != null)
            {
                AbilityArgs _abilityArgs = CreateAbilityArgsWithTarget(caster, target);
                GameClient.Get().animationManager.AddToQueue(ChangeAbilityArgs(_abilityArgs), gameObject);
                GameClient.Get().animationManager.AddToQueue(ChangeTargetSlots(new List<Slot>()), gameObject);
                GameClient.Get().animationManager.AddToQueue(PlaySnapFX(ability.caster_fx[ability.vfxIndex].FX, BoardSlot.Get(caster.slot).transform, Vector3.up*0.15f, abilityArgs, ability.target_audio), gameObject);
            }
        }

        private void OnAbilitySelectMana(AbilityData ability, Card caster, PlayerMana.ManaType manaType)
        {
            if (ability != null && caster != null && manaType != PlayerMana.ManaType.None)
            {
                AbilityArgs _abilityArgs = new AbilityArgs();
                _abilityArgs.ability = ability;
                _abilityArgs.caster = caster;
                _abilityArgs.manaType = manaType;
                GameClient.Get().animationManager.AddToQueue(ChangeAbilityArgs(_abilityArgs), gameObject);
                GameClient.Get().animationManager.AddToQueue(ChangeTargetSlots(new List<Slot>()), gameObject);
                GameClient.Get().animationManager.AddToQueue(PlaySnapFX(ability.caster_fx[ability.vfxIndex].FX,  BoardSlot.Get(caster.slot).transform, Vector3.up*0.30f, _abilityArgs, ability.cast_audio) , gameObject);
            }
        }

        void OnNewTurn(Card card)
        {
            AudioTool.Get().PlaySFX("turn", AssetData.Get().new_turn_audio);
            GameClient.Get().animationManager.AddToQueue(PlayFX(AssetData.Get().new_turn_fx, new Vector3(0, 1, 0)), gameObject);
        }
        
        void OnPlayCard(Card card, Slot slot)
        {
            if (card == null) return;
            
            int player_id = GameClient.Get().GetPlayerID();
            CardData icard = CardData.Get(card.cardID);
            
            if (icard.cardType == CardType.Spell)
            {
                GameObject prefab = player_id == card.playerID ? AssetData.Get().play_card_fx : AssetData.Get().play_card_other_fx;
                GameClient.Get().animationManager.AddToQueue(PlayCardFX(prefab, card, icard, slot), gameObject);
            }
            else if (icard.cardType == CardType.Trap)
            {
                GameObject sprefab = player_id == card.playerID ? AssetData.Get().play_secret_fx : AssetData.Get().play_secret_other_fx;
                GameClient.Get().animationManager.AddToQueue(PlayTrapCardFX(sprefab, card, icard), gameObject);
            }
        }

        private void OnAbility(AbilityData iability, Card caster)
        {
            if (iability != null)
            {
                AbilityArgs _abilityArgs = CreateAbilityArgsWithTarget(caster, null);
                GameClient.Get().animationManager.AddToQueue(ChangeAbilityArgs(_abilityArgs), gameObject);
                GameClient.Get().animationManager.AddToQueue(ChangeTargetSlots(new List<Slot>()), gameObject);
                GameClient.Get().animationManager.AddToQueue(PlayFX(iability.board_fx, iability.vfxIndex, new Vector3(0, 1, 0), _abilityArgs, iability.cast_audio), gameObject);
                
                // Handle caster effects for board cards
                HandleBoardCardCasterFX(iability, caster);
            }
        }

        private void OnAbilityTargetSlot(AbilityData iability, Card caster, Slot target, bool isSelectTarget)
        {
            if (iability != null && caster != null && target != null)
            {
                AbilityArgs _abilityArgs = CreateAbilityArgsWithTarget(caster, target);
                GameClient.Get().animationManager.AddToQueue(ChangeAbilityArgs(_abilityArgs), gameObject);
                GameClient.Get().animationManager.AddToQueue(ChangeTargetSlots(new List<Slot>()), gameObject);
                GameClient.Get().animationManager.AddToQueue(PlayAbilityTargetFX(iability, caster, target, isSelectTarget, _abilityArgs), gameObject);
            }
        }

        private void OnAbilityTargetCard(AbilityData iability, Card caster, Card target, bool isSelectTarget)
        {

            if (target == null) return;

            AbilityArgs _abilityArgs = CreateAbilityArgsWithTarget(caster, target);
                GameClient.Get().animationManager.AddToQueue(ChangeAbilityArgs(_abilityArgs), gameObject);
            GameClient.Get().animationManager.AddToQueue(ChangeTargetSlots(new List<Slot>()), gameObject);
            
            if (iability != null && caster != null && target != null)
            {
                GameClient.Get().animationManager.AddToQueue(PlayAbilityTargetCardFX(iability, caster, target, isSelectTarget, _abilityArgs), gameObject);
                
                // Handle target FX for board cards
                HandleBoardCardTargetFX(iability, caster, target, isSelectTarget);
            }
            else
            {
                GameClient.Get().animationManager.AddToQueue(PlayAbilityTargetFX(iability, caster, target.slot, isSelectTarget, _abilityArgs), gameObject);
            }
        }

        private void OnAbilityTargetMultiple(AbilityData ability, Card caster, List<Slot> slots)
        {
            Game game = GameClient.GetGameData();
            GameClient.Get().animationManager.AddToQueue(ChangeTargetSlots(slots), gameObject);
            GameClient.Get().animationManager.AddToQueue(ChangeAbilityArgs(new AbilityArgs(){caster=caster}), gameObject);
            foreach (var fx in ability.target_fx)
            {
                ITargetable target = slots[AbilityData.GetTargetIndex(fx.targetNumber)];
                Card targetCard = game.GetSlotCard((Slot)target);
                target = targetCard != null ? targetCard : target;
                AbilityArgs args = CreateAbilityArgsWithTarget(caster, target);
                Transform transform = targetCard != null ? BoardCard.Get(targetCard.uid).transform : BoardSlot.Get((Slot)target).transform;
                GameClient.Get().animationManager.AddToQueue(PlaySnapFX(fx.FX, transform, Vector3.up * 0.15f, args, ability.target_audio), gameObject);
            }
        }

        private IEnumerator ChangeAbilityArgs(AbilityArgs abilityArgs)
        {
            this.abilityArgs = abilityArgs;
            yield return null;
        }

        private IEnumerator ChangeTargetSlots(List<Slot> slots)
        {
            targetSlots = slots;
            yield return null;
        }
        
        private void OnAbilityAfter(AbilityData abilityData, Card caster)
        {
            // Handle any cleanup or additional effects after ability completes
        }

        private void OnSecret(Card secret, Card triggerer)
        {
            CardData icard = CardData.Get(secret.cardID);
            AudioTool.Get().PlaySFX("card_secret", icard.AttackAudio);
        }

        private void OnRoll(int value)
        {
            GameClient.Get().animationManager.AddToQueue(PlayRollFX(value), gameObject);
        }
        
        #endregion
        
        #region Board Card Specific Effects
        
        public void HandleBoardCardCasterFX(AbilityData abilityData, Card caster)
        {
            if (abilityData != null && caster != null)
            {
                BoardSlot boardSlot = BoardSlot.Get(caster.slot);
                if (boardSlot != null)
                {
                    if (ValidateAbilityFxTarget(abilityData.caster_fx, abilityData.vfxIndex, FXTarget.Piece))
                    {
                        GameClient.Get().animationManager.AddToQueue(DoCasterFx(abilityData, boardSlot), gameObject);
                        AudioTool.Get().PlaySFX("ability", abilityData.cast_audio);
                    }
                }
            }
        }
        
        public void HandleBoardCardTargetFX(AbilityData abilityData, Card caster, Card target, bool isSelectTarget)
        {
            if (abilityData != null && caster != null && target != null)
            {
                BoardCard boardTarget = (BoardCard)BoardCard.Get(target.uid);
                if (boardTarget != null)
                {
                    GameClient.Get().animationManager.AddToQueue(DoAbilityEffectFx(abilityData, caster, target, boardTarget, isSelectTarget), gameObject);
                }
            }
        }
        
        private IEnumerator DoCasterFx(AbilityData abilityData, BoardSlot boardSlot)
        {
            FXResult result = new FXResult();
            yield return FXTool.DoSnapFX(abilityData.caster_fx, abilityData.vfxIndex, boardSlot.transform, Vector3.up, result);
            
            if (result.fxObject != null)
            {
                AnimationQueueElement animElement = result.fxObject.GetComponent<AnimationQueueElement>();
                if (animElement != null)
                {
                    yield return animElement.AnimationCoroutine();
                }
                

            }
        }
        
        private IEnumerator DoAbilityEffectFx(AbilityData abilityData, Card caster, Card target, BoardCard boardTarget, bool isSelectTarget)
        {
            FXResult result = new FXResult();
            
            if (isSelectTarget)
            {
                if (ValidateAbilityFxTarget(abilityData.selectTargetFx, abilityData.vfxIndex, FXTarget.Piece))
                {
                    yield return FXTool.DoSnapFX(abilityData.selectTargetFx[abilityData.vfxIndex].FX, boardTarget.transform, result);
                    
                    if (result.fxObject != null)
                    {
                        AnimationQueueElement animElement = result.fxObject.GetComponent<AnimationQueueElement>();
                        if (animElement != null)
                        {
                            yield return animElement.AnimationCoroutine();
                        }
                    }
                        
                    AudioTool.Get().PlaySFX("ability_effect", abilityData.target_audio);
                }
            }
            else
            {
                if (ValidateAbilityFxTarget(abilityData.target_fx, abilityData.vfxIndex, FXTarget.Piece))
                {
                    yield return FXTool.DoSnapFX(abilityData.target_fx, abilityData.vfxIndex, boardTarget.transform, result);
                    
                    if (result.fxObject != null)
                    {
                        AnimationQueueElement animElement = result.fxObject.GetComponent<AnimationQueueElement>();
                        if (animElement != null)
                        {
                            yield return animElement.AnimationCoroutine();
                        }
                    }
                        
                    AudioTool.Get().PlaySFX("ability_effect", abilityData.target_audio);
                }
            }
        }
        
        #endregion
        
        #region FX Playback Coroutines
        
        private IEnumerator PlayCardFX(GameObject prefab, Card card, CardData icard, Slot slot)
        {
            AbilityArgs abilityArgs = CreateAbilityArgs(card);
            abilityArgs.target = slot;
            
            FXResult result = new FXResult();
            yield return FXTool.DoFX(prefab, new Vector3(0, 1, 0), abilityArgs, result);
            
            if (result.fxObject != null)
            {
                HandCardUIManager ui = result.fxObject.GetComponentInChildren<HandCardUIManager>();
                ui.SetCard(icard, card.VariantData);
                
                yield return WaitForAnimation(result.fxObject);
            }
            
            AudioTool.Get().PlaySFX("card_spell", icard.SpawnAudio);
        }
        
        private IEnumerator PlayTrapCardFX(GameObject prefab, Card card, CardData icard)
        {
            AbilityArgs abilityArgs = CreateAbilityArgs(card);
            
            FXResult result = new FXResult();
            yield return FXTool.DoFX(prefab, new Vector3(0, 1, 0), abilityArgs, result);

            if (result.fxObject != null)
            {
                yield return WaitForAnimation(result.fxObject);
            }
            
            AudioTool.Get().PlaySFX("card_spell", icard.SpawnAudio);
        }
        
        private IEnumerator PlayFX(GameObject prefab, Vector3 position, AudioClip audioClip = null)
        {
            FXResult result = new FXResult();
            yield return FXTool.DoFX(prefab, position, result);
            
            if (result.fxObject != null)
            {
                yield return WaitForAnimation(result.fxObject);
            }
            
            if (audioClip != null)
            {
                AudioTool.Get().PlaySFX("ability_effect", audioClip);
            }
        }
        
        private IEnumerator PlayFX(FXData[] fxData, int index, Vector3 position, AbilityArgs abilityArgs, AudioClip audioClip = null)
        {
            FXResult result = new FXResult();
            yield return FXTool.DoFX(fxData, index, position, abilityArgs, result);
            
            if (result.fxObject != null)
            {
                yield return WaitForAnimation(result.fxObject);
            }
            
            if (audioClip != null)
            {
                AudioTool.Get().PlaySFX("ability_effect", audioClip);
            }
        }
        
        private IEnumerator PlaySnapFX(GameObject prefab, Transform target, Vector3 offset, AbilityArgs abilityArgs, AudioClip audioClip = null)
        {
            FXResult result = new FXResult();
            yield return FXTool.DoSnapFX(prefab, target, offset, abilityArgs, result);
            
            if (result.fxObject != null)
            {
                yield return WaitForAnimation(result.fxObject);
            }
            
            if (audioClip != null)
            {
                AudioTool.Get().PlaySFX("ability_effect", audioClip);
            }
        }

        private IEnumerator PlayAbilityTargetFX(AbilityData ability, Card caster, Slot target, bool isSelectTarget, AbilityArgs abilityArgs)
        {
            if (ability == null || caster == null || target == Slot.None) 
            {
                yield break;
            }
            Transform targetTransform = BoardSlot.Get(target)?.transform;
            FXData[] fxData = isSelectTarget ? ability.selectTargetFx : ability.target_fx;
            
            if (ValidateAbilityFxTarget(fxData, ability.vfxIndex, FXTarget.Slot) && fxData[ability.vfxIndex].FX != null)
            {
                yield return PlaySnapFX(fxData[ability.vfxIndex].FX, targetTransform, Vector3.up * 0.15f, abilityArgs, ability.target_audio);
            }
        }

        private IEnumerator PlayAbilityTargetCardFX(AbilityData ability, Card caster, Card target, bool isSelectTarget, AbilityArgs abilityArgs)
        {
            if (ability == null || caster == null || target == null) 
            {
                yield break;
            }
            
            Transform targetTransform = BoardSlot.Get(target.slot).transform;
            
            if (isSelectTarget)
            {
                if (ValidateAbilityFxTarget(ability.selectTargetFx, ability.vfxIndex, FXTarget.Piece) && ability.selectTargetFx[ability.vfxIndex].FX != null)
                {
                    yield return PlaySnapFX(ability.selectTargetFx[ability.vfxIndex].FX, targetTransform, Vector3.up * 0.15f, abilityArgs, ability.target_audio);
                }
            }
            else
            {
                if (ValidateAbilityFxTarget(ability.target_fx, ability.vfxIndex, FXTarget.Piece) && ability.target_fx[ability.vfxIndex].FX != null)
                {
                    yield return PlaySnapFX(ability.target_fx[ability.vfxIndex].FX, targetTransform, Vector3.up * 0.15f, abilityArgs, ability.target_audio);
                }
            }
        }
        
        private IEnumerator PlayRollFX(int value)
        {
            FXResult result = new FXResult();
            yield return FXTool.DoFX(AssetData.Get().dice_roll_fx, new Vector3(0, 1, 0), result);
            
            if (result.fxObject != null)
            {
                DiceRollFX dice = result.fxObject.GetComponent<DiceRollFX>();
                if (dice != null)
                {
                    dice.value = value;
                }
                
                yield return WaitForAnimation(result.fxObject);
            }
        }
        
        #endregion
        
        #region Helper Methods
        
        private IEnumerator WaitForAnimation(GameObject fxObject)
        {
            AnimationQueueElement animElement = fxObject.GetComponent<AnimationQueueElement>();
            if (animElement != null)
            {
                yield return animElement.AnimationCoroutine();
            }
        }
        
        private AbilityArgs CreateAbilityArgs(Card caster)
        {
            return new AbilityArgs
            {
                caster = caster
            };
        }
        
        private AbilityArgs CreateAbilityArgsWithTarget(Card caster, ITargetable target)
        {
            return new AbilityArgs
            {
                caster = caster,
                target = target
            };
        }
        
        private bool ValidateAbilityFxTarget(FXData[] fxDatas, int index, FXTarget target)
        {
            if (fxDatas == null || fxDatas.Length <= index)
            {
                return false;
            }

            return (fxDatas[index].Target == target);
        }
        
        #endregion
    }
}