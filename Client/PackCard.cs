using System.Collections;
using System.Collections.Generic;
using Monarchs.Tools;
using TcgEngine;
using TcgEngine.UI;
using UnityEngine;

namespace Monarchs.Client
{
    /// <summary>
    /// The card can be flipped by clicking on it
    /// </summary>

    public class PackCard : MonoBehaviour
    {
        public float move_speed = 5f;
        public float flip_speed = 10f;

        public SpriteRenderer cardback;
        public HandCardUIManager card_ui;

        [Header("FX")]
        public GameObject card_flip_fx;
        public GameObject card_rare_flip_fx;
        public AudioClip card_flip_audio;
        public AudioClip card_rare_flip_audio;

        private CardData icard;
        private VariantData variant;

        private Vector3 target;
        private Quaternion rtarget;
        private bool revealed = false;
        private bool removed = false;
        private float timer = 0f;

        private static List<PackCard> card_list = new List<PackCard>();

        void Awake()
        {
            card_list.Add(this);
        }

        private void OnDestroy()
        {
            card_list.Remove(this);
        }

        void Update()
        {
            transform.position = Vector3.MoveTowards(transform.position, target, move_speed * Time.deltaTime);

            if (revealed)
            {
                timer += Time.deltaTime;
                transform.rotation = Quaternion.Slerp(transform.rotation, rtarget, flip_speed * Time.deltaTime);
            }

            if (removed && timer > 4f)
                Destroy(gameObject);
        }

        public void SetCard(PackData pack, CardData card, VariantData variant)
        {
            this.icard = card;
            this.variant = variant;

            if (cardback != null)
                cardback.sprite = pack.cardback_img;

            card_ui.SetCard(card, variant);
        }

        public void SetTarget(Vector3 pos)
        {
            target = pos;
            rtarget = Quaternion.Euler(0f, 180f, 0f);
            transform.rotation = rtarget;
        }

        public void Reveal()
        {
            if (revealed)
                return;

            revealed = true;
            rtarget = Quaternion.Euler(0f, 0f, 0f);

            StartCoroutine(PlayRevealFX());
        }
        
        private IEnumerator PlayRevealFX()
        {
            FXResult result = new FXResult();
            
            if (icard != null && icard.rarity.rank >= 3)
            {
                yield return FXTool.DoFX(card_rare_flip_fx, transform.position, result);
                
                // Add destruction after a delay
                if (result.fxObject != null)
                    Destroy(result.fxObject, 5f);
                    
                AudioTool.Get().PlaySFX("pack_open", card_rare_flip_audio);
            }
            else
            {
                yield return FXTool.DoFX(card_flip_fx, transform.position, result);
                
                // Add destruction after a delay
                if (result.fxObject != null)
                    Destroy(result.fxObject, 5f);
                    
                AudioTool.Get().PlaySFX("pack_open", card_flip_audio);
            }
        }

        public void Remove()
        {
            if (removed)
                return;

            removed = true;
            timer = 0f;
            target = Vector3.up * 10f;
        }

        public void OnMouseDown()
        {
            if (!GameUI.IsOverUILayer("UI"))
            {
                Reveal();
            }
        }

        public bool IsRevealed()
        {
            return revealed && timer > 0.5f;
        }

        public static List<PackCard> GetAll()
        {
            return card_list;
        }
    }
}