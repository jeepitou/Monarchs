// using System;
// using System.Collections;
// using System.Collections.Generic;
// using DG.Tweening;
// using Monarchs.Logic;
// using TcgEngine;
// using TcgEngine.Client;
// using TMPro;
// using UnityEngine;
// using UnityEngine.UI;
//
// public class PieceInitiativeUI : MonoBehaviour
// {
//     public float X_MOVE_ANIMATION;
//     public float ANIM_DURATION;
//     
//     public Color whiteGradientColor;
//     public Color blackGradientColor;
//     public static List<PieceInitiativeUI> allUIPieceInitiatives;
//
//     [SerializeField] private TextMeshProUGUI _initiativeText;
//     [SerializeField] private TextMeshProUGUI _pieceNameText;
//     [SerializeField] private Image _pieceImage;
//     [SerializeField] private Image _gradientImage;
//     [SerializeField] private Image _cohortImage;
//     public Card _card;
//     private const float HOVER_SCALE_FACTOR = 1.1f;
//     private float _initialScale;
//     private bool disappearing = false;
//     private InitiativeManagerUI _initiativeManagerUI;
//
//     private void Awake()
//     {
//         if (allUIPieceInitiatives == null)
//         {
//             allUIPieceInitiatives = new List<PieceInitiativeUI>();
//         }
//
//         allUIPieceInitiatives.Add(this);
//         _initialScale = transform.localScale.x;
//     }
//
//     private void OnDestroy()
//     {
//         allUIPieceInitiatives.Remove(this);
//     }
//
//     public void SetCard(string cohortUID)
//     {
//         _card = GameClient.GetGameData().GetBoardCardsOfCohort(cohortUID)?[0];
//
//         if (_card == null)
//         {
//             Debug.LogError("Tried to set card with invalid UID in UIPieceInitiative.");
//             return;
//         }
//     }
//
//     public void UpdateUI()
//     {
//         UpdateUI(false);
//     }
//     
//     
//     public void UpdateUI(bool newCard)
//     {
//         UpdateInitiative();
//         UpdatePieceName();
//         UpdatePieceArt();
//         UpdateGradiantColor();
//
//         int cohortSize;
//         if (newCard)
//         {
//             cohortSize = _card.cohortSize;
//         }
//         else
//         {
//             cohortSize = GameClient.GetGameData().GetBoardCardsOfCohort(_card.CohortUid).Count;
//         }
//         
//         UpdateCohortIcon(cohortSize);
//     }
//
//     public void UpdateInitiative()
//     {
//         _initiativeText.text = _card.GetInitiative().ToString();
//     }
//
//     public void UpdatePieceName()
//     {
//         _pieceNameText.text = _card.CardData.name;
//     }
//
//     public void UpdatePieceArt()
//     {
//         _pieceImage.sprite = _card.CardData.artBoard;
//     }
//
//     public void UpdateGradiantColor()
//     {
//         if (_card.playerID == GameClient.GetGameData().firstPlayer)
//         {
//             _gradientImage.color = whiteGradientColor;
//         }
//         else
//         {
//             _gradientImage.color = blackGradientColor;
//         }
//     }
//
//     public void UpdateCohortIcon(int cohortSize)
//     {
//         if (cohortSize > 1)
//         {
//             _cohortImage.sprite = GameplayData.Get().CohortIconLink.GetCohortIcon(cohortSize);
//             _cohortImage.gameObject.SetActive(true);
//             _cohortImage.enabled = true;
//             return;
//         }
//
//         _cohortImage.enabled = false;
//     }
//
//     public void StartAppearAnimation()
//     {
//         Vector3 initialScale = transform.localScale;
//         transform.localScale = new Vector3(initialScale.x, 0, initialScale.z);
//         
//         GameObject frame = _pieceImage.transform.parent.gameObject;
//         frame.SetActive(false);
//         _pieceNameText.transform.parent.gameObject.SetActive(false);
//         transform.DOScaleY(initialScale.y, 0f).OnComplete(() =>
//         {
//             GetComponent<HorizontalLayoutGroup>().enabled = false;
//             
//             Vector3 initialPosition = frame.GetComponent<RectTransform>().localPosition;
//
//             frame.GetComponent<RectTransform>().localPosition = initialPosition + Vector3.left * X_MOVE_ANIMATION;
//             
//             GetComponent<Animator>().SetTrigger("Appear");
//         
//             frame.transform.DOLocalMoveX(initialPosition.x, ANIM_DURATION, false).OnComplete(() =>
//             {
//                 GetComponent<HorizontalLayoutGroup>().enabled = true;
//             });
//         });
//     }
//
//     public void StartDisappearAnimation(InitiativeManagerUI initiativeManagerUI)
//     {
//         if (!disappearing)
//         {
//             GetComponent<Animator>().SetTrigger("Disappear");
//             disappearing = true;
//         }
//
//         _initiativeManagerUI = initiativeManagerUI;
//     }
//
//     public void FinishDisappearingAnimation()
//     {
//         _initiativeManagerUI.RemoveUIElement(this);
//         Destroy(gameObject);
//     }
//
//     public void OnPointerEnter()
//     {
//         BoardInputManager.Instance.SetHoverInitiativeCard(_card);
//         transform.DOScale(_initialScale * HOVER_SCALE_FACTOR, 0.2f);
//     }
//
//     public void OnPointerExit()
//     {
//         if (BoardInputManager.Instance.hoveredInitiativeCard == _card)
//         {
//             BoardInputManager.Instance.SetHoverInitiativeCard(null);
//         }
//         
//         transform.DOScale(_initialScale, 0.2f);
//     }
// }