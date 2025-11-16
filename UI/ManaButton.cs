using DG.Tweening;
using Monarchs.Ability;
using Monarchs.Client;
using Monarchs.Logic;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.UI;

namespace Monarchs.UI
{
    public class ManaButton : MonoBehaviour
    {
        [Required] public AbilityData manaGenerationAbility;
        public PlayerMana.ManaType manaType;
        public Image image;
        public ColorSO color;
        public Button button;
        public bool isOpponent;      
        
        private State _currentState = State.NotAnimated;
        
        private const float SCALE_VALUE = 0.3f;
        private const float DURATION = 0.8f;
        private Sequence _sequence;
        private Sequence _generatingManaSequence;

        private Image _bigHoverImage;
        private float _bigHoverImageScale;

        public Image glow;
        private float _glowInitialScale;
        
        private float _initialScale;
        private bool _isAnimated;
        private bool _isHovered;
        bool _canGenerateThisManaType;
        bool _canSpendThisManaType;

        public static float s_GeneratingManaTimer = 0f;
        private static bool s_TimerActive = false;
        private static ManaButton s_TimerOwner;

        public static void RestartGeneratingManaTimer()
        {
            s_GeneratingManaTimer = 0f;
            s_TimerActive = false;
            s_TimerOwner = null;
        }

        private void Start()
        {
            _initialScale = image.transform.localScale.x;
            _bigHoverImage = button.GetComponent<Image>();
            _bigHoverImage.color = color.color;
            HideBigHoverButton();
            _bigHoverImageScale = _bigHoverImage.transform.localScale.x;
            _glowInitialScale = glow.transform.localScale.x;
        }
        
        private Player GetPlayer()
        {
            if (isOpponent)
            {
                return GameClient.Get().GetOpponentPlayer();
            }
            return GameClient.Get().GetPlayer();
        }
        
        private bool MonarchCanGenerateThisManaType()
        {
            return GetPlayer().king.CardData.manaTypeThatCanBeGenerated.HasFlag(manaType);
        }

        // Update is called once per frame
        void Update()
        {
            if (!GameClient.Get().IsReady()) return;
            
            Game game = GameClient.GetGameData();
            var casters = game.GetCurrentCardTurn();
            if (casters.Count == 0) return;
            Card caster = casters[0];
            Card castedCard = game.GetCard(game.selectorCastedCardUID);
            Player player = GetPlayer();
            
            bool playerHasManaType = player.playerMana.HasMana(manaType);
            bool playerIsGeneratingManaThisTurn = player.playerMana.GetGeneratingMana().HasFlag(manaType);
            bool casterIsOwnedByPlayer = caster != null && caster.playerID == player.playerID;
            bool castedCardNeedThisManaType = castedCard != null && castedCard.GetManaCost().HasFlag(manaType);

            _canGenerateThisManaType = game.selector == SelectorType.SelectManaTypeToGenerate && !playerHasManaType && !playerIsGeneratingManaThisTurn &&
                                           casterIsOwnedByPlayer && MonarchCanGenerateThisManaType();

            
            _canSpendThisManaType = game.selector == SelectorType.SelectManaTypeToSpend && playerHasManaType && casterIsOwnedByPlayer && !castedCardNeedThisManaType;
            
            if (playerIsGeneratingManaThisTurn && _currentState != State.AnimatedToShowGeneratingMana)
            {
                ChangeState(State.AnimatedToShowGeneratingMana);
                return;
            }
            
            if (!playerIsGeneratingManaThisTurn && !_canGenerateThisManaType && !_canSpendThisManaType && _currentState != State.NotAnimated)
            {
                ChangeState(State.NotAnimated);
                return;
            }
            
            if (!isOpponent && (_canGenerateThisManaType || _canSpendThisManaType) && _currentState!=State.AnimatedToBeSelected && !_isHovered)
            {
                ChangeState(State.AnimatedToBeSelected);
            }

            SetBigHoverScale();

            // Only timer owner updates the timer
            if (s_TimerActive && s_TimerOwner == this)
            {
                s_GeneratingManaTimer += Time.deltaTime;
            }
        }
        
        private void ChangeState(State newState)
        {
            if (_currentState == newState) return;
            
            if (newState == State.NotAnimated)
            {
                if (s_TimerOwner == this)
                {
                    RestartGeneratingManaTimer();
                }
                button.interactable = false;
                StopGeneratingManaAnimation();
                StopAnimationSequence();
            }
            else if (newState == State.AnimatedToBeSelected)
            {
                button.interactable = true;
                StopGeneratingManaAnimation();
                StartAnimationSequence();
            }
            else if (newState == State.AnimatedToShowGeneratingMana)
            {
                button.interactable = false;
                StopAnimationSequence();
                StartGeneratingManaAnimation();
            }
            _currentState = newState;
        }
        
        private void StartGeneratingManaAnimation()
        {
            image.type = Image.Type.Filled;
            image.fillMethod = Image.FillMethod.Radial360;
            image.fillOrigin = 0;
            image.fillAmount = 0f;
            image.fillClockwise = true;
            image.color = color.color; // Ensure color is set before animation

            // Kill any previous sequence
            _generatingManaSequence?.Kill();

            _generatingManaSequence = DOTween.Sequence();

            // Step 1: Fill clockwise from 0 to 1 and animate color
            _generatingManaSequence.Append(image.DOFillAmount(1f, DURATION).SetEase(Ease.Linear));
            _generatingManaSequence.Join(image.DOColor(color.color, DURATION));
            // Step 2: Switch to counter-clockwise and empty from 1 to 0, animate color
            _generatingManaSequence.AppendCallback(() => image.fillClockwise = false);
            _generatingManaSequence.Append(image.DOFillAmount(0f, DURATION).SetEase(Ease.Linear));
            _generatingManaSequence.Join(image.DOColor(color.color, DURATION));
            // Step 3: Switch back to clockwise
            _generatingManaSequence.AppendCallback(() => image.fillClockwise = true);

            // Loop the sequence for a continuous effect
            _generatingManaSequence.SetLoops(-1, LoopType.Restart);
            _generatingManaSequence.Play();

            // Sync sequence position with timer
            float totalDuration = DURATION * 2; // fill + empty
            float position = s_GeneratingManaTimer % totalDuration;
            _generatingManaSequence.Goto(position, true);
            
            // Set fillClockwise to match the phase
            if (position < DURATION)
                image.fillClockwise = true;
            else
                image.fillClockwise = false;

            // Set timer owner if none
            if (s_TimerOwner == null)
            {
                s_TimerActive = true;
                s_TimerOwner = this;
            }
        }
        
        private void StopGeneratingManaAnimation()
        {
            _generatingManaSequence?.Kill(); // Stop the sequence
            image.DOKill(); // Stop all tweens on image
            image.fillAmount = 1f; // Reset fill to full
            // If this was the timer owner, release ownership
            if (s_TimerOwner == this)
            {
                s_TimerOwner = null;
                s_TimerActive = false;
            }
        }

        private void StartAnimationSequence()
        {
            // Create a sequence
            _sequence = DOTween.Sequence();

            // Append a color tween and a scale tween to the sequence
            _sequence.Join(image.DOBlendableColor(color.color, DURATION));
            _sequence.Join(image.transform.DOBlendableScaleBy(new Vector3(SCALE_VALUE, SCALE_VALUE, SCALE_VALUE), DURATION));

            // Set the sequence to loop
            _sequence.SetLoops(-1, LoopType.Yoyo);

            // Start the sequence
            _sequence.Play();
            
             ShowBigGlow();
        }

        private void StopAnimationSequence()
        {
            glow.enabled = false;
            
            _sequence.Kill();
            ResetVisuals();
        }
        
        public void OnClick()
        {
            GameClient.Get().ChooseManaType(manaType);
            
            HideBigHoverButton();
            ResetGlowScale();
        }

        // I want to interrupt the sequence when the mouse is over the button, but I want it to resume when I leave and still be synchronised with the other buttons
        public void OnMouseEnter()
        {
            _isHovered = true;
            if (_currentState == State.AnimatedToBeSelected)
            {
                ShowBigHoverButton();
                ShowBigGlow();
            }
        }
        
        public void OnMouseExit()
        {
            _isHovered = false;
            if (_currentState == State.AnimatedToBeSelected)
            {
                ResetGlowScale();
                HideBigHoverButton();
                glow.enabled = false;
            }
        }

        private void ResetVisuals()
        {
            //image.color = initialColor;
            image.transform.localScale = new Vector3(_initialScale, _initialScale, _initialScale);
        }

        private void SetBigHoverScale()
        {
            if (_isHovered && _currentState == State.AnimatedToBeSelected)
            {
                _bigHoverImage.transform.localScale = new Vector3(_bigHoverImageScale + SCALE_VALUE, _bigHoverImageScale + SCALE_VALUE, _bigHoverImageScale + SCALE_VALUE);
            }
            else
            {
                _bigHoverImage.transform.localScale = new Vector3(_bigHoverImageScale, _bigHoverImageScale, _bigHoverImageScale);
            }
        }

        private void ShowBigGlow()
        {
            glow.enabled = true;
        }

        private void ResetGlowScale()
        {
            glow.transform.localScale = new Vector3(_glowInitialScale, _glowInitialScale, _glowInitialScale);
        }
        
        private void ShowBigHoverButton()
        {
            _bigHoverImage.color = new Color(_bigHoverImage.color.r, _bigHoverImage.color.g, _bigHoverImage.color.b, 1f);
        }

        private void HideBigHoverButton()
        {
            _bigHoverImage.color = new Color(_bigHoverImage.color.r, _bigHoverImage.color.g, _bigHoverImage.color.b, 0f);
        }

        private enum State
        {
            NotAnimated,
            AnimatedToBeSelected,
            AnimatedToShowGeneratingMana
        }
    }
}
