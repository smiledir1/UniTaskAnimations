using System;
using UnityEngine;

namespace Common.UniTaskAnimations.SimpleTweens
{
    [Serializable]
    public abstract class BasePositionTween : SimpleTween
    {
        #region View

        [SerializeField]
        protected PositionType positionType;

        #endregion /View

        #region Properties

        public PositionType PositionType => positionType;

        #endregion /Properties

        #region Cache

        protected RectTransform RectTransform;

        protected BasePositionTween()
        {
        }

        protected BasePositionTween(
            GameObject tweenObject,
            float startDelay,
            float tweenTime,
            LoopType loop,
            AnimationCurve animationCurve) :
            base(tweenObject,
                startDelay,
                tweenTime,
                loop,
                animationCurve)
        {
        }

        #endregion

        public override void Initialize()
        {
            if (tweenObject != null) RectTransform = tweenObject.transform as RectTransform;
            base.Initialize();
        }

        #region Animation

        internal void GoToPosition(Vector3 position)
        {
            if (tweenObject == null || tweenObject.transform == null) return;
            switch (positionType)
            {
                case PositionType.Local:
                    tweenObject.transform.localPosition = position;
                    return;
                case PositionType.Global:
                    tweenObject.transform.position = position;
                    return;
                case PositionType.Anchored:
                    RectTransform.anchoredPosition = position;
                    return;
                case PositionType.Target:
                    tweenObject.transform.position = position;
                    return;
            }
        }

        #endregion
    }

    public enum PositionType
    {
        Local,
        Global,

        /// <summary>
        /// Only for UI elements
        /// </summary>
        Anchored,
        Target
    }
}