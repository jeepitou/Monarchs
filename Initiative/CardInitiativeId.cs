using System;
using Monarchs.Logic;

[System.Serializable]
public class CardInitiativeId
{
    public string cohortUid;
    public int initiative;
    public bool active;
    public int remainingMuster;
    public bool isAmbushing = false;

    public CardInitiativeId()
    {
    }

    public CardInitiativeId(string cohortUid, int initiative, bool active, int remainingMuster)
    {
        this.cohortUid = cohortUid;
        this.initiative = initiative;
        this.active = active;
        this.remainingMuster = remainingMuster;
    }

    public virtual void ReduceMuster(Game game)
    {
        if (!active)
        {
            foreach (var cardToCheck in game.GetBoardCardsOfCohort(cohortUid))
            {
                if (cardToCheck.remainingMuster > 0)
                {
                    cardToCheck.remainingMuster -= 1;
                    remainingMuster = cardToCheck.remainingMuster;
                }
                else
                {
                    active = true;
                }
            }
        }
    }

    public static bool operator ==(CardInitiativeId id1, CardInitiativeId id2)
    {
        if (id1 is null)
        {
            return id2 is null;
        }

        return id1.Equals(id2);
    }

    public static bool operator !=(CardInitiativeId id1, CardInitiativeId id2)
    {
        if (id1 is null)
        {
            return id2 is not null;
        }

        return !id1.Equals(id2);
    }

    public override bool Equals(object o)
    {
        if (ReferenceEquals(null, o)) return false;
        if (ReferenceEquals(this, o)) return true;
        if (o.GetType() != this.GetType()) return false;
        return Equals((CardInitiativeId)o);
    }

    protected bool Equals(CardInitiativeId other)
    {
        if (other == null)
        {
            return false;
        }
        return cohortUid == other.cohortUid && initiative == other.initiative &&
               active == other.active && remainingMuster == other.remainingMuster && isAmbushing == other.isAmbushing;
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(cohortUid, initiative, active, remainingMuster, isAmbushing);
    }

    public CardInitiativeId Clone()
    {
        CardInitiativeId cardInit = new CardInitiativeId();
        cardInit.cohortUid = cohortUid;
        cardInit.initiative = initiative;
        cardInit.active = active;
        cardInit.remainingMuster = remainingMuster;
        cardInit.isAmbushing = isAmbushing;
        return cardInit;
    }
}