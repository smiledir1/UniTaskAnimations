using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Common.UniTaskAnimations.SimpleTweens
{
    [Serializable]
    public class TransparencyCanvasGroupTween : SimpleTween
    {
        #region View

        [SerializeField]
        [Range(0, 1)]
        private float fromOpacity;

        [SerializeField]
        [Range(0, 1)]
        private float toOpacity;

        [SerializeField]
        private CanvasGroup tweenObjectRenderer;

        #endregion /View

        #region Properties

        public float FromOpacity => fromOpacity;
        public float ToOpacity => toOpacity;
        public CanvasGroup TweenObjectRenderer => tweenObjectRenderer;

        #endregion

        #region Constructor

        public TransparencyCanvasGroupTween()
        {
            fromOpacity = 0;
            toOpacity = 1;
        }

        public TransparencyCanvasGroupTween(
            GameObject tweenObject,
            float startDelay,
            float tweenTime,
            LoopType loop,
            AnimationCurve animationCurve,
            CanvasGroup tweenObjectRenderer,
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
            this.tweenObjectRenderer = tweenObjectRenderer;
        }

        #endregion /Constructor

        #region Animation

        protected override async UniTask Tween(
            bool reverse = false,
            bool startFromCurrentValue = false,
            CancellationToken cancellationToken = default)
        {
            if (tweenObjectRenderer == null)
            {
                tweenObjectRenderer = tweenObject.GetComponent<CanvasGroup>();
                if (tweenObjectRenderer == null) return;
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
                var currentValue = tweenObjectRenderer.alpha;
                var t = (currentValue - startOpacity) / (endOpacity - startOpacity);
                time = curTweenTime * t;
            }

            while (curLoop)
            {
                tweenObjectRenderer.alpha = startOpacity;

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
                tweenObjectRenderer.alpha = endValue;
                time -= curTweenTime;

                switch (Loop)
                {
                    case LoopType.Once:
                        curLoop = false;
                        break;

                    case LoopType.Loop:
                        break;

                    case LoopType.PingPong:
                        if (tweenObjectRenderer == null) return;
                        endOpacity = startOpacity;
                        startOpacity = tweenObjectRenderer.alpha;
                        break;
                }
            }
        }

        public override void ResetValues()
        {
            if (tweenObjectRenderer == null) tweenObjectRenderer = TweenObject.GetComponent<CanvasGroup>();
            tweenObjectRenderer.alpha = fromOpacity;
        }

        public override void EndValues()
        {
            if (tweenObjectRenderer == null) tweenObjectRenderer = TweenObject.GetComponent<CanvasGroup>();
            tweenObjectRenderer.alpha = toOpacity;
        }

        public override void SetTimeValue(float value)
        {
            if (tweenObjectRenderer == null) tweenObjectRenderer = TweenObject.GetComponent<CanvasGroup>();
            GoToValue(FromOpacity, ToOpacity, AnimationCurve, value);
        }

        public void SetTransparency(float from, float to)
        {
            fromOpacity = from;
            toOpacity = to;
        }

        private void GoToValue(float startOpacity, float endOpacity, AnimationCurve curve, float value)
        {
            var lerpTime = curve?.Evaluate(value) ?? value;
            var lerpValue = Mathf.LerpUnclamped(startOpacity, endOpacity, lerpTime);

            if (tweenObjectRenderer == null) return;
            tweenObjectRenderer.alpha = lerpValue;
        }

        #endregion /Animation
        
#if UNITY_EDITOR
        
        public override void OnGuiChange()
        {
            tweenObjectRenderer = tweenObject.GetComponent<CanvasGroup>();
            base.OnGuiChange();
        }

#endif

        #region Static

        public static TransparencyCanvasGroupTween Clone(
            TransparencyCanvasGroupTween tween,
            GameObject targetObject = null)
        {
            CanvasGroup canvasGroup = null;
            if (targetObject != null)
            {
                canvasGroup = targetObject.GetComponent<CanvasGroup>();
                if (canvasGroup == null) targetObject.AddComponent<CanvasGroup>();
            }

            var animationCurve = new AnimationCurve();
            animationCurve.CopyFrom(tween.AnimationCurve);

            return new TransparencyCanvasGroupTween(
                targetObject,
                tween.StartDelay,
                tween.TweenTime,
                tween.Loop,
                animationCurve,
                canvasGroup,
                tween.FromOpacity,
                tween.ToOpacity);
        }

        #endregion
    }
}