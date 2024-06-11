using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Common.UniTaskAnimations.SimpleTweens
{
    [Serializable]
    public class RotationTween : SimpleTween
    {
        #region View

        [SerializeField]
        private Vector3 fromRotation;

        [SerializeField]
        private Vector3 toRotation;

        #endregion /View

        #region Properties

        public Vector3 FromRotation => fromRotation;
        public Vector3 ToRotation => toRotation;

        #endregion

        #region Constructor

        public RotationTween()
        {
            fromRotation = Vector3.zero;
            toRotation = Vector3.zero;
        }

        public RotationTween(
            GameObject tweenObject,
            float startDelay,
            float tweenTime,
            LoopType loop,
            AnimationCurve animationCurve,
            Vector3 fromRotation,
            Vector3 toRotation) :
            base(tweenObject,
                startDelay,
                tweenTime,
                loop,
                animationCurve)
        {
            this.fromRotation = fromRotation;
            this.toRotation = toRotation;
        }

        #endregion /Constructor

        #region Animation

        protected override async UniTask Tween(
            bool reverse = false,
            bool startFromCurrentValue = false,
            CancellationToken cancellationToken = default)
        {
            if (TweenObject == null) return;

            Vector3 startRotation;
            Vector3 endRotation;
            AnimationCurve curve;
            var curTweenTime = TweenTime;
            if (Loop == LoopType.PingPong) curTweenTime /= 2;
            var time = 0f;
            var curLoop = true;

            if (reverse)
            {
                startRotation = toRotation;
                endRotation = fromRotation;
                curve = ReverseCurve;
            }
            else
            {
                startRotation = fromRotation;
                endRotation = toRotation;
                curve = AnimationCurve;
            }

            if (startFromCurrentValue)
            {
                var localRotation = TweenObject.transform.eulerAngles;
                var t = 1f;
                if (endRotation.x - startRotation.x != 0f)
                    t = (localRotation.x - startRotation.x) / (endRotation.x - startRotation.x);
                else if (endRotation.y - startRotation.y != 0f)
                    t = (localRotation.y - startRotation.y) / (endRotation.y - startRotation.y);
                else if (endRotation.z - startRotation.z != 0f)
                    t = (localRotation.z - startRotation.z) / (endRotation.z - startRotation.z);

                time = curTweenTime * t;
            }

            while (curLoop)
            {
                if (TweenObject != null) TweenObject.transform.eulerAngles = startRotation;

                while (time < curTweenTime)
                {
                    time += GetDeltaTime();

                    var normalizeTime = time / curTweenTime;
                   GoToValue(startRotation, endRotation, curve, normalizeTime);
                    if (cancellationToken.IsCancellationRequested) return;
                    await UniTask.Yield();
                }

                if (cancellationToken.IsCancellationRequested) return;
                if (TweenObject == null) return;
                var lastKeyIndex = AnimationCurve.keys.Length - 1;
                var lastKey = AnimationCurve.keys[lastKeyIndex];
                var endValue = Vector3.LerpUnclamped(startRotation, endRotation, lastKey.value);
                TweenObject.transform.eulerAngles = endValue;
                time -= curTweenTime;

                switch (Loop)
                {
                    case LoopType.Once:
                        curLoop = false;
                        break;

                    case LoopType.Loop:
                        break;

                    case LoopType.PingPong:
                        endRotation = startRotation;
                        if (TweenObject != null) startRotation = TweenObject.transform.eulerAngles;
                        break;
                }
            }
        }

        public override void ResetValues()
        {
            TweenObject.transform.eulerAngles = fromRotation;
        }

        public override void EndValues()
        {
            TweenObject.transform.eulerAngles = toRotation;
        }
        
        public override void SetTimeValue(float value)
        {
            GoToValue(FromRotation, ToRotation, AnimationCurve, value);
        }

        public void SetRotation(Vector3 from, Vector3 to)
        {
            fromRotation = from;
            toRotation = to;
        }

        private void GoToValue(Vector3 startRotation, Vector3 endRotation, AnimationCurve curve, float value)
        {
            var lerpTime = curve?.Evaluate(value) ?? value;
            var lerpValue = Vector3.LerpUnclamped(startRotation, endRotation, lerpTime);

            if (TweenObject != null) TweenObject.transform.eulerAngles = lerpValue;
        }

        #endregion /Animation

        #region Static

        public static RotationTween Clone(
            RotationTween tween,
            GameObject targetObject = null)
        {
            var animationCurve = new AnimationCurve();
            animationCurve.CopyFrom(tween.AnimationCurve);

            return new RotationTween(
                targetObject,
                tween.StartDelay,
                tween.TweenTime,
                tween.Loop,
                animationCurve,
                tween.FromRotation,
                tween.ToRotation);
        }

        #endregion
    }
}