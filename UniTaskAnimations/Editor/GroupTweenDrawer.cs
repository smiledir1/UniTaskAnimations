using UnityEditor;
using UnityEngine;

namespace Common.UniTaskAnimations.Editor
{
    [CustomPropertyDrawer(typeof(GroupTween), true)]
    public class GroupTweenDrawer : TweenDrawer
    {
        public override void OnGUI(Rect rect, SerializedProperty property, GUIContent label)
        {
            base.OnGUI(rect, property, label);

            if (property.isExpanded)
            {
                DrawButtons(rect, property);
            }
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label) =>
            base.GetPropertyHeight(property, label) + LinesHeight + Space;

        private void DrawButtons(Rect propertyRect, SerializedProperty property)
        {
            var buttonCount = 2;
            var buttonWidth = propertyRect.width / buttonCount;
            var x = propertyRect.x;
            var y = propertyRect.yMax - LineHeight - Space;

            var buttonRectFromComponents = new Rect(x, y, buttonWidth, LineHeight);
            if (GUI.Button(buttonRectFromComponents, "Make Tweens From Components"))
                MakeTweensFromComponents(property);

            var buttonXToComponents = x + buttonWidth;
            var buttonRectToComponents = new Rect(buttonXToComponents, y, buttonWidth, LineHeight);
            if (GUI.Button(buttonRectToComponents, "Make Tweens To Components"))
                MakeTweensToComponents(property);
        }

        private void MakeTweensFromComponents(SerializedProperty property)
        {
            if (property.managedReferenceValue is not GroupTween groupTween) return;
            foreach (var component in groupTween.Components)
            {
                if (component.Tween is not ITween iTween) continue;
                groupTween.Tweens.Add(iTween);
                Object.DestroyImmediate(component);
            }
            groupTween.Components.Clear();
        }

        private void MakeTweensToComponents(SerializedProperty property)
        {
            if (property.managedReferenceValue is not GroupTween groupTween) return;
            foreach (var tween in groupTween.Tweens)
            {
                if (tween is not SimpleTween simpleTween) continue;
                if (simpleTween.TweenObject == null) continue;
                var tweenComponent = simpleTween.TweenObject.AddComponent<TweenComponent>();
                tweenComponent.SetTween(simpleTween);
                groupTween.Components.Add(tweenComponent);
            }

            groupTween.Tweens.Clear();
        }
    }
}