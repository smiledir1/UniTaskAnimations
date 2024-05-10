#if UNITY_EDITOR
using Cysharp.Threading.Tasks;
using UnityEditor;
using UnityEngine;

namespace Common.UniTaskAnimations.Editor
{
    [CustomEditor(typeof(TweenComponent), true)]
    public class TweenComponentEditor : UnityEditor.Editor
    {
        private static TweenComponent _target;

        protected void OnEnable()
        {
            _target = target as TweenComponent;
        }

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            DrawAnimationButtons();
        }

        private void DrawAnimationButtons()
        {
            EditorGUILayout.BeginHorizontal();

            if (GUILayout.Button("Start (Reset) %#Z")) _target.Tween.StartAnimation().Forget();

            if (GUILayout.Button("Start (Current)"))
                _target.Tween.StartAnimation(false, true).Forget();

            if (GUILayout.Button("Start (Reverse)")) _target.Tween.StartAnimation(true).Forget();

            if (GUILayout.Button("Stop")) _target.Tween.StopAnimation().Forget();

            if (GUILayout.Button("Reset")) _target.Tween.ResetValues();

            if (GUILayout.Button("End")) _target.Tween.EndValues();

            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Start All (Reset)"))
            {
                var allAnimations = _target.GetComponentsInChildren<TweenComponent>();
                foreach (var animation in allAnimations)
                {
                    animation.Tween?.StartAnimation().Forget();
                }
            }

            if (GUILayout.Button("Stop All"))
            {
                var allAnimations = _target.GetComponentsInChildren<TweenComponent>();
                foreach (var animation in allAnimations)
                {
                    animation.Tween?.StopAnimation().Forget();
                }
            }

            if (GUILayout.Button("Reset All"))
            {
                var allAnimations = _target.GetComponentsInChildren<TweenComponent>();
                foreach (var animation in allAnimations)
                {
                    animation.Tween?.ResetValues();
                }
            }

            if (GUILayout.Button("End All"))
            {
                var allAnimations = _target.GetComponentsInChildren<TweenComponent>();
                foreach (var animation in allAnimations)
                {
                    animation.Tween?.EndValues();
                }
            }

            EditorGUILayout.EndHorizontal();
        }

        [MenuItem("Tools/Tween Animations/Start (Reset) Last Animation %#z")]
        private static void SpecialCommand()
        {
            _target.Tween.StartAnimation().Forget();
        }
    }
}
#endif