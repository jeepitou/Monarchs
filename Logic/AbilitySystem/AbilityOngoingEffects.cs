using Monarchs.Ability;
using Monarchs.Ability.Target;
using TcgEngine;
using UnityEngine.Profiling;

namespace Monarchs.Logic.AbilitySystem
{
    public class AbilityOngoingEffects
    {
        private ListSwap<ITargetable> _targetArray = new ListSwap<ITargetable>();
        
        //This function is called often to update status/stats affected by ongoing abilities
        //It basically first reset the bonus to 0 (CleanOngoing) and then recalculate it to make sure it it still present
        //Only cards in hand and on board are updated in this way
        public virtual void UpdateOngoingAbilities(GameLogic logic)
        {
            Profiler.BeginSample("Update Ongoing");
            
            ClearOngoing(logic);

            foreach (var slotStatus in logic.Game.slotStatusList)
            {
                if (slotStatus.SlotStatusData.triggerType == SlotStatusTriggerType.Ongoing)
                    slotStatus.SlotStatusData.DoOngoingEffect(logic, slotStatus.slot);
            }

            for (int p = 0; p < logic.Game.players.Length; p++)
            {
                Player player = logic.Game.players[p];

                for (int c = 0; c < player.cards_board.Count; c++)
                {
                    Card card = player.cards_board[c];
                    UpdateOngoingAbilities(logic, player, card);

                    //Status bonus
                    foreach (CardStatus status in card.status)
                        AddOngoingStatusBonus(card, status);
                    foreach (CardStatus status in card.ongoingStatus)
                        AddOngoingStatusBonus(card, status);
                }
            }
            
            
            Profiler.EndSample();
        }

        protected void ClearOngoing(GameLogic logic)
        {
            for (int p = 0; p < logic.Game.players.Length; p++)
            {
                Player player = logic.Game.players[p];
                player.ClearOngoing();

                for (int c = 0; c < player.cards_board.Count; c++)
                    player.cards_board[c].ClearOngoing();

                for (int c = 0; c < player.cards_hand.Count; c++)
                    player.cards_hand[c].ClearOngoing();
            }
        }

        protected virtual void UpdateOngoingAbilities(GameLogic logic, Player player, Card card)
        {
            if (card == null || !card.CanDoAbilities())
                return;

            foreach (var ability in card.GetAllCurrentAbilities())
            {
                AbilityArgs args = new AbilityArgs() {ability = ability, caster = card, castedCard = card};
                if (ability != null && ability.trigger == AbilityTrigger.Ongoing && ability.AreTriggerConditionsMet(logic.Game, args))
                {
                    var targets = ability.GetTargets(logic.Game, card, null, _targetArray);
                    
                    foreach (var target in targets)
                    {
                        args.target = target;
                        if (ability.AreTargetConditionsMet(logic.Game, args))
                        {
                            ability.DoOngoingEffects(logic, args);
                        }
                    }
                }
            }
        }

        protected virtual void AddOngoingStatusBonus(Card card, CardStatus status)
        {
            if (status.type == StatusType.AttackBonus)
                card.attackOngoing += status.value;
            if (status.type == StatusType.HPBonus)
                card.hpOngoing += status.value;
        }
    }
}