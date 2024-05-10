using System;
using System.Threading;
using Common.UniTaskAnimations.SimpleTweens;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Common.UniTaskAnimations
{
    [Serializable]
    public abstract class SimpleTween : ITween
    {
        #region View

        [SerializeField]
        protected GameObject tweenObject;

        [SerializeField]
        protected float startDelay;

        [SerializeField]
        protected float tweenTime;

        [SerializeField]
        protected LoopType loop;

        [SerializeField]
        protected AnimationCurve animationCurve;

        #endregion /View

        #region Cache

        protected CancellationTokenSource CancellationTokenSource;
#if UNITY_EDITOR
        protected double PrevTime;
#endif
        protected AnimationCurve ReverseCurve;
        protected bool _isInitialized;

        #endregion /Cache

        #region Abstract

        protected abstract UniTask Tween(
            bool reverse = false,
            bool startFromCurrentValue = false,
            CancellationToken cancellationToken = default);

        public abstract void ResetValues();

        public abstract void EndValues();

        #endregion /Abstract

        #region Constructor

        protected SimpleTween()
        {
            tweenObject = null;
            startDelay = 0f;
            tweenTime = 1f;
            loop = LoopType.Once;
            animationCurve = new AnimationCurve
            {
                keys = new[] {new Keyframe(0, 0), new Keyframe(1, 1)}
            };
        }

        protected SimpleTween(
            GameObject tweenObject,
            float startDelay,
            float tweenTime,
            LoopType loop,
            AnimationCurve animationCurve)
        {
            this.tweenObject = tweenObject;
            this.startDelay = startDelay;
            this.tweenTime = tweenTime;
            this.loop = loop;
            this.animationCurve = animationCurve;
        }

        #endregion /Constructor

        #region Animation

        public bool IsActiveAnimation => CancellationTokenSource != null;

        public GameObject TweenObject => tweenObject;

        public float StartDelay => startDelay;

        public float TweenTime => tweenTime;

        public LoopType Loop => loop;

        public AnimationCurve AnimationCurve => animationCurve;

        public bool IsInitialized => _isInitialized;

        public virtual void Initialize()
        {
            _isInitialized = true;
        }

        public async UniTask StartAnimation(
            bool reverse = false,
            bool startFromCurrentValue = false,
            CancellationToken cancellationToken = default)
        {
            await StopAnimation();
            CancellationTokenSource = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);

            if (!_isInitialized) Initialize();

            var lastKeyIndex = AnimationCurve.keys.Length - 1;
            //var lastKey = AnimationCurve.keys[lastKeyIndex];
            if (lastKeyIndex == -1)
                //     ||
                //     Math.Abs(lastKey.time - 1) > 0.01 ||
                //     Math.Abs(lastKey.value - 1) > 0.01)
                Debug.LogError("Wrong Curve");

            if (ReverseCurve == null && reverse && AnimationCurve != null)
            {
                ReverseCurve = new AnimationCurve();
                foreach (var k in AnimationCurve.keys)
                {
                    ReverseCurve.AddKey(new Keyframe(
                        1 - k.time,
                        1 - k.value,
                        k.inTangent,
                        k.outTangent));
                }
            }

            if (!startFromCurrentValue) ResetValues();

            await DelayAnimation(CancellationTokenSource.Token);
#if UNITY_EDITOR
            PrevTime = UnityEditor.EditorApplication.timeSinceStartup;
#endif
            await Tween(reverse, startFromCurrentValue, CancellationTokenSource.Token);
            CancellationTokenSource = null;
        }

        public async UniTask StopAnimation()
        {
            if (CancellationTokenSource != null)
            {
                CancellationTokenSource.Cancel();
                CancellationTokenSource = null;
                await UniTask.Yield();
            }
        }

        #endregion /Animation

        #region Protected

        protected async UniTask DelayAnimation(CancellationToken cancellationToken)
        {
            if (StartDelay > 0.001f)
                await UniTask.Delay(TimeSpan.FromSeconds(StartDelay), cancellationToken: cancellationToken);
        }

        protected float GetDeltaTime()
        {
#if UNITY_EDITOR
            if (!Application.isPlaying)
            {
                var timeSinceStartup = UnityEditor.EditorApplication.timeSinceStartup;
                var deltaTime = timeSinceStartup - PrevTime;
                PrevTime = timeSinceStartup;
                return (float) deltaTime;
            }
#endif
            return Time.deltaTime;
        }

        #endregion /Protected

        #region Static

        public static SimpleTween Clone(SimpleTween tween, GameObject targetObject = null)
        {
            if (tween == null) return null;
            var animationCurve = new AnimationCurve();
            animationCurve.CopyFrom(tween.AnimationCurve);

            SimpleTween newTween = tween switch
            {
                BezierPositionTween bezierPositionTween =>
                    BezierPositionTween.Clone(bezierPositionTween, targetObject),
                ColorImageTween colorImageTween =>
                    ColorImageTween.Clone(colorImageTween, targetObject),
                FillImageTween fillImageTween =>
                    FillImageTween.Clone(fillImageTween, targetObject),
                FrameByFrameTween frameByFrameTween =>
                    FrameByFrameTween.Clone(frameByFrameTween, targetObject),
                MultiPositionTween multiPositionTween =>
                    MultiPositionTween.Clone(multiPositionTween, targetObject),
                OffsetPositionTween offsetPositionTween =>
                    OffsetPositionTween.Clone(offsetPositionTween, targetObject),
                PositionTween positionTween =>
                    PositionTween.Clone(positionTween, targetObject),
                RotationTween rotationTween =>
                    RotationTween.Clone(rotationTween, targetObject),
                ScaleTween scaleTween =>
                    ScaleTween.Clone(scaleTween, targetObject),
                TransparencyCanvasGroupTween transparencyTween =>
                    TransparencyCanvasGroupTween.Clone(transparencyTween, targetObject),
                ColorSpriteRendererTween colorSpriteRendererTween =>
                    ColorSpriteRendererTween.Clone(colorSpriteRendererTween, targetObject),
                TransparencyColorImageTween transparencyColorImageTween =>
                    TransparencyColorImageTween.Clone(transparencyColorImageTween, targetObject),
                _ => null
            };

            return newTween;
        }

        #endregion /Static

        #region Editor

#if UNITY_EDITOR
        public static float GizmosSize = 10f;
        public virtual void OnGuiChange()
        {
            ReverseCurve = null;
        }
#endif

        #endregion
    }
}