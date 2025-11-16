using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Monarchs.Animations
{
    public class AnimationQueue
    {
        private readonly AnimationManager _coroutineRunner;
        private readonly Queue<QueuedCoroutine> _queue = new ();
        private bool _isRunning;

        public AnimationQueue(AnimationManager coroutineRunner)
        {
            _coroutineRunner = coroutineRunner;
        }
        
        public void Add(IEnumerator coroutine, GameObject owner)
        {
            //Debug.Log($"Adding coroutine to queue: {coroutine.ToString()}");
            _queue.Enqueue(new QueuedCoroutine(coroutine, owner));
            if (!_isRunning)
            {
                _isRunning = true;
                _coroutineRunner.StartCoroutine(RunQueue());
            }
        }

        private IEnumerator RunQueue()
        {
            while (_queue.Count > 0)
            {
                if (_queue.Peek() == null || _queue.Peek().Owner == null)
                {
                    _queue.Dequeue();
                    continue;
                }
                yield return _coroutineRunner.StartCoroutine(_queue.Dequeue().Coroutine);
            }
            _isRunning = false;
        }
        
        private class QueuedCoroutine
        {
            public IEnumerator Coroutine { get; }
            public GameObject Owner { get; }

            public QueuedCoroutine(IEnumerator coroutine, GameObject owner)
            {
                Coroutine = coroutine;
                Owner = owner;
            }
        }
    }
    
    
}
