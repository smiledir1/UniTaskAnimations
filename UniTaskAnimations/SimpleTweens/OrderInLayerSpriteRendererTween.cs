using UnityEngine;
using System.Threading;
using Cysharp.Threading.Tasks;

namespace Common.UniTaskAnimations.SimpleTweens
{
    public class OrderInLayerSpriteRendererTween : SimpleTween
    {
        #region View

        [SerializeField]
        private int fromOrder;

        [SerializeField]
        private int toOrder;

        [SerializeField]
        private SpriteRenderer tweenGraphic;

        #endregion /View

        #region Properties

        public int FromOrder => fromOrder;
        public int ToOrder => toOrder;
        public SpriteRenderer TweenObjectRenderer => tweenGraphic;

        #endregion

        #region Constructor

        public OrderInLayerSpriteRendererTween()
        {
            fromOrder = 0;
            toOrder = 0;
        }

        public OrderInLayerSpriteRendererTween(
            GameObject tweenObject,
            float startDelay,
            float tweenTime,
            LoopType loop,
            AnimationCurve animationCurve,
            SpriteRenderer tweenGraphic,
            int fromOrder,
            int toOrder) :
            base(tweenObject,
                startDelay,
                tweenTime,
                loop,
                animationCurve)
        {
            this.fromOrder = fromOrder;
            this.toOrder = toOrder;
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
                tweenGraphic = tweenObject.GetComponent<SpriteRenderer>();
                if (tweenGraphic == null) return;
            }

            int startOrder;
            int endOrder;
            AnimationCurve curve;
            var curTweenTime = TweenTime;
            if (Loop == LoopType.PingPong) curTweenTime /= 2;
            var time = 0f;
            var curLoop = true;

            if (reverse)
            {
                startOrder = toOrder;
                endOrder = fromOrder;
                curve = ReverseCurve;
            }
            else
            {
                startOrder = fromOrder;
                endOrder = toOrder;
                curve = AnimationCurve;
            }

            if (startFromCurrentValue)
            {
                var currentValue = tweenGraphic.sortingOrder;
                var t = (currentValue - startOrder) / (endOrder - startOrder);
                time = curTweenTime * t;
            }

            while (curLoop)
            {
                tweenGraphic.sortingOrder = startOrder;

                while (time < curTweenTime)
                {
                    time += GetDeltaTime();

                    var normalizeTime = time / curTweenTime;
                    GoToValue(startOrder, endOrder, curve, normalizeTime);
                    if (cancellationToken.IsCancellationRequested) return;
                    await UniTask.Yield();
                }

                var lastKeyIndex = AnimationCurve.keys.Length - 1;
                var lastKey = AnimationCurve.keys[lastKeyIndex];
                var endValue = Mathf.LerpUnclamped(startOrder, endOrder, lastKey.value);
                var lerpRoundValue = Mathf.RoundToInt(endValue);
                tweenGraphic.sortingOrder = lerpRoundValue;
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
                        endOrder = startOrder;
                        startOrder = tweenGraphic.sortingOrder;
                        break;
                }
            }
        }

        public override void ResetValues()
        {
            if (tweenGraphic == null) tweenGraphic = TweenObject.GetComponent<SpriteRenderer>();
            tweenGraphic.sortingOrder = fromOrder;
        }

        public override void EndValues()
        {
            if (tweenGraphic == null) tweenGraphic = TweenObject.GetComponent<SpriteRenderer>();
            tweenGraphic.sortingOrder = toOrder;
        }

        public override void SetTimeValue(float value)
        {
            if (tweenGraphic == null) tweenGraphic = TweenObject.GetComponent<SpriteRenderer>();
            GoToValue(FromOrder, ToOrder, AnimationCurve, value);
        }

        public void SetOrder(int from, int to)
        {
            fromOrder = from;
            toOrder = to;
        }

        private void GoToValue(float startOrder, float endOrder, AnimationCurve curve, float value)
        {
            var lerpTime = curve?.Evaluate(value) ?? value;
            var lerpValue = Mathf.LerpUnclamped(startOrder, endOrder, lerpTime);
            var lerpRoundValue = Mathf.RoundToInt(lerpValue);
            if (tweenGraphic == null) return;
            tweenGraphic.sortingOrder = lerpRoundValue;
        }

        #endregion /Animation

        #region Static

        public static OrderInLayerSpriteRendererTween Clone(
            OrderInLayerSpriteRendererTween tween,
            GameObject targetObject = null)
        {
            SpriteRenderer tweenRenderer = null;
            if (targetObject != null)
            {
                tweenRenderer = targetObject.GetComponent<SpriteRenderer>();
                if (tweenRenderer == null) targetObject.AddComponent<SpriteRenderer>();
            }

            var animationCurve = new AnimationCurve();
            animationCurve.CopyFrom(tween.AnimationCurve);

            return new OrderInLayerSpriteRendererTween(
                targetObject,
                tween.StartDelay,
                tween.TweenTime,
                tween.Loop,
                animationCurve,
                tweenRenderer,
                tween.FromOrder,
                tween.ToOrder);
        }

        #endregion
    }
}
