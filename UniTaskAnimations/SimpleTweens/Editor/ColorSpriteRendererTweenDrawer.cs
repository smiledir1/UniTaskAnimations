using Common.UniTaskAnimations.Editor;
using UnityEditor;
using UnityEngine;

namespace Common.UniTaskAnimations.SimpleTweens.Editor
{
    [CustomPropertyDrawer(typeof(ColorSpriteRendererTween), true)]
    public class ColorSpriteRendererTweenDrawer : SimpleTweenDrawer
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

            var fromColorRect = new Rect(x, y, partWidth, height);
            var fromColorProperty = property.FindPropertyRelative("fromColor");
            EditorGUI.PropertyField(fromColorRect, fromColorProperty);

            var buttonX = x + partWidth;
            var fromGoToButtonRect = new Rect(buttonX, y, buttonWidth, height);
            if (GUI.Button(fromGoToButtonRect, "Go To")) FromGotoColor();

            var buttonX2 = buttonX + buttonWidth;
            var fromCopyButtonRect = new Rect(buttonX2, y, buttonWidth, height);
            if (GUI.Button(fromCopyButtonRect, "Copy From OBJ")) FromCopyColor();
            y += height;

            var toColorRect = new Rect(x, y, partWidth, height);
            var toColorProperty = property.FindPropertyRelative("toColor");
            EditorGUI.PropertyField(toColorRect, toColorProperty);

            var toGoToButtonRect = new Rect(buttonX, y, buttonWidth, height);
            if (GUI.Button(toGoToButtonRect, "Go To")) ToGotoColor();

            var toCopyButtonRect = new Rect(buttonX2, y, buttonWidth, height);
            if (GUI.Button(toCopyButtonRect, "Copy From OBJ")) ToCopyColor();
            y += height;
            
            var ignoreAlphaRect = new Rect(x, y, partWidth, height);
            var ignoreAlphaProperty = property.FindPropertyRelative("ignoreAlpha");
            EditorGUI.PropertyField(ignoreAlphaRect, ignoreAlphaProperty);
            y += height;

            var tweenGraphicRect = new Rect(x, y, width, height);
            var tweenGraphicProperty = property.FindPropertyRelative("tweenGraphic");
            EditorGUI.PropertyField(tweenGraphicRect, tweenGraphicProperty);
            y += height;

            return y - propertyRect.y;
        }
         
        protected override float DrawTweenPropertiesHeight(SerializedProperty property) => LineHeight * 5;

        private void FromGotoColor()
        {
            if (TargetTween is not ColorSpriteRendererTween colorImageTween) return;
            colorImageTween.TweenGraphic.color = colorImageTween.FromColor;
        }

        private void FromCopyColor()
        {
            if (TargetTween is not ColorSpriteRendererTween colorImageTween) return;
            var color = colorImageTween.TweenGraphic.color;
            colorImageTween.SetColor(color, colorImageTween.ToColor);
        }

        private void ToGotoColor()
        {
            if (TargetTween is not ColorSpriteRendererTween colorImageTween) return;
            colorImageTween.TweenGraphic.color = colorImageTween.ToColor;
        }

        private void ToCopyColor()
        {
            if (TargetTween is not ColorSpriteRendererTween colorImageTween) return;
            var color = colorImageTween.TweenGraphic.color;
            colorImageTween.SetColor(colorImageTween.FromColor, color);
        }
    }
}