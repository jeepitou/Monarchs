using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TcgEngine
{

    public enum StatusType
    {
        None = 0,

        AttackBonus = 4,      //Attack status can be used for attack boost limited for X turns 
        HPBonus = 5,          //Attack status can be used for hp boost limited for X turns 

        Stealth = 10,       //Cant be attacked until do action
        Charge = 11,        // Can move twice in a turn
        Invincibility = 12, //Cant be attacked for X turns
        Shell = 13,         //Receives no damage the first time
        Protection = 14,    //Taunt, gives Protected to other cards
        Protected = 15,     //Cards that are protected by taunt
        Armor = 16,         //Receives less damage
        ArmorPenetration = 17, //Ignores armor
        SpellImmunity = 18, //Cant be targeted/damaged by spells
        MindControlled = 19,

        DeathStrike = 20,    //Kills when attacking a character
        Fury = 22,          //Can attack twice per turn
        Flying = 24,         //Can ignore taunt
        Trample = 26,         //Extra damage is assigned to player

        Sabotage = 29,          //Next attack deals no damage
        Silenced = 30,      //All abilities canceled
        Stunned = 32,     //Cant do any actions for X turns
        Immobilize = 33,    //Cant move, can still play card, and attack on melee. Won't move if it kills a card
        Poisoned = 34,     //Lose hp each start of turn
        Sleep = 36,         //Doesnt untap at the start of turn
        Disarmed = 37,       //Cannot attack during it's turn
        ImpendingDoom = 38,   //Target takes double damage from all source
        LostAbilities = 39,   //Lose ability
        MoveRange = 40,       //Change move range
        AttackRange = 41,     //Change attack range
        Rabies
    }

    /// <summary>
    /// Defines all status effects data
    /// Status are effects that can be gained or lost with abilities, and that will affect gameplay
    /// Status can have a duration
    /// </summary>

    [CreateAssetMenu(fileName = "status", menuName = "TcgEngine/StatusData", order = 7)]
    public class StatusData : ScriptableObject
    {
        public StatusType effect;
        public string id;
        public BuffOrStatus buffOrStatus;

        [Header("Display")]
        public string title;
        public Sprite icon;

        [TextArea(3, 5)]
        public string desc;

        [Header("FX")]
        public GameObject status_fx;

        [Header("AI")]
        public int hvalue;

        public static List<StatusData> status_list = new List<StatusData>();

        public string GetTitle()
        {
            return title;
        }

        public string GetDesc()
        {
            return GetDesc(1);
        }

        public string GetDesc(int value)
        {
            string des = desc.Replace("<value>", value.ToString());
            return des;
        }

        public static void Load(string folder = "")
        {
            if (status_list.Count == 0)
                status_list.AddRange(Resources.LoadAll<StatusData>(folder));
        }

        public static StatusData Get(StatusType effect)
        {
            foreach (StatusData status in GetAll())
            {
                if (status.effect == effect)
                    return status;
            }
            return null;
        }
        
        public static StatusData Get(string id)
        {
            foreach (StatusData status in GetAll())
            {
                if (status.id == id)
                    return status;
            }
            return null;
        }

        public static List<StatusData> GetAll()
        {
            return status_list;
        }

        public enum BuffOrStatus
        {
            Buff,
            Status
        }
    }
}