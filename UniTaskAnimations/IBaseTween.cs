using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Common.UniTaskAnimations
{
    public interface IBaseTween
    {
        public bool IsActiveAnimation { get; }
        UniTask StartAnimation(
            bool reverse = false,
            bool startFromCurrentValue = false,
            CancellationToken cancellationToken = default);
        UniTask StopAnimation();
        void ResetValues();
        void EndValues();

        public static IBaseTween Clone(IBaseTween tween, GameObject targetObject = null)
        {
            IBaseTween newTween = tween switch
            {
                GroupTween groupTween => GroupTween.Clone(groupTween, targetObject),
                SimpleTween simpleTween => SimpleTween.Clone(simpleTween, targetObject),
                MultiTween multiTween => MultiTween.Clone(multiTween, targetObject),
                _ => null
            };
            return newTween;
        }

        #region Editor

#if UNITY_EDITOR
        void OnGuiChange()
        {
        }
#endif

        #endregion
    }
}