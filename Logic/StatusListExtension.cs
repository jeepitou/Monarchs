using System.Collections.Generic;
using Monarchs.Logic;
using TcgEngine;

public static class StatusListExtension
{
    public static CardStatus GetStatus(this List<CardStatus> statuses, StatusType type)
    {
        return statuses.Find(s => s.type == type);
    }
    
    public static CardStatus GetStatus(this List<CardStatus> statuses, string id)
    {
        return statuses.Find(s => s.id == id);
    }
    
    public static void AddStatus(this List<CardStatus> statuses, CardStatus newStatus)
    {
        var status = newStatus.type == StatusType.None ? statuses.GetStatus(newStatus.id) : statuses.GetStatus(newStatus.type);
        if (status != null)
        {
            status.value = newStatus.value;
            status.duration = newStatus.duration;
            status.removeAtBeginningOfTurn = newStatus.removeAtBeginningOfTurn;
            status.id = newStatus.id;
            status.applierUID = newStatus.applierUID;
        }
        else
        {
            statuses.Add(newStatus);
        }
    }

    public static void RemoveStatus(this List<CardStatus> statuses, StatusType type)
    {
        statuses.RemoveAll(s => s.type == type);
    }

    public static bool HasStatus(this List<CardStatus> statuses, StatusType type)
    {
        return statuses.Exists(s => s.type == type);
    }
}
