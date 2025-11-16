using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TcgEngine
{
    /// <summary>
    /// Defines all factions data
    /// </summary>
    
    [CreateAssetMenu(fileName = "SubtypeData", menuName = "TcgEngine/SubtypeData", order = 1)]
    public class SubtypeData : ScriptableObject
    {
        public string id;
        public string title;
        public Sprite icon;

        public static List<SubtypeData> subtypeList = new List<SubtypeData>();

        public static void Load(string folder = "")
        {
            if (subtypeList.Count == 0)
                subtypeList.AddRange(Resources.LoadAll<SubtypeData>(folder));
        }

        public static SubtypeData Get(string id)
        {
            foreach (SubtypeData subtype in GetAll())
            {
                if (subtype.id == id)
                    return subtype;
            }
            return null;
        }

        public static List<SubtypeData> GetAll()
        {
            return subtypeList;
        }
    }
}