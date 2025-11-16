using System;
using System.Collections;
using System.Collections.Generic;
using Monarchs.Ability;
using Monarchs.Client;
using Monarchs.Logic;
using UnityEngine;
using UnityEngine.Events;

namespace Monarchs.Animations
{
    public class AnimationManager : MonoBehaviour
    {
        public List<TriggerAnimation> triggerAnimations = new List<TriggerAnimation>();
        private AnimationQueue _animationQueue;
        public UnityAction<Card, Card> onTrapTriggerBoardCard; //trap, triggerer
        public UnityAction<Card, Card> onTrapTriggerIconAndShowCard; //trap, triggerer

        private void Awake()
        {
            _animationQueue = new AnimationQueue(this);
        }

        private void Start()
        {
            GameClient.Get().onTrapTrigger += TriggerTrap;
        }

        public void AddToQueue(AnimationQueueElement element, GameObject owner)
        {
            _animationQueue.Add(element.AnimationCoroutine(), owner);
        }
        
        public void AddToQueue(IEnumerator coroutine, GameObject owner)
        {
            _animationQueue.Add(coroutine, owner);
        }

        public void AddTriggerAnimationToQueue(AbilityTrigger trigger, Card triggerer)
        {
            AddToQueue(TriggerAnimationCoroutine(trigger, triggerer), gameObject);
        }

        public IEnumerator TriggerAnimationCoroutine(AbilityTrigger trigger, Card triggerer)
        {
            foreach (var triggerAnimation in triggerAnimations)
            {
                if (triggerAnimation.trigger == trigger)
                {
                    GameObject fx = Instantiate(triggerAnimation.fx, BoardSlot.Get(triggerer.slot).transform.position + Vector3.up*.2f, Quaternion.identity);
                    fx.transform.SetAsFirstSibling();
                    fx.SetActive(true);

                    var animationQueueElement = fx.GetComponent<AnimationQueueElement>();
                    var abilityFX = fx.GetComponent<AbilityFX>();
                    if (animationQueueElement != null)
                    {
                        yield return animationQueueElement.AnimationCoroutine();
                    }
                    else if (abilityFX != null)
                    {
                        abilityFX.SetAbilityData(new Logic.AbilitySystem.AbilityArgs() {triggerer=triggerer});
                        yield return abilityFX.FXEnumerator();
                    }
                    else
                    {
                        Destroy(fx);
                    }
                }
            }
            yield return null;
        }

        
        
        private void TriggerTrap(Card trap, Card triggerer)
        {
            onTrapTriggerBoardCard?.Invoke(trap, triggerer);
            onTrapTriggerIconAndShowCard?.Invoke(trap, triggerer);
        }



        [Serializable]
        public struct TriggerAnimation 
        {
            public AbilityTrigger trigger;
            public GameObject fx;
        }
    }
}