namespace Monarchs.Ability
{
    public enum AbilityTrigger
    {
        None = 0,

        Ongoing = 2,  //Always active (does not work with all effects)
        Activate = 5, //Action

        OnPlay = 10,  //When playeds
        IntoTheFray = 11,
        OnPlayOther = 12,  //When another card played

        StartOfTurn = 18,
        EndOfTurn = 19,
        EndOfCasterTurn = 20, //End of the turn of the card that triggered the ability
        
        StartOfRound = 21, //Every turn
        EndOfRound = 22, //Every turn
        
        Enrage = 26, //When taking damage and surviving
        OnMoveOnSpecificSquare = 27,
        OnBeforeMove = 28,
        OnAfterMove = 29,
        OnBeforeAttack = 30, //When attacking, before damage
        OnAfterAttack = 31, //When attacking, after damage if still alive
        OnAfterRangeAttack = 34, //When attacking, after range attack
        OnAfterMeleeAttack = 35,
        OnBeforeDefend = 36, //When being attacked, before damage
        OnAfterDefend = 37, //When being attacked, after damage if still alive
        OnKill = 38,        //When killing another card during an attack
        EndOfTheLine = 39, //When a pawn reaches the end of the board

        OnDeath = 40, //When dying
        OnDeathOther = 42, //When another dying
    }
}