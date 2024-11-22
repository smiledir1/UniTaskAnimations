using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Common.UniTaskAnimations.SimpleTweens
{
    [Serializable]
    public class ScaleTween : SimpleTween
    {
        #region View

        [SerializeField]
        private Vector3 fromScale;

        [SerializeField]
        private Vector3 toScale;

        #endregion /View

        #region Properties

        public Vector3 FromScale => fromScale;
        public Vector3 ToScale => toScale;

        #endregion

        #region Constructor

        public ScaleTween()
        {
            fromScale = Vector3.zero;
            toScale = Vector3.one;
        }

        public ScaleTween(
            GameObject tweenObject,
            float startDelay,
            float tweenTime,
            LoopType loop,
            AnimationCurve animationCurve,
            Vector3 fromScale,
            Vector3 toScale) :
            base(tweenObject,
                startDelay,
                tweenTime,
                loop,
                animationCurve)
        {
            this.fromScale = fromScale;
            this.toScale = toScale;
        }

        #endregion /Constructor

        #region Animation

        protected override async UniTask Tween(
            bool reverse = false,
            bool startFromCurrentValue = false,
            CancellationToken cancellationToken = default)
        {
            if (TweenObject == null) return;

            Vector3 startScale;
            Vector3 endScale;
            AnimationCurve curve;
            var curTweenTime = TweenTime;
            if (Loop == LoopType.PingPong) curTweenTime /= 2;
            var time = 0f;
            var curLoop = true;

            if (reverse)
            {
                startScale = toScale;
                endScale = fromScale;
                curve = ReverseCurve;
            }
            else
            {
                startScale = fromScale;
                endScale = toScale;
                curve = AnimationCurve;
            }

            if (startFromCurrentValue)
            {
                var localScale = TweenObject.transform.localScale;
                var t = 1f;
                if (endScale.x - startScale.x != 0f)
                    t = (localScale.x - startScale.x) / (endScale.x - startScale.x);
                else if (endScale.y - startScale.y != 0f)
                    t = (localScale.y - startScale.y) / (endScale.y - startScale.y);
                else if (endScale.z - startScale.z != 0f)
                    t = (localScale.z - startScale.z) / (endScale.z - startScale.z);

                time = curTweenTime * t;
            }

            while (curLoop)
            {
                TweenObject.transform.localScale = startScale;

                while (time < curTweenTime)
                {
                    time += GetDeltaTime();

                    var normalizeTime = time / curTweenTime;
                    GoToValue(startScale, endScale, curve, normalizeTime);
                    if (cancellationToken.IsCancellationRequested) return;
                    await UniTask.Yield();
                }

                if (cancellationToken.IsCancellationRequested) return;
                if (TweenObject == null) return;
                var lastKeyIndex = AnimationCurve.keys.Length - 1;
                var lastKey = AnimationCurve.keys[lastKeyIndex];
                var endValue = Vector3.LerpUnclamped(startScale, endScale, lastKey.value);
                TweenObject.transform.localScale = endValue;

                time -= curTweenTime;

                switch (Loop)
                {
                    case LoopType.Once:
                        curLoop = false;
                        break;

                    case LoopType.Loop:
                        break;

                    case LoopType.PingPong:
                        endScale = startScale;
                        startScale = TweenObject.transform.localScale;
                        break;
                }
            }
        }

        public override void ResetValues()
        {
            if (TweenObject != null) TweenObject.transform.localScale = fromScale;
        }

        public override void EndValues()
        {
            if (TweenObject != null) TweenObject.transform.localScale = toScale;
        }

        public override void SetTimeValue(float value)
        {
            GoToValue(FromScale, ToScale, AnimationCurve, value);
        }

        public void SetScale(Vector3 from, Vector3 to)
        {
            fromScale = from;
            toScale = to;
        }

        private void GoToValue(Vector3 startScale, Vector3 endScale, AnimationCurve curve, float value)
        {
            var lerpTime = curve?.Evaluate(value) ?? value;
            var lerpValue = Vector3.LerpUnclamped(startScale, endScale, lerpTime);

            if (TweenObject != null) TweenObject.transform.localScale = lerpValue;
        }

        #endregion /Animation

        #region Static

        public static ScaleTween Clone(
            ScaleTween tween,
            GameObject targetObject = null)
        {
            var animationCurve = new AnimationCurve();
            animationCurve.CopyFrom(tween.AnimationCurve);

            return new ScaleTween(
                targetObject,
                tween.StartDelay,
                tween.TweenTime,
                tween.Loop,
                animationCurve,
                tween.FromScale,
                tween.ToScale);
        }

        #endregion
    }
}