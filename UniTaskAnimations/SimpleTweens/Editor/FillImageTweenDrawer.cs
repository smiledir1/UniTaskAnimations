using Common.UniTaskAnimations.Editor;
using UnityEditor;
using UnityEngine;

namespace Common.UniTaskAnimations.SimpleTweens.Editor
{
    [CustomPropertyDrawer(typeof(FillImageTween), true)]
    public class FillImageTweenDrawer : SimpleTweenDrawer
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

            var fromFillRect = new Rect(x, y, partWidth, height);
            var fromFillProperty = property.FindPropertyRelative("fromFill");
            EditorGUI.PropertyField(fromFillRect, fromFillProperty);

            var buttonX = x + partWidth;
            var fromGoToButtonRect = new Rect(buttonX, y, buttonWidth, height);
            if (GUI.Button(fromGoToButtonRect, "Go To")) FromGotoFill();

            var buttonX2 = buttonX + buttonWidth;
            var fromCopyButtonRect = new Rect(buttonX2, y, buttonWidth, height);
            if (GUI.Button(fromCopyButtonRect, "Copy From OBJ")) FromCopyFill();
            y += height;

            var toFillRect = new Rect(x, y, partWidth, height);
            var toFillProperty = property.FindPropertyRelative("toFill");
            EditorGUI.PropertyField(toFillRect, toFillProperty);

            var toGoToButtonRect = new Rect(buttonX, y, buttonWidth, height);
            if (GUI.Button(toGoToButtonRect, "Go To")) ToGotoFill();

            var toCopyButtonRect = new Rect(buttonX2, y, buttonWidth, height);
            if (GUI.Button(toCopyButtonRect, "Copy From OBJ")) ToCopyFill();
            y += height;

            var tweenGraphicRect = new Rect(x, y, width, height);
            var tweenGraphicProperty = property.FindPropertyRelative("tweenImage");
            EditorGUI.PropertyField(tweenGraphicRect, tweenGraphicProperty);
            y += height;

            return y - propertyRect.y;
        }

        private void FromGotoFill()
        {
            if (TargetTween is not FillImageTween fillImageTween) return;
            fillImageTween.TweenImage.fillAmount = fillImageTween.FromFill;
        }

        private void FromCopyFill()
        {
            if (TargetTween is not FillImageTween fillImageTween) return;
            var fill = fillImageTween.TweenImage.fillAmount;
            fillImageTween.SetFill(fill, fillImageTween.ToFill);
        }

        private void ToGotoFill()
        {
            if (TargetTween is not FillImageTween fillImageTween) return;
            fillImageTween.TweenImage.fillAmount = fillImageTween.ToFill;
        }

        private void ToCopyFill()
        {
            if (TargetTween is not FillImageTween fillImageTween) return;
            var fill = fillImageTween.TweenImage.fillAmount;
            fillImageTween.SetFill(fillImageTween.FromFill, fill);
        }
    }
}