using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Monarchs.Ability;
using Monarchs.Logic;
using Monarchs.Logic.AbilitySystem;
using TcgEngine;
using TcgEngine.Client;
using UnityEngine;

/// <summary>
/// This condition validates if the slot is on the movement scheme of the piece caster
/// </summary>

[CreateAssetMenu(fileName = "condition", menuName = "TcgEngine/Condition/ConditionSlotOnMovementScheme")]
public class ConditionSlotOnMovementScheme : ConditionData
{
    public bool canBeSelf = false;
    public bool checkTriggerer = false;
    public MovementScheme overwriteMovementScheme = null;
    public int checkFromTargetNumber = -1;
    public bool checkRegularMovementScheme = false; //To validate if we use ability movement scheme or regular movement scheme
    
    public override bool IsTargetConditionMetSlotTarget(Game data, AbilityArgs args)
    {
        Card cardToCheckFrom = args.caster;
        if (checkTriggerer)
        {
            cardToCheckFrom = (Card)args.triggerer;
        }
        if (checkFromTargetNumber >= 1)
        {
            cardToCheckFrom = ((Card)data.selectorTargets[checkFromTargetNumber - 1]);
        }
        if (overwriteMovementScheme==null && cardToCheckFrom.CardData.MovementScheme == null)
        {
            return false;
        }

        if (canBeSelf && (Slot)args.target == cardToCheckFrom.slot)
        {
            return true;
        }

        MovementScheme movementScheme;
        if (overwriteMovementScheme != null)
        {
            movementScheme = overwriteMovementScheme;
        }
        else
        {
            movementScheme = cardToCheckFrom.CardData.MovementScheme;
        }

        Vector2S[] legalMoves;
        if (!checkRegularMovementScheme)
        {
            legalMoves = movementScheme.GetAllSquaresOnMovementSchemeForAbility(cardToCheckFrom.GetCoordinates(), 10, data, cardToCheckFrom.playerID, true);
        }
        else
        {
            legalMoves = movementScheme.GetAllSquaresOnMovementScheme(cardToCheckFrom.GetCoordinates(), 10, data, cardToCheckFrom.playerID, true);
        }
       
        if (legalMoves.Contains(args.SlotTarget.GetCoordinate()))
        {
            return true;
        }

        {
            return false;
        }
    }
}
