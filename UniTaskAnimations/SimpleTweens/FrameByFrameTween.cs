using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

namespace Common.UniTaskAnimations.SimpleTweens
{
    [Serializable]
    public class FrameByFrameTween : SimpleTween
    {
        #region View

        [SerializeField]
        private Image tweenImage;

        [SerializeField]
        private List<Sprite> sprites;

        #endregion /View

        #region Properties

        public Image TweenImage => tweenImage;
        public List<Sprite> Sprites => sprites;

        #endregion

        #region Constructor

        public FrameByFrameTween()
        {
            sprites = new List<Sprite>();
        }

        public FrameByFrameTween(
            GameObject tweenObject,
            float startDelay,
            float tweenTime,
            LoopType loop,
            AnimationCurve animationCurve,
            Image tweenImage,
            List<Sprite> sprites) :
            base(tweenObject,
                startDelay,
                tweenTime,
                loop,
                animationCurve)
        {
            this.tweenImage = tweenImage;
            this.sprites = sprites;
        }

        #endregion /Constructor

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

            if (sprites.Count == 0) return;

            int startSprite;
            int toSprite;
            AnimationCurve curve;
            var curTweenTime = TweenTime;
            if (Loop == LoopType.PingPong) curTweenTime /= 2;
            var time = 0f;
            var curLoop = true;

            if (reverse)
            {
                startSprite = sprites.Count - 1;
                toSprite = 0;
                curve = ReverseCurve;
            }
            else
            {
                startSprite = 0;
                toSprite = sprites.Count - 1;
                curve = AnimationCurve;
            }

            if (startFromCurrentValue)
            {
                var currentPosition = GetImageSpritePosition();
                var t = (currentPosition - startSprite) / (toSprite - startSprite);
                time = curTweenTime * t;
            }

            while (curLoop)
            {
                tweenImage.sprite = sprites[startSprite];

                while (time < curTweenTime)
                {
                    time += GetDeltaTime();

                    var normalizeTime = time / curTweenTime;
                    GoToValue(startSprite, toSprite, curve, normalizeTime);
                    if (cancellationToken.IsCancellationRequested) return;
                    await UniTask.Yield();
                }

                tweenImage.sprite = sprites[toSprite];

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
                        toSprite = startSprite;
                        startSprite = GetImageSpritePosition();
                        break;
                }
            }
        }

        public override void ResetValues()
        {
            if (sprites == null || sprites.Count == 0) return;
            tweenImage.sprite = sprites[0];
        }

        public override void EndValues()
        {
            if (sprites == null || sprites.Count == 0) return;
            tweenImage.sprite = sprites[^1];
        }
        
        public override void SetTimeValue(float value)
        {
            if (sprites == null || sprites.Count == 0) return;
            GoToValue(0, Sprites.Count - 1, AnimationCurve, value);
        }

        public void SetSprites(List<Sprite> mainSprites)
        {
            sprites = mainSprites;
        }

        private void GoToValue(int startSprite, int endSprite, AnimationCurve curve, float value)
        {
            var lerpTime = curve?.Evaluate(value) ?? value;
            var lerpValue = Mathf.LerpUnclamped(startSprite, endSprite, lerpTime);

            if (tweenImage == null) return;
            var currentSpritePos = (int) (endSprite > startSprite
                ? Mathf.Ceil(lerpValue)
                : Mathf.Floor(lerpValue));
            tweenImage.sprite = sprites[currentSpritePos];
        }

        #endregion /Animation

        #region Static

        public static FrameByFrameTween Clone(
            FrameByFrameTween tween,
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

            return new FrameByFrameTween(
                targetObject,
                tween.StartDelay,
                tween.TweenTime,
                tween.Loop,
                animationCurve,
                tweenImage,
                tween.Sprites);
        }

        #endregion

        #region Private

        private int GetImageSpritePosition()
        {
            if (tweenImage == null || tweenImage.sprite == null) return 0;
            var imageSprite = tweenImage.sprite;
            for (var i = 0; i < sprites.Count; i++)
            {
                var sprite = sprites[i];
                if (sprite != imageSprite) continue;
                return i;
            }

            return 0;
        }

        #endregion
    }
}