using Common.UniTaskAnimations.Editor;
using UnityEditor;
using UnityEngine;

namespace Common.UniTaskAnimations.SimpleTweens.Editor
{
    [CustomPropertyDrawer(typeof(TransparencyColorSpriteRendererTween), true)]
    public class TransparencyColorSpriteRendererTweenDrawer : SimpleTweenDrawer
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

            var fromOpacityRect = new Rect(x, y, partWidth, height);
            var fromOpacityProperty = property.FindPropertyRelative("fromOpacity");
            EditorGUI.PropertyField(fromOpacityRect, fromOpacityProperty);

            var buttonX = x + partWidth;
            var fromGoToButtonRect = new Rect(buttonX, y, buttonWidth, height);
            if (GUI.Button(fromGoToButtonRect, "Go To")) FromGotoOpacity();

            var buttonX2 = buttonX + buttonWidth;
            var fromCopyButtonRect = new Rect(buttonX2, y, buttonWidth, height);
            if (GUI.Button(fromCopyButtonRect, "Copy From OBJ")) FromCopyOpacity();
            y += height;

            var toOpacityRect = new Rect(x, y, partWidth, height);
            var toOpacityProperty = property.FindPropertyRelative("toOpacity");
            EditorGUI.PropertyField(toOpacityRect, toOpacityProperty);

            var toGoToButtonRect = new Rect(buttonX, y, buttonWidth, height);
            if (GUI.Button(toGoToButtonRect, "Go To")) ToGotoOpacity();

            var toCopyButtonRect = new Rect(buttonX2, y, buttonWidth, height);
            if (GUI.Button(toCopyButtonRect, "Copy From OBJ")) ToCopyOpacity();
            y += height;

            var tweenGraphicRect = new Rect(x, y, width, height);
            var tweenGraphicProperty = property.FindPropertyRelative("tweenGraphic");
            EditorGUI.PropertyField(tweenGraphicRect, tweenGraphicProperty);
            y += height;

            return y - propertyRect.y;
        }
         
        protected override float DrawTweenPropertiesHeight(SerializedProperty property) => LineHeight * 4;

        private void FromGotoOpacity()
        {
            if (TargetTween is not TransparencyColorSpriteRendererTween tween) return;
            tween.TweenObjectRenderer.color = GetColorWithAlpha(
                tween.TweenObjectRenderer, 
                tween.FromOpacity);
        }

        private void FromCopyOpacity()
        {
            if (TargetTween is not TransparencyColorSpriteRendererTween tween) return;
            var opacity = tween.TweenObjectRenderer.color.a;
            tween.SetTransparency(opacity, tween.ToOpacity);
        }

        private void ToGotoOpacity()
        {
            if (TargetTween is not TransparencyColorSpriteRendererTween tween) return;
            tween.TweenObjectRenderer.color = GetColorWithAlpha(
                    tween.TweenObjectRenderer, 
                    tween.ToOpacity);
        }

        private void ToCopyOpacity()
        {
            if (TargetTween is not TransparencyColorSpriteRendererTween tween) return;
            var opacity = tween.TweenObjectRenderer.color.a;
            tween.SetTransparency(tween.FromOpacity, opacity);
        }
        
        private Color GetColorWithAlpha(SpriteRenderer tweenGraphic, float alpha)
        {
            var color = tweenGraphic.color;
            return new Color(color.r, color.g, color.b, alpha);
        }
    }
}