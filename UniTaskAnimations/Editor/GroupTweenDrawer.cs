using System;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Common.UniTaskAnimations.Editor
{
    [CustomPropertyDrawer(typeof(GroupTween), true)]
    public class GroupTweenDrawer : TweenDrawer
    {
        private float _currentSliderValue = -1f;
        
        public override void OnGUI(Rect rect, SerializedProperty property, GUIContent label)
        {
            base.OnGUI(rect, property, label);

            if (property.isExpanded)
            {
                DrawProgress(rect, property);
                DrawButtons(rect, property);
            }
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label) =>
            base.GetPropertyHeight(property, label) + LinesHeight + LinesHeight + Space;

        private void DrawProgress(Rect propertyRect, SerializedProperty property)
        {
            if (_currentSliderValue < 0f)
            {
                //TODO: get current time value
                _currentSliderValue = 0f;
            }
            
            var x = propertyRect.x;
            var y = propertyRect.yMax - LineHeight - LineHeight - Space;
            var progressWidth = propertyRect.width;
            var progressRect = new Rect(x, y, progressWidth, LineHeight);
            var sliderValue = EditorGUI.Slider(progressRect, _currentSliderValue, 0f, 1f);

            if (Math.Abs(_currentSliderValue - sliderValue) > 0.0001f)
            {
                _currentSliderValue = sliderValue;
                if (property.managedReferenceValue is GroupTween targetTween)
                {
                    foreach (var tween in targetTween.Tweens)
                    {
                        if (tween is not SimpleTween simpleTween) continue;
                        simpleTween.SetTimeValue(_currentSliderValue);
                    }
                }
            }
        }
        
        private void DrawButtons(Rect propertyRect, SerializedProperty property)
        {
            var buttonCount = 3;
            var buttonWidth = propertyRect.width / buttonCount;
            var x = propertyRect.x;
            var y = propertyRect.yMax - LineHeight - Space;

            var buttonRectFromComponents = new Rect(x, y, buttonWidth, LineHeight);
            if (GUI.Button(buttonRectFromComponents, "Make Tweens From Components"))
                MakeTweensFromComponents(property);

            var buttonXToComponents = x + buttonWidth;
            var buttonRectToComponents = new Rect(buttonXToComponents, y, buttonWidth, LineHeight);
            if (GUI.Button(buttonRectToComponents, "Make Tweens To Components"))
                MakeTweensToComponents(property);

            var buttonXFindComponents = buttonXToComponents + buttonWidth;
            var buttonRectFindComponents = new Rect(buttonXFindComponents, y, buttonWidth, LineHeight);
            if (GUI.Button(buttonRectFindComponents, "Find Components"))
                FindComponents(property);
        }
        
        private void MakeTweensFromComponents(SerializedProperty property)
        {
            if (property.managedReferenceValue is not GroupTween groupTween) return;
            foreach (var component in groupTween.Components)
            {
                if (component.Tween is not ITween iTween) continue;
                groupTween.Tweens.Add(iTween);
                Object.DestroyImmediate(component);
            }

            groupTween.Components.Clear();
        }

        private void MakeTweensToComponents(SerializedProperty property)
        {
            if (property.managedReferenceValue is not GroupTween groupTween) return;
            foreach (var tween in groupTween.Tweens)
            {
                if (tween is not SimpleTween simpleTween) continue;
                if (simpleTween.TweenObject == null) continue;
                var tweenComponent = simpleTween.TweenObject.AddComponent<TweenComponent>();
                tweenComponent.SetTween(simpleTween);
                groupTween.Components.Add(tweenComponent);
            }

            groupTween.Tweens.Clear();
        }

        private void FindComponents(SerializedProperty property)
        {
            if (property.managedReferenceValue is not GroupTween groupTween) return;
            if (property.serializedObject?.targetObject is not Component target) return;
            var components = target.GetComponentsInChildren<TweenComponent>();
            foreach (var component in components)
            {
                if (component.Tween is not ITween iTween) continue;
                if (groupTween.Components.Contains(component) || target == component) continue;
                groupTween.Components.Add(component);
            }
        }
    }
}