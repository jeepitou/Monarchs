using System;
using System.Collections;
using System.Collections.Generic;
using Monarchs.Logic;
using TcgEngine;
using TcgEngine.Client;
using UnityEngine;

/// <summary>
/// This class returns the legal moves of a bishop (diagonal movement)
/// </summary>
[CreateAssetMenu(fileName = "BishopMovement", menuName = "ChessTCG/Movement/BishopMovement")]
[Serializable]
public class BishopMovement : MovementScheme
{
    public override Vector2S[] GetLegalMoves(Vector2S currentPosition, int moveRange, bool jumping, int playerId, Game game, bool canTargetAlly=false)
    {
        List<Vector2S> coordinates = new List<Vector2S>();

        int minX = Math.Max(currentPosition.x - moveRange, 0);
        int maxX = Math.Min(currentPosition.x + moveRange, 7);

        coordinates.AddRange(AddDiagonal(currentPosition, minX, playerId, jumping, -1, -1, game, true , false, canTargetAlly));
        coordinates.AddRange(AddDiagonal(currentPosition, maxX, playerId, jumping, 1, -1, game, true , false, canTargetAlly));
        coordinates.AddRange(AddDiagonal(currentPosition, minX, playerId, jumping, -1, 1, game, true , false, canTargetAlly));
        coordinates.AddRange(AddDiagonal(currentPosition, maxX, playerId, jumping, 1, 1, game, true , false, canTargetAlly));
        
        return coordinates.ToArray();
    }

    /// <summary>
    /// This checks the diagonals in one X direction. It stops looking in a direction when it finds a piece, except if it's jumping.
    /// </summary>
    public Vector2S[] AddDiagonal(Vector2S currentPosition, int limitX, int playerId, bool jumping, int directionX,
        int directionY, Game game, bool canAttack = true, bool mustAttack = false, bool canTargetAlly=false)
    {
        List<Vector2S> coordinates = new List<Vector2S>();
        bool intercepted = false;
        int y;
        
        for (int x = currentPosition.x + directionX; x != limitX+directionX; x = x+directionX)
        {
            y = currentPosition.y + Math.Abs(currentPosition.x - x)*directionY;
            if (!intercepted && !canTargetAlly )
            {
                CoordinateValidation coordinateValidation =
                    ValidateCoordinateWithBoardState(x, y, playerId, jumping, game, canAttack, mustAttack);
                if (coordinateValidation.AddCoordinate)
                {
                    coordinates.Add(new Vector2Int(x, y));
                }

                if (coordinateValidation.LineIntercepted)
                {
                    intercepted = true;
                }
            }
            else if (canTargetAlly)
            {
                Card card = game.GetSlotCard(Slot.Get(x, y));
                if (card == null && mustAttack)
                {
                    continue;
                }
                
                coordinates.Add(new Vector2Int(x, y));
            }
        }

        return coordinates.ToArray();
    }
}
