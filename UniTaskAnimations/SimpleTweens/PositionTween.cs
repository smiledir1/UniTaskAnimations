using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Common.UniTaskAnimations.SimpleTweens
{
    [Serializable]
    public class PositionTween : BasePositionTween
    {
        #region View

        [SerializeField]
        private Vector3 fromPosition;

        [SerializeField]
        private Vector3 toPosition;

        [SerializeField]
        private Transform fromTarget;

        [SerializeField]
        private Transform toTarget;

        #endregion /View

        #region Properties

        public Vector3 FromPosition => fromPosition;
        public Vector3 ToPosition => toPosition;
        public Transform FromTarget => fromTarget;
        public Transform ToTarget => toTarget;

        #endregion

        #region Constructor

        public PositionTween()
        {
            positionType = PositionType.Local;
            fromPosition = Vector3.zero;
            toPosition = Vector3.zero;
        }

        public PositionTween(
            GameObject tweenObject,
            float startDelay,
            float tweenTime,
            LoopType loop,
            AnimationCurve animationCurve,
            PositionType positionType,
            Vector3 fromPosition,
            Vector3 toPosition,
            Transform fromTarget,
            Transform toTarget) :
            base(tweenObject,
                startDelay,
                tweenTime,
                loop,
                animationCurve)
        {
            this.positionType = positionType;
            this.fromPosition = fromPosition;
            this.toPosition = toPosition;
            this.fromTarget = fromTarget;
            this.toTarget = toTarget;
        }

        #endregion /Constructor

        #region Animation

        protected override async UniTask Tween(
            bool reverse = false,
            bool startFromCurrentValue = false,
            CancellationToken cancellationToken = default)
        {
            if (!IsInitialized)
            {
                Debug.Log($"Tween not initialized");
                return;
            }

            if (tweenObject == null)
            {
                Debug.Log($"TweenObject null");
                return;
            }
            
            Vector3 startPosition;
            Vector3 endPosition;
            AnimationCurve curve;
            var curTweenTime = tweenTime;
            if (Loop == LoopType.PingPong) curTweenTime /= 2;
            var time = 0f;
            var curLoop = true;

            if (positionType == PositionType.Target)
            {
                if (toTarget == null || fromTarget == null)
                {
                    Debug.LogError("Targets null");
                    return;
                }

                if (reverse)
                {
                    startPosition = toTarget.transform.position;
                    endPosition = fromTarget.transform.position;
                    curve = ReverseCurve;
                }
                else
                {
                    startPosition = fromTarget.transform.position;
                    endPosition = toTarget.transform.position;
                    curve = animationCurve;
                }
            }
            else
            {
                if (reverse)
                {
                    startPosition = toPosition;
                    endPosition = fromPosition;
                    curve = ReverseCurve;
                }
                else
                {
                    startPosition = fromPosition;
                    endPosition = toPosition;
                    curve = animationCurve;
                }
            }

            if (startFromCurrentValue)
            {
                var currentPosition = GetCurrentPosition();
                var t = 1f;
                if (endPosition.x - startPosition.x != 0f)
                    t = (currentPosition.x - startPosition.x) / (endPosition.x - startPosition.x);
                else if (endPosition.y - startPosition.y != 0f)
                    t = (currentPosition.y - startPosition.y) / (endPosition.y - startPosition.y);
                else if (endPosition.z - startPosition.z != 0f)
                    t = (currentPosition.z - startPosition.z) / (endPosition.z - startPosition.z);

                time = curTweenTime * t;
            }

            while (curLoop)
            {
                GoToPosition(startPosition);

                while (time < curTweenTime)
                {
                    time += GetDeltaTime();

                    var normalizeTime = time / curTweenTime;
                    GoToValue(startPosition, endPosition, curve, normalizeTime);
                    if (cancellationToken.IsCancellationRequested) return;
                    await UniTask.Yield();
                }

                var lastKeyIndex = AnimationCurve.keys.Length - 1;
                var lastKey = AnimationCurve.keys[lastKeyIndex];
                var endValue = Vector3.LerpUnclamped(startPosition, endPosition, lastKey.value);
                GoToPosition(endValue);
                time -= curTweenTime;

                switch (Loop)
                {
                    case LoopType.Once:
                        curLoop = false;
                        break;

                    case LoopType.Loop:
                        break;

                    case LoopType.PingPong:
                        endPosition = startPosition;
                        startPosition = GetCurrentPosition();
                        break;
                }
            }
        }

        public override void ResetValues()
        {
            RectTransform ??= tweenObject.transform as RectTransform;
            GoToPosition(fromPosition);
        }

        public override void EndValues()
        {
            RectTransform ??= tweenObject.transform as RectTransform;
            GoToPosition(toPosition);
        }

        public override void SetTimeValue(float value)
        {
            RectTransform ??= tweenObject.transform as RectTransform;
            if (positionType == PositionType.Target)
            {
                var from = fromTarget.position;
                var to = toTarget.position;
                GoToValue(from, to, AnimationCurve, value);
            }
            else
            {
                GoToValue(FromPosition, toPosition, AnimationCurve, value);
            }
        }

        public void SetPositions(Vector3 from, Vector3 to, PositionType curPositionType)
        {
            positionType = curPositionType;
            fromPosition = from;
            toPosition = to;
        }

        internal Vector3 GetCurrentPosition()
        {
            return positionType switch
            {
                PositionType.Local => tweenObject.transform.localPosition,
                PositionType.Global => tweenObject.transform.position,
                PositionType.Anchored => RectTransform.anchoredPosition,
                _ => Vector3.zero
            };
        }
        
        private void GoToValue(Vector3 startPosition, Vector3 endPosition, AnimationCurve curve, float value)
        {
            var lerpTime = curve?.Evaluate(value) ?? value;
            var lerpValue = Vector3.LerpUnclamped(startPosition, endPosition, lerpTime);

            GoToPosition(lerpValue);
        }

        #endregion /Animation

        #region Editor

#if UNITY_EDITOR
        [UnityEditor.DrawGizmo(UnityEditor.GizmoType.NonSelected | UnityEditor.GizmoType.Selected)]
        private static void OnDrawGizmo(TweenComponent component, UnityEditor.GizmoType gizmoType)
        {
            if (component.Tween is PositionTween positionTween)
            {
                DrawGizmos(positionTween);
                return;
            }
            
            if (component.Tween is GroupTween groupTween)
            {
                foreach (var tween in groupTween.Tweens)
                {
                    if (tween is PositionTween posTween)
                    {
                        DrawGizmos(posTween);
                    }
                }
            }
        }

        private static void DrawGizmos(PositionTween positionTween)
        {
            Gizmos.color = Color.magenta;

            switch (positionTween.PositionType)
            {
                case PositionType.Local:
                    DrawLocalPosition(positionTween);
                    break;
                case PositionType.Global:
                    DrawGlobalPosition(positionTween);
                    break;
                case PositionType.Anchored:
                    DrawAnchoredPosition(positionTween);
                    break;
                case PositionType.Target:
                    DrawTargetPosition(positionTween);
                    break;
            }
        }

        private static void DrawLocalPosition(PositionTween positionTween)
        {
            var parent = positionTween.TweenObject == null ||
                         positionTween.TweenObject.transform == null ||
                         positionTween.TweenObject.transform.parent == null
                ? null
                : positionTween.TweenObject.transform.parent;

            var parentPosition = parent == null ? Vector3.zero : parent.position;
            var parentScale = parent == null ? Vector3.one : parent.lossyScale;

            var fromPosition = parentPosition
                               + GetScaledPosition(parentScale, positionTween.FromPosition);
            var toPosition = parentPosition
                             + GetScaledPosition(parentScale, positionTween.ToPosition);
            Gizmos.DrawLine(fromPosition, toPosition);

            Gizmos.DrawSphere(fromPosition, GizmosSize);
            Gizmos.DrawSphere(toPosition, GizmosSize);
        }

        private static Vector3 GetScaledPosition(Vector3 scale, Vector3 position) =>
            new(position.x * scale.x,
                position.y * scale.y,
                position.z * scale.z);

        private static void DrawGlobalPosition(PositionTween positionTween)
        {
            Gizmos.DrawLine(positionTween.FromPosition, positionTween.ToPosition);

            Gizmos.DrawSphere(positionTween.FromPosition, GizmosSize);
            Gizmos.DrawSphere(positionTween.ToPosition, GizmosSize);
        }

        private static void DrawAnchoredPosition(PositionTween positionTween)
        {
            var parent = positionTween.TweenObject == null ||
                         positionTween.TweenObject.transform == null ||
                         positionTween.TweenObject.transform.parent == null
                ? null
                : positionTween.TweenObject.transform.parent;

            var parentPosition = parent == null ? Vector3.zero : parent.position;
            var parentScale = parent == null ? Vector3.one : parent.lossyScale;
            if (positionTween.RectTransform == null)
                positionTween.RectTransform = positionTween.tweenObject.transform as RectTransform;

            var rectTransform = positionTween.RectTransform;
            var difPosition = rectTransform.localPosition - rectTransform.anchoredPosition3D;
            var difScaled = GetScaledPosition(parentScale, difPosition);

            var fromPosition = parentPosition
                               + GetScaledPosition(parentScale, positionTween.FromPosition)
                               + difScaled;

            var toPosition = parentPosition +
                             GetScaledPosition(parentScale, positionTween.ToPosition)
                             + difScaled;

            Gizmos.DrawLine(fromPosition, toPosition);
            Gizmos.DrawSphere(fromPosition, GizmosSize);
            Gizmos.DrawSphere(toPosition, GizmosSize);
        }

        private static void DrawTargetPosition(PositionTween positionTween)
        {
            if (positionTween.fromTarget == null || positionTween.toTarget == null) return;
            var fromPosition = positionTween.fromTarget.transform.position;
            var toPosition = positionTween.toTarget.transform.position;
            Gizmos.DrawLine(fromPosition, toPosition);

            Gizmos.DrawSphere(fromPosition, GizmosSize);
            Gizmos.DrawSphere(toPosition, GizmosSize);
        }

        public override void OnGuiChange()
        {
            if (tweenObject != null) RectTransform = tweenObject.transform as RectTransform;
            base.OnGuiChange();
        }
#endif

        #endregion

        #region Static

        public static PositionTween Clone(
            PositionTween tween,
            GameObject targetObject = null)
        {
            var animationCurve = new AnimationCurve();
            animationCurve.CopyFrom(tween.AnimationCurve);

            return new PositionTween(
                targetObject,
                tween.StartDelay,
                tween.TweenTime,
                tween.Loop,
                animationCurve,
                tween.PositionType,
                tween.FromPosition,
                tween.ToPosition,
                tween.FromTarget,
                tween.ToTarget);
        }

        #endregion
    }
}