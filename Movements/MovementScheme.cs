using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Monarchs.Logic;
using TcgEngine;
using TcgEngine.Client;
using UnityEngine;

/// <summary>
/// This abstract class is the base of every possible movement scheme in the game.
/// It has a GetLegalMoves function that will be overwritten by each Child Class
/// to return the appropriate array of legal moves.
///
/// movementSchemeImage will be the image that is displayed on the card of the piece.
/// </summary>
[Serializable]
public abstract class MovementScheme : ScriptableObject
{
    public abstract Vector2S[] GetLegalMoves(Vector2S currentPosition, int moveRange, bool jumping, int playerId, Game game, bool canTargetAlly=false);
    public bool isPawn = false;
    
    public List<Vector2S> GetAllSquaresAttainableWithRangedAttack(Vector2S currentPosition, int minAttackRange, int maxAttackRange, Game game, bool indirectFire)
    {
        if (isPawn)
        {
            Vector2S[] slots =GetAllSquaresOnMovementSchemeForAbility(currentPosition, maxAttackRange, game, 0, indirectFire);
            return FilterRangedAttackThatAreTooClose(slots, currentPosition, minAttackRange);
        }
        return GetLegalRangedAttack(currentPosition, minAttackRange, maxAttackRange, 0, game, true, indirectFire, false);
    }
    
    public virtual List<Vector2S> GetLegalRangedAttack(Vector2S currentPosition, int minAttackRange, int maxAttackRange, int playerId, Game game, bool canAttackGround=false, bool indirectFire=false, bool canTargetAlly=false)
    {
        if (maxAttackRange <= 1)
        {
            return new List<Vector2S>();
        }
        
        Vector2S[] legalMoves = GetLegalMoves(currentPosition, maxAttackRange, indirectFire, playerId, game, canTargetAlly);
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
    
    public List<Vector2S> GetLegalMeleeAttack(Vector2S currentPosition, int moveRange, bool jumping, int playerID, Game game, bool canTargetAlly=false)
    {
        Vector2S[] legalMoves = GetLegalMoves(currentPosition, moveRange, jumping, playerID, game, canTargetAlly);
        legalMoves = FilterEmptySlots(game, legalMoves).ToArray();

        return legalMoves.ToList();
    }

    protected List<Vector2S> FilterRangedAttackThatAreTooClose(Vector2S[] rangedAttack, Vector2S currentPosition, int minRange)
    {
        if (minRange <= 1)
        {
            minRange = 2;
        }
        
        List<Vector2S> returnList = new List<Vector2S>();
        foreach (var coordinate in rangedAttack)
        {
            if (!Slot.Get(currentPosition.x, currentPosition.y).IsInDistance(Slot.Get(coordinate.x, coordinate.y), minRange-1))
            {
                returnList.Add(coordinate);
            }
        }

        return returnList;
    }

    protected List<Vector2S> FilterEmptySlots(Game game, Vector2S[] rangedAttack)
    {
        List<Vector2S> returnList = new List<Vector2S>();
        
        foreach (var coordinate in rangedAttack)
        {
            if (isSquareOccupied(coordinate.x, coordinate.y, game))
            {
                returnList.Add(coordinate);
            }
        }

        return returnList;
    }
    
    public virtual Vector2S[] GetAllSquaresOnMovementSchemeForAbility(Vector2S currentPosition, int moveRange, Game game, int playerId, bool indirectFire = false)
    {
        return GetAllSquaresOnMovementScheme(currentPosition, moveRange, game, playerId, indirectFire);
    }

    public Vector2S[] GetAllSquaresOnMovementScheme(Vector2S currentPosition, int moveRange, Game game, int playerId, bool indirectFire = false)
    {
        return GetLegalMoves(currentPosition, moveRange, indirectFire, playerId, game, true);
    }

    public virtual Vector2S[] GetClosestAvailableSquaresOnMoveTrajectory(Vector2S currentPosition,
        Vector2S targetPosition, Game game)
    {
        int directionX = Math.Sign(currentPosition.x - targetPosition.x);
        int directionY = Math.Sign(currentPosition.y - targetPosition.y);
        Vector2S direction = new Vector2S(directionX, directionY);

        Vector2S positionToCheck = targetPosition + direction;

        while (positionToCheck != currentPosition)
        {
            if (!isSquareOccupied(positionToCheck.x, positionToCheck.y, game))
            {
                return new Vector2S[]{positionToCheck};
            }

            positionToCheck = positionToCheck + direction;
        }
        
        return new Vector2S[]{currentPosition};
    }

    public Sprite movementSchemeImage;
    
    protected bool coordinateIsOnBoard(int x, int y)
    {
        return !(x < 0 || x > 7 || y < 0 || y > 7);
    }

    protected bool isSquareOccupied(int x, int y, Game game)
    {
        Card card = game.GetSlotCard(Slot.Get(x, y));

        if (card == null)
        {
            return false;
        }

        return true;
    }


    /// <summary>
    /// This verify if there is a piece on the coordinate and returns a CoordinateValidation
    /// CoordinateValidation.AddCoordinate is true if we should add the coordinate (when there is no pieces, or the piece is an enemy)
    /// CoordinateValidation.LineIntercepted is true if we found a piece and the piece isn't jumping.
    /// </summary>
    /// <param name="x">x Coordinate to check</param>
    /// <param name="y">y coordinate to check</param>
    /// <param name="playerId">Id of the player playing</param>
    /// <param name="jumping">bool saying if the piece is jumping</param>
    /// <param name="pieces">Dictionary of pieces</param>
    /// <returns></returns>
    protected CoordinateValidation ValidateCoordinateWithBoardState(int x, int y, int playerId, bool jumping, Game game, bool canAttack=true, bool mustAttack=false)
    {
        CoordinateValidation coordinateValidation = new CoordinateValidation();
        
        if (coordinateIsOnBoard(x, y))
        {
            Card card = game.GetSlotCard(Slot.Get(x, y));
            if (card != null)
            {
                // If the piece is an opponent, we can attack it
                if (canAttack)
                {
                    coordinateValidation.AddCoordinate = card.playerID != playerId ? true : false;
                }

                if (!jumping && !card.HasTrait("incorporeal")) //If we're not jumping, we stop to check the line.
                {
                    coordinateValidation.LineIntercepted = true;
                }
            }
            else
            {
                if (!mustAttack)
                {
                    coordinateValidation.AddCoordinate = true;
                }
            }
        }

        return coordinateValidation;
    }

    protected struct CoordinateValidation
    {
        public bool AddCoordinate;
        public bool LineIntercepted;
    }

    
}
