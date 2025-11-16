namespace Monarchs.Animations
{
    public static class AnimConstants
    {
        //TRAP PLAYED
        public static class TrapPlayed
        {
            public const float TRAP_PLAYED_TEXT_SHOW_TIME = 0.5f;
            public const float TRAP_PLAYED_TEXT_FADE_OUT_TIME = 0.6f;
            public const float TRAP_PLAYED_SCALE_DOWN_TIME = 1f;
            public const float TRAP_ICON_FADE_IN_DURATION = 0.6f;
            public const float TRAP_ICON_MOVE_DURATION = 1.5f;
        }
        
        //TRAP TRIGGERED
        public static class TrapTriggered
        {
            //Show and hide card
            public const float TRAP_TRIGGER_DELAY_BEFORE_SHOWING_CARD = 1f;
            public const float TRAP_CARD_FADE_IN_DURATION = 0.6f;
            public const float TRAP_CARD_FADE_OUT_DURATION = 0.6f;
            public const float TRAP_CARD_SHOW_DURATION = 1.5f;
            public const float DELAY_AFTER_TRAP_CARD_HIDDEN_BEFORE_RESOLVING_TRAP = 0.3f;
            
            //Remove trap icon
            public const float TRAP_ICON_REMOVE_INITIAL_SCALE_DURATION = 1f;
            public const float TRAP_ICON_REMOVE_INITIAL_SCALE_STRENGTH = 2f;
            public const float TRAP_ICON_REMOVE_SHAKE_DURATION = 0.5f;
            public const float TRAP_ICON_REMOVE_SHAKE_STRENGTH = 20f;
            public const float TRAP_ICON_REMOVE_FADE_OUT_DURATION = 0.6f;
            public const float TRAP_ICON_REMOVE_MOVE_DURATION = 1f;
            public const float TRAP_ICON_REMOVE_MOVE_DISTANCE = 100f;
            public const float TRAP_ICON_REMOVE_SCALE_DURING_FADE_OUT_DURATION = 1f;
            public const float TRAP_ICON_REMOVE_SCALE_DURING_FADE_OUT_STRENGTH = 5f;
        }

        public static class ReturnCardToHand
        {
            public const float BOARD_CARD_MOVE_Z_DURATION = 0.4f;
            public const float BOARD_CARD_MOVE_Z_AMOUNT = 0.3f;
            public const float CARD_FADE_DURATION = 0.5f;
            public const float DELAY_BEFORE_MOVING_CARD = 0.5f;
            public const float CARD_VELOCITY = 1000f;
            public const float CARD_DISPLAY_SCALE = 2f;
        }
    }
}