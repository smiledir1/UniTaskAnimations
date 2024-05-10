using Common.UniTaskAnimations.Editor;
using UnityEditor;
using UnityEngine;

namespace Common.UniTaskAnimations.SimpleTweens.Editor
{
    [CustomPropertyDrawer(typeof(FrameByFrameTween), true)]
    public class FrameByFrameTweenDrawer : SimpleTweenDrawer
    {
        private string _textFieldValue;
        private int _framesCount;
        private bool _expand;

        protected override float DrawTweenProperties(
            Rect propertyRect,
            SerializedProperty property,
            GUIContent label)
        {
            var x = propertyRect.x;
            var y = propertyRect.y;
            var width = propertyRect.width;
            var height = LineHeight;

            y += DrawTweenProperties(propertyRect, property);

            var calculateRect = new Rect(x, y, width, height);
            y += DrawCalculate(calculateRect, property);

            return y - propertyRect.y;
        }

        private float DrawTweenProperties(Rect propertyRect, SerializedProperty property)
        {
            var x = propertyRect.x;
            var y = propertyRect.y;
            var width = propertyRect.width;
            var height = LineHeight;

            var labelRect = new Rect(x, y, width, height);
            EditorGUI.LabelField(labelRect, "Current Tween", EditorStyles.boldLabel);
            y += height;

            var tweenImageRect = new Rect(x, y, width, height);
            var tweenImageProperty = property.FindPropertyRelative("tweenImage");
            EditorGUI.PropertyField(tweenImageRect, tweenImageProperty);
            y += height;

            var spritesRect = new Rect(x, y, width, height);
            var spritesProperty = property.FindPropertyRelative("sprites");
            EditorGUI.PropertyField(spritesRect, spritesProperty);
            y += EditorGUI.GetPropertyHeight(spritesProperty);

            return y - propertyRect.y;
        }

        private float DrawCalculate(
            Rect propertyRect,
            SerializedProperty property)
        {
            var width = propertyRect.width / 3;
            var x = propertyRect.x;
            var y = propertyRect.y;

            var labelRect = new Rect(x, y, width, LineHeight);
            GUI.Label(labelRect, "time to: frames in second (1 second)");

            x += width;
            var textFieldRect = new Rect(x, y, width, LineHeight);
            _textFieldValue = GUI.TextField(textFieldRect, _textFieldValue);
            if (!int.TryParse(_textFieldValue, out var value) ||
                value < 1)
            {
                _textFieldValue = "1";
                _framesCount = 1;
            }
            else
            {
                _framesCount = value;
            }

            x += width;
            var buttonRect = new Rect(x, y, width, LineHeight);
            if (GUI.Button(buttonRect, "Calculate"))
                if (property.managedReferenceValue is FrameByFrameTween currentTween)
                {
                    var newTime = currentTween.Sprites.Count / (float) _framesCount;
                    property.managedReferenceValue = new FrameByFrameTween(
                        currentTween.TweenObject,
                        currentTween.StartDelay,
                        newTime,
                        currentTween.Loop,
                        currentTween.AnimationCurve,
                        currentTween.TweenImage,
                        currentTween.Sprites);
                }

            return LineHeight;
        }
    }
}