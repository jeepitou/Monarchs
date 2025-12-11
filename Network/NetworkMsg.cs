using System.Collections.Generic;
using Monarchs.Ability.Target;
using Monarchs.Initiative;
using Monarchs.Logic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Events;

namespace TcgEngine
{
    //-------- Connection --------

    public class MsgPlayerConnect : INetworkSerializable
    {
        public string user_id;
        public string username;
        public string game_uid;
        public int nb_players;
        public bool observer; //join as observer

        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
        {
            serializer.SerializeValue(ref user_id);
            serializer.SerializeValue(ref username);
            serializer.SerializeValue(ref game_uid);
            serializer.SerializeValue(ref nb_players);
            serializer.SerializeValue(ref observer);
        }
    }

    public class MsgAfterConnected : INetworkSerializable
    {
        public bool success;
        public int playerID;
        public Game game_data;

        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
        {
            serializer.SerializeValue(ref success);
            serializer.SerializeValue(ref playerID);

            if (serializer.IsReader)
            {
                int size = 0;
                serializer.SerializeValue(ref size);
                if (size > 0)
                {
                    byte[] bytes = new byte[size];
                    serializer.SerializeValue(ref bytes);
                    game_data = NetworkTool.Deserialize<Game>(bytes);
                }
            }

            if (serializer.IsWriter)
            {
                byte[] bytes = NetworkTool.Serialize(game_data);
                int size = bytes.Length;
                serializer.SerializeValue(ref size);
                if(size > 0)
                    serializer.SerializeValue(ref bytes);
            }
        }
    }

    //-------- Matchmaking --------

    public class MsgMatchmaking : INetworkSerializable
    {
        public string user_id;
        public string username;
        public string group;
        public int players;
        public int elo;
        public bool refresh;
        public float time;

        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
        {
            serializer.SerializeValue(ref user_id);
            serializer.SerializeValue(ref username);
            serializer.SerializeValue(ref group);
            serializer.SerializeValue(ref players);
            serializer.SerializeValue(ref elo);
            serializer.SerializeValue(ref refresh);
            serializer.SerializeValue(ref time);
        }
    }

    public class MatchmakingResult : INetworkSerializable
    {
        public bool success;
        public int players;
        public string group;
        public string server_url;
        public string game_uid;

        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
        {
            serializer.SerializeValue(ref success);
            serializer.SerializeValue(ref players);
            serializer.SerializeValue(ref group);
            serializer.SerializeValue(ref server_url);
            serializer.SerializeValue(ref game_uid);
        }
    }

    public class MsgMatchmakingList : INetworkSerializable
    {
        public string username;

        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
        {
            serializer.SerializeValue(ref username);
        }
    }

    [System.Serializable]
    public struct MatchmakingListItem : INetworkSerializable
    {
        public string group;
        public string user_id;
        public string username;

        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
        {
            serializer.SerializeValue(ref group);
            serializer.SerializeValue(ref user_id);
            serializer.SerializeValue(ref username);
        }
    }

    public class MatchmakingList : INetworkSerializable
    {
        public MatchmakingListItem[] items;

        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
        {
            NetworkTool.NetSerializeArray(serializer, ref items);
        }
    }

    [System.Serializable]
    public class MatchListItem : INetworkSerializable
    {
        public string group;
        public string username;
        public string game_uid;
        public string game_url;

        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
        {
            serializer.SerializeValue(ref group);
            serializer.SerializeValue(ref username);
            serializer.SerializeValue(ref game_uid);
            serializer.SerializeValue(ref game_url);
        }
    }

    public class MatchList : INetworkSerializable
    {
        public MatchListItem[] items;

        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
        {
            NetworkTool.NetSerializeArray(serializer, ref items);
        }
    }

    //-------- In Game --------
    
    public class MsgMulliganDiscarded : INetworkSerializable
    {
        public string[] discardedCards;

        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
        {
            NetworkTool.NetSerializeArray(serializer, ref discardedCards);
        }
    }
    
    public class MsgMovePiece : INetworkSerializable
    {
        public string cardUID;
        public Vector2Int startPos;
        public Vector2Int endPos;

        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
        {
            serializer.SerializeValue(ref cardUID);
            serializer.SerializeValue(ref startPos);
            serializer.SerializeValue(ref endPos);
        }
    }
    
    public class MsgSpawnPiece : INetworkSerializable
    {
        public string cardUID;
        public Vector2Int position;

        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
        {
            serializer.SerializeValue(ref cardUID);
            serializer.SerializeValue(ref position);
        }
    }

    public class MsgPlayCard : INetworkSerializable
    {
        public string cardUID;
        public Slot slot;

        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
        {
            serializer.SerializeValue(ref cardUID);
            serializer.SerializeNetworkSerializable(ref slot);
        }
    }

    public class MsgCard : INetworkSerializable
    {
        public string cardUID;

        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
        {
            serializer.SerializeValue(ref cardUID);
        }
    }
    
    public class MsgCardWithID : INetworkSerializable
    {
        public string cardUID;
        public string cardID;

        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
        {
            serializer.SerializeValue(ref cardUID);
            serializer.SerializeValue(ref cardID);
        }
    }
    
    

    public class MsgPlayer : INetworkSerializable
    {
        public int playerID;

        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
        {
            serializer.SerializeValue(ref playerID);
        }
    }

    public class MsgSelectCasterTargetSlot : INetworkSerializable
    {
        public string casterUID;
        public int slotX;
        public int slotY;

        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
        {
            serializer.SerializeValue(ref slotX);
            serializer.SerializeValue(ref slotY);
            serializer.SerializeValue(ref casterUID);
        }
    }

    public class MsgAttack : INetworkSerializable
    {
        public string attackerUID;
        public int slotX;
        public int slotY;
        public int damage;

        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
        {
            serializer.SerializeValue(ref attackerUID);
            serializer.SerializeValue(ref slotX);
            serializer.SerializeValue(ref slotY);
            serializer.SerializeValue(ref damage);
        }
    }

    public class MsgAttackPlayer : INetworkSerializable
    {
        public string attackerUID;
        public int targetID;
        public int damage;

        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
        {
            serializer.SerializeValue(ref attackerUID);
            serializer.SerializeValue(ref targetID);
            serializer.SerializeValue(ref damage);
        }
    }

    public class MsgCastAbility : INetworkSerializable
    {
        public string abilityID;
        public string casterUID;
        public string target_uid;
        public int VFX_Index;
        public int slotX;
        public int slotY;
        public bool isSelectTarget = false;

        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
        {
            serializer.SerializeValue(ref abilityID);
            serializer.SerializeValue(ref casterUID);
            serializer.SerializeValue(ref target_uid);
            serializer.SerializeValue(ref VFX_Index);
            serializer.SerializeValue(ref isSelectTarget);
            serializer.SerializeValue(ref slotX);
            serializer.SerializeValue(ref slotY);
        }
    }

    public class MsgCastAbilityPlayer : INetworkSerializable
    {
        public string abilityID;
        public string casterUID;
        public int targetID;

        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
        {
            serializer.SerializeValue(ref abilityID);
            serializer.SerializeValue(ref casterUID);
            serializer.SerializeValue(ref targetID);
        }
    }

    public class MsgCastAbilitySlot : INetworkSerializable
    {
        public string abilityID;
        public string casterUID;
        public Slot slot;
        public bool isSelectTarget=false;

        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
        {
            serializer.SerializeValue(ref abilityID);
            serializer.SerializeValue(ref casterUID);
            serializer.SerializeNetworkSerializable(ref slot);
            serializer.SerializeValue(ref isSelectTarget);
        }
    }

    public class MsgCastAbilityMultipleTarget: INetworkSerializable
    {
        public string abilityID;
        public string casterUID;
        public Slot slot1;
        public Slot slot2;
        public Slot slot3;
        public Slot slot4;
        public Slot slot5;

        
        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
        {
            serializer.SerializeValue(ref abilityID);
            serializer.SerializeValue(ref casterUID);

            serializer.SerializeNetworkSerializable(ref slot1);
            serializer.SerializeNetworkSerializable(ref slot2);
            serializer.SerializeNetworkSerializable(ref slot3);
            serializer.SerializeNetworkSerializable(ref slot4);
            serializer.SerializeNetworkSerializable(ref slot5);
        }
    }
    
    public class MsgSlot: INetworkSerializable
    {
        public Slot slot;

        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
        {
            serializer.SerializeNetworkSerializable(ref slot);
        }
    }

    public class MsgTrap : INetworkSerializable
    {
        public string trapUID;
        public string triggerer_uid;

        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
        {
            serializer.SerializeValue(ref trapUID);
            serializer.SerializeValue(ref triggerer_uid);
        }
    }

    public class MsgInt : INetworkSerializable
    {
        public int value;

        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
        {
            serializer.SerializeValue(ref value);
        }
    }
    
    public class MsgClientID : INetworkSerializable
    {
        public ulong clientID;
        public int secondsUntilForfeit;

        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
        {
            serializer.SerializeValue(ref clientID);
            serializer.SerializeValue(ref secondsUntilForfeit);
        }
    }

    public class MsgChat : INetworkSerializable
    {
        public int playerID;
        public string msg;

        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
        {
            serializer.SerializeValue(ref playerID);
            serializer.SerializeValue(ref msg);
        }
    }

    public class MsgInitiativeOrder : INetworkSerializable
    {
        public InitiativeOrder initiativeOrder;

        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
        {
            if (serializer.IsReader)
            {
                int size = 0;
                serializer.SerializeValue(ref size);
                if (size > 0)
                {
                    byte[] bytes = new byte[size];
                    serializer.SerializeValue(ref bytes);
                    initiativeOrder = NetworkTool.Deserialize<InitiativeOrder>(bytes);
                }
            }

            if (serializer.IsWriter)
            {
                byte[] bytes = NetworkTool.Serialize(initiativeOrder);
                int size = bytes.Length;
                serializer.SerializeValue(ref size);
                if (size > 0)
                    serializer.SerializeValue(ref bytes);
            }
        }
    }

    public class MsgRefreshAll : INetworkSerializable
    {
        public Game game_data;

        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
        {
            if (serializer.IsReader)
            {
                int size = 0;
                serializer.SerializeValue(ref size);
                if (size > 0)
                {
                    byte[] bytes = new byte[size];
                    serializer.SerializeValue(ref bytes);
                    game_data = NetworkTool.Deserialize<Game>(bytes);
                }
            }

            if (serializer.IsWriter)
            {
                byte[] bytes = NetworkTool.Serialize(game_data);
                int size = bytes.Length;
                serializer.SerializeValue(ref size);
                if (size > 0)
                    serializer.SerializeValue(ref bytes);
            }
        }
    }

}