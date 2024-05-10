using UnityEngine;

namespace Common.UniTaskAnimations
{
    public interface ITween : IBaseTween
    {
        public static ITween Clone(ITween tween, GameObject targetObject = null)
        {
            ITween newTween = tween switch
            {
                GroupTween groupTween => GroupTween.Clone(groupTween, targetObject),
                SimpleTween simpleTween => SimpleTween.Clone(simpleTween, targetObject),
                _ => null
            };
            return newTween;
        }
    }
}