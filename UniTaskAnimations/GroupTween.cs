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
        
        [SerializeField]
        private float startDelay;

        [SerializeReference]
        private List<ITween> _tweens = new();

        [SerializeField]
        private List<TweenComponent> components;
        
        public bool Parallel => parallel;
        public List<ITween> Tweens => _tweens;
        public List<TweenComponent> Components => components;
        public float StartDelay => startDelay;
        public bool IsActiveAnimation => _currentToken != null;
        public float Length => StartDelay + CalculateTweenTime();
        public float TweenTime => CalculateTweenTime();

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
            await DelayAnimation(_currentToken.Token);
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
            _currentToken.Dispose();
            _currentToken = null;
        }

        public UniTask StopAnimation()
        {
            _currentToken?.Cancel();
            foreach (var tween in _tweens)
            {
                tween?.StopAnimation().Forget();
            }
            _currentToken?.Dispose();
            _currentToken = null;
            return UniTask.CompletedTask;
        }

        public void ResetValues()
        {
            for (var i = _tweens.Count - 1; i >= 0; i--)
            {
                var tween = _tweens[i];
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
        
        private async UniTask DelayAnimation(CancellationToken cancellationToken)
        {
            if (StartDelay > 0.001f)
                await UniTask.Delay(TimeSpan.FromSeconds(StartDelay), cancellationToken: cancellationToken);
        }

        private float CalculateTweenTime()
        {
            var length = 0f;

            if (parallel)
            {
                var maxTime = 0f;
                for (var i = Tweens.Count - 1; i >= 0; i--)
                {
                    var tweenLength = Tweens[i].Length;
                    if (tweenLength > maxTime) maxTime = tweenLength;
                }

                length += maxTime;
            }
            else
            {
                for (var i = Tweens.Count - 1; i >= 0; i--)
                {
                    length += Tweens[i].Length;
                }
            }

            return length;
        }
    }
}