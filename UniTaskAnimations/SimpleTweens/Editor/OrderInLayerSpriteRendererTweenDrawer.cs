using Common.UniTaskAnimations.Editor;
using UnityEditor;
using UnityEngine;

namespace Common.UniTaskAnimations.SimpleTweens.Editor
{
    [CustomPropertyDrawer(typeof(OrderInLayerSpriteRendererTween), true)]
    public class OrderInLayerSpriteRendererTweenDrawer : SimpleTweenDrawer
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
            var fromOpacityProperty = property.FindPropertyRelative("fromOrder");
            EditorGUI.PropertyField(fromOpacityRect, fromOpacityProperty);

            var buttonX = x + partWidth;
            var fromGoToButtonRect = new Rect(buttonX, y, buttonWidth, height);
            if (GUI.Button(fromGoToButtonRect, "Go To")) FromGotoOrder();

            var buttonX2 = buttonX + buttonWidth;
            var fromCopyButtonRect = new Rect(buttonX2, y, buttonWidth, height);
            if (GUI.Button(fromCopyButtonRect, "Copy From OBJ")) FromCopyOrder();
            y += height;

            var toOpacityRect = new Rect(x, y, partWidth, height);
            var toOpacityProperty = property.FindPropertyRelative("toOrder");
            EditorGUI.PropertyField(toOpacityRect, toOpacityProperty);

            var toGoToButtonRect = new Rect(buttonX, y, buttonWidth, height);
            if (GUI.Button(toGoToButtonRect, "Go To")) ToGotoOrder();

            var toCopyButtonRect = new Rect(buttonX2, y, buttonWidth, height);
            if (GUI.Button(toCopyButtonRect, "Copy From OBJ")) ToCopyOrder();
            y += height;

            var tweenGraphicRect = new Rect(x, y, width, height);
            var tweenGraphicProperty = property.FindPropertyRelative("tweenGraphic");
            EditorGUI.PropertyField(tweenGraphicRect, tweenGraphicProperty);
            y += height;

            return y - propertyRect.y;
        }
        
        protected override float DrawTweenPropertiesHeight(SerializedProperty property) => LineHeight * 4;
        
        private void FromGotoOrder()
        {
            if (TargetTween is not OrderInLayerSpriteRendererTween tween) return;
            tween.TweenObjectRenderer.sortingOrder = tween.FromOrder;
        }

        private void FromCopyOrder()
        {
            if (TargetTween is not OrderInLayerSpriteRendererTween tween) return;
            var order = tween.TweenObjectRenderer.sortingOrder;
            tween.SetOrder(order, tween.ToOrder);
        }
        
        private void ToGotoOrder()
        {
            if (TargetTween is not OrderInLayerSpriteRendererTween tween) return;
            tween.TweenObjectRenderer.sortingOrder = tween.ToOrder;
        }

        private void ToCopyOrder()
        {
            if (TargetTween is not OrderInLayerSpriteRendererTween tween) return;
            var order = tween.TweenObjectRenderer.sortingOrder;
            tween.SetOrder(tween.FromOrder, order);
        }
    }
}
