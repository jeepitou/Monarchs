using System.Collections.Generic;
using Monarchs.Logic;
using TcgEngine;

public static class StatusListExtension
{
    public static CardStatus GetStatus(this List<CardStatus> statuses, StatusType type)
    {
        return statuses.Find(s => s.type == type);
    }

    public static void SetStatus(this List<CardStatus> statuses, StatusType type, int value, int duration = 1)
    {
        var status = statuses.GetStatus(type);
        if (status != null)
        {
            status.value = value;
            status.duration = duration;
        }
        else
        {
            statuses.Add(new CardStatus(type, value, duration));
        }
    }

    public static void AddStatus(this List<CardStatus> statuses, StatusType type, int value, int duration = 1, bool stackValue = true)
    {
        var status = statuses.GetStatus(type);
        if (status != null)
        {
            status.value += value;
            status.duration = System.Math.Max(status.duration, duration);
        }
        else
        {
            statuses.SetStatus(type, value, duration);
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
