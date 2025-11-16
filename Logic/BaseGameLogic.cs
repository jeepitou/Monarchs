using System;
using System.Collections.Generic;
using Monarchs.Ability;
using Monarchs.Ability.Target;
using Monarchs.Logic.AbilitySystem;
using TcgEngine;
using UnityEngine;
using UnityEngine.Events;

namespace Monarchs.Logic
{
    /// <summary>
    /// Base class for game logic functionality, containing core functionality and common data
    /// </summary>
    public class BaseGameLogic
    {
        #region Events
        public UnityAction onGameStart;
        public UnityAction onBoardSetup;
        public UnityAction<Player> onGameEnd;          //Winner

        public UnityAction<int> onMulliganStart;
        public UnityAction onTurnStart;
        public UnityAction onRoundStart;
        public UnityAction onRoundEnd;
        public UnityAction onTurnPlay;
        public UnityAction onTurnEnd;

        public UnityAction<Card, Slot> onCardPlayed;      
        public UnityAction<Card, Slot> onCardSummoned;
        public UnityAction<Card, Slot> onCardMoved;
        public UnityAction<Card> onCardTransformed;
        public UnityAction<Card> onCardDiscarded;
        public UnityAction<int> onCardDrawn;
        public UnityAction<int> onRollValue;

        public UnityAction<AbilityData, Card> onAbilityStart;
        public UnityAction<AbilityArgs, bool> onAbilityTarget;
        public UnityAction<AbilityArgs, List<Slot>> onAbilityTargetMultiple;
        public UnityAction<AbilityArgs> onAbilityEnd;
        public UnityAction<string> onAbilitySummonedCardToHand;

        public UnityAction<Card, Card, int> onAttackStart;  //Attacker, Defender
        public UnityAction<Card, Card, int> onAttackEnd;     //Attacker, Defender
        public UnityAction<Card, Slot, int> onRangeAttackStart;
        public UnityAction<Card, Slot, int> onRangeAttackEnd;

        public UnityAction<Card, Card> onTrapTrigger;    
        public UnityAction<Card, Card> onTrapResolved;    

        public UnityAction onSelectorStart;
        public UnityAction onSelectorSelect;
        #endregion

        protected readonly SlotStatusTrigger _slotStatusTrigger;
        internal readonly AbilityLogicSystem _abilityLogicSystem;
        protected Game _game;

        protected readonly ResolveQueue _resolveQueue;

        public readonly System.Random random = new();

        protected readonly ListSwap<ITargetable> _cardArray = new();
        protected readonly ListSwap<ITargetable> _playerArray = new();
        protected readonly ListSwap<ITargetable> _slotArray = new();
        protected readonly ListSwap<ITargetable> _cardDataArray = new();

        public BaseGameLogic(bool isInstant)
        {
            //is_instant ignores all gameplay delays and process everything immediately, needed for AI prediction
            if (this is GameLogic gameLogic)
                _abilityLogicSystem = AbilityLogicSystem.Create(gameLogic);
            else
                throw new InvalidOperationException("BaseGameLogic must be instantiated as GameLogic");
                
            _slotStatusTrigger = new SlotStatusTrigger();
            _resolveQueue = new ResolveQueue(null, isInstant);
        }

        public BaseGameLogic(Game game, bool isInstant = false)
        {
            this._game = game;
            
            if (this is GameLogic gameLogic)
                _abilityLogicSystem = AbilityLogicSystem.Create(gameLogic);
            else
                throw new InvalidOperationException("BaseGameLogic must be instantiated as GameLogic");
                
            _slotStatusTrigger = new SlotStatusTrigger();
            _resolveQueue = new ResolveQueue(game, isInstant);
        }

        /// <summary>
        /// Sets the game data for this logic component
        /// </summary>
        public virtual void SetData(Game game)
        {
            this._game = game;
            _resolveQueue.SetData(game);
        }

        /// <summary>
        /// Updates the resolve queue with the given delta time
        /// </summary>
        public virtual void Update(float delta)
        {
            _resolveQueue.Update(delta);
        }

        /// <summary>
        /// Clears the current resolve queue
        /// </summary>
        public virtual void ClearResolve()
        {
            _resolveQueue.Clear();
        }

        /// <summary>
        /// Checks if the game is currently resolving actions
        /// </summary>
        /// <returns>True if resolving, false otherwise</returns>
        public virtual bool IsResolving()
        {
            return _resolveQueue.IsResolving();
        }

        /// <summary>
        /// Checks if the game has started
        /// </summary>
        /// <returns>True if the game has started, false otherwise</returns>
        public virtual bool IsGameStarted()
        {
            return _game.HasStarted();
        }

        /// <summary>
        /// Checks if the game has ended
        /// </summary>
        /// <returns>True if the game has ended, false otherwise</returns>
        public virtual bool IsGameEnded()
        {
            return _game.HasEnded();
        }

        /// <summary>
        /// Gets the current game data
        /// </summary>
        /// <returns>The current Game object</returns>
        public virtual Game GetGameData()
        {
            return _game;
        }

        /// <summary>
        /// Gets the random number generator used by this GameLogic instance
        /// </summary>
        /// <returns>The System.Random instance</returns>
        public System.Random GetRandom()
        {
            return random;
        }

        protected virtual void ClearRoundData()
        {
            _game.selector = SelectorType.None;
            _game.selectorAbilityID = "";
            _game.selectorCardUID = "";
            _game.selectorCasterUID = "";
            _game.selectorTargets = new List<ITargetable>();
            _resolveQueue.Clear();
            _cardArray.Clear();
            _playerArray.Clear();
            _slotArray.Clear();
            _cardDataArray.Clear();
            _game.lastPlayed = null;
            _game.lastKilled = null;
            _game.lastTarget = null;
            _game.abilityTriggerer = null;
            _game.abilityPlayed.Clear();
            _game.cardsAttacked.Clear();
        }

        /// <summary>
        /// Gets the current game data
        /// </summary>
        public virtual Game Game => _game;

        /// <summary>
        /// Gets the resolve queue
        /// </summary>
        public virtual ResolveQueue ResolveQueue => _resolveQueue;
    }
}