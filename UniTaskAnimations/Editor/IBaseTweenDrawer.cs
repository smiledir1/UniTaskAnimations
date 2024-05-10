using Common.UniTaskAnimations.SimpleTweens;
using Cysharp.Threading.Tasks;
using UnityEditor;
using UnityEngine;

namespace Common.UniTaskAnimations.Editor
{
    [CustomPropertyDrawer(typeof(IBaseTween), true)]
    public class BaseTweenDrawer : PropertyDrawer
    {
        protected const float LineHeight = 20f;
        protected static float Space => 10f;
        protected static float LinesHeight => LineHeight;

        protected static IBaseTween CachedTween;

        public override void OnGUI(Rect rect, SerializedProperty property, GUIContent label)
        {
            var propertyYAdd = 0f;
            if (property.managedReferenceId == -1 ||
                property.managedReferenceValue == null)
                property.isExpanded = true;

            if (property.isExpanded)
            {
                DrawButtons(rect, property);
                propertyYAdd = LinesHeight;
            }

            var propertyRect = new Rect(rect.x, rect.y + propertyYAdd, rect.width, rect.height);
            EditorGUI.PropertyField(propertyRect, property, label, true);

            if (GUI.changed && property.managedReferenceValue is IBaseTween baseTween) OnGuiChange(baseTween).Forget();
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label) =>
            property.isExpanded
                ? EditorGUI.GetPropertyHeight(property) + LinesHeight + Space
                : EditorGUI.GetPropertyHeight(property);

        protected void DrawButtons(
            Rect propertyRect,
            SerializedProperty property,
            bool hasMultiButton = true)
        {
            IBaseTween baseTween = null;

            var buttonCount = hasMultiButton ? 6 : 5;
            var buttonWidth = propertyRect.width / buttonCount;
            var x = propertyRect.x;
            var y = propertyRect.yMin;
            var buttonRect = new Rect(x, y, buttonWidth, LineHeight);

            if (GUI.Button(buttonRect, "Null")) property.managedReferenceValue = null;

            x = buttonRect.x + buttonWidth;
            buttonRect = new Rect(x, y, buttonWidth, LineHeight);
            if (GUI.Button(buttonRect, "Group")) baseTween = new GroupTween(true);

            if (hasMultiButton)
            {
                x = buttonRect.x + buttonWidth;
                buttonRect = new Rect(x, y, buttonWidth, LineHeight);
                if (GUI.Button(buttonRect, "Multi")) baseTween = new MultiTween(null, 0.2f);
            }

            x = buttonRect.x + buttonWidth;
            buttonRect = new Rect(x, y, buttonWidth, LineHeight);
            if (GUI.Button(buttonRect, "Simple")) baseTween = new PositionTween();

            x = buttonRect.x + buttonWidth;
            buttonRect = new Rect(x, y, buttonWidth, LineHeight);
            if (GUI.Button(buttonRect, "Copy")) CachedTween = property.managedReferenceValue as IBaseTween;

            x = buttonRect.x + buttonWidth;
            buttonRect = new Rect(x, y, buttonWidth, LineHeight);
            if (GUI.Button(buttonRect, "Paste"))
            {
                var currentTween = property.managedReferenceValue as IBaseTween;

                GameObject targetGo = null;
                if (currentTween is SimpleTween simpleTween) targetGo = simpleTween.TweenObject;

                if (targetGo == null)
                {
                    var component = property.serializedObject?.targetObject as Component;
                    if (component != null) targetGo = component.gameObject;
                }

                var cloneTween = IBaseTween.Clone(CachedTween, targetGo);
                property.managedReferenceValue = cloneTween;
            }

            if (baseTween != null)
            {
                if (property.managedReferenceId == -1)
                    Debug.Log("Shoud Be SerializeReference");
                else
                    property.managedReferenceValue = baseTween;
            }
        }

        protected async UniTask OnGuiChange(IBaseTween baseTween)
        {
            await UniTask.Yield();
            baseTween.OnGuiChange();
        }
    }
}