using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Common.UniTaskAnimations
{
    public class TweenComponent : MonoBehaviour
    {
        [Header("Component")]
        [SerializeField]
        private bool startOnAwake;
        
        [SerializeField]
        private bool startOnEnable;

        [SerializeField]
        [SerializeReference]
        private IBaseTween _tween;

        public IBaseTween Tween => _tween;

        internal void SetTween(IBaseTween tween)
        {
            _tween = tween;
        }

        #region Unity Life Cycle

        protected void Start()
        {
            if (startOnAwake) _tween.StartAnimation().Forget();
        }
        
        protected void OnEnable()
        {
            if (startOnEnable && !startOnAwake) _tween.StartAnimation().Forget();
        }

        #endregion /Unity Life Cycle

        public void PlayAnimation()
        {
            _tween.StartAnimation().Forget();
        }
    }
}