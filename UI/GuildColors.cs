using System;
using System.Collections;
using System.Collections.Generic;
using TcgEngine;
using UnityEngine;

namespace Monarchs
{
    [CreateAssetMenu(fileName = "GuildColors", menuName = "TcgEngine/GuildColors", order = 5)]
    public class GuildColors : ScriptableObject
    {
        public GuildColor[] guildColors;

        public ColorSO GetColor(GuildData guild)
        {
            foreach (var guildColor in guildColors)
            {
                if (guildColor.guild == guild)
                {
                    return guildColor.color;
                }
            }

            return null;
        }
    }

    [Serializable]
    public struct GuildColor
    {
        public GuildData guild;
        public ColorSO color;
    }
}
