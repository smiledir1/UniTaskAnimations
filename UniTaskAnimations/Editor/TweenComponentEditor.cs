#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEditor;
using UnityEngine;

namespace Common.UniTaskAnimations.Editor
{
    [CustomEditor(typeof(TweenComponent), true)]
    public class TweenComponentEditor : UnityEditor.Editor
    {
        private const string WindowsRectXNameKey = "WindowsRectX";
        private const string WindowsRectYNameKey = "WindowsRectY";

        private static TweenComponent _target;
        private static Rect _windowRect = new(10, 10, 200, 120);
        private float _currentSliderValue = -1f;

        protected void OnEnable()
        {
            _target = target as TweenComponent;

            _windowRect.x = EditorPrefs.GetFloat(WindowsRectXNameKey);
            _windowRect.y = EditorPrefs.GetFloat(WindowsRectYNameKey);
        }

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            DrawAnimationButtons();
        }

        private void OnSceneGUI()
        {
            Handles.BeginGUI();

            var newRect = GUILayout.Window(0, _windowRect, DrawWindow, "Tweens");
            if (newRect != _windowRect)
            {
                EditorPrefs.SetFloat(WindowsRectXNameKey, newRect.x);
                EditorPrefs.SetFloat(WindowsRectYNameKey, newRect.y);
            }

            _windowRect = newRect;

            Handles.EndGUI();
        }

        private void DrawWindow(int windowID)
        {
            EditorGUILayout.BeginHorizontal();

            if (GUILayout.Button("Start (Reset) %#Z")) _target.Tween.StartAnimation().Forget();

            if (GUILayout.Button("Start (Current)"))
                _target.Tween.StartAnimation(false, true).Forget();

            if (GUILayout.Button("Start (Reverse)")) _target.Tween.StartAnimation(true).Forget();

            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();

            if (GUILayout.Button("Stop")) _target.Tween.StopAnimation().Forget();

            if (GUILayout.Button("Reset")) _target.Tween.ResetValues();

            if (GUILayout.Button("End")) _target.Tween.EndValues();

            EditorGUILayout.EndHorizontal();

            // //TODO: to methods
            EditorGUILayout.BeginHorizontal();

            if (GUILayout.Button("Start All (Reset)")) StartAllReset();
            if (GUILayout.Button("Stop All")) StopAll();
            if (GUILayout.Button("Reset All")) ResetAll();
            if (GUILayout.Button("End All")) EndAll();

            EditorGUILayout.EndHorizontal();

            DrawProgress();

            GUI.DragWindow();
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

            if (GUILayout.Button("Start All (Reset)")) StartAllReset();
            if (GUILayout.Button("Stop All")) StopAll();
            if (GUILayout.Button("Reset All")) ResetAll();
            if (GUILayout.Button("End All")) EndAll();

            EditorGUILayout.EndHorizontal();
        }

        [MenuItem("Tools/Tween Animations/Start (Reset) Last Animation %#z")]
        private static void SpecialCommand()
        {
            _target.Tween.StartAnimation().Forget();
        }

        private void StartAllReset()
        {
            ResetAll();

            var allAnimations = _target.GetComponentsInChildren<TweenComponent>();
            foreach (var animation in allAnimations)
            {
                animation.Tween?.StartAnimation().Forget();
            }
        }

        private void StopAll()
        {
            var allAnimations = _target.GetComponentsInChildren<TweenComponent>();
            foreach (var animation in allAnimations)
            {
                animation.Tween?.StopAnimation().Forget();
            }
        }

        private void ResetAll()
        {
            var allAnimations = _target.GetComponentsInChildren<TweenComponent>();
            for (var i = allAnimations.Length - 1; i >= 0; i--)
            {
                var animation = allAnimations[i];
                animation.Tween?.ResetValues();
            }
        }

        private void EndAll()
        {
            var allAnimations = _target.GetComponentsInChildren<TweenComponent>();
            foreach (var animation in allAnimations)
            {
                animation.Tween?.EndValues();
            }
        }

        private void DrawProgress()
        {
            if (_currentSliderValue < 0f)
            {
                //TODO: get current time value
                _currentSliderValue = 0f;
            }

            // EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("CurrentValue");
            var sliderValue = EditorGUILayout.Slider(_currentSliderValue, 0f, 1f);

            if (Math.Abs(_currentSliderValue - sliderValue) > 0.0001f)
            {
                _currentSliderValue = sliderValue;
                if (_target.Tween is SimpleTween simpleTween)
                {
                    simpleTween.SetTimeValue(_currentSliderValue);
                }

                if (_target.Tween is GroupTween groupTween)
                {
                    SetTimeValueForGroup(groupTween, _currentSliderValue);
                }
            }

            //EditorGUILayout.EndHorizontal();
        }

        public static void SetTimeValueForGroup(GroupTween targetTween, float currentSliderValue)
        {
            var typeHash = new HashSet<Type>();
            targetTween.ResetValues();
            if (targetTween.Parallel)
            {
                var maxTime = 0f;
                foreach (var tween in targetTween.Tweens)
                {
                    if (maxTime < tween.Length) maxTime = tween.Length;
                }

                var currentTime = maxTime * currentSliderValue;
                foreach (var tween in targetTween.Tweens)
                {
                    var type = tween.GetType();
                    if (typeHash.Contains(type)) continue;
                    if (tween is GroupTween groupTween)
                    {
                        SetTimeValueForGroup(groupTween, currentSliderValue);
                    }

                    if (tween is SimpleTween simpleTween)
                    {
                        if (currentTime < simpleTween.StartDelay)
                        {
                            simpleTween.SetTimeValue(0f);
                            continue;
                        }

                        if (currentTime > tween.Length)
                        {
                            simpleTween.SetTimeValue(1f);
                            continue;
                        }

                        var currentValue = (currentTime - simpleTween.StartDelay) / simpleTween.TweenTime;
                        simpleTween.SetTimeValue(currentValue);
                        typeHash.Add(type);
                    }
                }
            }
            else
            {
                var maxTime = 0f;
                foreach (var tween in targetTween.Tweens)
                {
                    maxTime += tween.Length;
                }

                var subTime = 0f;
                var currentTime = maxTime * currentSliderValue;
                foreach (var tween in targetTween.Tweens)
                {
                    var type = tween.GetType();
                    if (typeHash.Contains(type)) continue;
                    
                    var startTweenTime = subTime;
                    subTime += tween.Length;

                    if (tween is GroupTween groupTween)
                    {
                        if (currentTime < startTweenTime)
                        {
                            SetTimeValueForGroup(groupTween, 0f);
                        }

                        if (currentTime > subTime)
                        {
                            SetTimeValueForGroup(groupTween, 1f);
                            continue;
                        }

                        var currentValue = (currentTime - groupTween.StartDelay - startTweenTime)
                                           / groupTween.TweenTime;
                        SetTimeValueForGroup(groupTween, currentValue);
                    }

                    if (tween is SimpleTween simpleTween)
                    {
                        if (currentTime < startTweenTime)
                        {
                            simpleTween.SetTimeValue(0f);
                            continue;
                        }

                        if (currentTime > subTime)
                        {
                            simpleTween.SetTimeValue(1f);
                            continue;
                        }

                        var currentValue = (currentTime - simpleTween.StartDelay - startTweenTime)
                                           / simpleTween.TweenTime;
                        simpleTween.SetTimeValue(currentValue);
                        typeHash.Add(type);
                    }
                }
            }
        }
    }
}
#endif