﻿using Common.UniTaskAnimations.Editor;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace Common.UniTaskAnimations.SimpleTweens.Editor
{
    [CustomPropertyDrawer(typeof(TransparencyColorImageTween), true)]
    public class TransparencyColorImageTweenDrawer : SimpleTweenDrawer
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

        private void FromGotoOpacity()
        {
            if (TargetTween is not TransparencyColorImageTween transparencyCanvasGroupTween) return;
            transparencyCanvasGroupTween.TweenObjectRenderer.color = GetColorWithAlpha(
                transparencyCanvasGroupTween.TweenObjectRenderer, 
                transparencyCanvasGroupTween.FromOpacity);
        }

        private void FromCopyOpacity()
        {
            if (TargetTween is not TransparencyColorImageTween transparencyCanvasGroupTween) return;
            var opacity = transparencyCanvasGroupTween.TweenObjectRenderer.color.a;
            transparencyCanvasGroupTween.SetTransparency(opacity, transparencyCanvasGroupTween.ToOpacity);
        }

        private void ToGotoOpacity()
        {
            if (TargetTween is not TransparencyColorImageTween transparencyCanvasGroupTween) return;
            transparencyCanvasGroupTween.TweenObjectRenderer.color = GetColorWithAlpha(
                    transparencyCanvasGroupTween.TweenObjectRenderer, 
                    transparencyCanvasGroupTween.ToOpacity);
        }

        private void ToCopyOpacity()
        {
            if (TargetTween is not TransparencyColorImageTween transparencyCanvasGroupTween) return;
            var opacity = transparencyCanvasGroupTween.TweenObjectRenderer.color.a;
            transparencyCanvasGroupTween.SetTransparency(transparencyCanvasGroupTween.FromOpacity, opacity);
        }
        
        private Color GetColorWithAlpha(Graphic tweenGraphic, float alpha)
        {
            var color = tweenGraphic.color;
            return new Color(color.r, color.g, color.b, alpha);
        }
    }
}