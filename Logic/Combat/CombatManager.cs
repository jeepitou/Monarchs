using System.Collections.Generic;
using Monarchs.Ability;
using Monarchs.Logic.AbilitySystem;
using TcgEngine;
using UnityEngine;
using UnityEngine.Events;

namespace Monarchs.Logic
{
    /// <summary>
    /// Manages combat and attack operations in the game
    /// </summary>
    public class CombatManager
    {
        private GameLogic _gameLogic;
        private Game _game;
        private readonly bool _isInstant;
        private readonly ResolveQueue _resolveQueue;
        private readonly System.Random _random = new System.Random();
        private AbilityLogicSystem _abilityLogicSystem => _gameLogic?._abilityLogicSystem;

        public CombatManager(Game game, bool isInstant = false)
        {
            _game = game;
            _isInstant = isInstant;
            _resolveQueue = new ResolveQueue(game, isInstant);
        }

        public void Initialize(GameLogic gameLogic)
        {
            _gameLogic = gameLogic;
        }

        public void SetData(Game game)
        {
            _game = game;
            _resolveQueue.SetData(game);
        }

        public void Update(float delta)
        {
            _resolveQueue.Update(delta);
        }

        /// <summary>
        /// Processes an attack from one card to a target slot
        /// </summary>
        /// <param name="attacker">The attacking card</param>
        /// <param name="targetSlot">The slot being attacked</param>
        /// <param name="skipCost">Whether to skip the attack cost</param>
        /// <param name="rangedAttack">Whether this is a ranged attack</param>
        /// <param name="canAttackAlly">Whether allies can be attacked</param>
        public virtual void AttackTarget(Card attacker, 
            Slot targetSlot, 
            CardManager cardManager, 
            BoardLogic boardManager, 
            bool skipCost = false, 
            bool rangedAttack = false, 
            bool canAttackAlly = false)
        {
            Card target = _game.GetSlotCard(targetSlot);
            bool canAttack = !rangedAttack ? 
                _game.CanAttackTarget(attacker, target, skipCost, canAttackAlly) : 
                _game.CanRangeAttackTarget(attacker, targetSlot, skipCost, canAttackAlly);
            
            if (canAttack)
            {
                // Check for traps when moving to attack in melee
                if (!rangedAttack)
                {
                    _game.lastMoveDestination = targetSlot;
                    Vector2S moveTarget =
                        attacker.GetCurrentMovementScheme().GetClosestAvailableSquaresOnMoveTrajectory(
                            attacker.GetCoordinates(), target.GetCoordinates(), _game)[0];
                    
                    // If the attacker can't jump and triggers a trap, abort attack
                    if (!attacker.CanJump() && _abilityLogicSystem.TriggerTrapsOnMovePath(
                            AbilityTrigger.OnMoveOnSpecificSquare, attacker, 
                            Slot.Get(moveTarget.x, moveTarget.y)))
                    {
                        return;
                    }
                }
                
                // Set game state for attack
                _game.lastAttackedCard = target;
                _game.lastSlotAttacked = targetSlot;

                // Trigger before attack abilities
                _abilityLogicSystem.TriggerCardAbilityType(AbilityTrigger.OnBeforeAttack, attacker, target);
                if (target != null)
                {
                    _abilityLogicSystem.TriggerCardAbilityType(AbilityTrigger.OnBeforeDefend, target, attacker);
                }
                
                // Queue attack resolution
                _resolveQueue.AddAttack(attacker, targetSlot, (a, s, sk, r) => ResolveAttack(a, s, cardManager, boardManager, sk, r), skipCost, rangedAttack);
                _resolveQueue.ResolveAll();
            }
        }

        protected virtual void ResolveAttack(Card attacker, Slot targetSlot, CardManager cardManager, BoardLogic boardManager, bool skipCost = false, bool rangedAttack = false)
        {
            Card target = _game.GetSlotCard(targetSlot);
            
            attacker.RemoveStatus(StatusType.Stealth);
            _abilityLogicSystem.UpdateOngoingEffect();

            if (!rangedAttack && attacker.CanMove() && attacker.CardData.GetPieceType() != PieceType.Knight)
            {
                Vector2S moveTarget =
                    attacker.GetCurrentMovementScheme().GetClosestAvailableSquaresOnMoveTrajectory(
                        attacker.GetCoordinates(), target.GetCoordinates(), _game)[0];
                Slot moveSlot = Slot.Get(moveTarget.x, moveTarget.y);
                
                _resolveQueue.AddMove(attacker, moveSlot, (a, s, skip, exhaust) => boardManager.ForceMoveCard(a, s, skip, exhaust));
            }
            
            _resolveQueue.AddAttack(attacker, targetSlot, (a, s, skip, r) => ResolveAttackHit(a, s, cardManager, boardManager, skip, r), skipCost, rangedAttack);
            _resolveQueue.ResolveAll(0.3f);
        }

        protected virtual void ResolveAttackHit(Card attacker, Slot targetSlot, CardManager cardManager, BoardLogic boardManager, bool skipCost = false, bool rangedAttack = false)
        {
            Card target = _game.GetSlotCard(targetSlot);
            
            
            //Count attack damage
            int damage = attacker.GetAttack();

            // If attacker is a range unit in melee range, we halve its damage.
            if (attacker.GetMaxAttackRange() > 1 && attacker.slot.GetDistanceTo(targetSlot) <= 1 && !attacker.HasTrait("no_melee_penalty"))
            {
                damage = (int)System.Math.Ceiling(((decimal)damage / 2));
            }

            if (attacker.HasStatus(StatusType.Sabotage))
            {
                damage = 0;
            }
            
            if (rangedAttack)
            {
                _gameLogic.onRangeAttackStart?.Invoke(attacker, targetSlot, damage);
            }
            else
            {
                _gameLogic.onAttackStart?.Invoke(attacker, target, damage);
            }

            //Damage Cards
            if (target != null)
            {
                bool targetWillDie = damage >= target.GetHP() + target.GetArmor();
                bool slotWillBeFree = !target.SpawnsACardOnSlotWhenInDies(_gameLogic, targetSlot, targetSlot);
                
                if (!rangedAttack && attacker.CanMove(true))
                {
                    if (!targetWillDie || !slotWillBeFree)
                    {
                        Vector2S fallbackSquare = attacker.GetCurrentMovementScheme()
                            .GetClosestAvailableSquaresOnMoveTrajectory(attacker.GetCoordinates(),
                                targetSlot.GetCoordinate(), _game)[0];
                        attacker.slot = Slot.Get(fallbackSquare);
                    }
                    else
                    {
                        attacker.slot = targetSlot;
                    }
                }
                
                
                cardManager.DamageCard(attacker, target, damage);    
            }
            
            if (!rangedAttack)
            {
                _game.history.AddMeleeAttackHistory(attacker.playerID, attacker, target, attacker.previousSlot, damage);
            }
            else
            {
                _game.history.AddRangeAttackHistory(attacker.playerID, attacker, target, targetSlot, attacker.previousSlot, damage);
            }

            //Save attack and exhaust
            if (!skipCost)
                attacker.ExhaustAfterAttack(rangedAttack);

            //Recalculate bonus
            _abilityLogicSystem.UpdateOngoingEffect();

            //Abilities
            bool attackerIsOnBoard = _game.IsOnBoard(attacker);

            if (attackerIsOnBoard)
            {
                _abilityLogicSystem.TriggerCardAbilityType(AbilityTrigger.OnAfterAttack, new AbilityArgs(){caster = attacker, castedCard = attacker}); //Target isn't setup because ability might target someone else
                _abilityLogicSystem.TriggerCardAbilityType(
                    rangedAttack ? AbilityTrigger.OnAfterRangeAttack : AbilityTrigger.OnAfterMeleeAttack,
                    new AbilityArgs() { caster = attacker, castedCard = attacker });
            }
                
            if (target != null)
            {
                bool defenderIsOnBoard = _game.IsOnBoard(target);
                if (defenderIsOnBoard)
                {
                    if (!rangedAttack && target.canRetaliate)
                    {
                        int retaliationDamage = target.GetRetaliationDamage();
                        
                        cardManager.DamageCard(target, attacker, retaliationDamage, false, true);
                        target.TurnRetaliationOff();

                        _abilityLogicSystem.TriggerCardAbilityType(AbilityTrigger.OnAfterDefend, target, attacker);
                    }
                }
                else
                {
                    if (!rangedAttack && attacker.CanMove(true))
                    {
                        _resolveQueue.AddMove(attacker, target.slot, (a, s, skip, exhaust) => boardManager.ForceMoveCard(a, s, skip, exhaust));
                    }
                }
            }

            if (rangedAttack)
            {
                _gameLogic.onRangeAttackEnd?.Invoke(attacker, targetSlot, damage);
            }
            else
            {
                attacker.numberOfMoveThisTurn++;
                _gameLogic.onAttackEnd?.Invoke(attacker, target, damage);
            }
            
            attacker.hasAttacked = true;

            if (attacker.CardData.GetPieceType() == PieceType.Knight)
            {
                bool defenderIsOnBoard = _game.IsOnBoard(target);
                if (defenderIsOnBoard)
                {
                    Vector2S fallbackSquare = attacker.GetCurrentMovementScheme()
                        .GetClosestAvailableSquaresOnMoveTrajectory(attacker.GetCoordinates(),
                            targetSlot.GetCoordinate(), _game)[0];
                    _resolveQueue.AddMove(attacker, Slot.Get(fallbackSquare),
                        (a, s, skip, exhaust) => boardManager.ForceMoveCard(a, s, skip, exhaust));
                }
                else
                {
                    _resolveQueue.AddMove(attacker, target.slot,
                        (a, s, skip, exhaust) => boardManager.ForceMoveCard(a, s, skip, exhaust));
                }
            }
            
            _resolveQueue.AddCallback(() => attacker.RemoveStatus(StatusType.Sabotage));
            
            // Check winner after attack
            _gameLogic.CheckForWinner();
            
            bool isShootOnTheMoveAttack = rangedAttack && attacker.HasTrait("shoot_on_the_move");
            bool isHitAndRun = attacker.HasTrait("hit_and_run") && attacker.numberOfMoveThisTurn < 2;
            
            if (attacker.AreAllPiecesOfCohortExhausted(_game) && !isShootOnTheMoveAttack && !isHitAndRun && !GameplayData.Get().canPlayCardAfterMove)
            {
                _resolveQueue.AddCallback(_gameLogic.EndTurn);
            }
            
            _resolveQueue.ResolveAll(0.5f);
        }

        /// <summary>
        /// Redirects an attack to a new target
        /// </summary>
        public virtual void RedirectAttack(Card attacker, Card newTarget)
        {
            foreach (AttackQueueElement att in _resolveQueue.GetAttackQueue())
            {
                if (att.attacker.uid == attacker.uid)
                {
                    att.targetSlot = newTarget.slot;
                    att.playerTarget = null;
                }
            }
        }

        /// <summary>
        /// Requests the player to select a ranged attacker for a target slot
        /// </summary>
        public virtual void RequestRangeAttackerChoice(System.Collections.Generic.List<Card> possibleAttackers, Slot targetSlot)
        {
            _abilityLogicSystem.RequestRangeAttackerChoice(targetSlot, possibleAttackers);
        }

        /// <summary>
        /// Selects a ranged attacker for an attack
        /// </summary>
        public virtual void SelectRangeAttacker(Card attacker)
        {
            _abilityLogicSystem.ReceiveSelectRangeAttacker(attacker);
        }

        /// <summary>
        /// Rolls a random value between min and max
        /// </summary>
        public virtual int RollRandomValue(int min, int max)
        {
            _game.rolledValue = _random.Next(min, max);
            _gameLogic.onRollValue?.Invoke(_game.rolledValue);
            _resolveQueue.SetDelay(1f);
            return _game.rolledValue;
        }

        /// <summary>
        /// Rolls a random value using a dice with specified number of sides
        /// </summary>
        public int RollRandomValue(int dice)
        {
            return RollRandomValue(1, dice + 1);
        }
    }
}