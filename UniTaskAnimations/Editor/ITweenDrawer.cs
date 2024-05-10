using Cysharp.Threading.Tasks;
using UnityEditor;
using UnityEngine;

namespace Common.UniTaskAnimations.Editor
{
    [CustomPropertyDrawer(typeof(ITween), true)]
    public class TweenDrawer : BaseTweenDrawer
    {
        public override void OnGUI(Rect rect, SerializedProperty property, GUIContent label)
        {
            var propertyYAdd = 0f;
            if (property.managedReferenceId == -1 ||
                property.managedReferenceValue == null)
                property.isExpanded = true;

            if (property.isExpanded)
            {
                DrawButtons(rect, property, false);
                propertyYAdd = LinesHeight;
            }

            var propertyRect = new Rect(rect.x, rect.y + propertyYAdd, rect.width, rect.height);
            EditorGUI.PropertyField(propertyRect, property, label, true);

            if (GUI.changed && property.managedReferenceValue is IBaseTween baseTween) OnGuiChange(baseTween).Forget();
        }
    }
}