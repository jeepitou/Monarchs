namespace Monarchs.Logic
{
    /// <summary>
    /// List of game actions and refreshes, that can be performed by the player or received
    /// </summary>

    public static class GameAction
    {
        public const ushort None = 0;

        //Commands (client to server)
        public const ushort PlayCard = 1000;
        public const ushort Mulligan = 1001;
        public const ushort MovePiece = 1009;
        public const ushort Attack = 1010;
        public const ushort RangeAttack = 1012;
        public const ushort Move = 1015;
        public const ushort CastAbility = 1020;
        public const ushort SelectCard = 1030;
        public const ushort SelectPlayer = 1032;
        public const ushort SelectSlot = 1034;
        public const ushort SelectChoice = 1036;
        public const ushort SelectCaster = 1037;   
        public const ushort SkipSelect = 1038;
        public const ushort CancelSelect = 1039;
        public const ushort CreateMana = 1040;
        public const ushort EndTurn = 1041;
        public const ushort Resign = 1050;
        public const ushort ChatMessage = 1090;

        public const ushort PlayerSettings = 1100; //After connect, send player data
        public const ushort PlayerSettingsAI = 1102; //After connect, send player data
        public const ushort GameSettings = 1105; //After connect, send gameplay settings

        //Refresh (server to client)
        public const ushort Connected = 2000;
        public const ushort PlayerReady = 2001;
        public const ushort Disconnected = 2002; //Player disconnected
        public const ushort Reconnected = 2003; //Player reconnected

        public const ushort GameStart = 2010;
        public const ushort GameEnd = 2012;
        public const ushort NewTurn = 2014;
        public const ushort NewRound = 2015;

        public const ushort CardPlayed = 2020;
        public const ushort PieceMoved = 2021;
        public const ushort CardSummoned = 2022;
        public const ushort CardTransformed = 2023;
        public const ushort CardDiscarded = 2025;
        public const ushort CardDrawn = 2026;
        public const ushort CardMoved = 2027;

        public const ushort AttackStart = 2030;
        public const ushort AttackEnd = 2032;
        public const ushort RangeAttackStart = 2034;
        public const ushort RangeAttackEnd = 2036;

        public const ushort AbilityTrigger = 2040;
        public const ushort AbilitySelectMana = 2041;
        public const ushort AbilityTargetCard = 2042;
        public const ushort AbilityTargetPlayer = 2043;
        public const ushort AbilityTargetSlot = 2044;
        public const ushort AbilitySummonedCardToHand = 2045;
        public const ushort AbilityEnd = 2048;
        public const ushort ChooseManaType = 2049;
        public const ushort AbilityTargetMultiple = 2050;

        public const ushort TrapTriggered = 2060;
        public const ushort TrapResolved = 2061;
        public const ushort ValueRolled = 2070;
        
        public const ushort HandCardHoveredByOpponent = 2080;
        public const ushort BoardSlotHoveredByOpponent = 2081;
        public const ushort AbilityHovered = 2082;

        public const ushort ServerMessage = 2190; //Server warning msg
        public const ushort RefreshAll = 2100;
        

        public static string GetString(ushort type)
        {
            if (type == GameAction.None)
                return "none";
            // Commands
            if (type == GameAction.PlayCard)
                return "play";
            if (type == GameAction.Mulligan)
                return "mulligan";
            if (type == GameAction.MovePiece)
                return "move_piece";
            if (type == GameAction.Move)
                return "move";
            if (type == GameAction.Attack)
                return "attack";
            if (type == GameAction.RangeAttack)
                return "range_attack";
            if (type == GameAction.CastAbility)
                return "cast_ability";
            if (type == GameAction.SelectCard)
                return "select_card";
            if (type == GameAction.SelectPlayer)
                return "select_player";
            if (type == GameAction.SelectSlot)
                return "select_slot";
            if (type == GameAction.SelectChoice)
                return "select_choice";
            if (type == GameAction.SelectCaster)
                return "select_caster";
            if (type == GameAction.SkipSelect)
                return "skip_select";
            if (type == GameAction.CancelSelect)
                return "cancel_select";
            if (type == GameAction.CreateMana)
                return "create_mana";
            if (type == GameAction.EndTurn)
                return "end_turn";
            if (type == GameAction.Resign)
                return "resign";
            if (type == GameAction.ChatMessage)
                return "chat";
            if (type == GameAction.PlayerSettings)
                return "player_settings";
            if (type == GameAction.PlayerSettingsAI)
                return "player_settings_ai";
            if (type == GameAction.GameSettings)
                return "game_settings";
            
            // Refresh
            if (type == GameAction.Connected)
                return "connected";
            if (type == GameAction.PlayerReady)
                return "player_ready";
            if (type == GameAction.GameStart)
                return "game_start";
            if (type == GameAction.GameEnd)
                return "game_end";
            if (type == GameAction.NewTurn)
                return "new_turn";
            if (type == GameAction.NewRound)
                return "new_round";
            if (type == GameAction.CardPlayed)
                return "card_played";
            if (type == GameAction.PieceMoved)
                return "piece_moved";
            if (type == GameAction.CardSummoned)
                return "card_summoned";
            if (type == GameAction.CardTransformed)
                return "card_transformed";
            if (type == GameAction.CardDiscarded)
                return "card_discarded";
            if (type == GameAction.CardDrawn)
                return "card_drawn";
            if (type == GameAction.CardMoved)
                return "card_moved";
            if (type == GameAction.AttackStart)
                return "attack_start";
            if (type == GameAction.AttackEnd)
                return "attack_end";
            if (type == GameAction.RangeAttackStart)
                return "range_attack_start";
            if (type == GameAction.RangeAttackEnd)
                return "range_attack_end";
            if (type == GameAction.AbilityTrigger)
                return "ability_trigger";
            if (type == GameAction.AbilityTargetCard)
                return "ability_target_card";
            if (type == GameAction.AbilityTargetPlayer)
                return "ability_target_player";
            if (type == GameAction.AbilityTargetSlot)
                return "ability_target_slot";
            if (type == GameAction.AbilitySummonedCardToHand)
                return "ability_summoned_card_to_hand";
            if (type == GameAction.AbilityEnd)
                return "ability_end";
            if (type == GameAction.ChooseManaType)
                return "choose_mana_type";
            if (type == GameAction.TrapTriggered)
                return "trap_triggered";
            if (type == GameAction.TrapResolved)
                return "trap_resolved";
            if (type == GameAction.ValueRolled)
                return "value_rolled";
            if (type == GameAction.HandCardHoveredByOpponent)
                return "hand_card_hovered_by_opponent";
            if (type == GameAction.BoardSlotHoveredByOpponent)
                return "board_slot_hovered_by_opponent";
            if (type == GameAction.AbilityHovered)
                return "ability_hovered";
            if (type == GameAction.ServerMessage)
                return "server_message";
            if (type == GameAction.RefreshAll)
                return "refresh_all";
            
            return type.ToString();
        }
    }
}