using System;
using System.Collections;
using System.Collections.Generic;
using Monarchs.Logic;
using TcgEngine;
using TcgEngine.Client;
using UnityEngine;

/// <summary>
/// This movement scheme returns the legal move for the rook (horizontal/vertical) movement
/// </summary>
[CreateAssetMenu(fileName = "RookMovement", menuName = "ChessTCG/Movement/RookMovement")]
[Serializable]
public class RookMovement : MovementScheme
{
    public override Vector2S[] GetLegalMoves(Vector2S currentPosition, int moveRange, bool jumping, int playerId, Game game, bool canTargetAlly=false)
    {
        List<Vector2S> coordinates = new List<Vector2S>();

        int minX = Math.Max(currentPosition.x - moveRange, 0);
        int maxX = Math.Min(currentPosition.x + moveRange, 7);
        int minY = Math.Max(currentPosition.y - moveRange, 0);
        int maxY = Math.Min(currentPosition.y + moveRange, 7);

        coordinates.AddRange(CheckLine(currentPosition, new Vector2S(minX, currentPosition.y), jumping, true, playerId, game, canTargetAlly));
        coordinates.AddRange(CheckLine(currentPosition, new Vector2S(maxX, currentPosition.y), jumping, true, playerId, game, canTargetAlly));
        coordinates.AddRange(CheckLine(currentPosition, new Vector2S(currentPosition.x, minY), jumping, true, playerId, game, canTargetAlly));
        coordinates.AddRange(CheckLine(currentPosition, new Vector2S(currentPosition.x, maxY), jumping, true, playerId, game, canTargetAlly));

        return coordinates.ToArray();
    }
    
    /// <summary>
    /// This checks a line between the piece position and the end position. It will stop checking when it finds a pieces, except if it's juumping.
    /// </summary>
    public Vector2S[] CheckLine(Vector2S piecePosition, Vector2S endPosition, bool jumping, bool canAttack, int playerId, Game game, bool canTargetAlly = false)
    {
        List<Vector2S> coordinates = new List<Vector2S>();
        
        
        int x1 = piecePosition.x;
        int x2 = endPosition.x;
        int y1 = piecePosition.y;
        int y2 = endPosition.y;

        if (x1 != x2 && y1 != y2)
        {
            Debug.LogError("Tried to check a rook line that wasn't on a straight line.");
            return null;
        }
        
        int direction = x1 == x2 ? Math.Sign(endPosition.y - piecePosition.y) : Math.Sign(endPosition.x - piecePosition.x);

        for (int x = x1; x != x2+direction; x=x+direction)
        {
            for (int y = y1; y != y2+direction; y = y + direction)
            {
                if (x == x1 && y == y1) //If it's the current position, we skip.
                {
                    continue;
                }
                
                Vector2Int coordinate = new Vector2Int(x, y);
                if (!canTargetAlly)
                {
                    CoordinateValidation coordinateValidation =
                        ValidateCoordinateWithBoardState(x, y, playerId, jumping, game, canAttack);

                    if (coordinateValidation.AddCoordinate)
                    {
                        coordinates.Add(coordinate);
                    }

                    if (coordinateValidation.LineIntercepted)
                    {
                        return coordinates.ToArray();
                    }
                }
                else
                {
                    coordinates.Add(coordinate);
                }
            }
        }

        return coordinates.ToArray();
    }
}
