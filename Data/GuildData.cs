using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TcgEngine
{
    /// <summary>
    /// Defines all factions data
    /// </summary>
    
    [CreateAssetMenu(fileName = "GuildData", menuName = "TcgEngine/GuildData", order = 1)]
    public class GuildData : ScriptableObject
    {
        public string id;
        public string title;
        public Sprite icon;

        public static List<GuildData> guild_list = new List<GuildData>();

        public static void Load(string folder = "")
        {
            if (guild_list.Count == 0)
                guild_list.AddRange(Resources.LoadAll<GuildData>(folder));
        }

        public static GuildData Get(string id)
        {
            foreach (GuildData guild in GetAll())
            {
                if (guild.id == id)
                    return guild;
            }
            return null;
        }

        public static List<GuildData> GetAll()
        {
            return guild_list;
        }
    }
}