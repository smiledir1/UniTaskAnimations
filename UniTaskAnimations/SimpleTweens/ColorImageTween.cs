using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

namespace Common.UniTaskAnimations.SimpleTweens
{
    [Serializable]
    public class ColorImageTween : SimpleTween
    {
        #region View

        [SerializeField]
        private Color fromColor;

        [SerializeField]
        private Color toColor;

        [SerializeField]
        private bool ignoreAlpha;

        [SerializeField]
        private Graphic tweenGraphic;

        #endregion /View

        #region Properties

        public Color FromColor => fromColor;
        public Color ToColor => toColor;
        private bool IgnoreAlpha => ignoreAlpha;
        public Graphic TweenGraphic => tweenGraphic;

        #endregion

        #region Constructor

        public ColorImageTween()
        {
            fromColor = Color.white;
            toColor = Color.black;
            ignoreAlpha = false;
        }

        public ColorImageTween(
            GameObject tweenObject,
            float startDelay,
            float tweenTime,
            LoopType loop,
            AnimationCurve animationCurve,
            Graphic tweenGraphic,
            Color fromColor,
            Color toColor,
            bool ignoreAlpha) :
            base(tweenObject,
                startDelay,
                tweenTime,
                loop,
                animationCurve)
        {
            this.fromColor = fromColor;
            this.toColor = toColor;
            this.ignoreAlpha = ignoreAlpha;
            this.tweenGraphic = tweenGraphic;
        }

        #endregion

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

            Color startColor;
            Color endColor;
            AnimationCurve curve;
            var curTweenTime = TweenTime;
            if (Loop == LoopType.PingPong) curTweenTime /= 2;
            var time = 0f;
            var curLoop = true;

            if (reverse)
            {
                startColor = toColor;
                endColor = fromColor;
                curve = ReverseCurve;
            }
            else
            {
                startColor = fromColor;
                endColor = toColor;
                curve = AnimationCurve;
            }

            if (startFromCurrentValue)
            {
                var localColor = tweenGraphic.color;
                var t = 1f;
                if (endColor.r - startColor.r != 0f)
                    t = (localColor.r - startColor.r) / (endColor.r - startColor.r);
                else if (endColor.g - startColor.g != 0f)
                    t = (localColor.g - startColor.g) / (endColor.g - startColor.g);
                else if (endColor.b - startColor.b != 0f)
                    t = (localColor.b - startColor.b) / (endColor.b - startColor.b);

                else if (!ignoreAlpha && endColor.a - startColor.a != 0f)
                    t = (localColor.a - startColor.a) / (endColor.a - startColor.a);

                time = curTweenTime * t;
            }

            while (curLoop)
            {
                tweenGraphic.color = ignoreAlpha ? GetIgnoreAlphaColor(startColor) : startColor;

                while (time < curTweenTime)
                {
                    time += GetDeltaTime();

                    var normalizeTime = time / curTweenTime;
                    GoToValue(startColor, endColor, curve, normalizeTime);
                    if (cancellationToken.IsCancellationRequested) return;
                    await UniTask.Yield();
                }

                var lastKeyIndex = AnimationCurve.keys.Length - 1;
                var lastKey = AnimationCurve.keys[lastKeyIndex];
                var endValue = Color.LerpUnclamped(startColor, endColor, lastKey.value);
                tweenGraphic.color = ignoreAlpha ? GetIgnoreAlphaColor(endValue) : endValue;
                time -= TweenTime;

                switch (Loop)
                {
                    case LoopType.Once:
                        curLoop = false;
                        break;

                    case LoopType.Loop:
                        break;

                    case LoopType.PingPong:
                        endColor = startColor;
                        startColor = tweenGraphic.color;
                        break;
                }
            }
        }

        public override void ResetValues()
        {
            if (tweenGraphic == null) tweenGraphic = TweenObject.GetComponent<Graphic>();
            tweenGraphic.color = ignoreAlpha ? GetIgnoreAlphaColor(fromColor) : toColor;
        }

        public override void EndValues()
        {
            if (tweenGraphic == null) tweenGraphic = TweenObject.GetComponent<Graphic>();
            tweenGraphic.color = ignoreAlpha ? GetIgnoreAlphaColor(toColor) : toColor;
        }

        public override void SetTimeValue(float value)
        {
            if (tweenGraphic == null) tweenGraphic = TweenObject.GetComponent<Graphic>();
            GoToValue(fromColor, toColor, AnimationCurve, value);
        }

        public void SetColor(Color from, Color to)
        {
            if (ignoreAlpha)
            {
                if (tweenGraphic == null) tweenGraphic = TweenObject.GetComponent<Graphic>();
                fromColor = GetIgnoreAlphaColor(from);
                toColor = GetIgnoreAlphaColor(to);
            }
            else
            {
                fromColor = from;
                toColor = to;
            }
        }

        private Color GetIgnoreAlphaColor(Color color) =>
            new(color.r, color.g, color.b, tweenGraphic.color.a);

        private void GoToValue(Color startColor, Color endColor, AnimationCurve curve, float value)
        {
            var lerpTime = curve?.Evaluate(value) ?? value;
            var lerpValue = Color.LerpUnclamped(startColor, endColor, lerpTime);

            if (tweenGraphic == null) return;
            tweenGraphic.color = ignoreAlpha ? GetIgnoreAlphaColor(lerpValue) : lerpValue;
        }

        #endregion /Animation

        #region Static

        public static ColorImageTween Clone(
            ColorImageTween tween,
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

            return new ColorImageTween(
                targetObject,
                tween.StartDelay,
                tween.TweenTime,
                tween.Loop,
                animationCurve,
                tweenImage,
                tween.FromColor,
                tween.ToColor,
                tween.IgnoreAlpha);
        }

        #endregion
    }
}