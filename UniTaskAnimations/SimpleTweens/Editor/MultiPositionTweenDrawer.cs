using Common.UniTaskAnimations.Editor;
using UnityEditor;
using UnityEngine;


namespace Common.UniTaskAnimations.SimpleTweens.Editor
{
    [CustomPropertyDrawer(typeof(MultiPositionTween), true)]
    public class MultiPositionTweenDrawer : SimpleTweenDrawer
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

            var positionTypeRect = new Rect(x, y, width, height);
            var positionTypeProperty = property.FindPropertyRelative("positionType");
            EditorGUI.PropertyField(positionTypeRect, positionTypeProperty);
            y += height;

            var lineTypeRect = new Rect(x, y, width, height);
            var lineTypeProperty = property.FindPropertyRelative("lineType");
            EditorGUI.PropertyField(lineTypeRect, lineTypeProperty);
            y += height;

            if (positionTypeProperty.intValue == (int) PositionType.Target)
            {
                var positionsRect = new Rect(x, y, width, height);
                var positionsProperty = property.FindPropertyRelative("targets");
                EditorGUI.PropertyField(positionsRect, positionsProperty);
                y += EditorGUI.GetPropertyHeight(positionsProperty);
            }
            else
            {
                var positionsRect = new Rect(x, y, width, height);
                var positionsProperty = property.FindPropertyRelative("positions");
                EditorGUI.PropertyField(positionsRect, positionsProperty);
                y += EditorGUI.GetPropertyHeight(positionsProperty);

                var addCurrentPositionButtonRect = new Rect(x, y, width, height);
                if (GUI.Button(addCurrentPositionButtonRect, "Add Current Position")) AddCurrentPosition();
                y += height;
            }

            if (TargetTween is MultiPositionTween multiPositionTween &&
                multiPositionTween.LineType != MultiLineType.Line)
            {
                var precisionRect = new Rect(x, y, width, height);
                var precisionProperty = property.FindPropertyRelative("precision");
                EditorGUI.PropertyField(precisionRect, precisionProperty);
                y += height;

                var alphaRect = new Rect(x, y, width, height);
                var alphaProperty = property.FindPropertyRelative("alpha");
                EditorGUI.PropertyField(alphaRect, alphaProperty);
                y += height;
            }

            var gizmosHelperRect = new Rect(x, y, width, height);
            SimpleTween.GizmosSize =
                EditorGUI.FloatField(gizmosHelperRect, "Gizmos Size", SimpleTween.GizmosSize);
            y += height;

            var buttonRedrawRect = new Rect(x, y, width, height);
            if (GUI.Button(buttonRedrawRect, "Redraw")) TargetTween?.OnGuiChange();
            y += height;

            return y - propertyRect.y;
        }

        private void AddCurrentPosition()
        {
            if (TargetTween is not MultiPositionTween multiPositionTween) return;
            var position = multiPositionTween.GetCurrentPosition();
            multiPositionTween.PointsPositions.Add(position);
        }
    }
}