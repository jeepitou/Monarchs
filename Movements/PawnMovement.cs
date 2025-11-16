using System;
using System.Collections;
using System.Collections.Generic;
using Monarchs.Logic;
using TcgEngine;
using TcgEngine.Client;
using UnityEngine;

[CreateAssetMenu(fileName = "PawnMovement", menuName = "ChessTCG/Movement/PawnMovement")]
[Serializable]
public class PawnMovement : MovementScheme
{
    public RookMovement rookMovement;
    public BishopMovement bishopMovement;
    public bool canAttackStraight = false;
    private const bool CAN_ATTACK_DIAGONAL = true; 
    
    public override Vector2S[] GetLegalMoves(Vector2S currentPosition, int moveRange, bool jumping, int playerId, Game game, bool canTargetAlly=false)
    {
        List<Vector2S> coordinates = new List<Vector2S>();
        Card card = game.GetSlotCard(Slot.Get(currentPosition.x, currentPosition.y));

        int direction;
        ValidatePromotion(game, card, playerId, currentPosition);
        if (card!=null && card.promoted)
        {
            return card.GetCurrentMovementScheme().GetLegalMoves(currentPosition, card.GetMoveRange(), jumping,
                playerId, game);
        }
        
        if (game.firstPlayer == playerId)
        {
            direction = 1;
        }
        else
        {
            direction = -1;
        }

        int maxY;
        if (card==null || !card.hasMovedThisGame)
        {
            maxY = currentPosition.y + direction*moveRange + direction;
        }
        else
        {
            maxY = currentPosition.y + direction*moveRange;
        }

        Vector2S endPosition = new Vector2S(currentPosition.x, maxY);
        coordinates.AddRange(rookMovement.CheckLine(currentPosition, endPosition, jumping, canAttackStraight, playerId, game, canTargetAlly));

        if (CAN_ATTACK_DIAGONAL)
        {
            coordinates.AddRange(bishopMovement.AddDiagonal(currentPosition, currentPosition.x-moveRange, playerId, jumping, -1, direction, game, true, true, canTargetAlly ));
            coordinates.AddRange(bishopMovement.AddDiagonal(currentPosition, currentPosition.x+moveRange, playerId, jumping, 1, direction, game, true, true, canTargetAlly ));
        }

        return coordinates.ToArray();
    }

    public override Vector2S[] GetAllSquaresOnMovementSchemeForAbility(Vector2S currentPosition, int moveRange,
        Game game, int playerId, bool indirectFire = false)
    {
        Card card = game.GetSlotCard(Slot.Get(currentPosition.x, currentPosition.y));
        ValidatePromotion(game, card, playerId, currentPosition);
        if (card!=null && card.promoted)
        {
            return card.GetCurrentMovementScheme().GetAllSquaresOnMovementSchemeForAbility(currentPosition, card.GetMoveRange(), game,
                playerId, indirectFire);
        }
        
        List<Vector2S> coordinates = new List<Vector2S>();
        int direction;
        
        if (game.firstPlayer == playerId)
        {
            direction = 1;
        }
        else
        {
            direction = -1;
        }
        
        coordinates.AddRange(bishopMovement.AddDiagonal(currentPosition, currentPosition.x-moveRange, playerId, indirectFire, -1, direction, game, true, false, true ));
        coordinates.AddRange(bishopMovement.AddDiagonal(currentPosition, currentPosition.x + moveRange, playerId, indirectFire, 1,
            direction, game, true, false, true));
        
        return coordinates.ToArray();
    }

    public override List<Vector2S> GetLegalRangedAttack(Vector2S currentPosition, int minAttackRange, int maxAttackRange, int playerId, Game game, bool canAttackGround=false, bool indirectFire=false, bool canTargetAlly=false)
    {
        if (maxAttackRange <= 1)
        {
            return new List<Vector2S>();
        }

        Vector2S[] legalMoves = GetAllSquaresOnMovementSchemeForAbility(currentPosition, maxAttackRange,
            game, playerId, indirectFire);
        
        List<Vector2S> returnList = new List<Vector2S>();
        if (canAttackGround)
        {
            returnList = FilterRangedAttackThatAreTooClose(legalMoves, currentPosition, minAttackRange);
            return returnList;
        } 
        
        returnList = FilterEmptySlots(game, legalMoves);
        returnList = FilterRangedAttackThatAreTooClose(returnList.ToArray(), currentPosition, minAttackRange);
        return returnList;
    }

    private void ValidatePromotion(Game game, Card card, int playerId, Vector2S currentPosition)
    {
        if (game.firstPlayer == playerId)
        {
            if (currentPosition.y == Slot.yMax)
            {
                card.Promote();
            }
            
        }
        else
        {
            if (currentPosition.y == 0)
            {
                card.Promote();
            }
        }
    }
}
