using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Common.UniTaskAnimations
{
    [Serializable]
    public class MultiTween : IBaseTween
    {
        [SerializeField]
        protected Transform parentObject;

        [SerializeField]
        protected float perObjectSecondsDelay;

        [SerializeReference]
        protected ITween Tween;

        public Transform ParentObject => parentObject;
        public float PerObjectSecondsDelay => perObjectSecondsDelay;
        public ITween CurTween => Tween;

        private List<ITween> _animations = new();

        public MultiTween(Transform parentObject, float perObjectSecondsDelay)
        {
            this.parentObject = parentObject;
            this.perObjectSecondsDelay = perObjectSecondsDelay;
        }

        public static MultiTween Clone(MultiTween tween, GameObject targetObject = null)
        {
            var targetTransform = targetObject == null ? null : targetObject.transform;
            var newTween = new MultiTween(targetTransform, tween.PerObjectSecondsDelay)
            {
                Tween = ITween.Clone(tween.Tween, targetObject)
            };

            return newTween;
        }

        public async UniTask StartAnimation(
            bool reverse = false,
            bool startFromCurrentValue = false,
            CancellationToken cancellationToken = default)
        {
            CheckInitialize();
            foreach (var animation in _animations)
            {
                await UniTask.Delay(TimeSpan.FromSeconds(perObjectSecondsDelay),
                    cancellationToken: cancellationToken);
                animation.StartAnimation(reverse, startFromCurrentValue, cancellationToken).Forget();
            }
        }

        public UniTask StopAnimation()
        {
            CheckInitialize();
            foreach (var animation in _animations)
            {
                animation.StopAnimation().Forget();
            }

            return UniTask.CompletedTask;
        }

        public void ResetValues()
        {
            CheckInitialize();
            foreach (var animation in _animations)
            {
                animation.ResetValues();
            }
        }

        public void EndValues()
        {
            CheckInitialize();
            foreach (var animation in _animations)
            {
                animation.EndValues();
            }
        }

        public void InitializeChildren()
        {
            _animations.Clear();
            for (var i = 0; i < parentObject.childCount; i++)
            {
                var child = parentObject.GetChild(i);
                if (!child.gameObject.activeSelf) continue;
                var animation = ITween.Clone(Tween, child.gameObject);
                _animations.Add(animation);
            }
        }

        private void CheckInitialize()
        {
            if (_animations == null) _animations = new List<ITween>();
            if (_animations.Count == 0) InitializeChildren();
        }

#if UNITY_EDITOR
        public virtual void OnGuiChange()
        {
            _animations = new List<ITween>();
        }
#endif
    }
}