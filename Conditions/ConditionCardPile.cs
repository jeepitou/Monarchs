using System.Collections;
using System.Collections.Generic;
using Monarchs.Ability;
using Monarchs.Logic;
using Monarchs.Logic.AbilitySystem;
using UnityEngine;

namespace TcgEngine
{
    /// <summary>
    /// Condition that checks in which pile a card is (deck/discard/hand/board/secrets)
    /// </summary>

    [CreateAssetMenu(fileName = "condition", menuName = "TcgEngine/Condition/CardPile", order = 10)]
    public class ConditionCardPile : ConditionData
    {
        [Header("Card is in pile")]
        public PileType type;
        public ConditionOperatorBool oper;

        public override bool IsTargetConditionMetCardTarget(Game data, AbilityArgs args)
        {
            Card target = args.CardTarget;
            if (target == null || data == null)
            {
                Debug.LogError("Tried to apply ConditionCardPile on null target or Game.");
                return false;
            }

            if (type == PileType.Hand)
            {
                return CompareBool(data.IsInHand(target), oper);
            }

            if (type == PileType.Board)
            {
                return CompareBool(data.IsOnBoard(target), oper);
            }

            if (type == PileType.Deck)
            {
                return CompareBool(data.IsInDeck(target), oper);
            }

            if (type == PileType.Discard)
            {
                return CompareBool(data.IsInDiscard(target), oper);
            }

            if (type == PileType.Temp)
            {
                return CompareBool(data.IsInTemp(target), oper);
            }

            return false;
        }

        public override bool IsTargetConditionMetPlayerTarget(Game data, AbilityArgs args)
        {
            return false; //Player cannot be in a pile
        }

        public override bool IsTargetConditionMetSlotTarget(Game data, AbilityArgs args)
        {
            return type == PileType.Board && args.SlotTarget != Slot.None; //Slot is always on board
        }
    }
}