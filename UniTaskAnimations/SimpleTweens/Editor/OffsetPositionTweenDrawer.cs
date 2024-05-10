using Common.UniTaskAnimations.Editor;
using UnityEditor;
using UnityEngine;

namespace Common.UniTaskAnimations.SimpleTweens.Editor
{
    [CustomPropertyDrawer(typeof(OffsetPositionTween), true)]
    public class OffsetPositionTweenDrawer : SimpleTweenDrawer
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

            var labelRect = new Rect(x, y, width, height);
            EditorGUI.LabelField(labelRect, "Current Tween", EditorStyles.boldLabel);
            y += height;

            var fromPositionRect = new Rect(x, y, width, height);
            var fromPositionProperty = property.FindPropertyRelative("fromPosition");
            EditorGUI.PropertyField(fromPositionRect, fromPositionProperty);
            y += height;

            var toPositionRect = new Rect(x, y, width, height);
            var toPositionProperty = property.FindPropertyRelative("toPosition");
            EditorGUI.PropertyField(toPositionRect, toPositionProperty);
            y += height;

            return y - propertyRect.y;
        }
    }
}