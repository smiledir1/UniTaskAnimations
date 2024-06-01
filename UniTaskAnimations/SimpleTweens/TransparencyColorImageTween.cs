using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

namespace Common.UniTaskAnimations.SimpleTweens
{
    [Serializable]
    public class TransparencyColorImageTween : SimpleTween
    {
        #region View

        [SerializeField]
        [Range(0, 1)]
        private float fromOpacity;

        [SerializeField]
        [Range(0, 1)]
        private float toOpacity;

        [SerializeField]
        private Graphic tweenGraphic;

        #endregion /View

        #region Properties

        public float FromOpacity => fromOpacity;
        public float ToOpacity => toOpacity;
        public Graphic TweenObjectRenderer => tweenGraphic;

        #endregion

        #region Constructor

        public TransparencyColorImageTween()
        {
            fromOpacity = 0;
            toOpacity = 1;
        }

        public TransparencyColorImageTween(
            GameObject tweenObject,
            float startDelay,
            float tweenTime,
            LoopType loop,
            AnimationCurve animationCurve,
            Graphic tweenGraphic,
            float fromOpacity,
            float toOpacity) :
            base(tweenObject,
                startDelay,
                tweenTime,
                loop,
                animationCurve)
        {
            this.fromOpacity = fromOpacity;
            this.toOpacity = toOpacity;
            this.tweenGraphic = tweenGraphic;
        }

        #endregion /Constructor

        #region Animation

        protected override async UniTask Tween(
            bool reverse = false,
            bool startFromCurrentValue = false,
            CancellationToken cancellationToken = default)
        {
            if (tweenGraphic == null)
            {
                tweenGraphic = tweenObject.GetComponent<Graphic>();
                if (tweenGraphic == null) return;
            }

            float startOpacity;
            float endOpacity;
            AnimationCurve curve;
            var curTweenTime = TweenTime;
            if (Loop == LoopType.PingPong) curTweenTime /= 2;
            var time = 0f;
            var curLoop = true;

            if (reverse)
            {
                startOpacity = toOpacity;
                endOpacity = fromOpacity;
                curve = ReverseCurve;
            }
            else
            {
                startOpacity = fromOpacity;
                endOpacity = toOpacity;
                curve = AnimationCurve;
            }

            if (startFromCurrentValue)
            {
                var currentValue = tweenGraphic.color.a;
                var t = (currentValue - startOpacity) / (endOpacity - startOpacity);
                time = curTweenTime * t;
            }

            while (curLoop)
            {
                tweenGraphic.color = GetColorWithAlpha(startOpacity);

                while (time < curTweenTime)
                {
                    time += GetDeltaTime();

                    var normalizeTime = time / curTweenTime;
                    GoToValue(startOpacity, endOpacity, curve, normalizeTime);
                    if (cancellationToken.IsCancellationRequested) return;
                    await UniTask.Yield();
                }

                var lastKeyIndex = AnimationCurve.keys.Length - 1;
                var lastKey = AnimationCurve.keys[lastKeyIndex];
                var endValue = Mathf.LerpUnclamped(startOpacity, endOpacity, lastKey.value);
                tweenGraphic.color = GetColorWithAlpha(endValue);
                time -= curTweenTime;

                switch (Loop)
                {
                    case LoopType.Once:
                        curLoop = false;
                        break;

                    case LoopType.Loop:
                        break;

                    case LoopType.PingPong:
                        if (tweenGraphic == null) return;
                        endOpacity = startOpacity;
                        startOpacity = tweenGraphic.color.a;
                        break;
                }
            }
        }

        public override void ResetValues()
        {
            if (tweenGraphic == null) tweenGraphic = TweenObject.GetComponent<Graphic>();
            tweenGraphic.color = GetColorWithAlpha(fromOpacity);
        }

        public override void EndValues()
        {
            if (tweenGraphic == null) tweenGraphic = TweenObject.GetComponent<Graphic>();
            tweenGraphic.color = GetColorWithAlpha(toOpacity);
        }
        
        public override void SetTimeValue(float value)
        {
            if (tweenGraphic == null) tweenGraphic = TweenObject.GetComponent<Graphic>();
            GoToValue(FromOpacity, ToOpacity, AnimationCurve, value);
        }

        public void SetTransparency(float from, float to)
        {
            fromOpacity = from;
            toOpacity = to;
        }

        private Color GetColorWithAlpha(float alpha)
        {
            var color = tweenGraphic.color;
            return new Color(color.r, color.g, color.b, alpha);
        }

        private void GoToValue(float startOpacity, float endOpacity, AnimationCurve curve, float value)
        {
            var lerpTime = curve?.Evaluate(value) ?? value;
            var lerpValue = Mathf.LerpUnclamped(startOpacity, endOpacity, lerpTime);

            if (tweenGraphic == null) return;
            tweenGraphic.color = GetColorWithAlpha(lerpValue);
        }

        #endregion /Animation

        #region Static

        public static TransparencyColorImageTween Clone(
            TransparencyColorImageTween tween,
            GameObject targetObject = null)
        {
            Graphic tweenImage = null;
            if (targetObject != null)
            {
                tweenImage = targetObject.GetComponent<Graphic>();
                if (tweenImage == null) targetObject.AddComponent<Image>();
            }

            var animationCurve = new AnimationCurve();
            animationCurve.CopyFrom(tween.AnimationCurve);

            return new TransparencyColorImageTween(
                targetObject,
                tween.StartDelay,
                tween.TweenTime,
                tween.Loop,
                animationCurve,
                tweenImage,
                tween.FromOpacity,
                tween.ToOpacity);
        }

        #endregion
    }
}