using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Common.UniTaskAnimations.SimpleTweens
{
    [Serializable]
    public class BezierPositionTween : BasePositionTween
    {
        #region View

        [SerializeField]
        private Vector3 fromPosition;

        [SerializeField]
        private Vector3 toPosition;

        [SerializeField]
        private Vector3 bezier1Offset;

        [SerializeField]
        private Vector3 bezier2Offset;

        [SerializeField]
        private Transform fromTarget;

        [SerializeField]
        private Transform toTarget;

        [SerializeField]
        private Transform bezier1OffsetTarget;

        [SerializeField]
        private Transform bezier2OffsetTarget;

        [SerializeField]
        [Range(0.001f, 0.5f)]
        private float precision = 0.05f;

        #endregion /View

        #region Properties

        public Vector3 FromPosition => fromPosition;
        public Vector3 ToPosition => toPosition;
        public Vector3 Bezier1Offset => bezier1Offset;
        public Vector3 Bezier2Offset => bezier2Offset;
        public float Precision => precision;
        public Transform FromTarget => fromTarget;
        public Transform ToTarget => toTarget;
        public Transform Bezier1OffsetTarget => bezier1OffsetTarget;
        public Transform Bezier2OffsetTarget => bezier2OffsetTarget;

        #endregion

        #region Cache

        private Vector3[] _bezierPoints;
        private float[] _bezierLens;

        #endregion

        #region Constructor

        public BezierPositionTween()
        {
            positionType = PositionType.Local;
            fromPosition = Vector3.zero;
            toPosition = Vector3.zero;
            bezier1Offset = Vector3.zero;
            bezier2Offset = Vector3.zero;
            precision = 0.05f;
            CreatePoints();
        }

        public BezierPositionTween(
            GameObject tweenObject,
            float startDelay,
            float tweenTime,
            LoopType loop,
            AnimationCurve animationCurve,
            PositionType positionType,
            Vector3 fromPosition,
            Vector3 toPosition,
            Vector3 bezier1Offset,
            Vector3 bezier2Offset,
            Transform fromTarget = null,
            Transform toTarget = null,
            Transform bezier1OffsetTarget = null,
            Transform bezier2OffsetTarget = null,
            float precision = 0.05f) :
            base(tweenObject,
                startDelay,
                tweenTime,
                loop,
                animationCurve)
        {
            if (precision <= 0f) throw new Exception("precision must be > 0");

            this.positionType = positionType;
            this.fromPosition = fromPosition;
            this.toPosition = toPosition;
            this.bezier1Offset = bezier1Offset;
            this.bezier2Offset = bezier2Offset;
            this.precision = precision;
            this.fromTarget = fromTarget;
            this.toTarget = toTarget;
            this.bezier1OffsetTarget = bezier1OffsetTarget;
            this.bezier2OffsetTarget = bezier2OffsetTarget;

            CreatePoints();
        }

        #endregion /Constructor

        #region Animation

        public override void Initialize()
        {
            CreatePoints();
            base.Initialize();
        }

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
            AnimationCurve reverseCurve;
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
                    reverseCurve = ReverseCurve;
                }
                else
                {
                    startPosition = fromTarget.transform.position;
                    endPosition = toTarget.transform.position;
                    reverseCurve = animationCurve;
                }
            }
            else
            {
                if (reverse)
                {
                    startPosition = toPosition;
                    endPosition = fromPosition;
                    reverseCurve = ReverseCurve;
                }
                else
                {
                    startPosition = fromPosition;
                    endPosition = toPosition;
                    reverseCurve = AnimationCurve;
                }
            }

            if (startFromCurrentValue)
            {
                var localPosition = GetCurrentPosition();

                var t2 = 0f;
                for (var i = 1; i < _bezierPoints.Length; i++)
                {
                    var from = _bezierPoints[i - 1];
                    var to = _bezierPoints[i];
                    var qtx = to.x - from.x == 0f
                        ? 0f
                        : (localPosition.x - from.x) / (to.x - from.x);
                    var qty = to.y - from.y == 0f
                        ? 0f
                        : (localPosition.y - from.y) / (to.y - from.y);
                    var qtz = to.z - from.z == 0f
                        ? 0f
                        : (localPosition.z - from.z) / (to.z - from.z);
                    var qt = qtx > qty
                        ? qtx > qtz
                            ? qtx
                            : qtz
                        : qty > qtz
                            ? qty
                            : qtz;
                    if (qtx is < 0 or > 1 ||
                        qty is < 0 or > 1 ||
                        qtz is < 0 or > 1) continue;
                    var fromLen = _bezierLens[i - 1];
                    var toLen = _bezierLens[i];
                    var ft = fromLen + (toLen - fromLen) * qt;
                    t2 = ft;
                }

                time = curTweenTime * t2;
            }

            while (curLoop)
            {
                GoToPosition(startPosition);
                var cur = reverse ? _bezierLens.Length - 2 : 1;
                while (time < curTweenTime)
                {
                    time += GetDeltaTime();

                    //TODO: gotoValue? optimal and not
                    var normalizeTime = time / curTweenTime;
                    var lerpTime = reverseCurve?.Evaluate(normalizeTime) ?? normalizeTime;

                    Vector3 startPoint;
                    Vector3 toPoint;
                    float startLen;
                    float endLen;

                    if (reverse)
                    {
                        lerpTime = 1f - lerpTime;
                        for (var i = cur; i >= 0; i--)
                        {
                            if (_bezierLens[i] > lerpTime) continue;
                            cur = i;
                            break;
                        }

                        startPoint = _bezierPoints[cur];
                        toPoint = _bezierPoints[cur + 1];

                        startLen = _bezierLens[cur];
                        endLen = _bezierLens[cur + 1];
                    }
                    else
                    {
                        for (var i = cur; i < _bezierLens.Length; i++)
                        {
                            if (_bezierLens[i] < lerpTime) continue;
                            cur = i;
                            break;
                        }

                        startPoint = _bezierPoints[cur - 1];
                        toPoint = _bezierPoints[cur];

                        startLen = _bezierLens[cur - 1];
                        endLen = _bezierLens[cur];
                    }

                    var valueTime = (lerpTime - startLen) / (endLen - startLen);

                    var lerpValue = Vector3.LerpUnclamped(startPoint, toPoint, valueTime);
                    GoToPosition(lerpValue);
                    if (cancellationToken.IsCancellationRequested) return;
                    await UniTask.Yield();
                }

                GoToPosition(endPosition);
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
                        reverse = !reverse;
                        break;
                }
            }
        }

        public override void ResetValues()
        {
            RectTransform ??= tweenObject.transform as RectTransform;
            GoToPosition(positionType != PositionType.Target ? fromPosition : fromTarget.position);
        }

        public override void EndValues()
        {
            RectTransform ??= tweenObject.transform as RectTransform;
            GoToPosition(positionType != PositionType.Target ? toPosition : toTarget.position);
        }

        public override void SetTimeValue(float value)
        {
            RectTransform ??= tweenObject.transform as RectTransform;
            GoToValue(_bezierPoints, _bezierLens, AnimationCurve, value);
        }

        public void SetPositions(
            PositionType curPositionType,
            Vector3 from,
            Vector3 to,
            Vector3 curBezier1Offset,
            Vector3 curBezier2Offset)
        {
            positionType = curPositionType;
            fromPosition = from;
            toPosition = to;
            bezier1Offset = curBezier1Offset;
            bezier2Offset = curBezier2Offset;
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

        private void GoToValue(Vector3[] points, float[] lens, AnimationCurve curve, float value)
        {
            if (points.Length < 2) return;
            var cur = 1;
            var lerpTime = curve?.Evaluate(value) ?? value;

            for (var i = cur; i < lens.Length; i++)
            {
                if (lens[i] < lerpTime) continue;
                cur = i;
                break;
            }

            var startPoint = points[cur - 1];
            var toPoint = points[cur];

            var startLen = lens[cur - 1];
            var endLen = lens[cur];


            var valueTime = (lerpTime - startLen) / (endLen - startLen);

            var lerpValue = Vector3.LerpUnclamped(startPoint, toPoint, valueTime);
            GoToPosition(lerpValue);
        }

        #endregion /Animation

        #region Private

        private void CreatePoints()
        {
            Vector3 b0;
            Vector3 b3;
            Vector3 b1;
            Vector3 b2;

            if (positionType == PositionType.Target)
            {
                if (FromTarget == null ||
                    ToTarget == null ||
                    Bezier1OffsetTarget == null ||
                    Bezier2OffsetTarget == null) return;

                b0 = FromTarget.transform.position;
                b3 = ToTarget.transform.position;

                b1 = Bezier1OffsetTarget.transform.position;
                b2 = Bezier2OffsetTarget.transform.position;
            }
            else
            {
                b0 = fromPosition;
                b3 = toPosition;

                b1 = b0 + bezier1Offset;
                b2 = b3 + bezier2Offset;
            }

            var min = 0.0001f;
            var count = (int) ((1f - min) / precision) + 2;

            _bezierPoints = new Vector3[count];
            _bezierLens = new float[count];

            _bezierPoints[0] = b0;
            _bezierLens[0] = 0f;

            var lastPos = count - 1;
            var fullSqrPath = 0f;
            float len;
            for (var i = 1; i < lastPos; i++)
            {
                var t = i * precision;
                _bezierPoints[i] = CalculatePointPosition(b0, b1, b2, b3, t);

                len = (_bezierPoints[i] - _bezierPoints[i - 1]).magnitude;
                _bezierLens[i] = fullSqrPath + len;
                fullSqrPath += len;
            }

            _bezierPoints[lastPos] = b3;
            len = (_bezierPoints[lastPos] - _bezierPoints[lastPos - 1]).magnitude;
            _bezierLens[lastPos] = fullSqrPath + len;
            fullSqrPath += len;


            for (var i = 0; i < count; i++)
            {
                _bezierLens[i] /= fullSqrPath;
            }
        }

        private static Vector3 CalculatePointPosition(
            Vector3 b0,
            Vector3 b1,
            Vector3 b2,
            Vector3 b3,
            float t) =>
            Mathf.Pow(1 - t, 3) * b0 +
            3 * Mathf.Pow(1 - t, 2) * t * b1 +
            3 * (1 - t) * Mathf.Pow(t, 2) * b2 +
            Mathf.Pow(t, 3) * b3;

        #endregion

        #region Editor

#if UNITY_EDITOR
        private static float _oldGenerateTime;

        private void OnEnable()
        {
            CreatePoints();
        }

        [UnityEditor.DrawGizmo(UnityEditor.GizmoType.NonSelected | UnityEditor.GizmoType.Selected)]
        private static void OnDrawGizmo(TweenComponent component, UnityEditor.GizmoType gizmoType)
        {
            if (component.Tween is BezierPositionTween bezierPositionTween)
            {
                Recalculate(bezierPositionTween);
                DrawGizmos(bezierPositionTween);
                return;
            }

            if (component.Tween is GroupTween groupTween)
            {
                foreach (var tween in groupTween.Tweens)
                {
                    if (tween is BezierPositionTween posTween)
                    {
                        DrawGizmos(posTween);
                    }
                }
            }
        }

        private static void Recalculate(BezierPositionTween bezierPositionTween)
        {
            var curTime = (float) UnityEditor.EditorApplication.timeSinceStartup;
            if (Application.isPlaying) return;
            if (Math.Abs(curTime - _oldGenerateTime) < Settings.Instance.GizmosUpdateInterval) return;
            _oldGenerateTime = curTime;
            bezierPositionTween.CreatePoints();
        }

        private static void DrawGizmos(BezierPositionTween bezierPositionTween)
        {
            if (bezierPositionTween.Precision < 0.001f) return;

            Gizmos.color = Color.magenta;

            switch (bezierPositionTween.PositionType)
            {
                case PositionType.Local:
                    DrawLocalPosition(bezierPositionTween);
                    break;
                case PositionType.Global:
                    DrawGlobalPosition(bezierPositionTween);
                    break;
                case PositionType.Anchored:
                    DrawAnchoredPosition(bezierPositionTween);
                    break;
                case PositionType.Target:
                    DrawTargetPosition(bezierPositionTween);
                    break;
            }
        }

        private static void DrawLocalPosition(BezierPositionTween bezierPositionTween)
        {
            var parent = bezierPositionTween.TweenObject == null ||
                         bezierPositionTween.TweenObject.transform == null ||
                         bezierPositionTween.TweenObject.transform.parent == null
                ? null
                : bezierPositionTween.TweenObject.transform.parent;

            var parentPosition = parent == null ? Vector3.zero : parent.position;
            var parentScale = parent == null ? Vector3.one : parent.lossyScale;

            var b0 = parentPosition
                     + GetScaledPosition(parentScale, bezierPositionTween.FromPosition);
            var b3 = parentPosition
                     + GetScaledPosition(parentScale, bezierPositionTween.ToPosition);

            var b1 = b0
                     + GetScaledPosition(parentScale, bezierPositionTween.Bezier1Offset);
            var b2 = b3
                     + GetScaledPosition(parentScale, bezierPositionTween.Bezier2Offset);

            var count = bezierPositionTween._bezierPoints.Length;
            var lines = new Vector3[count];

            for (var i = 0; i < count; i++)
            {
                lines[i] = parentPosition
                           + GetScaledPosition(parentScale, bezierPositionTween._bezierPoints[i]);
            }

            Gizmos.DrawSphere(b0, Settings.Instance.GizmosSize);
            Gizmos.DrawSphere(b1, Settings.Instance.GizmosSize);
            Gizmos.DrawSphere(b2, Settings.Instance.GizmosSize);
            Gizmos.DrawSphere(b3, Settings.Instance.GizmosSize);
            Gizmos.DrawLineStrip(lines, false);
        }

        private static Vector3 GetScaledPosition(Vector3 scale, Vector3 position) =>
            new(position.x * scale.x,
                position.y * scale.y,
                position.z * scale.z);

        private static void DrawGlobalPosition(BezierPositionTween bezierPositionTween)
        {
            var b0 = bezierPositionTween.FromPosition;
            var b3 = bezierPositionTween.ToPosition;

            var b1 = b0 + bezierPositionTween.Bezier1Offset;
            var b2 = b3 + bezierPositionTween.Bezier2Offset;

            Gizmos.DrawSphere(b0, Settings.Instance.GizmosSize);
            Gizmos.DrawSphere(b1, Settings.Instance.GizmosSize);
            Gizmos.DrawSphere(b2, Settings.Instance.GizmosSize);
            Gizmos.DrawSphere(b3, Settings.Instance.GizmosSize);
            Gizmos.DrawLineStrip(bezierPositionTween._bezierPoints, false);
        }

        private static void DrawAnchoredPosition(BezierPositionTween bezierPositionTween)
        {
            var parent = bezierPositionTween.TweenObject == null ||
                         bezierPositionTween.TweenObject.transform == null ||
                         bezierPositionTween.TweenObject.transform.parent == null
                ? null
                : bezierPositionTween.TweenObject.transform.parent;

            var parentPosition = parent == null ? Vector3.zero : parent.position;
            var parentScale = parent == null ? Vector3.one : parent.lossyScale;
            if (bezierPositionTween.RectTransform == null)
                bezierPositionTween.RectTransform = bezierPositionTween.tweenObject.transform as RectTransform;
            var rectTransform = bezierPositionTween.RectTransform;
            var difPosition = rectTransform.localPosition - rectTransform.anchoredPosition3D;
            var difScaled = GetScaledPosition(parentScale, difPosition);

            var b0 = parentPosition
                     + GetScaledPosition(parentScale, bezierPositionTween.FromPosition)
                     + difScaled;
            var b3 = parentPosition
                     + GetScaledPosition(parentScale, bezierPositionTween.ToPosition)
                     + difScaled;
            var b1 = b0
                     + GetScaledPosition(parentScale, bezierPositionTween.Bezier1Offset);
            var b2 = b3
                     + GetScaledPosition(parentScale, bezierPositionTween.Bezier2Offset);

            var count = bezierPositionTween._bezierPoints.Length;
            var lines = new Vector3[count];

            for (var i = 0; i < count; i++)
            {
                lines[i] = parentPosition
                           + GetScaledPosition(parentScale, bezierPositionTween._bezierPoints[i])
                           + difScaled;
            }

            Gizmos.DrawSphere(b0, Settings.Instance.GizmosSize);
            Gizmos.DrawSphere(b1, Settings.Instance.GizmosSize);
            Gizmos.DrawSphere(b2, Settings.Instance.GizmosSize);
            Gizmos.DrawSphere(b3, Settings.Instance.GizmosSize);
            Gizmos.DrawLineStrip(lines, false);
        }

        private static void DrawTargetPosition(BezierPositionTween bezierPositionTween)
        {
            if (bezierPositionTween.FromTarget == null ||
                bezierPositionTween.ToTarget == null ||
                bezierPositionTween.Bezier1OffsetTarget == null ||
                bezierPositionTween.Bezier2OffsetTarget == null) return;

            var fromPosition = bezierPositionTween.FromTarget.transform.position;
            var toPosition = bezierPositionTween.ToTarget.transform.position;
            var bezier1Offset = bezierPositionTween.Bezier1OffsetTarget.transform.position;
            var bezier2Offset = bezierPositionTween.Bezier2OffsetTarget.transform.position;

            var b0 = fromPosition;
            var b3 = toPosition;

            var b1 = bezier1Offset;
            var b2 = bezier2Offset;

            var currentGizmosColor = Gizmos.color;
            Gizmos.color = Color.blue;
            Gizmos.DrawSphere(b0, Settings.Instance.GizmosSize);
            Gizmos.color = Color.green;
            Gizmos.DrawSphere(b3, Settings.Instance.GizmosSize);
            Gizmos.color = Color.red;
            Gizmos.DrawSphere(b1, Settings.Instance.GizmosSize);
            Gizmos.DrawSphere(b2, Settings.Instance.GizmosSize);
            Gizmos.DrawLineStrip(bezierPositionTween._bezierPoints, false);
            Gizmos.color = currentGizmosColor;
        }

        public override void OnGuiChange()
        {
            RectTransform = tweenObject.transform as RectTransform;
            CreatePoints();
            base.OnGuiChange();
        }

#endif

        #endregion

        #region Static

        public static BezierPositionTween Clone(
            BezierPositionTween tween,
            GameObject targetObject = null)
        {
            var animationCurve = new AnimationCurve();
            animationCurve.CopyFrom(tween.AnimationCurve);

            return new BezierPositionTween(
                targetObject,
                tween.StartDelay,
                tween.TweenTime,
                tween.Loop,
                animationCurve,
                tween.PositionType,
                tween.FromPosition,
                tween.ToPosition,
                tween.Bezier1Offset,
                tween.Bezier2Offset,
                tween.FromTarget,
                tween.ToTarget,
                tween.Bezier1OffsetTarget,
                tween.Bezier2OffsetTarget,
                tween.Precision);
        }

        #endregion
    }
}