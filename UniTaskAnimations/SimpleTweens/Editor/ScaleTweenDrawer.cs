using Common.UniTaskAnimations.Editor;
using UnityEditor;
using UnityEngine;

namespace Common.UniTaskAnimations.SimpleTweens.Editor
{
    [CustomPropertyDrawer(typeof(ScaleTween), true)]
    public class ScaleTweenDrawer : SimpleTweenDrawer
    {
        protected override float DrawTweenProperties(
            Rect propertyRect,
            SerializedProperty property,
            GUIContent label)
        {
            var x = propertyRect.x;
            var y = propertyRect.y;
            var width = propertyRect.width;
            var height = LineHeight;

            var partWidth = width * 2 / 3;
            var buttonWidth = width / 6;

            var labelRect = new Rect(x, y, width, height);
            EditorGUI.LabelField(labelRect, "Current Tween", EditorStyles.boldLabel);
            y += height;

            var fromScaleRect = new Rect(x, y, partWidth, height);
            var fromScaleProperty = property.FindPropertyRelative("fromScale");
            EditorGUI.PropertyField(fromScaleRect, fromScaleProperty);

            var buttonX = x + partWidth;
            var fromGoToButtonRect = new Rect(buttonX, y, buttonWidth, height);
            if (GUI.Button(fromGoToButtonRect, "Go To")) FromGotoScale();

            var buttonX2 = buttonX + buttonWidth;
            var fromCopyButtonRect = new Rect(buttonX2, y, buttonWidth, height);
            if (GUI.Button(fromCopyButtonRect, "Copy From OBJ")) FromCopyScale();
            y += height;

            var toScaleRect = new Rect(x, y, partWidth, height);
            var toScaleProperty = property.FindPropertyRelative("toScale");
            EditorGUI.PropertyField(toScaleRect, toScaleProperty);

            var toGoToButtonRect = new Rect(buttonX, y, buttonWidth, height);
            if (GUI.Button(toGoToButtonRect, "Go To")) ToGotoScale();

            var toCopyButtonRect = new Rect(buttonX2, y, buttonWidth, height);
            if (GUI.Button(toCopyButtonRect, "Copy From OBJ")) ToCopyScale();
            y += height;

            return y - propertyRect.y;
        }

        private void FromGotoScale()
        {
            if (TargetTween is not ScaleTween scaleTween) return;
            TweenObject.transform.localScale = scaleTween.FromScale;
        }

        private void FromCopyScale()
        {
            if (TargetTween is not ScaleTween scaleTween) return;
            var scale = TweenObject.transform.localScale;
            scaleTween.SetScale(scale, scaleTween.ToScale);
        }

        private void ToGotoScale()
        {
            if (TargetTween is not ScaleTween scaleTween) return;
            TweenObject.transform.localScale = scaleTween.ToScale;
        }

        private void ToCopyScale()
        {
            if (TargetTween is not ScaleTween scaleTween) return;
            var scale = TweenObject.transform.localScale;
            scaleTween.SetScale(scaleTween.FromScale, scale);
        }
    }
}