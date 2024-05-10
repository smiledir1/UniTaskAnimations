using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

namespace Common.UniTaskAnimations.SimpleTweens
{
    [Serializable]
    public class FillImageTween : SimpleTween
    {
        #region View

        [SerializeField]
        [Range(0, 1)]
        private float fromFill;

        [SerializeField]
        [Range(0, 1)]
        private float toFill;

        [SerializeField]
        private Image tweenImage;

        #endregion /View

        #region Properties

        public float FromFill => fromFill;
        public float ToFill => toFill;
        public Image TweenImage => tweenImage;

        #endregion

        #region Constructor

        public FillImageTween()
        {
            fromFill = 0;
            toFill = 1;
        }

        public FillImageTween(
            GameObject tweenObject,
            float startDelay,
            float tweenTime,
            LoopType loop,
            AnimationCurve animationCurve,
            Image tweenImage,
            float fromFill,
            float toFill) :
            base(tweenObject,
                startDelay,
                tweenTime,
                loop,
                animationCurve)
        {
            this.fromFill = fromFill;
            this.toFill = toFill;
            this.tweenImage = tweenImage;
        }

        #endregion

        #region Animation

        protected override async UniTask Tween(
            bool reverse = false,
            bool startFromCurrentValue = false,
            CancellationToken cancellationToken = default)
        {
            if (tweenImage == null)
            {
                tweenImage = tweenObject.GetComponent<Image>();
                if (tweenImage == null) return;
            }

            float startFill;
            float endFill;
            AnimationCurve reverseCurve;
            var curTweenTime = TweenTime;
            if (Loop == LoopType.PingPong) curTweenTime /= 2;
            var time = 0f;
            var curLoop = true;

            if (reverse)
            {
                startFill = toFill;
                endFill = fromFill;
                reverseCurve = ReverseCurve;
            }
            else
            {
                startFill = fromFill;
                endFill = toFill;
                reverseCurve = AnimationCurve;
            }

            if (startFromCurrentValue)
            {
                var currentValue = tweenImage.fillAmount;
                var t = (currentValue - startFill) / (endFill - startFill);
                time = curTweenTime * t;
            }

            while (curLoop)
            {
                tweenImage.fillAmount = startFill;

                while (time < curTweenTime)
                {
                    time += GetDeltaTime();

                    var normalizeTime = time / curTweenTime;
                    var lerpTime = reverseCurve?.Evaluate(normalizeTime) ?? normalizeTime;
                    var lerpValue = Mathf.LerpUnclamped(startFill, endFill, lerpTime);

                    if (tweenImage == null) return;
                    tweenImage.fillAmount = lerpValue;
                    if (cancellationToken.IsCancellationRequested) return;
                    await UniTask.Yield();
                }

                var lastKeyIndex = AnimationCurve.keys.Length - 1;
                var lastKey = AnimationCurve.keys[lastKeyIndex];
                var endValue = Mathf.LerpUnclamped(startFill, endFill, lastKey.value);
                tweenImage.fillAmount = endValue;
                time -= curTweenTime;

                switch (Loop)
                {
                    case LoopType.Once:
                        curLoop = false;
                        break;

                    case LoopType.Loop:
                        break;

                    case LoopType.PingPong:
                        if (tweenImage == null) return;
                        endFill = startFill;
                        startFill = tweenImage.fillAmount;
                        break;
                }
            }
        }

        public override void ResetValues()
        {
            if (tweenImage == null) tweenImage = TweenObject.GetComponent<Image>();
            tweenImage.fillAmount = fromFill;
        }

        public override void EndValues()
        {
            if (tweenImage == null) tweenImage = TweenObject.GetComponent<Image>();
            tweenImage.fillAmount = toFill;
        }

        public void SetFill(float from, float to)
        {
            fromFill = from;
            toFill = to;
        }

        #endregion /Animation

        #region Static

        public static FillImageTween Clone(
            FillImageTween tween,
            GameObject targetObject = null)
        {
            Image tweenImage = null;
            if (targetObject != null)
            {
                tweenImage = targetObject.GetComponent<Image>();
                if (tweenImage == null) targetObject.AddComponent<Image>();
            }

            var animationCurve = new AnimationCurve();
            animationCurve.CopyFrom(tween.AnimationCurve);

            return new FillImageTween(
                targetObject,
                tween.StartDelay,
                tween.TweenTime,
                tween.Loop,
                animationCurve,
                tweenImage,
                tween.FromFill,
                tween.ToFill);
        }

        #endregion
    }
}