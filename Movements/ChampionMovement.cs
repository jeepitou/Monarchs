using System;
using System.Collections.Generic;
using Monarchs.Logic;
using TcgEngine;
using TcgEngine.Client;
using UnityEngine;


/// <summary>
/// This movement scheme returns the legal moves for a queen (all directions) movement.
/// </summary>
[CreateAssetMenu(fileName = "QueenMovement", menuName = "ChessTCG/Movement/QueenMovement")]
public class ChampionMovement : MovementScheme
{
    [SerializeField] private RookMovement rookMovement;
    [SerializeField] private BishopMovement bishopMovement;
    public override Vector2S[] GetLegalMoves(Vector2S currentPosition, int moveRange, bool jumping, int playerId, Game game, bool canTargetAlly=false)
    {
        List<Vector2S> coordinates = new List<Vector2S>();
        coordinates.AddRange(rookMovement.GetLegalMoves(currentPosition, moveRange, jumping, playerId, game, canTargetAlly));
        coordinates.AddRange(bishopMovement.GetLegalMoves(currentPosition, moveRange, jumping, playerId, game, canTargetAlly));

        return coordinates.ToArray();
    }
    
}
