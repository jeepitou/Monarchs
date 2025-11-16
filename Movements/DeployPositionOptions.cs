using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Monarchs.Logic;
using UnityEngine;

public class DeployPositionOptions : MonoBehaviour
{
    public static DeployPositions DefaultPosition {get{
        if (_defaultPosition == null)
        {
            CalculatePositions();
        }
        return _defaultPosition;
    }}
    
    public static DeployPositions VanguardPosition
    {
        get{
        if (_vanguardPosition == null)
        {
            CalculatePositions();
        }
        return _vanguardPosition;
    }}
    
    public static DeployPositions RearGuardPosition
    {
        get{
        if (_rearGuardPosition == null)
        {
            CalculatePositions();
        }
        return _rearGuardPosition;
    }}
    
    public static DeployPositions InfiltratedPosition
    {
        get{
        if (_infiltratedPosition == null)
        {
            CalculatePositions();
        }
        return _infiltratedPosition;
    }}
    
    public static DeployPositions FlankerPosition
    {
        get{
            if (_flankerPosition == null)
            {
                CalculatePositions();
            }
            return _flankerPosition;
        }}

    public static DeployPositions GetDeployPosition(DeployChoices choice, Game game)
    {
        DeployPositions returnValue = new DeployPositions();
        
        if (choice.HasFlag(DeployChoices.Default))
        {
            returnValue += DefaultPosition;
        }

        if (choice.HasFlag(DeployChoices.Vanguard))
        {
            returnValue += VanguardPosition;
        }

        if (choice.HasFlag(DeployChoices.Rearguard))
        {
            returnValue += RearGuardPosition;
        }

        if (choice.HasFlag(DeployChoices.Infiltrated))
        {
            returnValue += InfiltratedPosition;
        }

        if (choice.HasFlag(DeployChoices.Flanker))
        {
            returnValue += FlankerPosition;
        }
        
        if (choice.HasFlag(DeployChoices.Bodyguard))
        {
            returnValue += CalculateBodyGuardPositions(game);
        }
        
        return returnValue;
    }
    
    private static DeployPositions _defaultPosition;
    private static DeployPositions _vanguardPosition;
    private static DeployPositions _rearGuardPosition;
    private static DeployPositions _infiltratedPosition;
    private static DeployPositions _flankerPosition;
    
    private static void CalculatePositions()
    {
        CalculateRearGuardPosition();
        CalculateDefaultPosition();
        CalculateVanguardPosition();
        CalculateInfiltratedPosition();
        CalculateFlankerPosition();
    }
    
    private static void CalculateRearGuardPosition()
    {
        List<Vector2S> whitePositions = new List<Vector2S>();
        List<Vector2S> blackPositions = new List<Vector2S>();
        for (int i = 0; i <= Slot.yMax; i++)
        {
            whitePositions.Add(new Vector2S(i, 0));
            blackPositions.Add(new Vector2S(i, Slot.yMax));
        }

        _rearGuardPosition = new DeployPositions();
        _rearGuardPosition.PositionsWhite = whitePositions.ToArray();
        _rearGuardPosition.PositionsBlack = blackPositions.ToArray();

    }

    private static void CalculateDefaultPosition()
    {
        List<Vector2S> whitePositions = new List<Vector2S>();
        List<Vector2S> blackPositions = new List<Vector2S>();
        whitePositions.AddRange(_rearGuardPosition.PositionsWhite);
        blackPositions.AddRange(_rearGuardPosition.PositionsBlack);
        for (int i = 0; i <= Slot.yMax; i++)
        {
            whitePositions.Add(new Vector2S(i, 1));
            blackPositions.Add(new Vector2S(i, Slot.yMax - 1));
        }

        _defaultPosition = new DeployPositions();
        _defaultPosition.PositionsWhite = whitePositions.ToArray();
        _defaultPosition.PositionsBlack = blackPositions.ToArray();
    }

    private static void CalculateVanguardPosition()
    {
        List<Vector2S> whitePositions = new List<Vector2S>();
        List<Vector2S> blackPositions = new List<Vector2S>();
        whitePositions.AddRange(_defaultPosition.PositionsWhite);
        blackPositions.AddRange(_defaultPosition.PositionsBlack);
        for (int i = 0; i <= 7; i++)
        {
            whitePositions.Add(new Vector2S(i, 2));
            blackPositions.Add(new Vector2S(i, Slot.yMax - 2));
        }

        _vanguardPosition = new DeployPositions();
        _vanguardPosition.PositionsWhite = whitePositions.ToArray();
        _vanguardPosition.PositionsBlack = blackPositions.ToArray();
    }
    
    private static void CalculateInfiltratedPosition()
    {
        _infiltratedPosition = new DeployPositions();
        
        List<Vector2S> whitePositions = new List<Vector2S>();
        List<Vector2S> blackPositions = new List<Vector2S>();
        
        whitePositions.AddRange(_defaultPosition.PositionsWhite);
        blackPositions.AddRange(_defaultPosition.PositionsBlack);
        
        whitePositions.AddRange(_rearGuardPosition.PositionsBlack);
        blackPositions.AddRange(_rearGuardPosition.PositionsWhite);
        
        _infiltratedPosition.PositionsWhite = whitePositions.ToArray();
        _infiltratedPosition.PositionsBlack = blackPositions.ToArray();
    }

    private static void CalculateFlankerPosition()
    {
        List<Vector2S> whitePositions = new List<Vector2S>();
        List<Vector2S> blackPositions = new List<Vector2S>();
        whitePositions.AddRange(_defaultPosition.PositionsWhite);
        blackPositions.AddRange(_defaultPosition.PositionsBlack);
        
        for (int y = 0; y <= 7; y++)
        {
            whitePositions.Add(new Vector2S(0, y));
            whitePositions.Add(new Vector2S(Slot.xMax, y));
            blackPositions.Add(new Vector2S(0, y));
            blackPositions.Add(new Vector2S(Slot.xMax, y));
        }

        _flankerPosition = new DeployPositions();
        _flankerPosition.PositionsWhite = whitePositions.ToArray();
        _flankerPosition.PositionsBlack = blackPositions.ToArray();
    }

    private static DeployPositions CalculateBodyGuardPositions(Game game)
    {
        DeployPositions bodyguardPosition = new DeployPositions();

        List<Vector2S> whitePositions = new List<Vector2S>();
        foreach (var slot in game.players[game.firstPlayer].king.slot.GetSlotsInRange(1))
        {
            whitePositions.Add(new Vector2S(slot.x, slot.y));
        }
        
        List<Vector2S> blackPositions = new List<Vector2S>();
        foreach (var slot in game.players[game.SecondPlayerId()].king.slot.GetSlotsInRange(1))
        {
            blackPositions.Add(new Vector2S(slot.x, slot.y));
        }
        
        bodyguardPosition.PositionsWhite = whitePositions.ToArray();
        bodyguardPosition.PositionsBlack = blackPositions.ToArray();

        return bodyguardPosition;
    }
}

public class DeployPositions
{
    public Vector2S[] PositionsWhite = new Vector2S[0];
    public Vector2S[] PositionsBlack = new Vector2S[0];
    
    public static DeployPositions operator +(DeployPositions pos1, DeployPositions pos2)
    {
        DeployPositions returnValue = new DeployPositions();

        returnValue.PositionsWhite = pos1.PositionsWhite.Union(pos2.PositionsWhite).ToArray();
        returnValue.PositionsBlack = pos1.PositionsBlack.Union(pos2.PositionsBlack).ToArray();

        return returnValue;
    }
}

[Flags]
public enum DeployChoices
{
    Default = 1,
    Vanguard = 2,
    Rearguard = 4,
    Infiltrated = 8,
    Flanker = 16,
    Bodyguard = 32
}
