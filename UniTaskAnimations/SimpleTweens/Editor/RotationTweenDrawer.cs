using Common.UniTaskAnimations.Editor;
using UnityEditor;
using UnityEngine;

namespace Common.UniTaskAnimations.SimpleTweens.Editor
{
    [CustomPropertyDrawer(typeof(RotationTween), true)]
    public class RotationTweenDrawer : SimpleTweenDrawer
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

            var fromRotationRect = new Rect(x, y, partWidth, height);
            var fromRotationProperty = property.FindPropertyRelative("fromRotation");
            EditorGUI.PropertyField(fromRotationRect, fromRotationProperty);

            var buttonX = x + partWidth;
            var fromGoToButtonRect = new Rect(buttonX, y, buttonWidth, height);
            if (GUI.Button(fromGoToButtonRect, "Go To")) FromGotoRotation();

            var buttonX2 = buttonX + buttonWidth;
            var fromCopyButtonRect = new Rect(buttonX2, y, buttonWidth, height);
            if (GUI.Button(fromCopyButtonRect, "Copy From OBJ")) FromCopyRotation();
            y += height;

            var toRotationRect = new Rect(x, y, partWidth, height);
            var toRotationProperty = property.FindPropertyRelative("toRotation");
            EditorGUI.PropertyField(toRotationRect, toRotationProperty);

            var toGoToButtonRect = new Rect(buttonX, y, buttonWidth, height);
            if (GUI.Button(toGoToButtonRect, "Go To")) ToGotoRotation();

            var toCopyButtonRect = new Rect(buttonX2, y, buttonWidth, height);
            if (GUI.Button(toCopyButtonRect, "Copy From OBJ")) ToCopyRotation();
            y += height;

            return y - propertyRect.y;
        }

        private void FromGotoRotation()
        {
            if (TargetTween is not RotationTween rotationTween) return;
            TweenObject.transform.eulerAngles = rotationTween.FromRotation;
        }

        private void FromCopyRotation()
        {
            if (TargetTween is not RotationTween rotationTween) return;
            var rotation = TweenObject.transform.eulerAngles;
            rotationTween.SetRotation(rotation, rotationTween.ToRotation);
        }

        private void ToGotoRotation()
        {
            if (TargetTween is not RotationTween rotationTween) return;
            TweenObject.transform.eulerAngles = rotationTween.ToRotation;
        }

        private void ToCopyRotation()
        {
            if (TargetTween is not RotationTween rotationTween) return;
            var rotation = TweenObject.transform.eulerAngles;
            rotationTween.SetRotation(rotationTween.FromRotation, rotation);
        }
    }
}