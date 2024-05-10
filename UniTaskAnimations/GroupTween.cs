﻿using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Common.UniTaskAnimations
{
    [Serializable]
    public class GroupTween : ITween
    {
        [SerializeField]
        private bool parallel;

        [SerializeReference]
        private List<ITween> _tweens = new();

        [SerializeField]
        private List<TweenComponent> components;
        
        public bool Parallel => parallel;
        public List<ITween> Tweens => _tweens;
        public List<TweenComponent> Components => components;

        private CancellationTokenSource _currentToken;

        public GroupTween(bool isParallel)
        {
            parallel = isParallel;
        }

        public async UniTask StartAnimation(
            bool reverse = false,
            bool startFromCurrentValue = false,
            CancellationToken cancellationToken = default)
        {
            _currentToken = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            ResetValues();
            if (Parallel)
            {
                var tasks = new List<UniTask>();
                foreach (var tween in _tweens)
                {
                    if (tween == null) continue;
                    tasks.Add(
                        tween.StartAnimation(
                            reverse,
                            startFromCurrentValue,
                            _currentToken.Token));
                }

                await UniTask.WhenAll(tasks);
            }
            else
            {
                foreach (var tween in _tweens)
                {
                    if (_currentToken.IsCancellationRequested) return;
                    if (tween == null) continue;
                    await tween.StartAnimation(
                        reverse,
                        startFromCurrentValue,
                        _currentToken.Token);
                }
            }
        }

        public UniTask StopAnimation()
        {
            _currentToken.Cancel();
            foreach (var tween in _tweens)
            {
                tween?.StopAnimation().Forget();
            }
            return UniTask.CompletedTask;
        }

        public void ResetValues()
        {
            foreach (var tween in _tweens)
            {
                tween?.ResetValues();
            }
        }

        public void EndValues()
        {
            foreach (var tween in _tweens)
            {
                tween?.EndValues();
            }
        }

        public void AddTween(ITween tween)
        {
            _tweens.Add(tween);
        }

        public static GroupTween Clone(GroupTween tween, GameObject targetObject = null)
        {
            var newTween = new GroupTween(tween.parallel);
            foreach (var inTween in tween.Tweens)
            {
                var newInTween = ITween.Clone(inTween, targetObject);
                newTween.AddTween(newInTween);
            }

            return newTween;
        }
    }
}