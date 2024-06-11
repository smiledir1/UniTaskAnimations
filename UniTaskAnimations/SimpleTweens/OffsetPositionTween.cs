using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Common.UniTaskAnimations.SimpleTweens
{
    [Serializable]
    public class OffsetPositionTween : SimpleTween
    {
        #region View

        [SerializeField]
        private Vector3 fromPosition;

        [SerializeField]
        private Vector3 toPosition;

        #endregion /View

        #region Properties

        public Vector3 FromPosition => fromPosition;
        public Vector3 ToPosition => toPosition;

        #endregion

        #region Constructor

        public OffsetPositionTween()
        {
            fromPosition = Vector3.zero;
            toPosition = Vector3.zero;
        }

        public OffsetPositionTween(
            GameObject tweenObject,
            float startDelay,
            float tweenTime,
            LoopType loop,
            AnimationCurve animationCurve,
            Vector3 fromPosition,
            Vector3 toPosition) :
            base(tweenObject,
                startDelay,
                tweenTime,
                loop,
                animationCurve)
        {
            this.fromPosition = fromPosition;
            this.toPosition = toPosition;
        }

        #endregion /Constructor

        #region Animation

        protected override async UniTask Tween(
            bool reverse = false,
            bool startFromCurrentValue = false,
            CancellationToken cancellationToken = default)
        {
            if (TweenObject == null) return;

            var targetPosition = TweenObject.transform.localPosition;

            Vector3 startPosition;
            Vector3 endPosition;
            AnimationCurve curve;
            var endTweenTime = TweenTime;
            if (Loop == LoopType.PingPong) endTweenTime /= 2;
            var time = 0f;
            var curLoop = true;

            if (reverse)
            {
                startPosition = targetPosition + toPosition;
                endPosition = targetPosition + fromPosition;
                curve = ReverseCurve;
            }
            else
            {
                startPosition = targetPosition + fromPosition;
                endPosition = targetPosition + toPosition;
                curve = AnimationCurve;
            }

            if (startFromCurrentValue)
            {
                var localPosition = TweenObject.transform.localPosition;
                var t = 1f;
                if (endPosition.x - startPosition.x != 0f)
                    t = (localPosition.x - startPosition.x) / (endPosition.x - startPosition.x);
                else if (endPosition.y - startPosition.y != 0f)
                    t = (localPosition.y - startPosition.y) / (endPosition.y - startPosition.y);
                else if (endPosition.z - startPosition.z != 0f)
                    t = (localPosition.z - startPosition.z) / (endPosition.z - startPosition.z);

                time = endTweenTime * t;
            }

            while (curLoop)
            {
                TweenObject.transform.localPosition = startPosition;

                while (time < endTweenTime)
                {
                    time += GetDeltaTime();

                    var normalizeTime = time / endTweenTime;
                    var lerpTime = curve?.Evaluate(normalizeTime) ?? normalizeTime;
                    var lerpValue = Vector3.LerpUnclamped(startPosition, endPosition, lerpTime);

                    if (TweenObject != null && TweenObject.transform != null)
                        TweenObject.transform.localPosition = lerpValue;
                    if (cancellationToken.IsCancellationRequested) return;
                    await UniTask.Yield();
                }

                if (cancellationToken.IsCancellationRequested) return;
                if (TweenObject != null && TweenObject.transform != null)
                    TweenObject.transform.localPosition = endPosition;
                time -= endTweenTime;

                switch (Loop)
                {
                    case LoopType.Once:
                        curLoop = false;
                        break;

                    case LoopType.Loop:
                        break;

                    case LoopType.PingPong:
                        endPosition = startPosition;
                        startPosition = TweenObject.transform.localPosition;
                        break;
                }
            }
        }

        public override void ResetValues()
        {
        }
        
        public override void EndValues()
        {
        }
        
        public override void SetTimeValue(float value)
        {
        }

        public void SetPositions(Vector3 from, Vector3 to)
        {
            fromPosition = from;
            toPosition = to;
        }

        #endregion /Animation

        #region Editor

#if UNITY_EDITOR
        [UnityEditor.DrawGizmo(UnityEditor.GizmoType.Selected)]
        private static void OnDrawGizmo(TweenComponent component, UnityEditor.GizmoType gizmoType)
        {
            if (component.Tween is not OffsetPositionTween positionTween) return;
            Gizmos.color = Color.magenta;
            var targetPosition =
                positionTween.TweenObject == null ||
                positionTween.TweenObject.transform == null
                    ? Vector3.zero
                    : positionTween.TweenObject.transform.position;
            var fromPosition = targetPosition + positionTween.FromPosition;
            var toPosition = targetPosition + positionTween.ToPosition;
            Gizmos.DrawLine(fromPosition, toPosition);
        }
#endif

        #endregion

        #region Static

        public static OffsetPositionTween Clone(
            OffsetPositionTween tween,
            GameObject targetObject = null)
        {
            var animationCurve = new AnimationCurve();
            animationCurve.CopyFrom(tween.AnimationCurve);

            return new OffsetPositionTween(
                targetObject,
                tween.StartDelay,
                tween.TweenTime,
                tween.Loop,
                animationCurve,
                tween.FromPosition,
                tween.ToPosition);
        }

        #endregion
    }
}