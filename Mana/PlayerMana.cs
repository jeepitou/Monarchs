using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Monarchs.Client;
using Monarchs.Logic;
using TcgEngine;
using UnityEngine;
using Random = System.Random;

[Serializable]
public class PlayerMana
{
    private ManaType _availableMana;
    private ManaType _generatingMana; // Mana that is being generated this turn, but not yet available

    public PlayerMana()
    {
        _availableMana = ManaType.None;
        _generatingMana = ManaType.None;
    }

    public void AddMana(ManaType type)
    {
        if (GameplayData.Get().generateManaOnNextTurn && !_availableMana.HasFlag(type))
        {
            _generatingMana |= type;
            return;
        }
        _availableMana |= type;
    }
    
    public void AddGeneratingMana()
    {
        _availableMana |= _generatingMana;
        _generatingMana = ManaType.None;
    }
    
    public ManaType GetGeneratingMana()
    {
        return _generatingMana;
    }
    
    public ManaType GetAvailableMana()
    {
        return _availableMana;
    }

    public ManaType GetRandomOwnedMana()
    {
        if (_availableMana == ManaType.None)
        {
            return ManaType.None;
        }
        
        var individualMana = _availableMana.GetIndividualFlags();
        
        var rand = new Random();
        int randomIndex = rand.Next(individualMana.Count());
        
        return (ManaType)individualMana.ElementAt(randomIndex);
    }

    public void AddRandomMana(ManaType choices, bool allowOwnedMana, Random random)
    {
        if (choices == ManaType.None) { return;}

        if (!allowOwnedMana)
        {
            choices &= ~_availableMana;
        }
        
        if (choices == ManaType.None) { return;}
        
        var individualMana = choices.GetIndividualFlags();
        
        int randomIndex = random.Next(individualMana.Count());
        
        AddMana((ManaType)individualMana.ElementAt(randomIndex));
    }
    
    public PlayerMana Clone()
    {
        PlayerMana playerMana = new PlayerMana();
        playerMana._availableMana = this._availableMana;

        return playerMana;
    }

    public bool HasManaForCard(Card card, bool isMonarchTurn)
    {
        bool hasMana = _availableMana.HasFlag(card.GetMana());
        bool hasAdditionalMana = hasMana && _availableMana != card.GetManaCost();
        
       return isMonarchTurn ? hasMana : hasAdditionalMana;
    }
    
    public void SpendMana(Card card)
    {
        _availableMana &= ~card.GetMana(); // This removes type from flag
    }

    public bool HasMana(ManaType type)
    {
        return _availableMana.HasFlag(type);
    }

    public void SpendMana(ManaType type)
    {
        if (!_availableMana.HasFlag(type))
        {
            Debug.LogError("Tried to spend mana that wasn't available.");
            return;
        }

        _availableMana &= ~type; // This removes type from flag
    }
    
    [System.Flags][Serializable]
    public enum ManaType
    {
        None = 0,
        Fire = 1 << 1,
        Light = 1 << 2,
        Dark = 1 << 3,
        Water = 1 << 4,
        Earth = 1 << 5,
        Air = 1 << 6
    }
}
