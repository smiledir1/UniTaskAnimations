using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Common.UniTaskAnimations.SimpleTweens
{
    [Serializable]
    public class MultiPositionTween : BasePositionTween
    {
        #region View

        [SerializeField]
        private MultiLineType lineType;

        [SerializeField]
        private List<Vector3> positions;

        [SerializeField]
        private List<Transform> targets;

        [SerializeField]
        [Range(0.001f, 0.5f)]
        private float precision = 0.05f;

        [SerializeField]
        [Range(0f, 1f)]
        private float alpha = 1f;

        #endregion /View

        #region Properties

        public MultiLineType LineType => lineType;
        public IReadOnlyList<Vector3> Positions => positions;
        public IReadOnlyList<Transform> Targets => targets;
        public float Precision => precision;
        internal List<Vector3> PointsPositions => positions;

        #endregion

        #region Cache

        private Vector3[] _curvePoints;
        private float[] _linesLens;

        #endregion

        #region Constructor

        public MultiPositionTween()
        {
            positionType = PositionType.Local;
            lineType = MultiLineType.Line;
            positions = new List<Vector3>();
            precision = 0.05f;
            CreatePoints();
        }

        public MultiPositionTween(
            GameObject tweenObject,
            float startDelay,
            float tweenTime,
            LoopType loop,
            AnimationCurve animationCurve,
            PositionType positionType,
            MultiLineType lineType,
            List<Vector3> positions,
            List<Transform> targets = null,
            float precision = 0.05f) :
            base(tweenObject,
                startDelay,
                tweenTime,
                loop,
                animationCurve)
        {
            if (precision <= 0f) throw new Exception("precision must be > 0");

            this.positionType = positionType;
            this.lineType = lineType;
            this.positions = positions;
            this.targets = targets;
            this.precision = precision;

            CreatePoints();
        }

        #endregion

        #region Animation

        public override void Initialize()
        {
            RectTransform = tweenObject.transform as RectTransform;
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
            Vector3 toPosition;
            AnimationCurve curve;
            var curTweenTime = tweenTime;
            if (Loop == LoopType.PingPong) curTweenTime /= 2;
            var time = 0f;
            var curLoop = true;

            if (reverse)
            {
                startPosition = _curvePoints[^1];
                toPosition = _curvePoints[0];
                curve = ReverseCurve;
            }
            else
            {
                startPosition = _curvePoints[0];
                toPosition = _curvePoints[^1];
                curve = AnimationCurve;
            }

            if (startFromCurrentValue)
            {
                var localPosition = GetCurrentPosition();

                var t2 = 0f;
                for (var i = 1; i < _curvePoints.Length; i++)
                {
                    var from = _curvePoints[i - 1];
                    var to = _curvePoints[i];
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
                    var fromLen = _linesLens[i - 1];
                    var toLen = _linesLens[i];
                    var ft = fromLen + (toLen - fromLen) * qt;
                    t2 = ft;
                }

                time = curTweenTime * t2;
            }

            while (curLoop)
            {
                GoToPosition(startPosition);
                var cur = reverse ? _linesLens.Length - 2 : 1;
                while (time < curTweenTime)
                {
                    time += GetDeltaTime();

                    var normalizeTime = time / curTweenTime;
                    var lerpTime = curve?.Evaluate(normalizeTime) ?? normalizeTime;

                    Vector3 startPoint;
                    Vector3 toPoint;
                    float startLen;
                    float endLen;

                    if (reverse)
                    {
                        lerpTime = 1f - lerpTime;
                        for (var i = cur; i >= 0; i--)
                        {
                            if (_linesLens[i] > lerpTime) continue;
                            cur = i;
                            break;
                        }

                        startPoint = _curvePoints[cur];
                        toPoint = _curvePoints[cur + 1];

                        startLen = _linesLens[cur];
                        endLen = _linesLens[cur + 1];
                    }
                    else
                    {
                        for (var i = cur; i < _linesLens.Length; i++)
                        {
                            if (_linesLens[i] < lerpTime) continue;
                            cur = i;
                            break;
                        }

                        startPoint = _curvePoints[cur - 1];
                        toPoint = _curvePoints[cur];

                        startLen = _linesLens[cur - 1];
                        endLen = _linesLens[cur];
                    }

                    var valueTime = (lerpTime - startLen) / (endLen - startLen);

                    var lerpValue = Vector3.LerpUnclamped(startPoint, toPoint, valueTime);
                    GoToPosition(lerpValue);
                    if (cancellationToken.IsCancellationRequested) return;
                    await UniTask.Yield();
                }

                if (cancellationToken.IsCancellationRequested) return;
                if (TweenObject == null) return;
                GoToPosition(toPosition);
                time -= curTweenTime;

                switch (Loop)
                {
                    case LoopType.Once:
                        curLoop = false;
                        break;

                    case LoopType.Loop:
                        break;

                    case LoopType.PingPong:
                        toPosition = startPosition;
                        startPosition = GetCurrentPosition();
                        reverse = !reverse;
                        break;
                }
            }
        }

        public override void ResetValues()
        {
            if (positions.Count < 2) return;
            RectTransform ??= tweenObject.transform as RectTransform;
            GoToPosition(positions[0]);
        }

        public override void EndValues()
        {
            if (positions.Count < 2) return;
            RectTransform ??= tweenObject.transform as RectTransform;
            GoToPosition(positions[^1]);
        }
        
        public override void SetTimeValue(float value)
        {
            RectTransform ??= tweenObject.transform as RectTransform;
            GoToValue(_curvePoints, _linesLens, AnimationCurve, value);
        }

        public void SetPositions(
            PositionType curPositionType,
            MultiLineType curLineType,
            List<Vector3> curPositions,
            float curPrecision)
        {
            positionType = curPositionType;
            lineType = curLineType;
            positions = curPositions;
            precision = curPrecision;
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
            if (positionType == PositionType.Target)
            {
                positions = new List<Vector3>(Targets.Count);
                for (var i = 0; i < Targets.Count; i++)
                {
                    if (Targets[i] == null) continue;
                    positions.Add(Targets[i].transform.position);
                }
            }

            switch (lineType)
            {
                case MultiLineType.Line:
                    CreateLinePoints();
                    break;
                case MultiLineType.CatmullRom:
                    CreateCatmullRomPoints();
                    break;
            }
        }

        private void CreateLinePoints()
        {
            var count = positions.Count;
            if (count < 2) return;

            _curvePoints = new Vector3[count];
            _linesLens = new float[count];

            var fullSqrPath = 0f;
            _curvePoints[0] = positions[0];
            _linesLens[0] = 0f;

            for (var i = 1; i < count; i++)
            {
                _curvePoints[i] = positions[i];
                var len = (_curvePoints[i] - _curvePoints[i - 1]).magnitude;
                _linesLens[i] = fullSqrPath + len;
                fullSqrPath += len;
            }

            for (var i = 0; i < count; i++)
            {
                _linesLens[i] /= fullSqrPath;
            }
        }

        private void CreateCatmullRomPoints()
        {
            var positionsCount = positions.Count;
            if (positionsCount < 2) return;

            var min = 0.0001f;
            var count = (int) ((1f - min) / precision) + 2;
            var curAlpha = alpha;

            var fullCount = (positionsCount - 1) * count;
            _curvePoints = new Vector3[fullCount];
            _linesLens = new float[fullCount];

            var lastPos = positionsCount - 1;
            var fullSqrPath = 0f;
            _curvePoints[0] = positions[0];
            _linesLens[0] = 0f;

            var p0 = positions[0] - positions[1];
            var p1 = positions[0];
            var p2 = positions[1];
            var p3 = positions[2];

            fullSqrPath = CountPoints(p0, p1, p2, p3,
                curAlpha, 1, count, fullSqrPath, 1);

            for (var i = 2; i < lastPos; i++)
            {
                p0 = positions[i - 2];
                p1 = positions[i - 1];
                p2 = positions[i];
                p3 = positions[i + 1];

                fullSqrPath = CountPoints(p0, p1, p2, p3,
                    curAlpha, i, count, fullSqrPath, 0);
            }

            p0 = positions[lastPos - 2];
            p1 = positions[lastPos - 1];
            p2 = positions[lastPos];
            p3 = positions[lastPos] + p2;

            fullSqrPath = CountPoints(p0, p1, p2, p3,
                curAlpha, lastPos, count, fullSqrPath, 0);

            for (var i = 0; i < fullCount; i++)
            {
                _linesLens[i] /= fullSqrPath;
            }
        }

        private float CountPoints(
            Vector2 p0,
            Vector2 p1,
            Vector2 p2,
            Vector2 p3,
            float curAlpha,
            int pointPos,
            int count,
            float fullSqrPath,
            int startArrayPos)
        {
            for (var j = startArrayPos; j < count; j++)
            {
                var arrayPos = count * (pointPos - 1) + j;
                var t = j * precision;
                t = t > 1 ? 1 : t;
                _curvePoints[arrayPos] = GetPoint(
                    p0, p1, p2, p3, t, curAlpha);
                var len = (_curvePoints[arrayPos] - _curvePoints[arrayPos - 1]).magnitude;
                _linesLens[arrayPos] = fullSqrPath + len;
                fullSqrPath += len;
            }

            return fullSqrPath;
        }

        public Vector2 GetPoint(
            Vector2 p0,
            Vector2 p1,
            Vector2 p2,
            Vector2 p3,
            float t,
            float curAlpha)
        {
            // calculate knots
            const float K0 = 0;
            var k1 = GetKnotInterval(p0, p1, curAlpha);
            var k2 = GetKnotInterval(p1, p2, curAlpha) + k1;
            var k3 = GetKnotInterval(p2, p3, curAlpha) + k2;

            // evaluate the point
            var u = Mathf.LerpUnclamped(k1, k2, t);
            var a1 = Remap(K0, k1, p0, p1, u);
            var a2 = Remap(k1, k2, p1, p2, u);
            var a3 = Remap(k2, k3, p2, p3, u);
            var b1 = Remap(K0, k2, a1, a2, u);
            var b2 = Remap(k1, k3, a2, a3, u);
            return Remap(k1, k2, b1, b2, u);
        }

        private static Vector2 Remap(float a, float b, Vector2 c,
            Vector2 d, float u) =>
            Vector2.LerpUnclamped(c, d, (u - a) / (b - a));

        private float GetKnotInterval(Vector2 a, Vector2 b, float curAlpha) =>
            Mathf.Pow(Vector2.SqrMagnitude(a - b), 0.5f * curAlpha);

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
            if (component.Tween is MultiPositionTween multiPositionTween)
            {
                Recalculate(multiPositionTween);
                DrawGizmos(multiPositionTween);
                return;
            }
            
            if (component.Tween is GroupTween groupTween)
            {
                foreach (var tween in groupTween.Tweens)
                {
                    if (tween is MultiPositionTween posTween)
                    {
                        DrawGizmos(posTween);
                    }
                }
            }
        }
        
        private static void Recalculate(MultiPositionTween multiPositionTween)
        {
            var curTime = (float) UnityEditor.EditorApplication.timeSinceStartup;
            if (Application.isPlaying) return;
            if (Math.Abs(curTime - _oldGenerateTime) < Settings.Instance.GizmosUpdateInterval) return;
            _oldGenerateTime = curTime;
            multiPositionTween.CreatePoints();
        }

        private static void DrawGizmos(MultiPositionTween multiPositionTween)
        {
            if (multiPositionTween.Precision < 0.001f) return;
            if (multiPositionTween.Positions.Count < 2) return;
            if (multiPositionTween._curvePoints == null) return;

            switch (multiPositionTween.PositionType)
            {
                case PositionType.Local:
                    DrawLocalPosition(multiPositionTween);
                    break;
                case PositionType.Global:
                    DrawGlobalPosition(multiPositionTween);
                    break;
                case PositionType.Anchored:
                    DrawAnchoredPosition(multiPositionTween);
                    break;
                case PositionType.Target:
                    DrawTargetPosition(multiPositionTween);
                    break;
            }
        }

        private static void DrawLocalPosition(MultiPositionTween multiPositionTween)
        {
            var parent = multiPositionTween.TweenObject == null ||
                         multiPositionTween.TweenObject.transform == null ||
                         multiPositionTween.TweenObject.transform.parent == null
                ? null
                : multiPositionTween.TweenObject.transform.parent;

            var parentPosition = parent == null ? Vector3.zero : parent.position;
            var parentScale = parent == null ? Vector3.one : parent.lossyScale;

            var count = multiPositionTween._curvePoints.Length;
            var lines = new Vector3[count];

            for (var i = 0; i < count; i++)
            {
                lines[i] = parentPosition
                           + GetScaledPosition(parentScale, multiPositionTween._curvePoints[i]);
            }

            Gizmos.color = Color.blue;
            for (var i = 0; i < multiPositionTween.Positions.Count; i++)
            {
                var point = multiPositionTween.Positions[i];
                var pointPosition = parentPosition
                                    + GetScaledPosition(parentScale, point);
                Gizmos.DrawSphere(pointPosition, 10f);
            }

            Gizmos.color = Color.magenta;
            Gizmos.DrawLineStrip(lines, false);
        }

        private static Vector3 GetScaledPosition(Vector3 scale, Vector3 position) =>
            new(position.x * scale.x,
                position.y * scale.y,
                position.z * scale.z);

        private static void DrawGlobalPosition(MultiPositionTween multiPositionTween)
        {
            Gizmos.color = Color.blue;
            for (var i = 0; i < multiPositionTween.Positions.Count; i++)
            {
                Gizmos.DrawSphere(multiPositionTween.Positions[i], Settings.Instance.GizmosSize);
            }

            Gizmos.color = Color.magenta;
            Gizmos.DrawLineStrip(multiPositionTween._curvePoints, false);
        }

        private static void DrawAnchoredPosition(MultiPositionTween multiPositionTween)
        {
            var parent = multiPositionTween.TweenObject == null ||
                         multiPositionTween.TweenObject.transform == null ||
                         multiPositionTween.TweenObject.transform.parent == null
                ? null
                : multiPositionTween.TweenObject.transform.parent;

            var parentPosition = parent == null ? Vector3.zero : parent.position;
            var parentScale = parent == null ? Vector3.one : parent.lossyScale;
            if (multiPositionTween.RectTransform == null)
                multiPositionTween.RectTransform = multiPositionTween.tweenObject.transform as RectTransform;
            var rectTransform = multiPositionTween.RectTransform;
            var difPosition = rectTransform.localPosition - rectTransform.anchoredPosition3D;
            var difScaled = GetScaledPosition(parentScale, difPosition);

            var count = multiPositionTween._curvePoints.Length;
            var lines = new Vector3[count];

            for (var i = 0; i < count; i++)
            {
                lines[i] = parentPosition
                           + GetScaledPosition(parentScale, multiPositionTween._curvePoints[i])
                           + difScaled;
            }

            Gizmos.color = Color.blue;
            for (var i = 0; i < multiPositionTween.Positions.Count; i++)
            {
                var point = multiPositionTween.Positions[i];
                var pointPosition = parentPosition
                                    + GetScaledPosition(parentScale, point)
                                    + difScaled;
                Gizmos.DrawSphere(pointPosition, Settings.Instance.GizmosSize);
            }

            Gizmos.color = Color.magenta;
            Gizmos.DrawLineStrip(lines, false);
        }

        private static void DrawTargetPosition(MultiPositionTween multiPositionTween)
        {
            if (multiPositionTween.Targets.Count < 2 ||
                multiPositionTween.Targets[0] == null ||
                multiPositionTween.Targets[^1] == null) return;

            Gizmos.color = Color.blue;
            Gizmos.DrawSphere(multiPositionTween.Targets[0].transform.position, Settings.Instance.GizmosSize);
            Gizmos.color = Color.red;
            for (var i = 1; i < multiPositionTween.Targets.Count - 1; i++)
            {
                if (multiPositionTween.Targets[i] == null) continue;
                Gizmos.DrawSphere(multiPositionTween.Targets[i].transform.position, Settings.Instance.GizmosSize);
            }

            Gizmos.color = Color.green;
            Gizmos.DrawSphere(multiPositionTween.Targets[^1].transform.position, Settings.Instance.GizmosSize);

            Gizmos.color = Color.magenta;
            Gizmos.DrawLineStrip(multiPositionTween._curvePoints, false);
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

        public static MultiPositionTween Clone(
            MultiPositionTween tween,
            GameObject targetObject = null)
        {
            var animationCurve = new AnimationCurve();
            animationCurve.CopyFrom(tween.AnimationCurve);

            var newPositions = new List<Vector3>(tween.Positions);
            var newTargets = new List<Transform>(tween.Targets);

            return new MultiPositionTween(
                targetObject,
                tween.StartDelay,
                tween.TweenTime,
                tween.Loop,
                animationCurve,
                tween.PositionType,
                tween.LineType,
                newPositions,
                newTargets,
                tween.Precision);
        }

        #endregion
    }

    public enum MultiLineType
    {
        Line,

        CatmullRom
        //TODO: bezier
    }
}