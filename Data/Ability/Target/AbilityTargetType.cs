namespace Monarchs.Ability
{
    public enum AbilityTargetType
    {
        None = 0,
        Self = 1,

        PlayerSelf = 4,
        PlayerOpponent = 5,
        AllPlayers = 7,

        CurrentPiece = 9,
        AllCardsBoard = 10,
        AllCardsHand = 11,
        AllCardsAllPiles = 12,
        AllSlots = 15,
        AllCardData = 17,       //For card Create effects only

        PlayTarget = 20,        //The target selected at the same time the spell was played (spell only)    
        PlayTargetAndSelectMultiplesTarget = 21,
        AbilityTriggerer = 25,   //The card that triggered the trap
        CardSavedForAbility = 26,   //The card that was saved for the ability

        SelectTarget = 30,        //Select a card, player or slot on board
        SelectMultipleTarget = 31,
        SelectManaType = 32,
        CardSelector = 40,          //Card selector menu
        ChoiceSelector = 50,        //Choice selector menu
        
        LastAttackedSlot = 68,
        LastAttackedCard = 69,          //Last card that was targeted by an attack
        LastPlayed = 70,            //Last card that was played
        LastTargeted = 72,          //Last card that was targeted with an ability
        LastKilled = 74,            //Last card that was killed

    }
}