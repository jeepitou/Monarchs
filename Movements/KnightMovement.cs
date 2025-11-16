using System;
using System.Collections;
using System.Collections.Generic;
using Monarchs.Logic;
using TcgEngine;
using TcgEngine.Client;
using UnityEngine;

/// <summary>
/// This class returns all the legal moves of a knight (L movement)
/// </summary>
[CreateAssetMenu(fileName = "KnightMovement", menuName = "ChessTCG/Movement/KnightMovement")]
[Serializable]
public class KnightMovement : MovementScheme
{
    private const bool KNIGHT_MOVE_ONE_SQUARE_THEN_TWO = true;
    public override Vector2S[] GetLegalMoves(Vector2S currentPosition, int moveRange,bool jumping, int playerId, Game game, bool canTargetAlly=false)
    {
        List<Vector2S> coordinates = new List<Vector2S>();
        
        (int x, int y)[] knightMovements = new[] {(-2, -1), ( -2, 1), ( -1, -2), ( -1, 2 ),
            ( 1, -2 ), ( 1, 2 ), ( 2, -1 ), ( 2, 1 )};

        foreach (var movement in knightMovements)
        {
            Vector2S coordinate = new Vector2S(currentPosition.x + movement.x, currentPosition.y + movement.y);
            
            if (ValidateCoordinateWithBoardState(coordinate.x, coordinate.y, playerId, jumping, game).AddCoordinate || canTargetAlly)
            {
                coordinates.Add(coordinate);
            }
        }

        return coordinates.ToArray();
    }
    
    // This returns an array with the closest available square next to a target position on the knight trajectory.
    // It considers both move trajectory that a knight could potentially take.
    public override Vector2S[] GetClosestAvailableSquaresOnMoveTrajectory(Vector2S currentPosition,
        Vector2S targetPosition, Game game)
    {
        int directionX = Math.Sign(currentPosition.x - targetPosition.x);
        int directionY = Math.Sign(currentPosition.y - targetPosition.y);

        (int x, int y)[] path;

        if (KNIGHT_MOVE_ONE_SQUARE_THEN_TWO)
        {
            if (Math.Abs(currentPosition.x - targetPosition.x) < Math.Abs(currentPosition.y - targetPosition.y))
            {
                path = new[] {(directionX, directionY), (directionX, 0)};

            }
            else
            {
                path = new[] {(directionX, directionY), (0, directionY)};
            }
        }
        else
        {
            if (Math.Abs(currentPosition.x - targetPosition.x) < Math.Abs(currentPosition.y - targetPosition.y))
            {
                path = new[] {(directionX*2, 0), (directionX, 0)};

            }
            else
            {
                path = new[] {(0, directionY*2), (0, directionY)};
            }
        }
        

        List<Vector2S> returnList = new List<Vector2S>();

        for (int i = 0; i < path.Length; i++)
        {
            if (!isSquareOccupied(currentPosition.x-path[i].x, currentPosition.y-path[i].y, game))
            {
                returnList.Add(new Vector2S(currentPosition.x-path[i].x, currentPosition.y-path[i].y));
            }

            if (returnList.Count > 0)
            {
                break;
            }
        }

        if (returnList.Count > 0)
        {
            return returnList.ToArray();
        }

        return new Vector2S[] {currentPosition};
    }
}
