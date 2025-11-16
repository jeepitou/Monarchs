using System;
using System.Collections;
using System.Collections.Generic;
using Monarchs.Ability;
using Monarchs.Ability.Target;
using Monarchs.Logic;
using Monarchs.Logic.AbilitySystem;
using UnityEngine;
using UnityEngine.Events;

namespace TcgEngine
{
    /// <summary>
    /// Resolve abilities and actions one by one, with an optional delay in between each
    /// </summary>

    public class ResolveQueue 
    {
        private Pool<AbilityQueueElement> ability_elem_pool = new Pool<AbilityQueueElement>();
        private Pool<TrapTriggerQueueElement> trap_trigger_elem_pool = new Pool<TrapTriggerQueueElement>();
        private Pool<TrapResolveQueueElement> trap_resolve_elem_pool = new Pool<TrapResolveQueueElement>();
        private Pool<AttackQueueElement> attack_elem_pool = new Pool<AttackQueueElement>();
        private Pool<MoveQueueElement> move_elem_pool = new Pool<MoveQueueElement>();
        private Pool<CallbackQueueElement> callback_elem_pool = new Pool<CallbackQueueElement>();

        private Queue<AbilityQueueElement> ability_cast_queue = new Queue<AbilityQueueElement>();
        private Queue<TrapTriggerQueueElement> trapTrigger_queue = new Queue<TrapTriggerQueueElement>();
        private Queue<TrapResolveQueueElement> trapResolve_queue = new Queue<TrapResolveQueueElement>();
        private Queue<AttackQueueElement> attack_queue = new Queue<AttackQueueElement>();
        private Queue<CallbackQueueElement> callback_queue = new Queue<CallbackQueueElement>();
        private Queue<MoveQueueElement> move_queue = new Queue<MoveQueueElement>();

        private Game _gameData;
        private bool _isResolving = false;
        private float _resolveDelay = 0f;
        private bool skip_delay = false;

        public ResolveQueue(Game data, bool skip)
        {
            _gameData = data;
            skip_delay = skip;
        }

        public void SetData(Game data)
        {
            _gameData = data;
        }

        public virtual void Update(float delta)
        {
            if (_resolveDelay > 0f)
            {
                _resolveDelay -= delta;
                if (_resolveDelay <= 0f)
                    ResolveAll();
            }
        }

        public virtual void AddAbility(AbilityArgs args, Action<Game, AbilityArgs> callback)
        {
            if (args.ability != null)
            {
                AbilityQueueElement elem = ability_elem_pool.Create();
                elem.args = args;
                elem.callback = callback;
                ability_cast_queue.Enqueue(elem);
            }
        }
        
        public virtual void AddAttack(Card attacker, Slot targetSlot, Action<Card, Slot, bool, bool> callback, bool skipCost = false, bool rangedAttack=false)
        {
            if (attacker != null && targetSlot != null)
            {
                AttackQueueElement elem = attack_elem_pool.Create();
                elem.attacker = attacker;
                elem.targetSlot = targetSlot;
                elem.playerTarget = null;
                elem.skipCost = skipCost;
                elem.rangedAttack = rangedAttack;
                elem.callback = callback;
                attack_queue.Enqueue(elem);
            }
        }

        public virtual void AddAttack(Card attacker, Player target, Action<Card, Player, bool, bool> callback, bool skipCost = false)
        {
            if (attacker != null && target != null)
            {
                AttackQueueElement elem = attack_elem_pool.Create();
                elem.attacker = attacker;
                elem.targetSlot = Slot.None;
                elem.playerTarget = target;
                elem.skipCost = skipCost;
                elem.playerCallback = callback;
                attack_queue.Enqueue(elem);
            }
        }
        
        public virtual void AddMove(Card card, Slot slot, Action<Card, Slot, bool, bool> callback)
        {
            if (card != null)
            {
                MoveQueueElement elem = move_elem_pool.Create();
                elem.card = card;
                elem.slot = slot;
                elem.callback = callback;
                move_queue.Enqueue(elem);
            }
        }

        public virtual void AddTrapTrigger(AbilityArgs args, Action<Game, AbilityArgs> callback)
        {
            if (args.castedCard != null && args.triggerer != null)
            {
                TrapTriggerQueueElement elem = trap_trigger_elem_pool.Create();
                elem.args = args;
                elem.callback = callback;
                trapTrigger_queue.Enqueue(elem);
            }
        }
        
        public virtual void AddTrapResolve(AbilityArgs args, Action<Game, AbilityArgs> callback)
        {
            if (args.castedCard != null && args.triggerer != null)
            {
                TrapResolveQueueElement elem = trap_resolve_elem_pool.Create();
                elem.args = args;
                elem.callback = callback;
                trapResolve_queue.Enqueue(elem);
            }
        }

        public virtual void AddCallback(Action callback)
        {
            if (callback != null)
            {
                CallbackQueueElement elem = callback_elem_pool.Create();
                elem.callback = callback;
                callback_queue.Enqueue(elem);
            }
        }

        public virtual void Resolve()
        {
            if (ability_cast_queue.Count > 0)
            {
                //Resolve Ability
                AbilityQueueElement elem = ability_cast_queue.Dequeue();
                ability_elem_pool.Dispose(elem);
                elem.callback?.Invoke(_gameData, elem.args);
            }
            else if (trapTrigger_queue.Count > 0)
            {
                //Resolve Secret
                TrapTriggerQueueElement elem = trapTrigger_queue.Dequeue();
                trap_trigger_elem_pool.Dispose(elem);
                elem.callback?.Invoke(_gameData, elem.args);
            }
            else if (trapResolve_queue.Count > 0)
            {
                //Resolve Secret
                TrapResolveQueueElement elem = trapResolve_queue.Dequeue();
                trap_resolve_elem_pool.Dispose(elem);
                elem.callback?.Invoke(_gameData, elem.args);
            }
            else if (move_queue.Count > 0)
            {
                //Resolve Move
                MoveQueueElement elem = move_queue.Dequeue();
                move_elem_pool.Dispose(elem);
                elem.callback?.Invoke(elem.card, elem.slot, true, true);
            }
            else if (attack_queue.Count > 0)
            {
                //Resolve Attack
                AttackQueueElement elem = attack_queue.Dequeue();
                attack_elem_pool.Dispose(elem);
                if (elem.playerTarget != null)
                    elem.playerCallback?.Invoke(elem.attacker, elem.playerTarget, elem.skipCost, elem.rangedAttack);
                else
                    elem.callback?.Invoke(elem.attacker, elem.targetSlot, elem.skipCost, elem.rangedAttack);
            }
            else if (callback_queue.Count > 0)
            {
                CallbackQueueElement elem = callback_queue.Dequeue();
                callback_elem_pool.Dispose(elem);
                elem.callback.Invoke();
            }
        }

        public virtual void ResolveAll(float delay)
        {
            SetDelay(delay);
            ResolveAll();  //Resolve now if no delay
        }

        public virtual void ResolveAll()
        {
            if (_isResolving)
                return;

            _isResolving = true;
            while (CanResolve())
            {
                Resolve();
            }
            _isResolving = false;
        }

        public virtual void SetDelay(float delay)
        {
            if (!skip_delay)
            {
                _resolveDelay = Mathf.Max(_resolveDelay, delay);
            }
        }

        public virtual bool CanResolve()
        {
            if (_resolveDelay > 0f)
                return false;   //Is waiting delay
            if (_gameData.State == GameState.GameEnded)
                return false; //Cant execute anymore when game is ended
            if (_gameData.selector != SelectorType.None)
                return false; //Waiting for player input, in the middle of resolve loop
            return move_queue.Count > 0 || attack_queue.Count > 0 || ability_cast_queue.Count > 0 || trapTrigger_queue.Count > 0 ||trapResolve_queue.Count > 0 || callback_queue.Count > 0;
        }

        public virtual bool IsResolving()
        {
            return _isResolving || _resolveDelay > 0f;
        }

        public virtual void Clear()
        {
            attack_elem_pool.DisposeAll();
            ability_elem_pool.DisposeAll();
            trap_trigger_elem_pool.DisposeAll();
            trap_resolve_elem_pool.DisposeAll();
            callback_elem_pool.DisposeAll();
            attack_queue.Clear();
            ability_cast_queue.Clear();
            trapResolve_queue.Clear();
            trapTrigger_queue.Clear();
            callback_queue.Clear();
        }

        public Queue<AttackQueueElement> GetAttackQueue()
        {
            return attack_queue;
        }

        public Queue<AbilityQueueElement> GetAbilityQueue()
        {
            return ability_cast_queue;
        }

        public Queue<TrapTriggerQueueElement> GetTrapTriggerQueue()
        {
            return trapTrigger_queue;
        }
        
        public Queue<TrapResolveQueueElement> GetTrapResolveQueue()
        {
            return trapResolve_queue;
        }

        public Queue<CallbackQueueElement> GetCallbackQueue()
        {
            return callback_queue;
        }
    }

    public class AbilityQueueElement
    {
        public AbilityArgs args;
        public Action<Game, AbilityArgs> callback;
    }
    public class AttackQueueElement
    {
        public Card attacker;
        public Slot targetSlot;
        public Player playerTarget;
        public bool skipCost;
        public bool rangedAttack;
        public Action<Card, Slot, bool, bool> callback;
        public Action<Card, Player, bool, bool> playerCallback;
    }
    
    public class TrapTriggerQueueElement
    {
        public AbilityArgs args;
        public Action<Game, AbilityArgs> callback;
    }

    public class TrapResolveQueueElement
    {
        public AbilityArgs args;
        public Action<Game, AbilityArgs> callback;
    }
    
    
    public class MoveQueueElement
    {
        public Card card;
        public Slot slot;
        public Action<Card, Slot, bool, bool> callback;
    }

    public class CallbackQueueElement
    {
        public Action callback;
    }
}
