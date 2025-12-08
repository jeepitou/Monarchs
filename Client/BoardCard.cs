using System.Collections;
using Monarchs.Ability;
using Monarchs.Board;
using Monarchs.FX;
using Monarchs.Logic;
using Monarchs.UI;
using Sirenix.OdinInspector;
using TcgEngine;
using TcgEngine.FX;
using TcgEngine.UI;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace Monarchs.Client
{
    /// <summary>
    /// Represents the visual aspect of a card on the board.
    /// Will take the data from Card.cs and display it
    /// </summary>

    public class BoardCard : BoardElement
    {
        [Required]public TMP_Text hpText;
        [Required]public TMP_Text attackText;
        [Required]public GameObject rangeUnitIcon;

        [Required]public Image armorIcon;
        [Required]public Image hasAbilityImage;
        [Required]public GameObject hasDyingWish;
        [Required]public TMP_Text armor;

        public CanvasGroup statusGroup;
        public Text statusText;

        public AbilityButton[] buttons;
        public MovementScheme movementScheme = null;

        [Required][FormerlySerializedAs("outlineManager")] public CurrentPieceHighlightManager highlightManager;
        
        public static UnityAction<Card, Card, int> MoveAttackIsOver;

        public bool isExported = false;
        public const float MOVE_SPEED = 12f;
        private Card _card;
        private bool _movingBeforeAttack = false;
        private int _lastAttackDamage;
        private Card _lastTargetOfAttack;
        private CardUI _cardUI;
        private BoardCardFX _cardFX;
        private Canvas _canvas;
        private bool updateHP = true;
        private bool _gettingDestroyed = false;
        private bool _movingInCoroutine = false;

        public int currentHPAfterDamage;
        
        protected override void OnDestroy()
        {
            if (isExported)
                return;
            GameClient.Get().onAttackEnd -= OnAttack;
            GameClient.Get().onRangeAttackEnd -= OnRangeAttack;
            GameClient.Get().onCardTransformed -= OnCardTransform;
            GameClient.Get().onNewTurn -= UpdateUIOnNewRound;
            GameClient.Get().onRefreshAll -= UpdateOutline;
            StopAllCoroutines();
            base.OnDestroy();
        }

        protected virtual void Start()
        {
            if (isExported)
                return;
            GameClient.Get().onAttackEnd += OnAttack;
            GameClient.Get().onRangeAttackEnd += OnRangeAttack;
            GameClient.Get().onCardTransformed += OnCardTransform;
            GameClient.Get().onNewTurn += UpdateUIOnNewRound;
            GameClient.Get().onRefreshAll += UpdateOutline;
            GameClient.Get().onPlayerReconnected += UpdateOutline;
            SetPosition();
            Game data = GameClient.GetGameData();
            Card card = data.GetCard(_cardUID);
            currentHPAfterDamage = card.GetHP();
            UpdateUI(card, data, false);
            UpdateOutline();
        }
        

        protected virtual void FixedUpdate()
        {
            if (isExported || !GameClient.Get().IsReady())
                return;
            
            Game data = GameClient.GetGameData();
            Card card = data.GetCard(_cardUID);
            
            if (!_gettingDestroyed)
            {
                UpdateUI(card, data);
            }

            if (!_movingInCoroutine)
            {
                Vector3 targetPos = GetTargetPos();
                targetPos.y = transform.position.y;
                //UpdatePosition(targetPos, card);
            }
        }
        
        public override void OnMouseEnter()
        {
            if (GameUI.IsUIOpened())
                return;

            if (GameTool.IsMobile())
                return;

            _focus = true;
        }

        public override void OnMouseDown()
        {
            if (GameUI.IsUIOpened())
                return;
            
            PlayerControls.Get().SelectCard(GetCard());

            if (GameTool.IsMobile())
            {
                _focus = true;
            }
        }

        public void HideNumbersAndIcons()
        {
            attackText.color = new Color(attackText.color.r, attackText.color.g, attackText.color.b, 0);
            hpText.color = new Color(hpText.color.r, hpText.color.g, hpText.color.b, 0);
            rangeUnitIcon.SetActive(false);
            armorIcon.gameObject.SetActive(false); 
            armor.color = new Color(armor.color.r, armor.color.g, armor.color.b, 0);
            _gettingDestroyed = true;
            
        }

        private void OnCardTransform(Card card)
        {
            if (card.uid == _cardUID)
            {
                SetCard(card);
            }
        }
        
        private void OnAttack(Card attacker, Card target, int damage)
        {
            Game data = GameClient.GetGameData();
            if (attacker.uid == _cardUID)
            {
                _movingBeforeAttack = true;
                _lastAttackDamage = damage;
                _lastTargetOfAttack = target;
            }

            if (target == null)
            {
                target = GameClient.GetGameData().lastAttackedCard;
            }

            if (attacker.uid == _cardUID || target.uid == _cardUID)
            {
                updateHP = false;
            }
        }
        
        private void OnRangeAttack(Card attacker, Card target, Slot slot, int damage)
        {
            Game data = GameClient.GetGameData();
            if (attacker == data.GetCard(_cardUID))
            {
                _lastAttackDamage = damage;
                _lastTargetOfAttack = target;
                float delay = 0f;
                if (attacker.CardData.RangeAttackFX != null)
                {
                    GameObject projectile = Instantiate(attacker.CardData.RangeAttackFX, transform.position, Quaternion.identity);
                    Vector3 targetPosition;
                    if (target == null)
                    {
                        targetPosition = BoardManager.Instance.GetPositionFromCoordinate(slot.GetCoordinate());
                    }
                    else
                    {
                        targetPosition = BoardManager.Instance.GetPositionFromCoordinate(target.GetCoordinates());
                    }
                    
                    projectile.GetComponent<Projectile>().SetTargetPosition(targetPosition);
                    delay = (targetPosition - transform.position).magnitude / projectile.GetComponent<Projectile>().speed;
                }

                StartCoroutine(UpdateRangeAttackHP(target, delay));
            }
            
            if (target == data.GetCard(_cardUID))
            {
                updateHP = false;
            }
        }

        private IEnumerator UpdateRangeAttackHP(Card target, float delay)
        {
            yield return new WaitForSeconds(delay);
            updateHP = true;
            if (target == null) yield break;
            Game data = GameClient.GetGameData();
            Card card = data.GetCard(target.uid);
            ((BoardCard)Get(card.uid)).UpdateHP(card, data);
        }

        void UpdatePosition(Vector3 targetPos, Card card)
        {
            if (transform.position - targetPos == Vector3.zero && _movingBeforeAttack)
            {
                _movingBeforeAttack = false;
                MoveAttackIsOver?.Invoke(card, _lastTargetOfAttack, _lastAttackDamage);
            }
            
            transform.position = Vector3.MoveTowards(transform.position, targetPos, MOVE_SPEED * Time.deltaTime);
        }

        void SetPosition()
        {
            Vector3 position = GetTargetPos();
            position.y = transform.position.y;
            transform.position = position;
        }

        private void UpdateUIOnNewRound(Card c)
        {
            if (_gettingDestroyed)
                return;
            Game data = GameClient.GetGameData();
            Card card = data.GetCard(_cardUID);
            UpdateUI(card, data, false);
        }

        public override void UpdateUI(Card card, Game data, bool showFX=true)
        {
            if (updateHP)
            {
                UpdateHP(card, data, showFX);
            }

            SetAttack(card);
            SetArmor(card);
            
            hasAbilityImage.gameObject.SetActive(card.CardData.HasAbility(AbilityTrigger.Activate));
            rangeUnitIcon.SetActive(card.GetMaxAttackRange() > 1);
            hasDyingWish.SetActive(card.CardData.HasAbility(AbilityTrigger.OnDeath));

            //Status bar
            if (statusGroup != null)
                statusGroup.alpha = Mathf.MoveTowards(statusGroup.alpha, _statusAlphaTarget, 5f * Time.deltaTime);
            
        }

        public void SetUIForExport(Card card)
        {
            hpText.text = card.GetHP().ToString();
            SetAttack(card);
            SetArmor(card);
            rangeUnitIcon.SetActive(card.GetMaxAttackRange() > 1);
        }

        public void UpdateHP(Card card, Game data, bool showFX=true)
        {
            if (hpText.text == "")
            {
                return;
            }
            
            int damageValue = card.GetHP() - currentHPAfterDamage;
            currentHPAfterDamage = card.GetHP();
            if (damageValue != 0 && showFX)
            {
                StartCoroutine(GetComponent<BoardCardFX>().ShowDamageFX(damageValue));
            }

            hpText.text = card.GetHP().ToString();
            updateHP = true;
        }

        private void SetAttack(Card card)
        {
            attackText.text = card.GetAttack().ToString();
        }

        private void SetArmor(Card card)
        {
            int armorVal = card.GetArmor();
            armor.text = armorVal.ToString();
            armor.enabled = armorVal > 0;
            armorIcon.gameObject.SetActive(armorVal > 0);
        }

        public override void SetCard(Card card)
        {
            Game game = GameClient.GetGameData();
             _cardUID = card.uid;

            SetPosition();
            
            UpdateUI(card, game, false);
        }

        private void UpdateOutline()
        {
            Game game = GameClient.GetGameData();
            Card card = game.GetCard(_cardUID);
            highlightManager.UpdateOutline(game, card);
        }
        
        public void SetMovingInCoroutine(bool value)
        {
            _movingInCoroutine = value;
        }
        
        public bool GetMovingInCoroutine()
        {
            return _movingInCoroutine;
        }
    }
}